using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Test123.Models
{
    [Table("Category")]
    public class Category
    {
        [Key]
        public int Id { get; set; }

        // Tiêu đề Category
        [StringLength(100, MinimumLength = 3, ErrorMessage = "{0} dài {1} đến {2}")]
        [Display(Name = "Tên danh mục")]
        public string Title { get; set; }

        // Nội dung, thông tin chi tiết về Category
        [DataType(DataType.Text)]
        [Display(Name = "Nội dung danh mục")]
        public string Description { set; get; }

        //chuỗi Url
        [StringLength(100, MinimumLength = 3, ErrorMessage = "{0} dài {1} đến {2}")]
        [RegularExpression(@"^[a-z0-9-]*$", ErrorMessage = "Chỉ dùng các ký tự [a-z0-9-]")]
        [Display(Name = "Url hiện thị")]
        public string Slug { set; get; }

        // Các danh mục con
        public ICollection<Category>? CategoryChildren { get; set; }

        // Category cha (FKey)
        [Display(Name = "Danh mục cha")]
        public int? ParentCategoryId { get; set; }

        [ForeignKey("ParentCategoryId")]  //khóa ngoại
        [Display(Name = "Danh mục cha")]
        public Category? ParentCategory { set; get; }

        //mỗi category có thể là con của 1 category khác (xây dựng theo dạng cây)
        //1 category có nhiều category con
        //tạo ra khóa ngoại để xác định cha của nó ParentCategoryId


        public void ChildCategoryIDs(ICollection<Category> childcates, List<int> lists)
        {
            if (childcates == null)
            {
                childcates = this.CategoryChildren;
            }
            foreach (Category category in childcates)
            {
                lists.Add(category.Id);
                ChildCategoryIDs(category.CategoryChildren, lists);
            }
        }

        //breadcrumb
        public List<Category> ListParents()
        {
            List<Category> li = new List<Category>();
            var parent = this.ParentCategory;
            while (parent != null)
            {
                li.Add(parent);
                parent = parent.ParentCategory;
            }
            li.Reverse();
            return li;
        }

    }
}

