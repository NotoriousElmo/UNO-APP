using DAL;
using Domain.Database;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace WebApp.Pages.Games
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;
        private readonly IGameRepository _repository;

        public IndexModel(AppDbContext context, IGameRepository repository)
        {
            _context = context;
            _repository = repository;

            // DB
            _repository = new GameRepositoryEF(_context);
        }
        
        public IList<Game> Game { get;set; } = default!;
        


        public async Task OnGetAsync()
        {
            // For DB
            Game = await _context.Games
                .Include(g => g.Players)
                .OrderByDescending(g => g.UpdatedTime)
                .ToListAsync();



            // For json
            // var games = _repository.GetSavedGames();
            //
            // Game = new List<Game>();
            //
            // foreach (var game in games)
            // {
            //     var state = _repository.LoadGame(game.id);
            //     Game.Add(new Game()
            //     {
            //         Id = game.id,
            //         CreatedTime = game.ct,
            //         UpdatedTime = game.dt,
            //         State = System.Text.Json.JsonSerializer.Serialize(state, JsonHelper.JsonSerializerOptions),
            //         Players = state.Players.Select(p => new Domain.Database.Player()
            //         {
            //             Name = p.Name,
            //             PlayerType = p.PlayerType,
            //             GameId = game.id,
            //             Id = p.Id,
            //         }).ToList(),
            //     });
            // }
        }
    }
}
