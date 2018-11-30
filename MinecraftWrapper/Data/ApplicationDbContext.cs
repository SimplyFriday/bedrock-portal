using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MinecraftWrapper.Models;

namespace MinecraftWrapper.Data
{
    public class ApplicationDbContext : IdentityDbContext<AuthorizedUser>
    {
        public ApplicationDbContext ( DbContextOptions<ApplicationDbContext> options )
            : base ( options )
        {
        }

        public DbSet<AuthorizationKey> AuthorizationKey { get; set; }
        public DbSet<AdditionalUserData> AdditionalUserData { get; set; }
        public DbSet<ApplicationLog> ApplicationLog { get; set; }
    }
}
