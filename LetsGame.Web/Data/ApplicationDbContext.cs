using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace LetsGame.Web.Data
{
    public class ApplicationDbContext : IdentityDbContext<AppUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Group> Groups { get; set; }
        public DbSet<GroupGame> GroupGames { get; set; }
        public DbSet<Membership> Memberships { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            
            void Entity<T>(Action<EntityTypeBuilder<T>> action) where T : class => action(builder.Entity<T>());

            Entity<Group>(_ =>
            {
                _.HasMany(x => x.Members)
                    .WithMany(x => x.Groups)
                    .UsingEntity<Membership>(
                        m => m.HasOne(x => x.User).WithMany(x => x.Memberships),
                        m => m.HasOne(x => x.Group).WithMany(x => x.Memberships));

                _.HasMany(x => x.Games)
                    .WithOne(x => x.Group)
                    .IsRequired();
            });
            
            Entity<Membership>(_ =>
            {
                _.Property(x => x.Role)
                    .HasMaxLength(20)
                    .HasConversion(new EnumToStringConverter<GroupRole>());
            });
        }
    }
}