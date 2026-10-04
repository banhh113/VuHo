using CLBVo.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CLBVo.ViewComponents;

public class ClubInfoViewComponent : ViewComponent
{
    private readonly AppDbContext _db;
    public ClubInfoViewComponent(AppDbContext db) => _db = db;

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var info = await _db.ClubInfos.FirstOrDefaultAsync();
        return View(info);
    }
}