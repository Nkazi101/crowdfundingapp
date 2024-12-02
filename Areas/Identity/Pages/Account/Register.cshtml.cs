

#nullable disable // Disable nullable reference types for this file

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations; // For validation attributes
using System.Linq;
using System.Text;
using System.Text.Encodings.Web; // For HTML encoding
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Crowdfunding.Models; // Your application's models
using Microsoft.AspNetCore.Identity; // Identity classes
using Microsoft.AspNetCore.Identity.UI.Services; // For email sending
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages; // For Razor Pages
using Microsoft.AspNetCore.WebUtilities; // For encoding/decoding URLs
using Microsoft.Extensions.Logging; // For logging
using Crowdfunding.Enums; // Your application's enums

namespace Crowdfunding.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel // Represents the registration page model
    {
        // Dependency injection of required services
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly IUserStore<User> _userStore;
        private readonly IUserEmailStore<User> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;

        // Constructor that initializes the services
        public RegisterModel(
            UserManager<User> userManager,
            IUserStore<User> userStore,
            SignInManager<User> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender)
        {
            _userManager = userManager; // Manages users
            _userStore = userStore; // Stores user data
            _emailStore = GetEmailStore(); // Email-specific user store
            _signInManager = signInManager; // Manages sign-in operations
            _logger = logger; // Logs information
            _emailSender = emailSender; // Sends emails
        }

        /// <summary>
        /// Input model bound to the registration form
        /// </summary>
        [BindProperty] // Binds the property to the form inputs
        public InputModel Input { get; set; }

        /// <summary>
        /// URL to redirect to after successful registration
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        /// List of external authentication providers
        /// </summary>
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        /// <summary>
        /// Model for user input fields
        /// </summary>
        public class InputModel
        {
            [Required] // Field is required
            [EmailAddress] // Must be a valid email format
            [Display(Name = "Email")] // Label for the field
            public string Email { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)] // Masks input as a password
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")] // Ensures passwords match
            public string ConfirmPassword { get; set; }

            [Required]
            [Display(Name = "Role")]
            public UserRole Role { get; set; } // User role selected during registration
        }

        // Handles GET requests to the registration page
        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl; // Sets the return URL
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList(); // Retrieves external login providers
        }

        // Handles POST requests when the registration form is submitted
        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/"); // Sets default return URL if none provided
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList(); // Retrieves external login providers

            if (ModelState.IsValid) // Checks if form data is valid
            {
                var user = CreateUser(); // Creates a new user instance

                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None); // Sets the username
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None); // Sets the email

                // Set the user's role from the input
                user.Role = Input.Role;

                var result = await _userManager.CreateAsync(user, Input.Password); // Attempts to create the user

                if (result.Succeeded) // If user creation was successful
                {
                    _logger.LogInformation("User created a new account with password."); // Logs the event

                    // Adds the user to the specified role
                    await _userManager.AddToRoleAsync(user, user.Role.ToString());

                    var userId = await _userManager.GetUserIdAsync(user); // Retrieves the user ID
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user); // Generates email confirmation token
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code)); // Encodes the token

                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail", // Confirmation page
                        pageHandler: null,
                        values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl }, // Route parameters
                        protocol: Request.Scheme); // Uses the current request scheme (HTTP/HTTPS)

                    // Sends confirmation email to the user
                    await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                        $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                    // Checks if email confirmation is required before sign-in
                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        // Redirects to the registration confirmation page
                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                    }
                    else
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false); // Signs in the user
                        return LocalRedirect(returnUrl); // Redirects to the specified return URL
                    }
                }

                // Adds errors to the model state if user creation failed
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description); // Displays the error message
                }
            }

            // If validation failed or user creation failed, redisplay the form
            return Page();
        }

        // Creates a new user instance
        private User CreateUser()
        {
            try
            {
                return Activator.CreateInstance<User>(); // Creates a new instance of the User class
            }
            catch
            {
                // Throws an exception if user creation fails
                throw new InvalidOperationException($"Can't create an instance of '{nameof(User)}'. " +
                    $"Ensure that '{nameof(User)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        // Retrieves the email store from the user store
        private IUserEmailStore<User> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail) // Checks if the user manager supports email
            {
                // Throws an exception if email is not supported
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<User>)_userStore; // Casts and returns the email store
        }
    }
}