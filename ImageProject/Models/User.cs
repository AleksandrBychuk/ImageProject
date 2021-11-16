using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ImageProject.Models
{
    public class User : IdentityUser
    {
        public List<UserImage> UserImages { get; set; } = new();
        public DateTime RegisterDate { get; set; }
    }
}
