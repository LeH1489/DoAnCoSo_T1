// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using App.Areas.Identity.Models.AccountViewModels;
using App.Areas.Identity.Models.ManageViewModels;
using App.Areas.Identity.Models.RoleViewModels;
using App.Areas.Identity.Models.UserViewModels;
using App.Data;
using App.ExtendMethods;
using App.Models;
using App.Services;
using HocAspMVC4.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace App.Areas.Identity.Controllers
{

    [Authorize(Policy = "AllowEditRole")]
    [Area("Identity")]
    [Route("/ManageUser/[action]")]
    public class UserController : Controller
    {
        
        private readonly ILogger<RoleController> _logger;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly AppDbContext1 _context;

        private readonly UserManager<AppUser> _userManager;

        public UserController(ILogger<RoleController> logger, RoleManager<IdentityRole> roleManager, AppDbContext1 context, UserManager<AppUser> userManager)
        {
            _logger = logger;
            _roleManager = roleManager;
            _context = context;
            _userManager = userManager;
        }

        [Route("/AccessDenied2")]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [TempData]
        public string StatusMessage { get; set; }

        //
        // GET: /ManageUser/Index 
        [HttpGet]
        public async Task<IActionResult> Index([FromQuery(Name = "p")] int currentPage)
        {
            var model = new UserListModel();  //trong model này có chứa properties cho paging.cshtml
            model.currentPage = currentPage;

            var qr = _userManager.Users.OrderBy(u => u.UserName);  //lấy ra name tất cả những user

            model.totalUsers = await qr.CountAsync(); //tính tổng user lưu vào totalUsers

                                        //  lấy tổng user/10
                                        // ví dụ 20 users/10 = 2 ==> chỉ có 2 trang thôi  
            model.countPages = (int)Math.Ceiling((double)model.totalUsers / model.ITEMS_PER_PAGE);

            if (model.currentPage < 1)  //nếu index của paging < 1 ==> đưa về trang 1
                model.currentPage = 1;
            if (model.currentPage > model.countPages)
                model.currentPage = model.countPages; //đưa về index cuối cùng 

            //ví dụ: trang 1 ==> (1-1)*10 = 0 sẽ skip 0 phần tử
            // trang 2 ==> (2-1)*10 = 10 sẽ skip 10 phần tử (vì đó là của trang 1)
            var qr1 = qr.Skip((model.currentPage - 1) * model.ITEMS_PER_PAGE)
                        .Take(model.ITEMS_PER_PAGE)  //lấy 10 user
                        .Select(u => new UserAndRole() {  //cái này nó kế thừa từ AppUser
                            Id = u.Id,
                            UserName = u.UserName,  
                        });   //cái qr này chỉ có name 

            model.users = await qr1.ToListAsync(); //model này chứa list đã lấy ở trên 

            foreach (var user in model.users)
            {
                var roles = await _userManager.GetRolesAsync(user);  //lấy tên của Role của user
                user.RoleNames = string.Join(",", roles); //nối tên role này thành chuỗi
            } 
            
            return View(model);  //truyền model vào view
        } 


        private async Task GetClaims(AddUserRoleModel model)
        {
            //cái join này là nó join 2 bảng Roles với UserRoles để lấy ra những role mà user có
            var listRoles = from r in _context.Roles  
                join ur in _context.UserRoles on r.Id equals ur.RoleId  
                where ur.UserId == model.user.Id   //id của user bằng UserId trong bảng UserRoles
                select r;  

            //cái join này là nó join bảng RoleClaims với listRoles ở trên để lấy ra những claims mà user đó có
            var _claimsInRole  = from c in _context.RoleClaims
                                 join r in listRoles on c.RoleId  equals r.Id
                                 select c;

            model.claimsInRole = await _claimsInRole.ToListAsync(); //claims mà user có từ Role

            model.claimsInUserClaim  = await (from c in _context.UserClaims //lấy claims trực tiếp của user
            where c.UserId == model.user.Id select c).ToListAsync();
            

        }

        // GET: /ManageUser/AddRole/id     //Get VÀO TRANG ADD ROLE CHO USER 
        [HttpGet("{id}")]
        public async Task<IActionResult> AddRoleAsync(string id)
        {
            // public SelectList allRoles { get; set; }
            var model = new AddUserRoleModel();

            if (string.IsNullOrEmpty(id))
            {
                return NotFound($"Không có user");
            }

            model.user = await _userManager.FindByIdAsync(id); //tìm user có id binding đến

            if (model.user == null)
            {
                return NotFound($"Không thấy user, id = {id}.");
            }

            //lấy ra role của user đó rồi truyền vào model hiển thị ra view
            model.RoleNames = (await _userManager.GetRolesAsync(model.user)).ToArray<string>();


            //lấy ra hết name của role trong csdl, dùng để truyền cho ViewBag trong View
            List<string> roleNames = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
            ViewBag.allRoles = new SelectList(roleNames); //truyền ds trên vào ViewBag


            await GetClaims(model);

            return View(model);  //truyền model vào view 
        }

        // POST: /ManageUser/AddRole/id   //THỰC HIỆN ADD ROLE CHO USER
        [HttpPost("{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddRoleAsync(string id, [Bind("RoleNames")] AddUserRoleModel model)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound($"Không có user");
            }

            model.user = await _userManager.FindByIdAsync(id);

            if (model.user == null)
            {
                return NotFound($"Không thấy user, id = {id}.");
            }
            await GetClaims(model);

            //lấy ra những role name trước đó của user 
            var OldRoleNames = (await _userManager.GetRolesAsync(model.user)).ToArray();

          
            //nếu role name cũ ko nằm trong role name mới (model user input đến) thì role đó cần xóa
            var deleteRoles = OldRoleNames.Where(oldrole => !model.RoleNames.Contains(oldrole));

            //nếu role mới khác role cũ thì thêm vào 
            var addRoles = model.RoleNames.Where(newrole => !OldRoleNames.Contains(newrole));

            //phải nạp lại danh sách những cái role 
            List<string> roleNames = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
            ViewBag.allRoles = new SelectList(roleNames);            


            var resultDelete = await _userManager.RemoveFromRolesAsync(model.user,deleteRoles);
            if (!resultDelete.Succeeded)  //xóa những role cần phải xóa 
            {
                ModelState.AddModelError(resultDelete);
                return View(model);
            }
            
            //Bắt đầu thực hiện add role cho user
            var resultAdd = await _userManager.AddToRolesAsync(model.user,addRoles);
            if (!resultAdd.Succeeded)  //thêm role cần phải thêm
            {
                ModelState.AddModelError(resultAdd);
                return View(model);
            }

            
            StatusMessage = $"Vừa cập nhật role cho user: {model.user.UserName}";

            return RedirectToAction("Index");
        }


        [HttpGet("{id}")]    //Get VÀO TRANG ĐẶT LẠI MẬT KHẨU
        public async Task<IActionResult> SetPasswordAsync(string id) //id của user binding đến
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound($"Không có user");
            }

            var user = await _userManager.FindByIdAsync(id);
            ViewBag.user = ViewBag;

            if (user == null)
            {
                return NotFound($"Không thấy user, id = {id}.");
            }

            return View();
        }

        [HttpPost("{id}")]
        [ValidateAntiForgeryToken]  //Post THỰC HIỆN ĐẶT LẠI MẬT KHẨU CHO USER
        public async Task<IActionResult> SetPasswordAsync(string id, SetUserPasswordModel model)
        {
            if (string.IsNullOrEmpty(id))  
            {
                return NotFound($"Không có user");
            }

            var user = await _userManager.FindByIdAsync(id); //tìm user theo id có trong url

            ViewBag.user = ViewBag;

            if (user == null)
            {
                return NotFound($"Không thấy user, id = {id}.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }
             
            await _userManager.RemovePasswordAsync(user);  //phải xóa trước thì mới xài AddPasswordAsync dc

            var addPasswordResult = await _userManager.AddPasswordAsync(user, model.NewPassword);
            if (!addPasswordResult.Succeeded)
            {
                foreach (var error in addPasswordResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(model);
            }

            StatusMessage = $"Vừa cập nhật mật khẩu cho user: {user.UserName}";

            return RedirectToAction("Index");
        }        


        //PHẦN CLAIM CHO USER  (Claim TRỰC TIẾP TỪ BẢNG UserClaims)

        [HttpGet("{userid}")]
        public async Task<ActionResult> AddClaimAsync(string userid)
        {
            
            var user = await _userManager.FindByIdAsync(userid);
            if (user == null) return NotFound("Không tìm thấy user");
            ViewBag.user = user;
            return View();
        }

        
        [HttpPost("{userid}")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddClaimAsync(string userid, AddUserClaimModel model)
        {
            var user = await _userManager.FindByIdAsync(userid);
            if (user == null) return NotFound("Không tìm thấy user");

            ViewBag.user = user;

            if (!ModelState.IsValid) return View(model);

            var claims = _context.UserClaims.Where(c => c.UserId == user.Id); //lấy ra claim hiện có của user

            //kiểm tra xem claim submit đến có trùng vs claim hiện có của user hay chưa 
            if (claims.Any(c => c.ClaimType == model.ClaimType && c.ClaimValue == model.ClaimValue))
            {
                ModelState.AddModelError(string.Empty, "Đặc tính này đã có");
                return View(model);
            }
            
            //thêm 1 claim cho user 
            await _userManager.AddClaimAsync(user, new Claim(model.ClaimType, model.ClaimValue));
            StatusMessage = "Đã thêm đặc tính cho user";
                        
            return RedirectToAction("AddRole", new {id = user.Id});
        }        

        [HttpGet("{claimid}")]       
        public async Task<IActionResult> EditClaim(int claimid)  //clamId binding đến
        {
            //tìm trong bảng UserClaims có id = claimid binding đến 
            var userclaim = _context.UserClaims.Where(c => c.Id == claimid).FirstOrDefault();

            //UserId của bảng UserClaim = Id của user, để tìm user sở hữu claim đó
            var user = await _userManager.FindByIdAsync(userclaim.UserId);  

            if (user == null) return NotFound("Không tìm thấy user");

            var model = new AddUserClaimModel()   //mục đích là để hiện thị ra tên claim cũ và giá trị cũ để sửa 
            {
                ClaimType = userclaim.ClaimType,  //input sẽ = tên claim cũ
                ClaimValue = userclaim.ClaimValue  //input = value cũ 

            };
            ViewBag.user = user;
            ViewBag.userclaim = userclaim;
            return View("AddClaim", model);
        }


        [HttpPost("{claimid}")]
        [ValidateAntiForgeryToken]     //tiến hành edit claim 
        public async Task<IActionResult> EditClaim(int claimid, AddUserClaimModel model)
        {
            var userclaim = _context.UserClaims.Where(c => c.Id == claimid).FirstOrDefault();
            var user = await _userManager.FindByIdAsync(userclaim.UserId);
            if (user == null) return NotFound("Không tìm thấy user");

            if (!ModelState.IsValid) return View("AddClaim", model);

            //kiểm tra xem có claim nào bị trùng hay ko
            if (_context.UserClaims.Any(c => c.UserId == user.Id 
                && c.ClaimType == model.ClaimType 
                && c.ClaimValue == model.ClaimValue 
                && c.Id != userclaim.Id))  //chỉ kiểm tra claim khác với claim id đang thi hành
                {
                    ModelState.AddModelError("Claim này đã có");
                    return View("AddClaim", model);
                }


            userclaim.ClaimType = model.ClaimType;
            userclaim.ClaimValue = model.ClaimValue;

            await _context.SaveChangesAsync();
            StatusMessage = "Bạn vừa cập nhật claim";
            
            ViewBag.user = user;
            ViewBag.userclaim = userclaim;
            return RedirectToAction("AddRole", new {id = user.Id});
        }



        [HttpPost("{claimid}")]
        [ValidateAntiForgeryToken]    //TIẾN HÀNH XÓA CLAIM
        public async Task<IActionResult> DeleteClaimAsync(int claimid)
        {
            var userclaim = _context.UserClaims.Where(c => c.Id == claimid).FirstOrDefault();
            var user = await _userManager.FindByIdAsync(userclaim.UserId);

            if (user == null) return NotFound("Không tìm thấy user");

            await _userManager.RemoveClaimAsync(user, new Claim(userclaim.ClaimType, userclaim.ClaimValue));

            StatusMessage = "Bạn đã xóa claim";
            
            return RedirectToAction("AddRole", new {id = user.Id});
        }

        
  }
}
