using CLBVo.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CLBVo.Data;

public class AppDbContext : IdentityDbContext<IdentityUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Achievement> Achievements => Set<Achievement>();
    public DbSet<ClubInfo> ClubInfos => Set<ClubInfo>();
    public DbSet<ClubEvent> Events => Set<ClubEvent>();
    public DbSet<GalleryImage> GalleryImages => Set<GalleryImage>();
    public DbSet<Member> Members => Set<Member>();
    public DbSet<Post> Posts => Set<Post>();
    public DbSet<Registration> Registrations => Set<Registration>();
    public DbSet<TimeSlot> TimeSlots => Set<TimeSlot>();
    public DbSet<ClubLocation> ClubLocations => Set<ClubLocation>();
    public DbSet<Tournament> Tournaments => Set<Tournament>();
    public DbSet<TournamentResult> TournamentResults => Set<TournamentResult>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<TournamentResult>()
            .HasOne(r => r.Tournament)
            .WithMany(t => t.Results)
            .HasForeignKey(r => r.TournamentId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<TournamentResult>()
            .HasOne(r => r.Member)
            .WithMany()
            .HasForeignKey(r => r.MemberId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}