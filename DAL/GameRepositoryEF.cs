using System.Text.Json;
using Domain;
using Domain.Database;
using UNOEngine;
using Player = Domain.Database.Player;

namespace DAL;

public class GameRepositoryEF : IGameRepository
{
    private readonly AppDbContext _context;

    public GameRepositoryEF(AppDbContext context)
    {
        _context = context;
    }

    public void SaveGame(Guid id, GameState state)
    {
        var game = _context.Games.FirstOrDefault(g => g.Id == state.GameId);

        if (game == null)
        {
            game = new Game()
            {
                Id = state.GameId,
                CreatedTime = DateTime.Now,
                UpdatedTime = DateTime.Now,
                State = JsonSerializer.Serialize(state, JsonHelper.JsonSerializerOptions),
                Players = state.Players.Select(p=> new Player()
                {
                    Id = p.Id,
                    Name = p.Name,
                    PlayerType = p.PlayerType,
                    GameId = state.GameId
                }).ToList()
            };
            _context.Add(game);
        }
        else
        {
            game.UpdatedTime = DateTime.Now;
            game.State = JsonSerializer.Serialize(state, JsonHelper.JsonSerializerOptions);
        }
        
        var changeCount = _context.SaveChanges();
        Console.WriteLine("SaveChanges: " + changeCount);
    }

    public GameState LoadGame(Guid id)
    {
        var game = _context.Games.First(g => g.Id == id);
        var state = JsonSerializer.Deserialize<GameState>(game.State, JsonHelper.JsonSerializerOptions)!;
        
        var fixedStack = new Stack<GameCard>();
        foreach (var card in state.PlayedCards)
        {
            fixedStack.Push(card);
        }

        state.PlayedCards = fixedStack;
        return state;
    }

    public List<(Guid, DateTime, DateTime)> GetSavedGames()
    {
        return _context.Games
            .OrderByDescending(g => g.UpdatedTime)
            .ToList()
            .Select(g => (g.Id, g.CreatedTime, g.UpdatedTime))
            .ToList();
    }

    public void DeleteGame(Guid id)
    {
        var game = _context.Games.FirstOrDefault(g => g.Id == id);
        if (game != null)
        {
            _context.Games.Remove(game);
            _context.SaveChanges();
        }
    }
}