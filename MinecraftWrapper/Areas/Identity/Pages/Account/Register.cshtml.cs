﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using MinecraftWrapper.Data;
using MinecraftWrapper.Data.Entities;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace MinecraftWrapper.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly UserRepository _userRepository;

        public RegisterModel (
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            UserRepository userRepository )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _userRepository = userRepository;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            [Display ( Name = "Email" )]
            public string Email { get; set; }

            [Required]
            [StringLength ( 128, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long. Click <a href=\"PasswordPolicy\">here</a> to understand our password policy.", MinimumLength = 8 )]
            [DataType ( DataType.Password )]
            [Display ( Name = "Password" )]
            public string Password { get; set; }

            [DataType ( DataType.Password )]
            [Display ( Name = "Confirm password" )]
            [Compare ( "Password", ErrorMessage = "The password and confirmation password do not match." )]
            public string ConfirmPassword { get; set; }

            [Required]
            [Display(Name = "Discord Handle")]
            [MaxLength(255)]
            public string DiscordHandle { get; set; }

            [Required]
            [Display ( Name = "Gamer Tag" )]
            [MaxLength ( 255 )]
            public string GamerTag { get; set; }
        }

        public void OnGet ( string returnUrl = null )
        {
            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync ( string returnUrl = null )
        {
            returnUrl = returnUrl ?? Url.Content ( "~/" );
            if ( ModelState.IsValid )
            {
                var user = new ApplicationUser { UserName = Input.Email, Email = Input.Email, GamerTag = Input.GamerTag, DiscordId = Input.DiscordHandle, Rank = 1, IsActive = false };
                var result = await _userManager.CreateAsync ( user, Input.Password );
                if ( result.Succeeded )
                {
                    _logger.LogInformation ( "User created a new account with password." );

                    var code = await _userManager.GenerateEmailConfirmationTokenAsync ( user );
                    var callbackUrl = Url.Page (
                            "/Account/ConfirmEmail",
                            pageHandler: null,
                            values: new { userId = user.Id, code = code },
                            protocol: Request.Scheme );

                    await _emailSender.SendEmailAsync ( Input.Email, "Confirm your email",
                        $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode ( callbackUrl )}'>clicking here</a>." );

                    // If there are no admins, add the next create user as an admin
                    var adminUsers = await _userManager.GetUsersInRoleAsync ( "Admin" );
                    if ( adminUsers.Count == 0 )
                    {
                        await _userManager.AddToRoleAsync ( user, "Admin" );
                        await _userManager.UpdateAsync ( user );
                    }

                    await _signInManager.SignInAsync ( user, isPersistent: false );
                    
                    return LocalRedirect ( returnUrl );
                }
                foreach ( var error in result.Errors )
                {
                    ModelState.AddModelError ( string.Empty, error.Description );
                }

            }

            // If we got this far, something failed, redisplay form
            return Page ();
        }
    }
}
