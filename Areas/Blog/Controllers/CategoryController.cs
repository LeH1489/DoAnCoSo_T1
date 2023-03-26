using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HocAspMVC4.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Test123.Models;

namespace Test123.Areas.Blog.Controllers
{
    [Area("Blog")]
    [Route("admin/blog/category/[action]/{id?}")]
    [Authorize(Roles ="Admin")]
    public class CategoryController : Controller
    {
        private readonly AppDbContext1 _context;

        public CategoryController(AppDbContext1 context)
        {
            _context = context;
        }


        //Hoang
        // GET: Blog/Category
        public async Task<IActionResult> Index()
        {
            var qr = (from c in _context.Categories select c)
                        .Include(c => c.ParentCategory)   //lấy ra tham chiếu ParentCategory (danh mục cha của nó cũng dc lấy ra theo)
                        .Include(c => c.CategoryChildren); //lấy ra cả những danh mục con 

            //chỉ lấy ra các cate ko có danh mục cha
            var categoriesNoParent = (await qr.ToListAsync())
                            .Where(c => c.ParentCategory == null)  
                            .ToList();

            //nhưng thực tế cái categories còn chứa các category con nữa
            return View(categoriesNoParent);
        }


        // GET: Blog/Category/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Categories == null)
            {
                return NotFound();
            }

            var categoryDetail = await _context.Categories
                .Include(c => c.ParentCategory) 
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (categoryDetail == null)
            {
                return NotFound();
            }

            return View(categoryDetail);
        }

        // GET: Blog/Category/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Categories == null)
            {
                return NotFound();
            }

            var categoryDelete = await _context.Categories
                .Include(c => c.ParentCategory)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (categoryDelete == null)
            {
                return NotFound();
            }

