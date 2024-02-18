using DAL;
using Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UNOEngine;
using Player = UNOEngine.Player;

namespace WebApp.Pages.Games;

public class Names : PageModel
{
    private readonly AppDbContext _context;
    private readonly IGameRepository _repository;

    public Names(AppDbContext context, IGameRepository repository)
    {
        _context = context;
        _repository = repository;

        // DB
        _repository = new GameRepositoryEF(context);
    }
    
    [BindProperty(SupportsGet = true)]
    public String? StateJson { get; set; }

    public GameState State { get; set; } = default!;
    public Guid GameId { get; set; }
    
    [BindProperty]
    public List<Player> SettablePlayers { get; set; } = new();


    public void OnGet()
    {
        State = System.Text.Json.JsonSerializer.Deserialize<GameState>(StateJson!, JsonHelper.JsonSerializerOptions)!;
        
        GameId = State.GameId;
        
        for (int i = 0; i < State.PlayerTypes[EPlayerType.Human]; i++)
        {
            SettablePlayers.Add(new Player()
            {
                Name = $"Human{i + 1}",
                PlayerType = EPlayerType.Human,
            });
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        
        await _context.SaveChangesAsync();
        
        var engine = new GameEngine();

        State = System.Text.Json.JsonSerializer.Deserialize<GameState>(StateJson!, JsonHelper.JsonSerializerOptions)!;
        
        GameId = State.GameId;

        State = engine.InitializeFullDeck(State);
            
        State.PlayedCards.Push(State.DeckOfCardsInPlay[^1]);
        State.DeckOfCardsInPlay.RemoveAt(State.DeckOfCardsInPlay.Count - 1);
            
        while (State.PlayedCards.Peek().CardSuite == ECardSuite.Wild)
        {
            State.DeckOfCardsInPlay.Insert(0, State.PlayedCards.Pop());
            State.PlayedCards.Push(State.DeckOfCardsInPlay[^1]);
            State.DeckOfCardsInPlay.RemoveAt(State.DeckOfCardsInPlay.Count - 1);
        }
        
        State.Players = engine.InitializePlayerNamesAndTypes(SettablePlayers, State.PlayerTypes);

        State = engine.InitializePlayerDecks(State);
        State.Players = engine.Shuffle(State.Players);
        
        _repository.SaveGame(GameId, State);
        
        return RedirectToPage("./Index");
    }
}