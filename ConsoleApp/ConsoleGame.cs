using DAL;
using Domain;
using Draw;
using UNOEngine;

namespace ConsoleApp;

public class ConsoleGame <TKey>
{
    private GameState _state;
    private readonly GameEngine _engine;
    private IGameRepository GameRepository { get; set; }
    private DrawClass DrawClass { get; set; }


    public ConsoleGame(IGameRepository repository,
        Dictionary<EPlayerType, int> playerTypes, int handSize,
        bool skipAfter, bool canStack, List<Player> players, Dictionary<ECardValue, int> amountOfCardValues,
        bool canPlayDraw, GameState? load = null)
    {
        _state = new GameState();
        _engine = new GameEngine();
        GameRepository = repository;
        _state.PlayerTypes = playerTypes;
        _state.HandSize = handSize;
        _state.SkipAfterDraw = skipAfter;
        _state.CanStackCards = canStack;
        _state.CanPlayDrawWithoutRestrictions = canPlayDraw;
        _state.Players = players;
        _state.AmountOfCardValues = amountOfCardValues;
        DrawClass = new DrawClass();
        if (load == null)
        {
            _state = _engine.InitializeFullDeck(_state);


            _state.PlayedCards.Push(_state.DeckOfCardsInPlay[^1]);
            _state.DeckOfCardsInPlay.RemoveAt(_state.DeckOfCardsInPlay.Count - 1);
            while (_state.PlayedCards.Peek().CardSuite == ECardSuite.Wild)
            {
                _state.DeckOfCardsInPlay.Insert(0, _state.PlayedCards.Pop());
                _state.PlayedCards.Push(_state.DeckOfCardsInPlay[^1]);
                _state.DeckOfCardsInPlay.RemoveAt(_state.DeckOfCardsInPlay.Count - 1);
            }

            _state.DeckOfCardsInPlay = _engine.Shuffle(_state.DeckOfCardsInPlay);
            
            _state = _engine.InitializePlayerDecks(_state);
            _state.Players = _engine.Shuffle(_state.Players);
        }
        else
        {
            _state = load;
        }
    }
    
    public void PlayGame()
    {
        while (_engine.Finished(_state) == null)
        {
            try
            {
                if (_state.DeckOfCardsInPlay.Count < 4)
                {
                    _engine.ReShuffle(_state);
                }

                _engine.CheckForEndOfPlayers(_state);
                _state.Reason = null;
                GameRepository.SaveGame(_state.GameId, _state);
                var player = _state.Players[_state.OrderCount];
                switch (player.PlayerType)
                {
                    case EPlayerType.Human:
                        ShowPlayerDeck(player);
                        break;
                    case EPlayerType.Bot:
                    {
                        var (playedCard, gameState) = _engine.MakeRandomMove(player, _state);
                        _state = gameState;
                        if (playedCard == null)
                        {
                            DrawClass.ShowPlayerTakesCard(player, _state);
                        }
                        else
                        {
                            // var playedCards = new List<GameCard> { playedCard };

                            // if (_state.CanStackCards)
                            // {
                            //     var otherPlayableCards = 
                            //         _engine.GetOtherPlayableCards(playedCard, player, _state);
                            //     var input = GetPlayerStackedCards(otherPlayableCards, player, _state);
                            //     playedCards = input;
                            // }
                            DrawClass.ShowPlayerPlaysCard(player, playedCard, _state);
                        }

                        if (GameEngine.CheckForUno(player, _state))
                        {
                            _state = DrawClass.ShowUno(player, _state);
                        }

                        break;
                    }
                }

                _engine.CheckForEndOfPlayers(_state);
                _state.OrderCount++;
                GameRepository.SaveGame(_state.GameId, _state);
            }
            catch (ApplicationException)
            {
                return;
            }
        }

        DrawClass.ShowWinner(_state);

        HandleFinishScreenInput();

    }

    private void HandleFinishScreenInput()
    {
        GameRepository.DeleteGame(_state.GameId);
    }

