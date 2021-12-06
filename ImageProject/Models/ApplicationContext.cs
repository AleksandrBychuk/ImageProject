using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ImageProject.Models
{
    public class ApplicationContext : IdentityDbContext<User>
    {
        public DbSet<UserImage> UserImages { get; set; }
        public DbSet<СonstituentСolor> СonstituentСolors { get; set; }
        public DbSet<ImageCoord> ImageCoords { get; set; }
        public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options) { }
    }
}
