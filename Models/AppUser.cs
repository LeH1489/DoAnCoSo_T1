
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace HocAspMVC4.Models
{

    public class AppUser : IdentityUser
    {
        [Column(TypeName="nvarchar")]
        [StringLength(400)]
        public string? HomeAdress { set; get; }
    

        [DataType(DataType.Date)]
        public DateTime? Birthday { set; get; }

    }
}
