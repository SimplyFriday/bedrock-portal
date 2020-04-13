﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using MinecraftWrapper.Data;
using MinecraftWrapper.Data.Entities;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MinecraftWrapper.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ExternalLoginModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<ExternalLoginModel> _logger;
        private readonly UserRepository _userRepository;
        private readonly RoleManager<ApplicationUser> _roleManager;

        public ExternalLoginModel (
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            ILogger<ExternalLoginModel> logger,
            UserRepository userRepository,
            RoleManager<ApplicationUser> roleManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
            _userRepository = userRepository;
            _roleManager = roleManager;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string LoginProvider { get; set; }

        public string ReturnUrl { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }


            [Required]
            [Display ( Name = "Discord Handle" )]
            [MaxLength ( 255 )]
            public string DiscordHandle { get; set; }

            [Required]
            [Display ( Name = "Gamer Tag" )]
            [MaxLength ( 255 )]
            public string GamerTag { get; set; }
        }

        public IActionResult OnGetAsync ()
        {
            return RedirectToPage ( "./Login" );
        }

        public IActionResult OnPost ( string provider, string returnUrl = null )
        {
            // Request a redirect to the external login provider.
            var redirectUrl = Url.Page("./ExternalLogin", pageHandler: "Callback", values: new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return new ChallengeResult ( provider, properties );
        }

        public async Task<IActionResult> OnGetCallbackAsync ( string returnUrl = null, string remoteError = null )
        {
            returnUrl = returnUrl ?? Url.Content ( "~/" );
            if ( remoteError != null )
            {
                ErrorMessage = $"Error from external provider: {remoteError}";
                return RedirectToPage ( "./Login", new { ReturnUrl = returnUrl } );
            }
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if ( info == null )
            {
                ErrorMessage = "Error loading external login information.";
                return RedirectToPage ( "./Login", new { ReturnUrl = returnUrl } );
            }

            // Sign in the user with this external login provider if the user already has a login.
            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor : true);
            if ( result.Succeeded )
            {
                _logger.LogInformation ( "{Name} logged in with {LoginProvider} provider.", info.Principal.Identity.Name, info.LoginProvider );
                return LocalRedirect ( returnUrl );
            }
            if ( result.IsLockedOut )
            {
                return RedirectToPage ( "./Lockout" );
            }
            else
            {
                // If the user does not have an account, then ask the user to create an account.
                ReturnUrl = returnUrl;
                LoginProvider = info.LoginProvider;
                if ( info.Principal.HasClaim ( c => c.Type == ClaimTypes.Email ) )
                {
                    Input = new InputModel
                    {
                        Email = info.Principal.FindFirstValue ( ClaimTypes.Email )
                    };
                }
                return Page ();
            }
        }

        public async Task<IActionResult> OnPostConfirmationAsync ( string returnUrl = null )
        {
            returnUrl = returnUrl ?? Url.Content ( "~/" );
            // Get the information about the user from the external login provider
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if ( info == null )
            {
                ErrorMessage = "Error loading external login information during confirmation.";
                return RedirectToPage ( "./Login", new { ReturnUrl = returnUrl } );
            }

            if ( ModelState.IsValid )
            {
                var user = new ApplicationUser { UserName = Input.Email, Email = Input.Email, GamerTag = Input.GamerTag, EmailConfirmed = true, DiscordId = Input.DiscordHandle, Rank = 1, IsActive = false };

                var result = await _userManager.CreateAsync ( user );
                if ( result.Succeeded )
                {
                    result = await _userManager.AddLoginAsync ( user, info );
                    if ( result.Succeeded )
                    {
                        await _signInManager.SignInAsync ( user, isPersistent: false );
                        _logger.LogInformation ( "User created an account using {Name} provider.", info.LoginProvider );

                        // If there are no admins, add the next create user as an admin
                        var adminUsers = await _userManager.GetUsersInRoleAsync ( "Admin" );
                        if ( adminUsers.Count == 0 )
                        {
                            await _userManager.AddToRoleAsync ( user, "Admin" );
                            await _userManager.UpdateAsync ( user );
                        }

                        return LocalRedirect ( returnUrl );
                    }

                }
                foreach ( var error in result.Errors )
                {
                    ModelState.AddModelError ( string.Empty, error.Description );
                }

            }

            LoginProvider = info.LoginProvider;
            ReturnUrl = returnUrl;
            return Page ();
        }
    }
}
