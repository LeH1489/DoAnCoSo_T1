using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HocAspMVC4_Test.Models.Blog
{
    [Table("PostPhoto")]
    public class PostPhoto
    {
        [Key]
        public int Id { set; get; }


        //truy cập theo path: /contents/Products/abc.jpeg
        public string FileName { get; set; }

        //tham chiếu
        public int? PostID { get; set; }

        [ForeignKey("PostID")]
        public Post? Post { get; set; }

    }
}

