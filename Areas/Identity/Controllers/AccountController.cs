// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using App.Areas.Identity.Models.AccountViewModels;
using App.ExtendMethods;
using App.Models;
using App.Utilities;
using HocAspMVC4.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;

namespace App.Areas.Identity.Controllers
{
    [Authorize]
    [Area("Identity")]
    [Route("/Account/[action]")]
    public class AccountController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        // GET: /Account/Register   //Get VÀO TRANG ĐĂNG KÝ
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register(string returnUrl = null)
        {
            //lấy url trước đó của user để khi user đăng ký xong thì trả về trang đó cho user
            returnUrl ??= Url.Content("~/"); 
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: /Account/Register   //Post THỰC HIỆN ĐĂNG KÝ
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
                                                //model binding từ input của user
        public async Task<IActionResult> Register(RegisterViewModel model, string returnUrl = null) 
        {
            returnUrl ??= Url.Content("~/");
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)  //nếu input binding đến hợp lệ 
            {
                //tạo 1 user từ input binding đến
                var user = new AppUser { UserName = model.UserName, Email = model.Email };
                var result = await _userManager.CreateAsync(user, model.Password); //thêm 1 user vào csdl

                if (result.Succeeded) 
                {
                    _logger.LogInformation("Đã tạo user mới.");

                    //yêu cầu user phải xác nhận tài khoản mới dc truy cập (mặc định cái này là false)
                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        //return LocalRedirect(Url.Action(nameof(RegisterConfirmation)));
                    }
                    else  //vì là false nên nó sẽ cho truy cập 
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false); //thực hiện đăng nhập 
                        return LocalRedirect(returnUrl); //chuyển hướng đến trang trước đó của user
                    }

                }

                ModelState.AddModelError(result);
            }

            // nếu có lỗi thì trả về view của action này cùng vs dữ liệu user đã input
            return View(model); 
        }


        // GET: /Account/Login    //Get VÀO TRANG ĐĂNG NHẬP 
        [HttpGet("/login/")]
        [AllowAnonymous]
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost("/login/")]  //Post THỰC HIỆN ĐĂNG NHẬP
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)  //input user gửi đến
            {
                
                //dùng dịch vụ signInManager để đăng nhập
                var result = await _signInManager.PasswordSignInAsync(model.UserNameOrEmail, model.Password, model.RememberMe, lockoutOnFailure: true);                

                // Tìm UserName theo Email, đăng nhập lại trong trường hợp user dùng email để login
                if ((!result.Succeeded) && AppUtilities.IsValidEmail(model.UserNameOrEmail))
                {
                    //tìm user theo email 
                    var user = await _userManager.FindByEmailAsync(model.UserNameOrEmail);
                    if (user != null)  
                    {
                        //đăng nhập cho user
                        result = await _signInManager.PasswordSignInAsync(user.UserName, model.Password, model.RememberMe, lockoutOnFailure: true);
                    }
                } 
                if (result.Succeeded)
                {
                    _logger.LogInformation(1, "User logged in.");
                    return LocalRedirect(returnUrl);
                }
                if (result.IsLockedOut) //nếu tài khoản bị khóa
                {
                    _logger.LogWarning(2, "Tài khoản bị khóa");
                    return View("Lockout");
                }
                else
                {
                    ModelState.AddModelError("Không thể đăng nhập, vui lòng kiểm tra lại thông tin");
                    return View(model);
                }
            }
            // nếu có lỗi thì trả về view của action này cùng vs dữ liệu user đã input
            return View(model); 
        }

        // POST: /Account/LogOff    //Post ĐĂNG XUẤT
        [HttpPost("/logout/")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogOff()
        {
            await _signInManager.SignOutAsync();   
            _logger.LogInformation("User đăng xuất");
            return RedirectToAction("Index", "Home", new {area = ""});
        }


        //
        // GET: /Account/ForgotPassword
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View();
        }


        // GET: /Account/ResetPassword
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(string code = null)
        {
            return code == null ? View("Error") : View();
        }

        
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return RedirectToAction(nameof(AccountController.ResetPasswordConfirmation), "Account");
            }
            var code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(model.Code));

            var result = await _userManager.ResetPasswordAsync(user, code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction(nameof(AccountController.ResetPasswordConfirmation), "Account");
            }
            ModelState.AddModelError(result);
            return View();
        }

        //
        // GET: /Account/ResetPasswordConfirmation
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }        


        [Route("/khongduoctruycap.html")]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }



    
  }
}
