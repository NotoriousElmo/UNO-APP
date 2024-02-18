using DAL;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApp.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly AppDbContext _context;

    public IndexModel(ILogger<IndexModel> logger, AppDbContext context)
    {
        _logger = logger;
        _context = context;
    }
    
    public int Count { get; set; }

    public void OnGet()
    {
        // json
        // var gameRepo = new GameRepositoryFileSystem();
        // Count = gameRepo.GetSavedGames().Count;
        
        // db
        Count = _context.Games.Count();
    }
}