using DAL;
using Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UNOEngine;

namespace WebApp.Pages.Play;

public class Index : PageModel
{

    private readonly AppDbContext _context;
    public IGameRepository GameRepository;
    public GameState State = default!;
    public GameEngine Engine = default!;

    public Index(AppDbContext context, IGameRepository repository)
    {
        _context = context;
        GameRepository = repository;
        
        // DB
        GameRepository = new GameRepositoryEF(_context);
    }
    
    [BindProperty(SupportsGet = true)]
    public Guid GameId { get; set; }
    [BindProperty(SupportsGet = true)]
    public Guid PlayerId { get; set; }

    public IActionResult OnGet()
    {
        State = GameRepository.LoadGame(GameId);
        // State.PlayedCards = FixReversion(State.PlayedCards);
        Engine = new GameEngine();

        if (Engine.Finished(State) != null)
        {
            return RedirectToPage("./End", new { GameId, _gameRepository = GameRepository, Player = Engine.Finished(State) });
        }

        // To handle situation, where upon loading the current player is a bot. Without it the game is at a pause.
        if (Engine.GetActivePlayer(State).PlayerType == EPlayerType.Bot)
        {
            State = Engine.PlayBotForWeb(State);
            GameRepository.SaveGame(GameId, State);
        }
        
        return Page();
    }

    // For some reason the stack gets reversed with OnGet() method, can't really figure out why. Only happens with DB.
    // public Stack<GameCard> FixReversion(Stack<GameCard> stack)
    // {
    //     Stack<GameCard> fixedStack = new();
    //     foreach (var card in State.PlayedCards)
    //     {
    //         fixedStack.Push(card);
    //     }
    //
    //     return fixedStack;
    // }

    public IActionResult OnPost(string card, string action)
    {

        Engine = new GameEngine();
        State = GameRepository.LoadGame(GameId);
        
        if (Engine.Finished(State) != null)
        {
            return RedirectToPage("./End", new { GameId, _gameRepository = GameRepository, Player = Engine.Finished(State) });
        }

        // State.PlayedCards = FixReversion(State.PlayedCards);

        var currPlayer = GetPlayer();

        if (action == "CONTINUE")
        {
            return HandleContinue();
        }

        if (action == "PLAYALL")
        {
            if (currPlayer != null) return HandlePlayAll(currPlayer);
        }

        if (action == "TAKE")
        {
            return HandleTake(currPlayer);
        }
        if (action == "UNO")
        {
            if (currPlayer != null) return HandleUno(currPlayer);
        }
        return HandlePlay(card, currPlayer!);
    }

    public Player? GetPlayer()
    {
        Player? currPlayer = null;

        foreach (var player in State.Players)
        {
            if (player.Id == PlayerId)
            {
                currPlayer = player;
            }
        }

        return currPlayer;
    }

    private IActionResult HandlePlayAll(Player player)
    {
        foreach (var card in State.TmpDeck!)
        {
            State = Engine.Play(card, player, State, true);
            State.PlayerDecks[player.Name] = RemoveCardFromPlayerDeck(State.PlayerDecks[player.Name], card);
            State.PlayedCards.Push(card);
            State = Engine.CheckForEndOfPlayers(State);
        }
        
        HandleEndOfStack();

        State.OrderCount++;

        State = Engine.CheckForEndOfPlayers(State);
        
        State = Engine.CheckDeUno(State);

        GameRepository.SaveGame(GameId, State);

        return RedirectToPage(new { GameId, PlayerId });
    }

    private IActionResult HandleContinue()
    {
        HandleEndOfStack();

        State.OrderCount++;

        State = Engine.CheckForEndOfPlayers(State);
        
        State = Engine.CheckDeUno(State);

        GameRepository.SaveGame(GameId, State);

        return RedirectToPage(new { GameId, PlayerId });
    }

    private IActionResult HandleUno(Player player)
    {
        foreach (var varPlayer in State.Players)
        {

            if (!State.UnoDict.ContainsKey(player.Name))
            {
                State.UnoDict[player.Name] = false;
            }
            if (State.PlayerDecks[varPlayer.Name].Count < 2 && !varPlayer.Name.Equals(player.Name) && 
                !State.UnoDict[varPlayer.Name])
            {
                State = Engine.AddCardsToPlayer(varPlayer, State);
                State = Engine.AddCardsToPlayer(varPlayer, State);
                State = Engine.CheckDeUno(State);
                State.UnoDict[varPlayer.Name] = true;
                
                GameRepository.SaveGame(GameId, State);

                return RedirectToPage(new { GameId, PlayerId });
            }
            else if (State.PlayerDecks[varPlayer.Name].Count < 2 &&
                     varPlayer.Name.Equals(player.Name) &&
                     !State.UnoDict[player.Name])
            {
                State.UnoDict[player.Name] = true;
                
                GameRepository.SaveGame(GameId, State);
                
                return RedirectToPage(new { GameId, PlayerId });
            }
        }
        
        State = Engine.AddCardsToPlayer(player, State);
        State = Engine.AddCardsToPlayer(player, State);
        
        GameRepository.SaveGame(GameId, State);

        return RedirectToPage(new { GameId, PlayerId });
    }

