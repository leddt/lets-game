using LetsGame.Web.Extensions;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace LetsGame.Web.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : IdentityDbContext<AppUser>(options), IDataProtectionKeyContext
    {
        public DbSet<Group> Groups => Set<Group>();
        
        public DbSet<Membership> Memberships => Set<Membership>();
        
        public DbSet<GroupGame> GroupGames => Set<GroupGame>();
        
        public DbSet<GroupEvent> GroupEvents => Set<GroupEvent>();
        public DbSet<GroupEventSlot> GroupEventSlots => Set<GroupEventSlot>();
        public DbSet<GroupEventSlotVote> GroupEventSlotVotes => Set<GroupEventSlotVote>();
        public DbSet<GroupEventCantPlay> GroupEventCantPlays => Set<GroupEventCantPlay>();
        public DbSet<GroupInvite> GroupInvites => Set<GroupInvite>();
        
        public DbSet<DataProtectionKey> DataProtectionKeys => Set<DataProtectionKey>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            
            builder.HasDefaultSchema("private");
            
            builder.Entity<Group>(e =>
            {
                e.HasMany(x => x.Members)
                    .WithMany(x => x.Groups)
                    .UsingEntity<Membership>(
                        m => m.HasOne(x => x.User).WithMany(x => x.Memberships),
                        m => m.HasOne(x => x.Group).WithMany(x => x.Memberships));

                e.HasMany(x => x.Games)
                    .WithOne(x => x.Group)
                    .IsRequired();
            });
            
            builder.Entity<Membership>(e =>
            {
                e.Property(x => x.Role)
                    .HasMaxLength(20)
                    .HasConversion(new EnumToStringConverter<GroupRole>());
            });
            
            builder.Entity<GroupEvent>(e =>
            {
                e.HasOne(x => x.Group).WithMany(x => x.Events).IsRequired().OnDelete(DeleteBehavior.NoAction);
                e.HasOne(x => x.Game).WithMany().IsRequired(false);
                e.HasOne(x => x.Creator).WithMany().OnDelete(DeleteBehavior.SetNull);
                e.HasMany(x => x.Slots).WithOne(x => x.Event).IsRequired();
            });
            
            builder.Entity<GroupEventCantPlay>(e =>
            {
                e.HasKey(x => new {x.EventId, x.UserId});
                e.HasOne(x => x.Event).WithMany(x => x.CantPlays).IsRequired();
                e.HasOne(x => x.User).WithMany().IsRequired();
            });
            
            builder.Entity<GroupEventSlot>(e =>
            {
                e.HasMany(x => x.Voters)
                    .WithMany("SlotVotes")
                    .UsingEntity<GroupEventSlotVote>(
                        v => v.HasOne(x => x.Voter).WithMany(),
                        v => v.HasOne(x => x.Slot).WithMany(x => x.Votes));
            });
        }
    }

    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
            WebApplicationBuilderExtensions.ConfigureDbContext(builder);
            
            return new ApplicationDbContext(builder.Options);
        }
    }
}