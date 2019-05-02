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
using System;
using System.Threading.Tasks;

namespace MinecraftWrapper.Controllers
{
    [Authorize ( Roles = "Admin" )]
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

        // GET: Store
        public async Task<IActionResult> EditIndex ()
        {
            var items = await _storeRepository.GetAllItems ();

            return View ( items );
        }

        // GET: Store/Details/5
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

        // GET: Store/Create
        public IActionResult Create ()
        {
            return View ();
        }

        // POST: Store/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create ( [Bind ( "StoreItemId,Description,StoreItemTypeId,MinimumRank,Price,Title,Effect" )] StoreItem storeItem )
        {
            if ( ModelState.IsValid )
            {
                await _storeRepository.SaveStoreItemAsync ( storeItem );
                return RedirectToAction ( nameof ( EditIndex ) );
            }
            return View ( storeItem );
        }

        // GET: Store/Edit/5
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
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Index ( [FromQuery] string statusMessage )
        {
            var user = await _userManager.GetUserAsync ( HttpContext.User );
            var items = await _storeRepository.GetAvailableStoreItemsByRank ( user.Rank );
            var currentMoney = await _storeRepository.GetCurrencyTotalForUser ( user.Id );

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

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> PurchaseItem ( Guid? id )
        {
            string statusMessage = null;

            if ( id != null && id != Guid.Empty )
            {
                var user = await _userManager.GetUserAsync ( HttpContext.User );
                var item = await _storeRepository.GetStoreItemByIdAsync( id );
                var currentMoney = await _storeRepository.GetCurrencyTotalForUser ( user.Id );

                if ( currentMoney >= item.Price ) 
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

            var uc = new UserCurrency
            {
                Amount = model.Amount,
                CurrencyTransactionReasonId = model.CurrencyTransactionReason,
                CurrencyTypeId = CurrencyType.Normal,
                DateNoted = DateTime.UtcNow,
                UserId = user.Id
            };

            await _storeRepository.SaveUserCurrency ( uc );

            return Ok ();
        }
    }
}
