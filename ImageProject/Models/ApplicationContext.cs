﻿using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ImageProject.Models
{
    public class ApplicationContext : IdentityDbContext
    {
        public override DbSet<IdentityUser> Users { get; set; }
        public DbSet<UserImage> UserImages { get; set; }
        public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options) { }

    }
}
