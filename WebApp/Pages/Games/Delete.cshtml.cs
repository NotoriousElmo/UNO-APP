using DAL;
using Domain.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace WebApp.Pages.Games
{
    public class DeleteModel : PageModel
    {
        private readonly AppDbContext _context;
        private readonly IGameRepository _gameRepository;

        public DeleteModel(AppDbContext context, IGameRepository repository)
        {
            _context = context;
            _gameRepository = repository;

            // DB
            _gameRepository = new GameRepositoryEF(_context);
        }

        [BindProperty]
        public Game Game { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            // JSON
            // var state = _gameRepository.LoadGame(id);
            // var fileInfo = _gameRepository.GetSavedGames().FirstOrDefault(g => g.id == id);
            //
            // Game = new Game()
            // {
            //     Id = id,
            //     CreatedTime = fileInfo.Item2,
            //     UpdatedTime = fileInfo.Item3,
            //     State = System.Text.Json.JsonSerializer.Serialize(state, JsonHelper.JsonSerializerOptions),
            //     Players = state.Players.Select(p => new Domain.Database.Player()
            //     {
            //         Name = p.Name,
            //         PlayerType = p.PlayerType,
            //         GameId = id,
            //         Id = p.Id,
            //     }).ToList(),
            // };
                
            
            // DB
            var game = await _context.Games.FirstOrDefaultAsync(m => m.Id == id);
            
            Game = game!;

            return Page();
        }

        public Task<IActionResult> OnPostAsync(Guid id)
        {
            _gameRepository.DeleteGame(id);

            return Task.FromResult<IActionResult>(RedirectToPage("./Index"));
        }
    }
}
