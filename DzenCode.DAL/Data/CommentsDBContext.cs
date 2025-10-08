using DzenCode.Common.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DzenCode.DAL.Data
{
    public class CommentsDBContext : DbContext
    {
        public CommentsDBContext(DbContextOptions<CommentsDBContext> options) : base(options)
        {
        }

        public DbSet<Comment> Comments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Comment>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.UserName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.HomePage)
                    .HasMaxLength(500);

                entity.Property(e => e.Text)
                    .IsRequired();

                entity.Property(e => e.ImagePath)
                    .HasMaxLength(500);

                entity.Property(e => e.TextFilePath)
                    .HasMaxLength(500);

                entity.HasOne(e => e.Parent)
                    .WithMany(e => e.Replies)
                    .HasForeignKey(e => e.ParentId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => e.CreatedAt);
                entity.HasIndex(e => e.UserName);
                entity.HasIndex(e => e.Email);
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
