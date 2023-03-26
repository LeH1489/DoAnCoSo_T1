// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using App.Areas.Identity.Models.ManageViewModels;
using App.Areas.Identity.Models.RoleViewModels;
using App.Data;
using App.ExtendMethods;
using App.Models;
using App.Services;
using HocAspMVC4.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace App.Areas.Identity.Controllers
{

    [Authorize(Policy ="AllowEditRole")]
    [Area("Identity")]
    [Route("/Role/[action]")]
    public class RoleController : Controller
    {
        
        private readonly ILogger<RoleController> _logger;
        private readonly RoleManager<IdentityRole> _roleManager;  //dùng để quản lý role của user (service của Identity)
        private readonly AppDbContext1 _context;

        private readonly UserManager<AppUser> _userManager;

        //
        public RoleController(ILogger<RoleController> logger, RoleManager<IdentityRole> roleManager, AppDbContext1 context, UserManager<AppUser> userManager)
        {
            _logger = logger;
            _roleManager = roleManager;
            _context = context;
            _userManager = userManager;
        }

        [TempData]
        public string StatusMessage { get; set; }

        [Route("/AccessDenied1")]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }

        //
        // GET: /Role/Index
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            
           var r = await _roleManager.Roles.OrderBy(r => r.Name).ToListAsync(); //lấy ra tất cả role name 
           var roles = new List<RoleModel>(); //tạo ra danh sách các role model

           foreach (var _r in r)   //mỗi role sẽ có lấy ra các claims của nó 
           {
               var claims = await _roleManager.GetClaimsAsync(_r); //lấy ra tất cả claim của role đó 
               var claimsString = claims.Select(c => c.Type  + "=" + c.Value); //type: tên claim
                //2 dòng code trên để phục vụ cho thg Claims ở dưới
            
               var rm = new RoleModel()  //với mỗi _r (role name) trong r (ds các role) sẽ tạo ra p/tử kiểu RoleModel
               {
                   Name = _r.Name,  //hiển thị ra tên của role đó
                   Id = _r.Id,
                   Claims = claimsString.ToArray() //những cái claim mà role đó có (nếu có trả về tên claim và value)
               };
               roles.Add(rm); //thêm vào vào list 
           }

            return View(roles); //hiển thị ra view với cái list đó
        } 

        // GET: /Role/Create   //Get VÀO TRANG TẠO MỘT ROLE MỚI 
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }
        
        // POST: /Role/Create
        [HttpPost, ActionName(nameof(Create))]     //Post THỰC HIỆN TẠO 1 ROLE MỚI
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAsync(CreateRoleModel model)
        {
            if  (!ModelState.IsValid)  //kiểm tra input đến 
            {
                return View();
            }
            //IdentityRole là table Roles trong csdl của Identity
                                            //tên role mà user input vào
            var newRole = new IdentityRole(model.Name); //tạo 1 đối tg IdentityRole để truyền vào Create

            //dùng RoleManager thêm cái role đó vào trong csdl 
            var result = await _roleManager.CreateAsync(newRole); //tạo 1 role mới

            if (result.Succeeded)
            {
                StatusMessage = $"Bạn vừa tạo role mới: {model.Name}";
                return RedirectToAction(nameof(Index));   //quay về trang Index quản lý các role
            }
            else
            {
                ModelState.AddModelError(result); //nếu có lỗi 
            }

            //nếu lỗi thì lại trả về view của action này 
            return View(model); 
        }


        // GET: /Role/Delete/roleid   //Get VÀO TRANG XÁC NHẬN XÓA ROLE 
        [HttpGet("{roleid}")]  
        public async Task<IActionResult> DeleteAsync(string roleid) //tìm RoleId binding đến
        {
            if (roleid == null) return NotFound("Không tìm thấy role");

            var role = await _roleManager.FindByIdAsync(roleid);  //tìm role theo id binding đến

            if (role == null)
            {
                return NotFound("Không tìm thấy role");
            }

            return View(role);  //trả về view (1 cái page hỏi có xóa hay ko)
        }
        
        // POST: /Role/Edit/1    //Post TIẾN HÀNH XÓA ROLE 
        [HttpPost("{roleid}"), ActionName("Delete")]  
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmAsync(string roleid)
        {
            if (roleid == null) return NotFound("Không tìm thấy role");

            var role = await _roleManager.FindByIdAsync(roleid); //tìm role theo id binding đến

            if  (role == null) return NotFound("Không tìm thấy role");
             
            var result = await _roleManager.DeleteAsync(role);  //xóa role 

            if (result.Succeeded)
            {
                StatusMessage = $"Bạn vừa xóa: {role.Name}"; 
                return RedirectToAction(nameof(Index)); 
            }
            else
            {
                ModelState.AddModelError(result);  //nếu xóa ko thành công
            }

            //nếu lỗi thì lại trả về view của action này 
            return View(role);
        }     


        //PHẦN VỀ CLAIM        
        // GET: /Role/Edit/roleid    //Get VÀO TRANG EDIT ROLE 
        [HttpGet("{roleid}")]  
        public async Task<IActionResult> EditAsync(string roleid, [Bind("Name")]EditRoleModel model)
        {
            if (roleid == null) return NotFound("Không tìm thấy role");

            var role = await _roleManager.FindByIdAsync(roleid);  //lấy ra role theo id binding đến

            if (role == null)
            {
                return NotFound("Không tìm thấy role");
            } 
            model.Name = role.Name;   //hiện lại tên role cũ 

            //Lấy các claims trực tiếp từ bảng RoleClaims, chỉ lấy RoleClaims thuộc về role như ở trên
            model.Claims = await _context.RoleClaims.Where(rc => rc.RoleId == role.Id).ToListAsync();
            model.role = role; 
            ModelState.Clear();
            return View(model);

        }
        
        // POST: /Role/Edit/1   //đổi tên role
        [HttpPost("{roleid}"), ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditConfirmAsync(string roleid, [Bind("Name")]EditRoleModel model)
        {
            var role = await _roleManager.FindByIdAsync(roleid); //lấy ra role theo id binding đến 

            if (roleid == null) return NotFound("Không tìm thấy role");

          
            if (role == null)
            {
                return NotFound("Không tìm thấy role");
            }

            model.Claims = await _context.RoleClaims.Where(rc => rc.RoleId == role.Id).ToListAsync();

            model.role = role; 

            if (!ModelState.IsValid) //kiểm tra dữ liệu submit đến 
            {
                return View(model);
            }
    
            role.Name = model.Name;  //tên role mới bằng dữ liệu user input đến
            var result = await _roleManager.UpdateAsync(role);

            if (result.Succeeded)
            {
                StatusMessage = $"Bạn vừa đổi tên: {model.Name}";
                return RedirectToAction(nameof(Index));
            }
            else
            {
                ModelState.AddModelError(result); 
            }

            return View(model);
        }

        // GET: /Role/AddRoleClaim/roleid      //THÊM CLAIM CHO 1 ROLE XÁC ĐỊNH BẰNG id binding đến
        [HttpGet("{roleid}")]        
        public async Task<IActionResult> AddRoleClaimAsync(string roleid)
        {
            var role = await _roleManager.FindByIdAsync(roleid);
            if (roleid == null) return NotFound("Không tìm thấy role");
            if (role == null)
            {
                return NotFound("Không tìm thấy role");
            } 

            var model = new EditClaimModel()
            {
                role = role
            };
            return View(model);
        }             

        // POST: /Role/AddRoleClaim/roleid    //THỰC HIỆN ADD 1 CLAIM
        [HttpPost("{roleid}")]  
        [ValidateAntiForgeryToken]      
        public async Task<IActionResult> AddRoleClaimAsync(string roleid, [Bind("ClaimType", "ClaimValue")]EditClaimModel model)
        {
            var role = await _roleManager.FindByIdAsync(roleid);
            if (roleid == null) return NotFound("Không tìm thấy role");
           

            if (role == null)
            {
                return NotFound("Không tìm thấy role");
            }

            model.role = role;
            if (!ModelState.IsValid) return View(model);



            //lấy tất cả các claim kiểm tra xem claim submit đến đã có trong role đó chưa
            //có trả về true
            if ((await _roleManager.GetClaimsAsync(role)).Any(c => c.Type == model.ClaimType && c.Value == model.ClaimValue))
            {
                ModelState.AddModelError(string.Empty, "Claim này đã có trong role");
                return View(model);
            }

            //nếu claim đó chưa có trong role thì thêm vào:
            var newClaim = new Claim(model.ClaimType, model.ClaimValue);

            var result = await _roleManager.AddClaimAsync(role, newClaim);  //thêm claim cho role

            if (!result.Succeeded)
            {
                ModelState.AddModelError(result);
                return View(model);
            }

            StatusMessage = "Vừa thêm đặc tính (claim) mới";
            
            return RedirectToAction("Edit", new {roleid = role.Id});

        }           

        // GET: /Role/EditRoleClaim/claimid
        [HttpGet("{claimid:int}")]        
        public async Task<IActionResult> EditRoleClaim(int claimid) //trong csdl claimid là kiểu int
        {
            var claim = _context.RoleClaims.Where(c => c.Id == claimid).FirstOrDefault();
            if (claim == null) return NotFound("Không tìm thấy role");

            var role = await _roleManager.FindByIdAsync(claim.RoleId); //lấy role có id = claim.RoleId
            if (role == null) return NotFound("Không tìm thấy role");
            ViewBag.claimid = claimid;

            var Input = new EditClaimModel()
            {
                ClaimType = claim.ClaimType,
                ClaimValue = claim.ClaimValue,
                role = role
            };

            return View(Input);
        }             

        // POST: /Role/EditRoleClaim/claimid
        [HttpPost("{claimid:int}")]        
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditRoleClaim(int claimid, [Bind("ClaimType", "ClaimValue")]EditClaimModel Input)
        {
            var claim = _context.RoleClaims.Where(c => c.Id == claimid).FirstOrDefault();
            if (claim == null) return NotFound("Không tìm thấy role");

            ViewBag.claimid = claimid;

            var role = await _roleManager.FindByIdAsync(claim.RoleId);
            if (role == null) return NotFound("Không tìm thấy role");
            Input.role = role;
            if  (!ModelState.IsValid)
            {
                return View(Input);
            }

            //kiểm tra p/tử roleclaims xem có p/tử nào bị trùng 
            if (_context.RoleClaims.Any(c => c.RoleId == role.Id && c.ClaimType == Input.ClaimType && c.ClaimValue == Input.ClaimValue && c.Id != claim.Id))
            {
                ModelState.AddModelError(string.Empty, "Claim này đã có trong role");
                return View(Input);
            }

            //nếu chưa có thì cho cập nhật 
            claim.ClaimType = Input.ClaimType;
            claim.ClaimValue = Input.ClaimValue;
            
            await _context.SaveChangesAsync();
            
            StatusMessage = "Vừa cập nhật claim";
            
            return RedirectToAction("Edit", new {roleid = role.Id});
        }        
        // POST: /Role/EditRoleClaim/claimid     //THỰC HIỆN XÓA CLAIMS 
        [HttpPost("{claimid:int}")]        
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteClaim(int claimid, [Bind("ClaimType", "ClaimValue")]EditClaimModel Input)
        {
            var claim = _context.RoleClaims.Where(c => c.Id == claimid).FirstOrDefault();
            if (claim == null) return NotFound("Không tìm thấy role");

            var role = await _roleManager.FindByIdAsync(claim.RoleId);
            if (role == null) return NotFound("Không tìm thấy role");

            Input.role = role;
            if  (!ModelState.IsValid)
            {
                return View(Input);
            }
            
            if (_context.RoleClaims.Any(c => c.RoleId == role.Id && c.ClaimType == Input.ClaimType && c.ClaimValue == Input.ClaimValue && c.Id != claim.Id))
            {
                ModelState.AddModelError(string.Empty, "Claim này đã có trong role");
                return View(Input);
            }
 

            await _roleManager.RemoveClaimAsync(role, new Claim(claim.ClaimType, claim.ClaimValue));
            
            StatusMessage = "Vừa xóa claim";

            
            return RedirectToAction("Edit", new {roleid = role.Id});
        }        


    }
}
