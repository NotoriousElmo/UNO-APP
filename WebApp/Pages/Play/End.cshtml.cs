using DAL;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApp.Pages.Play;

public class End : PageModel
{

    public IGameRepository GameRepository;
    private readonly AppDbContext _context;
    
    public End(AppDbContext context, IGameRepository repository)
    {
        _context = context;
        GameRepository = repository;

        // DB
        GameRepository = new GameRepositoryEF(_context);
    }

    [BindProperty(SupportsGet = true)]
    public Guid GameId { get; set; }
    [BindProperty(SupportsGet = true)]
    public string? Player { get; set; }
    
    public IActionResult OnGet()
    {
        return Page();
    }

    public IActionResult OnPost()
    {
        GameRepository.DeleteGame(GameId);
        return RedirectToPage("../Games/Index");
    }
}