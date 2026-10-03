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
}