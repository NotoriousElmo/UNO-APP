using DAL;
using Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UNOEngine;

namespace WebApp.Pages.Play
{
    public class ChangeColor : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public Guid GameId { get; set; }
        [BindProperty(SupportsGet = true)]
        public Guid PlayerId { get; set; }
        public GameCard? LastCard { get; set; }
        
        private readonly AppDbContext _context;
        private readonly IGameRepository _gameRepository;

        public ChangeColor(AppDbContext context, IGameRepository repository)
        {
            _context = context;
            _gameRepository = repository;
            
            // DB
            _gameRepository = new GameRepositoryEF(_context);
        }

        public IActionResult OnPost(string selectedColor)
        {
            var state = _gameRepository.LoadGame(GameId);

            LastCard = state.ChangeColors;

            switch (selectedColor)
            {
                case "red": LastCard!.CardSuite = ECardSuite.Red; break;
                case "green": LastCard!.CardSuite = ECardSuite.Green; break;
                case "yellow": LastCard!.CardSuite = ECardSuite.Yellow; break;
                case "blue": LastCard!.CardSuite = ECardSuite.Blue; break;
            }

            var engine = new GameEngine();

            state.OrderCount++;
            state = engine.CheckForEndOfPlayers(state);
            state.ChangeColors = null;


            state.PlayedCards.Push(LastCard!);

            if (state.SkipAfterDraw && LastCard!.CardValue == ECardValue.Draw4)
            {
                state.OrderCount++;
            }
            
            state = engine.CheckForEndOfPlayers(state);
            
            state = engine.CheckDeUno(state);

            _gameRepository.SaveGame(GameId, state);

            return RedirectToPage("./Index", new {GameId,PlayerId});
        }
    }
}