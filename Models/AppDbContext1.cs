using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Test123.Models;
using HocAspMVC4_Test.Models.Blog;


namespace HocAspMVC4.Models
{
    public class AppDbContext1 : IdentityDbContext<AppUser>
    {
        public AppDbContext1(DbContextOptions options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder builder)
        {
            base.OnConfiguring(builder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                var tableName = entityType.GetTableName();
                if (tableName.StartsWith("AspNet"))
                {
                    entityType.SetTableName(tableName.Substring(6));
                }
            }

            //phần blog
            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasIndex(c => c.Slug).IsUnique(); //đánh chỉ mục thuộc tính Slug là duy nhất trong category
            });

            modelBuilder.Entity<PostCategory>(entity =>
            {
                entity.HasKey(c => new { c.PostID, c.CategoryID}); //2 prop này sẽ là khóa chính của table PostCategory
            });

            modelBuilder.Entity<Post>(entity =>
            {
                entity.HasIndex(p => p.Slug).IsUnique();  //đánh chỉ mục thuộc tính Slug là duy nhất trong Post
            });

        }


        
        public DbSet<Category> Categories { set; get; }

        public DbSet<Post> Posts { set; get; }

        public DbSet<Comment> Comments { set; get; }

        public DbSet<PostCategory> postCategories { set; get; }

        public DbSet<PostPhoto> PostPhotos { set; get; }

    }

}