            return View(categoryDelete);
        }

        // POST: Blog/Category/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            //lấy ra danh mục cần xóa
            var categoryDelete = _context.Categories
                .Include(c => c.CategoryChildren)
                .FirstOrDefault(c => c.Id == id);


            if (categoryDelete == null)
            {
                return NotFound();
            }

            //duyệt qua các dm con của dm cần xóa và gán danh mục cha của các dm con đó = dm cha của danh mục cần xóa
            foreach (var cCategory in categoryDelete.CategoryChildren)
            {
                cCategory.ParentCategoryId = categoryDelete.ParentCategoryId;
            }

            _context.Categories.Remove(categoryDelete);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        //Hai
        //source là danh mục nguồn => xử lý => và đưa vào danh mục đích: des
        private void CreateSelectItems(List<Category> source, List<Category> des, int level)
        {
            string prefix = string.Concat(Enumerable.Repeat("----", level));
            foreach (var category in source) //duyệt qua từng p/tử ở nguồn
            {
                //category.Title = prefix + category.Title;

                des.Add(new Category()
                {
                    Id = category.Id,
                    Title = prefix + " " + category.Title
                }); //thêm vào danh sách đích

                if(category.CategoryChildren?.Count > 0) //nếu có danh mục đó có các danh mục con
                {
                    CreateSelectItems(category.CategoryChildren.ToList(), des, level+1); //gọi đệ quy
                }
            }
        }


        // GET: Blog/Category/Create
        public async Task<IActionResult> CreateAsync()
        {
            var qr = (from c in _context.Categories select c)
                        .Include(c => c.ParentCategory)   //lấy ra tham chiếu parentCategory (danh mục cha của nó cũng dc lấy ra theo)
                        .Include(c => c.CategoryChildren); //lấy ra các danh mục con 

            var categoriesNoParent = (await qr.ToListAsync())
                            .Where(c => c.ParentCategory == null)  //chỉ lấy ra các cate ko có danh mục cha
                            .ToList();
            //lấy ra các danh mục ko có dm cha (gốc)

            //0 == vị trí ở đầu 
            categoriesNoParent.Insert(0, new Category() //trong danh sách các dm gốc lấy dc ta chèn thêm vào 1 Category có id = -1 và title
            {
                Id = -1,
                Title = "Không có danh mục cha"
            });

            //dùng để đưa qua select list để chọn
            var items = new List<Category>(); //tạo 1 ds mới
            CreateSelectItems(categoriesNoParent, items, 0); 
            var selectList = new SelectList(items, "Id", "Title");


            ViewData["ParentCategoryId"] = selectList; //đổ sang view
            return View();
        }

        // POST: Blog/Category/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Description,Slug,ParentCategoryId")] Category categoryCreate)
        {
            if (ModelState.IsValid) 
            {
                //nếu user chọn ko có danh mục cha tức là ParentCategoryId = -1 thì gán danh mục cha của nó bằng null
                //để biến danh mục đó là danh mục gốc
                if (categoryCreate.ParentCategoryId == -1) categoryCreate.ParentCategoryId = null;
                
                _context.Add(categoryCreate);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            var qr = (from c in _context.Categories select c)
                      .Include(c => c.ParentCategory)   //lấy ra tham chiếu parentCategory (danh mục cha của nó cũng dc lấy ra theo)
                      .Include(c => c.CategoryChildren); //lấy ra các danh mục con 

            var categories = (await qr.ToListAsync())
                            .Where(c => c.ParentCategory == null)  //chỉ lấy ra các cate ko có danh mục cha
                            .ToList();

            categories.Insert(0, new Category()
            {
                Id = -1,
                Title = "Không có danh mục cha"
            });

            var items = new List<Category>();
            CreateSelectItems(categories, items, 0);
            var selectList = new SelectList(items, "Id", "Title");

            ViewData["ParentCategoryId"] = selectList;
            return View(categoryCreate);
        }

        // GET: Blog/Category/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Categories == null)
            {
                return NotFound();
            }

            var categoryEdit = await _context.Categories.FindAsync(id);

            if (categoryEdit == null)
            {
                return NotFound();
            }

            var qr = (from c in _context.Categories select c)
                    .Include(c => c.ParentCategory)   //lấy ra tham chiếu parentCategory (danh mục cha của nó cũng dc lấy ra theo)
                    .Include(c => c.CategoryChildren); //lấy ra các danh mục con 

            var categories = (await qr.ToListAsync())
                            .Where(c => c.ParentCategory == null)  //chỉ lấy ra các cate ko có danh mục cha
                            .ToList();

            categories.Insert(0, new Category()
            {
                Id = -1,
                Title = "Không có danh mục cha"
            });

            var items = new List<Category>();
            CreateSelectItems(categories, items, 0);
            var selectList = new SelectList(items, "Id", "Title");

            ViewData["ParentCategoryId"] = selectList;
            return View(categoryEdit);
        }

        // POST: Blog/Category/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,Slug,ParentCategoryId")] Category category)
        {
            if (id != category.Id)
            {
                return NotFound();
            }

            bool canUpdate = true;

            if (category.ParentCategoryId == category.Id) //nếu user chọn chính nó làm dm cha của nó
            {
                ModelState.AddModelError(string.Empty, "Phải chọn danh mục cha khác");
                canUpdate = false;
            }

            //lấy toàn bộ danh mục con của danh mục này để kiểm tra xem là có thiết lập cha của dm này có thuộc danh mục con nào ko
            if (canUpdate && category.ParentCategoryId != null)
            {
                var childCates = (from c in _context.Categories select c) //lấy ra toàn bộ dm trong csdl
                    .Include(c => c.CategoryChildren)
                    .ToList()
                    .Where(c => c.ParentCategoryId == category.Id);
                //so sánh ParentCategoryId của những dm đó với id cua dm đang edit này
                //để lấy ra toàn bộ dm con của danh mục đang edit này


                //khai báo func check id
                //List<Category là param nhận vào, bool là kiểu trả về
                Func<List<Category>, bool> checkCateIds = null;
                checkCateIds = (cates) =>
                {
                    foreach (var cate in cates)
                    {
                        Console.WriteLine(cate.Title);
                        if (cate.Id == category.ParentCategoryId)
                        {
                            canUpdate = false;
                            ModelState.AddModelError(string.Empty, "Phải chọn danh mục cha khác");
                            return true;
                        }
                        if (cate.CategoryChildren != null)
                        {
                            return checkCateIds(cate.CategoryChildren.ToList());
                        }
                    }
                    return false;
                };
                //End func
                //ta bỏ cái list childCates ở trên vào hàm func này để nó kiểm tra
                //nếu dm cha của dm đang edit bằng id của dm con của nó thì ko cho update
                checkCateIds(childCates.ToList());  
            }


            if (ModelState.IsValid && canUpdate)
            {
                try
                {
                    if (category.ParentCategoryId == -1)
                    {
                        category.ParentCategoryId = null;
                    }

                    var dtc = _context.Categories.FirstOrDefault(c => c.Id == id);
                    _context.Entry(dtc).State = EntityState.Detached;


                    _context.Update(category);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(category.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            var qr = (from c in _context.Categories select c)
                    .Include(c => c.ParentCategory)   //lấy ra tham chiếu parentCategory (danh mục cha của nó cũng dc lấy ra theo)
                    .Include(c => c.CategoryChildren); //lấy ra các danh mục con 

            var categories = (await qr.ToListAsync())
                            .Where(c => c.ParentCategory == null)  //chỉ lấy ra các cate ko có danh mục cha
                            .ToList();

            categories.Insert(0, new Category()
            {
                Id = -1,
                Title = "Không có danh mục cha"
            });

            var items = new List<Category>();
            CreateSelectItems(categories, items, 0);
            var selectList = new SelectList(items, "Id", "Title");

            ViewData["ParentCategoryId"] = selectList;
            return View(category);
        }

        
        private bool CategoryExists(int id)
        {
          return (_context.Categories?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