    // If stacking is allowed, than handle stackable card play.
    private List<GameCard> GetPlayerStackedCards(List<GameCard> cards, Player player)
    {
        var playedCards = new List<GameCard>(); 
        var userChoice = "";
        var count = 0;
        var length = cards.Count;
        
        while (userChoice != "x" && userChoice != "l" && count < length)
        {
            userChoice = DrawClass.DrawPlayerStack(cards, player, _state);
            if (int.TryParse(userChoice, out var numeric))
            {
                // Play 1 card
                playedCards.Add(cards[numeric]);
                cards.RemoveAt(numeric);
                count++;
            }
        }

        // Play all
        if (userChoice == "l")
        {
            return cards;
        }

        return playedCards;
    }

    private string GetPlayerPlay(Player player)
    {
        var userChoice = "";
        while (userChoice != "t" && userChoice != "x" &&
               (!int.TryParse(userChoice, out _) || int.Parse(userChoice) > _state.PlayerDecks[player.Name].Count - 1 ||
                int.Parse(userChoice) < 0 ||
                !_engine.IsPlayPossible(_state.PlayerDecks[player.Name][int.Parse(userChoice)], player, _state)))
        {
            userChoice = DrawClass.DrawPlayerDeck(_state, _state.PlayerDecks[player.Name], player);
        }

        if (userChoice == "x")
        {
            throw new ApplicationException();
        }
        return userChoice;
    }

    private void ShowPlayerDeck(Player player)
    {
        var playedCards = new List<GameCard>();
        GameCard playedCard = default!;
        var key = ConsoleKey.A;
        
        while (key != ConsoleKey.Enter)
        {
            key = DrawClass.DrawPlayerStartSignal(_state, player);
        }
        Console.Clear();

        var userChoice = GetPlayerPlay(player);

        if (userChoice.Trim().ToLower() != "t")
        {
            playedCard = _state.PlayerDecks[player.Name][int.Parse(userChoice)];
            _state = _engine.Play(playedCard, player, _state);
            _state = _engine.CheckForEndOfPlayers(_state);
            _state.PlayedCards.Push(playedCard);
            _state.PlayerDecks[player.Name].Remove(playedCard);
            if (playedCard.CardSuite == ECardSuite.Wild)
            {
                _state = DrawClass.ChangeColors(playedCard, _state);
            }
        }
        else if (userChoice.Trim().ToLower() == "t")
        {
            _state = _engine.AddCardsToPlayer(player, _state);

            DrawClass.ShowPlayerTakesCard(player, _state);
            return;
        }

        if (_state.CanStackCards)
        {
            HandleStackPlay(player, playedCard, playedCards);
        }
        else
        {
            DrawClass.ShowPlayerPlaysCard(player, playedCard, _state);
            
            if (GameEngine.CheckForUno(player, _state))
            {
                _state = DrawClass.ShowUno(player, _state);
            }
        }
    }

    private void HandleStackPlay(Player player, GameCard playedCard, List<GameCard> playedCards)
    {
        var otherPlayableCards =
            _engine.GetStackableCards(playedCard, player, _state);
        var input = GetPlayerStackedCards(otherPlayableCards, player);
        playedCards.AddRange(input);

        DrawClass.ShowPlayerPlaysCard(player, playedCard, _state);

        if (GameEngine.CheckForUno(player, _state))
        {
            _state = DrawClass.ShowUno(player, _state);
        }

        _engine.CheckForEndOfPlayers(_state);

        // Play all chosen cards.
        foreach (var card in playedCards)
        {
            _state = _engine.Play(card, player, _state);
            _state.PlayedCards.Push(card);
            _state.PlayerDecks[player.Name].Remove(card);
            DrawClass.ShowPlayerPlaysCard(player, card, _state);
            if (GameEngine.CheckForUno(player, _state))
            {
                _state = DrawClass.ShowUno(player, _state);
            }

            _engine.CheckForEndOfPlayers(_state);
        }
    }
}