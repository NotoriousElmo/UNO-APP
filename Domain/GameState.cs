using UNOEngine;

namespace Domain;

public class GameState
{
    public Guid GameId { get; set; } = Guid.NewGuid();
    public List<GameCard> DeckOfCardsInPlay { get; set; } = new();
    public Dictionary<EPlayerType, int> PlayerTypes { get; set; } = new();
    public Dictionary<string, List<GameCard>> PlayerDecks { get; set; } = new();
    public Stack<GameCard> PlayedCards { get; set; } = new();
    public int OrderCount { get; set; }
    public List<Player> Players { get; set; } = new();
    public int Round { get; set; } = 1;
    public string? Reason { get; set; }
    public GameCard? ChangeColors { get; set; }
    public int HandSize { get; set; } = 7;
    public bool SkipAfterDraw { get; set; } = true;
    public bool CanStackCards { get; set; } = true;
    public bool CanPlayDrawWithoutRestrictions { get; set; } = false;
    public Dictionary<string, bool> UnoDict { get; set; } = new();
    public Dictionary<ECardValue, int> AmountOfCardValues { get; set; } = new();
    public List<GameCard>? TmpDeck { get; set; } = null;
    public int OrderCountBuffer { get; set; } = 0;

}