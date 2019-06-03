using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MinecraftWrapper.Data;
using MinecraftWrapper.Data.Constants;
using MinecraftWrapper.Data.Entities;
using MinecraftWrapper.Models;
using MinecraftWrapper.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MinecraftWrapper.Controllers
{
    [Authorize]
    public class StoreController : Controller
    {
        private readonly UserRepository _userRepository;
        private readonly StoreRepository _storeRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationSettings _applicationSettings;
        private readonly MinecraftStoreService _minecraftStoreService;

        public StoreController ( UserRepository userRepository, UserManager<ApplicationUser> userManager, IOptions<ApplicationSettings> options, StoreRepository storeRepository, MinecraftStoreService minecraftStoreService )
        {
            _userRepository = userRepository;
            _userManager = userManager;
            _applicationSettings = options.Value;
            _storeRepository = storeRepository;
            _minecraftStoreService = minecraftStoreService;
        }

        [Authorize ( Roles = "Admin" )]
        public async Task<IActionResult> EditIndex ()
        {
            var items = await _storeRepository.GetAllItems ();

            return View ( items );
        }

        [Authorize ( Roles = "Admin" )]
        public async Task<IActionResult> Details ( Guid? id )
        {
            if ( id == null )
            {
                return NotFound ();
            }

            var storeItem = await _storeRepository.GetStoreItemByIdAsync ( id );

            if ( storeItem == null )
            {
                return NotFound ();
            }

            return View ( storeItem );
        }

        [Authorize ( Roles = "Admin" )]
        public IActionResult Create ()
        {
            return View ();
        }

        // POST: Store/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize ( Roles = "Admin" )]
        public async Task<IActionResult> Create ( [Bind ( "StoreItemId,Description,StoreItemTypeId,MinimumRank,Price,Title,Effect" )] StoreItem storeItem )
        {
            if ( ModelState.IsValid )
            {
                await _storeRepository.SaveStoreItemAsync ( storeItem );
                return RedirectToAction ( nameof ( EditIndex ) );
            }
            return View ( storeItem );
        }

        [Authorize ( Roles = "Admin" )]
        public async Task<IActionResult> Edit ( Guid? id )
        {
            if ( id == null )
            {
                return NotFound ();
            }

            var storeItem = await _storeRepository.GetStoreItemByIdAsync ( id );
            if ( storeItem == null )
            {
                return NotFound ();
            }
            return View ( storeItem );
        }

        // POST: Store/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize ( Roles = "Admin" )]
        public async Task<IActionResult> Edit ( Guid id, [Bind ( "StoreItemId,Description,StoreItemTypeId,MinimumRank,Price,Title,Effect" )] StoreItem storeItem )
        {
            if ( id != storeItem.StoreItemId )
            {
                return NotFound ();
            }

            if ( ModelState.IsValid )
            {
                try
                {
                    await _storeRepository.SaveStoreItemAsync ( storeItem );
                }
                catch ( DbUpdateConcurrencyException )
                {
                    if ( !_storeRepository.StoreItemExists ( storeItem.StoreItemId ) )
                    {
                        return NotFound ();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction ( nameof ( EditIndex ) );
            }
            return View ( storeItem );
        }

        [Authorize ( Roles = "Admin" )]
        [HttpGet]
        public async Task<IActionResult> Delete ( Guid? id )
        {
            if ( id == null )
            {
                return NotFound ();
            }

            var storeItem = await _storeRepository.GetStoreItemByIdAsync ( id );

            if ( storeItem == null )
            {
                return NotFound ();
            }

            return View ( storeItem );
        }

        [ValidateAntiForgeryToken]
        [HttpPost, ActionName ( "Delete" )]
        [Authorize ( Roles = "Admin" )]
        public async Task<IActionResult> DeleteConfirmed (Guid? id)
        {
            if ( id == null )
            {
                return NotFound ();
            }

            var storeItem = await _storeRepository.GetStoreItemByIdAsync ( id );

            if ( storeItem != null ) 
            {
                await _storeRepository.DeleteStoreItemAsync ( storeItem );
                ViewBag.Status = "Item deleted";
            }
            else
            {
                return NotFound ();
            }

            return RedirectToAction ( nameof ( EditIndex ) );
        }

        /**************************************************************************************************************
         **************************************************************************************************************
         ********************************************** NON ADMIN METHODS *********************************************
         **************************************************************************************************************
         **************************************************************************************************************/
         

        [HttpGet]
        public async Task<IActionResult> Index ( [FromQuery] string statusMessage = "" )
        {
            var user = await _userManager.GetUserAsync ( HttpContext.User );
            var items = await _storeRepository.GetAvailableStoreItemsByRank ( user.Rank );
            var currentMoney = await _storeRepository.GetCurrencyTotalForUserAsync ( user.Id, CurrencyType.Normal );
            
            foreach (var item in items )
            {
                item.Price = _minecraftStoreService.GetDiscountedValueForUser ( item.Price, user );
            }

            var viewModel = new StoreIndexViewModel
            {
                StoreItems = items,
                UserCurrencyTotel = currentMoney
            };

            if ( !string.IsNullOrEmpty ( statusMessage ) ) 
            {
                ViewBag.Status = statusMessage;
            }

            return View ( viewModel );
        }

        [HttpGet]
        public async Task<IActionResult> PurchaseItem ( Guid? id )
        {
            string statusMessage;

            if ( id != null && id != Guid.Empty )
            {
                var user = await _userManager.GetUserAsync ( HttpContext.User );
                var item = await _storeRepository.GetStoreItemByIdAsync( id );
                var currentMoney = await _storeRepository.GetCurrencyTotalForUserAsync ( user.Id, CurrencyType.Normal );
                var realPrice = _minecraftStoreService.GetDiscountedValueForUser(item.Price, user);

                if ( currentMoney >= realPrice ) 
                {
                    await _minecraftStoreService.PurchaseItemAsync ( item, user );
                    statusMessage = $"{item.Title} purchased!";
                }
                else
                {
                    statusMessage = $"{item.Title} requires {item.Price} {_applicationSettings.MinecraftCurrencyName} but you only have {currentMoney} {_applicationSettings.MinecraftCurrencyName}";
                }
            }
            else
            {
                statusMessage = $"storeItemId is a required parameter";
            }

            // TODO figure out how to preserve ViewBag after redirect
            return RedirectToAction ( nameof ( Index ), "Store", new { statusMessage }, "" );
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> AddCurrencyToUser ( [FromBody] AddCurrencyToUserModel model )
        {
            if ( model.Secret != _applicationSettings.DiscordBotSecret ) 
            {
                return Unauthorized ();
            }

            var user = await _userManager.Users.SingleOrDefaultAsync ( u => u.DiscordId.StartsWith ( model.DiscordId ) );

            if ( user == null ) 
            {
                return NotFound ( $"No user was found for DiscordId: {model.DiscordId}" );
            }

            var lastMessage = _storeRepository.GetMostRecentUserCurrencyByUserIdAndReason (user.Id, model.CurrencyTransactionReason);

            if ( lastMessage == null || (DateTime.UtcNow - lastMessage.DateNoted).TotalSeconds >= _applicationSettings.DiscordPointCooldownInSeconds ) 
            {
                var uc = new UserCurrency
                {
                    Amount = model.Amount,
                    CurrencyTransactionReasonId = model.CurrencyTransactionReason,
                    CurrencyTypeId = CurrencyType.Normal,
                    DateNoted = DateTime.UtcNow,
                    UserId = user.Id
                };

                await _storeRepository.SaveUserCurrency ( uc );
            }

            return Ok ();
        }

        [HttpGet]
        public async Task<IActionResult> GiftCurrency ()
        {
            if ( !string.IsNullOrEmpty ( TempData["Status"]?.ToString () ) ) 
            {
                ViewBag.Status = TempData["Status"];
            }

            var user = await _userManager.GetUserAsync ( HttpContext.User );
            var giftCurrency = await _storeRepository.GetCurrencyTotalForUserAsync ( user.Id, CurrencyType.Gift );
            var activeUsers = await _userManager.Users.Where ( u => u.IsActive && u.Id != user.Id ).ToListAsync ();

            var model = new GiftCurrencyViewModel
            {
                GiftCurrancy = giftCurrency,
                Users = activeUsers
            };

            return View ( model );
        }

        [HttpGet]
        public async Task<IActionResult> SendGift (string id)
        {
            if ( !string.IsNullOrEmpty ( TempData["Status"]?.ToString () ) )
            {
                ViewBag.Status = TempData["Status"];
            }

            var user = await _userManager.GetUserAsync(HttpContext.User);

            var model = new SendGiftViewModel
            {
                Amount = 0,
                CurrentGiftCurrency = await _storeRepository.GetCurrencyTotalForUserAsync ( user.Id, CurrencyType.Gift ),
                GamerTag = id
            };

            return View ( model );
        }

        [HttpPost]
        public async Task<IActionResult> SendGift ( SendGiftViewModel model )
        {
            var currentUser = await _userManager.GetUserAsync ( HttpContext.User );
            var targetUser = await _userRepository.GetUserByGamerTagAsync ( model.GamerTag );
            var currentUserCurrency = await _storeRepository.GetCurrencyTotalForUserAsync(currentUser.Id, CurrencyType.Gift);
            
            if (model.Amount > currentUserCurrency )
            {
                ModelState.AddModelError ( "NotEnoughMoney", "You don't have enough gift currency to send that much!" );
            }

            if (targetUser == null )
            {
                ModelState.AddModelError ( "GamerTagNotFound", $"{model.GamerTag} doesn't exist!" );
            }

            if (targetUser?.Id == currentUser.Id )
            {
                ModelState.AddModelError ( "SelfishBastard", "You cannot send gifts to yourself!" );
            }

            if ( model.Amount <= 0 )
            {
                ModelState.AddModelError ( "TooLittle", "Amount must be a positive number!" );
            }

            if ( ModelState.IsValid )
            {
                await _minecraftStoreService.AddCurrencyForUser ( currentUser.GamerTag, -model.Amount, CurrencyTransactionReason.Gift, CurrencyType.Gift );
                await _minecraftStoreService.AddCurrencyForUser ( targetUser.GamerTag, model.Amount, CurrencyTransactionReason.Gift, CurrencyType.Normal );

                TempData["Status"] = $"{model.Amount} was sent to {model.GamerTag}!";

                return RedirectToAction ( nameof ( GiftCurrency ) );
            }

            TempData["Status"] = "ERROR: " + string.Join(Environment.NewLine,ModelState.Values
                .SelectMany(state => state.Errors)
                .Select(error => error.ErrorMessage));

            return RedirectToAction ( nameof ( SendGift ), "Store", "id", model.GamerTag );
        }
    }
}
