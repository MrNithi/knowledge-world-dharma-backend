using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using knowledge_world_dharma_backend.Models;

namespace knowledge_world_dharma_backend.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<knowledge_world_dharma_backend.Models.Post> Post { get; set; }
        public DbSet<knowledge_world_dharma_backend.Models.Like> Like { get; set; }
        public DbSet<knowledge_world_dharma_backend.Models.UserModel> UserModel { get; set; }
        public DbSet<knowledge_world_dharma_backend.Models.Annoucement> Annoucement { get; set; }
    }
}
