using System;
using System.ComponentModel.DataAnnotations;

namespace HocAspMVC4_Test.Models.Blog
{
	public class CreatePostModel : Post
	{
        //cho thêm 1 mảng số nguyên chứa các id của các danh mục, để cho biết bài post này thuộc category nào
        [Display(Name = "Chuyên mục")]
        public int[]? CategoryIDs { get; set; }

    }
}