    private IActionResult HandleTake(Player? currPlayer)
    {
        State = Engine.AddCardsToPlayer(currPlayer!, State);

        State.OrderCount++;

        State = Engine.CheckForEndOfPlayers(State);

        State = Engine.CheckDeUno(State);

        GameRepository.SaveGame(GameId, State);

        return RedirectToPage(new { GameId, PlayerId });
    }

    private IActionResult HandlePlay(string card, Player player)
    {
        var suite = int.Parse(card.Split(",")[0].Trim());
        var value = int.Parse(card.Split(",")[1].Trim());
        var playedCard = new GameCard(suite, value);
        
        // Find if there are any stackable cards.
        var deck = Engine.GetStackableCards(playedCard, player, State);

        if (State.CanStackCards && deck.Count > 1 && State.TmpDeck == null)
        {
            return HandleNewStack(player, deck, playedCard);
        }
        else if (State.CanStackCards && State.TmpDeck != null)
        {
            return HandleOldStack(player, playedCard);
        }

        return PlayCard(player, playedCard);
    }

    private IActionResult PlayCard(Player player, GameCard playedCard)
    {
        State = Engine.Play(playedCard, player, State, true);
        if (playedCard.CardValue != ECardValue.Draw4 && playedCard.CardValue != ECardValue.ChangeColor)
        {
            State.PlayedCards.Push(playedCard);
        }

        State = Engine.CheckForEndOfPlayers(State);

        State.PlayerDecks[player.Name] = RemoveCardFromPlayerDeck(State.PlayerDecks[player.Name], playedCard);

        if (State.ChangeColors != null)
        {
            State.ChangeColors = playedCard;
            GameRepository.SaveGame(GameId, State);
            return RedirectToPage("./ChangeColor", new { GameId, PlayerId });
        }

        // If cards are running out shuffle the played cards and add them back to the stack.
        if (State.DeckOfCardsInPlay.Count < 4)
        {
            State = Engine.ReShuffle(State);
        }

        State.OrderCount++;

        State = Engine.CheckForEndOfPlayers(State);

        if (Engine.Finished(State) != null)
        {
            return RedirectToPage("./End",
                new { GameId, _gameRepository = GameRepository, Player = Engine.Finished(State) });
        }
        
        State = Engine.CheckDeUno(State);

        GameRepository.SaveGame(GameId, State);

        return RedirectToPage(new { GameId, PlayerId });
    }

    private IActionResult HandleOldStack(Player player, GameCard playedCard)
    {
        var prev = State.OrderCount;

        State = Engine.Play(playedCard, player, State);
        State.PlayedCards.Push(playedCard);

        var after = State.OrderCount;

        if (prev != after)
        {
            State.OrderCountBuffer++;
            State.OrderCount = prev;
        }

        State.TmpDeck = RemoveCardFromPlayerDeck(State.TmpDeck!, playedCard);
        State.PlayerDecks[player.Name] = RemoveCardFromPlayerDeck(State.PlayerDecks[player.Name], playedCard);

        if (State.TmpDeck.Count == 0)
        {
            HandleEndOfStack();

            State.OrderCount++;

            State = Engine.CheckForEndOfPlayers(State);
            
            if (Engine.Finished(State) != null)
            {
                return RedirectToPage("./End",
                    new { GameId, _gameRepository = GameRepository, Player = Engine.Finished(State) });
            }
        }

        State = Engine.CheckDeUno(State);

        GameRepository.SaveGame(GameId, State);

        return RedirectToPage(new { GameId, PlayerId });
    }

    private IActionResult HandleNewStack(Player player, List<GameCard> deck, GameCard playedCard)
    {
        State.TmpDeck = deck;

        var prev = State.OrderCount;

        State = Engine.Play(playedCard, player, State);

        var after = State.OrderCount;

        if (prev != after)
        {
            State.OrderCountBuffer++;
            State.OrderCount = prev;
        }

        State.TmpDeck = RemoveCardFromPlayerDeck(State.TmpDeck!, playedCard);
        State.PlayerDecks[player.Name] = RemoveCardFromPlayerDeck(State.PlayerDecks[player.Name], playedCard);
        State.PlayedCards.Push(playedCard);

        if (State.TmpDeck.Count == 0)
        {
            HandleEndOfStack();

            State.OrderCount++;

            State = Engine.CheckForEndOfPlayers(State);
            
            if (Engine.Finished(State) != null)
            {
                return RedirectToPage("./End",
                    new { GameId, _gameRepository = GameRepository, Player = Engine.Finished(State) });
            }

        }

        State = Engine.CheckDeUno(State);

        GameRepository.SaveGame(GameId, State);

        return RedirectToPage(new { GameId, PlayerId });
    }

    private List<GameCard> RemoveCardFromPlayerDeck(List<GameCard> deck, GameCard playedCard)
    {
        var chosen = false;
        for (int i = 0; i < deck.Count; i++)
        {
            if (deck[i].CardSuite == playedCard.CardSuite
                && deck[i].CardValue == playedCard.CardValue
                && !chosen)
            {
                deck.RemoveAt(i);
                chosen = true;
            }
        }

        return deck;
    }

    private void HandleEndOfStack()
    {
        while (State.OrderCountBuffer != 0)
        {
            State.OrderCountBuffer--;
            State.OrderCount++;
            State = Engine.CheckForEndOfPlayers(State);
        }
        State.TmpDeck = null;
    }
}
