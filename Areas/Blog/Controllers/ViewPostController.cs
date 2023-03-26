using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Threading.Tasks;
using App.Models;
using HocAspMVC4.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Test123.Models;

namespace HocAspMVC4_Test.Areas.Blog.Controllers
{
    [Area("Blog")]
    public class ViewPostController : Controller
    {
        private readonly AppDbContext1 _context;
        private readonly ILogger<ViewPostController> _logger;

        public ViewPostController(AppDbContext1 context, ILogger<ViewPostController> logger)
        {
            _context = context;
            _logger = logger;
        }


        // /post
        // post/{categorySlug}
        [Route("/post/{categoryslug?}")]
        public IActionResult Index(string categoryslug, [FromQuery(Name ="p")]int currentPage, int pagesize)
        {
            var categories = GetCategories();
            ViewBag.categories = categories;
            ViewBag.categoryslug = categoryslug;

            Category category = null;

            if (!string.IsNullOrEmpty(categoryslug)) //nếu slug ko null
            {
                category = _context.Categories.Where(c => c.Slug == categoryslug)
                                               .Include(c => c.CategoryChildren)
                                               .FirstOrDefault();
                if (category == null)
                {
                    return NotFound("Không tìm thấy danh mục này");
                }
            }

            var posts = _context.Posts
                .Include(p => p.Author)
                .Include(p => p.Photos)
                .Include(p => p.PostCategories)
                .ThenInclude(p => p.Category)
                .OrderByDescending(p => p.DateUpdated)
                .AsQueryable();

            //posts.OrderByDescending(p => p.DateUpdated);


            if (category != null)
            {
                var ids = new List<int>();
                category.ChildCategoryIDs(null, ids); //chứa ds các id con
                ids.Add(category.Id);

                posts = posts.Where(p => p.PostCategories.Where(pc => ids.Contains(pc.CategoryID)).Any());

            }

            //phân trang
            int totalPost = posts.Count();
            if (pagesize <= 0)
            {
                pagesize = 6;
            }
            int countPages = (int)Math.Ceiling((double)totalPost / pagesize);


            if (currentPage > countPages)
                currentPage = countPages; //đưa về index cuối cùng 
            if (currentPage < 1)  //nếu index của paging < 1 ==> đưa về trang 1
                currentPage = 1;


            var pagingModel = new PagingModel()
            {
                countpages = countPages,
                currentpage = currentPage,
                generateUrl = (pageNumber) => Url.Action("Index", new
                {
                    p = pageNumber,
                    pagesize = pagesize
                })
            };

            //lấy ra số lượng bài post trong 1 trang
            var postsInPage = posts.Skip((currentPage - 1) * pagesize)
                       .Take(pagesize);

            ViewBag.pagingModel = pagingModel;
            ViewBag.totalPosts = totalPost;

            ViewBag.category = category;
            return View(postsInPage.ToList());
        }

        [Route("/post/{postslug}.html")]
        public IActionResult Details(string postslug)
        {
            var categories = GetCategories();
            ViewBag.categories = categories;

            var postDetail = _context.Posts.Where(p => p.Slug == postslug)
                                        .Include(p => p.Author)
                                        .Include(p => p.Photos)
                                        .Include(p => p.PostCategories)
                                        .ThenInclude(pc => pc.Category)
                                        .FirstOrDefault();

            if (postDetail == null)
            {
                return NotFound();
            }

            Category category = postDetail.PostCategories.FirstOrDefault()?.Category;
            ViewBag.category = category;

            //lấy ra những bài post có cùng danh mục với cả bài post hiện tại
            var otherPosts = _context.Posts.Where(p => p.PostCategories.Any(c => c.CategoryID == category.Id))
                                            .Where(p => p.PostId != postDetail.PostId)
                                            .OrderByDescending(p => p.DateUpdated)
                                            .Take(5);
            ViewBag.otherPosts = otherPosts;

            //Bài viết mới
            var postmoi = _context.Posts
              .Include(p => p.Author)
              .Include(p => p.PostCategories)
              .ThenInclude(p => p.Category)
              .AsQueryable();
            postmoi = postmoi.OrderByDescending(p => p.DateUpdated).Take(3);
            ViewBag.postmoi = postmoi;


            //lấy views
            

            return View(postDetail);
        }


        //method lấy ra tất cả danh mục
        private List<Category> GetCategories()
        {
            var categories = _context.Categories
                .Include(c => c.CategoryChildren) //đã lấy luôn những dm con
                .AsEnumerable()
                .Where(c => c.ParentCategory == null).ToList(); //lấy những dm cha
            return categories;
        }


    }
}