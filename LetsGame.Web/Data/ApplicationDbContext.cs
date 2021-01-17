using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace LetsGame.Web.Data
{
    public class ApplicationDbContext : IdentityDbContext<AppUser>, IDataProtectionKeyContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Group> Groups { get; set; }
        
        public DbSet<Membership> Memberships { get; set; }
        
        public DbSet<GroupGame> GroupGames { get; set; }
        
        public DbSet<GroupEvent> GroupEvents { get; set; }
        public DbSet<GroupEventSlot> GroupEventSlots { get; set; }
        public DbSet<GroupEventSlotVote> GroupEventSlotVotes { get; set; }
        

        public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }

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
            
            Entity<GroupEvent>(_ =>
            {
                _.HasOne(x => x.Group).WithMany(x => x.Events).IsRequired().OnDelete(DeleteBehavior.NoAction);
                _.HasOne(x => x.Game).WithMany().IsRequired();
                _.HasOne(x => x.Creator).WithMany().OnDelete(DeleteBehavior.SetNull);
                _.HasMany(x => x.Slots).WithOne(x => x.Event).IsRequired();
            });
            
            Entity<GroupEventSlot>(_ =>
            {
                _.HasMany(x => x.Voters)
                    .WithMany("SlotVotes")
                    .UsingEntity<GroupEventSlotVote>(
                        v => v.HasOne(x => x.Voter).WithMany(),
                        v => v.HasOne(x => x.Slot).WithMany(x => x.Votes));
            });
        }
    }
}