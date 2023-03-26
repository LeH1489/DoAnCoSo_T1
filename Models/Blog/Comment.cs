using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using HocAspMVC4.Models;

namespace HocAspMVC4_Test.Models.Blog
{
    public class Comment
    {
        [Key]
        public int CommentId { set; get; }

        [Required(ErrorMessage = "Phải nhập nội dung comment")]
        public string Content { set; get; }

        public DateTime DateCreated { set; get; }

        public string? UserId { set; get; }

        [ForeignKey("UserId")]
        public AppUser? User { set; get; }
    }
}

