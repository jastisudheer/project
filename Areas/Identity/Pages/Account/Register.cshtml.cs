﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;

namespace Persol_HMS.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly IUserStore<User> _userStore;
        private readonly IUserEmailStore<User> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;

        public RegisterModel(
            UserManager<User> userManager,
            IUserStore<User> userStore,
            SignInManager<User> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public class InputModel
        {

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }

            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required(ErrorMessage = "Please enter your username")]
            public string UserName { get; set; }

            [Required(ErrorMessage = "Please enter your firstname")]
            public string FirstName { get; set; }

            [Required(ErrorMessage = "Please select a department")]
            [Display(Name = "Department")]
            public int DepartmentId { get; set; }

            [Required(ErrorMessage = "Please enter your middle name")]
            public string MiddleName { get; set; }

            [Required(ErrorMessage = "Please enter your lastname")]
            public string LastName { get; set; }

            [Required(ErrorMessage = "Please enter your birthdate")]
            [DataType(DataType.Date)]
            [Display(Name = "Date of Birth")]
            public DateTime DateOfBirth { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {
                var user = CreateUser();
                var departmentRole = Enum.GetName(typeof(DepartmentType), Input.DepartmentId);

                // await _userManager.AddToRoleAsync(user, departmentRole);
                await _userStore.SetUserNameAsync(user, Input.UserName, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);
                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    var userId = await _userManager.GetUserIdAsync(user);
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                        protocol: Request.Scheme);

                    await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                        $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                    }
                    else
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);

                        switch (user.DepartmentId)
                        {
                            case 1:
                                returnUrl = Url.Action("RecordsClerk", "Staff");
                                break;
                            case 2:
                                returnUrl = Url.Action("NurseQueue", "Staff");
                                break;
                            case 3:
                                returnUrl = Url.Action("DoctorQueue", "Staff");
                                break;
                            case 4:
                                returnUrl = Url.Action("LabQueue", "Staff");
                                break;
                            case 5:
                                returnUrl = Url.Action("Index", "Admin");
                                break;
                            case 6:
                                returnUrl = Url.Action("PharmacyQueue", "Staff");
                                break;
                            case 7:
                                returnUrl = Url.Action("CashierQueue", "Staff");
                                break;
                            default:
                                returnUrl = Url.Content("~/");
                                break;
                        }

                        return LocalRedirect(returnUrl);
                    }
                }

                // Log errors
                foreach (var error in result.Errors)
                {
                    _logger.LogError($"Error creating user: {error.Description}");
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }

        private User CreateUser()
        {
            try
            {
                var user = new User
                {
                    FirstName = Input.FirstName,
                    MiddleName = Input.MiddleName,
                    LastName = Input.LastName,
                    DateOfBirth = Input.DateOfBirth,
                    UserName = Input.UserName,
                    Email = Input.Email,
                    DepartmentId = Input.DepartmentId,
                    Status = "Active",
                    CreatedDate = DateTime.Now,
                    Attempts = 0
                };

                return user;
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(User)}'.");
            }
        }



        private IUserEmailStore<User> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<User>)_userStore;
        }
    }
}
