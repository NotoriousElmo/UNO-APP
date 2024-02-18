using System.Text;
using Domain;

namespace UNOEngine;

public class GameEngine
{
    private Random Rnd { get; set; } = new();
    

    public GameState CheckForEndOfPlayers(GameState state)
    {
        if (state.OrderCount > state.Players.Count - 1)
        {
            state.OrderCount = 0;
            state.Round++;
        }
        return state;
    }
    
    public List<Player> InitializePlayerNamesAndTypes(List<Player> setPlayers, Dictionary<EPlayerType, int> playerTypes)
    {
        var players = new List<Player>();
        for (var count = 1; count <= playerTypes[EPlayerType.Bot]; count++)
        {
            var player = new Player
            {
                Name = $"Bot{count}",
                PlayerType = EPlayerType.Bot
            };
            players.Add(player);
        }
        foreach (var player in setPlayers)
        {
            players.Add(player);
        }
        for (var count = 1; count <= playerTypes[EPlayerType.Human] - setPlayers.Count; count++)
        {
            var player = new Player
            {
                Name = $"Human{count}",
                PlayerType = EPlayerType.Human
            };
            players.Add(player);
        }

        return players;
    }

    // Take played cards and add them to cards in play, with the first card remaining the first.
    public GameState ReShuffle(GameState state)
    {
        if (state.PlayedCards.Count > 1) {
            var reShuffle = new List<GameCard>();
            bool isFirstCard = true;
            GameCard firstCard = default!;
            foreach (var card in state.PlayedCards)
            {
                if (card.CardValue is ECardValue.Draw4 or ECardValue.ChangeColor)
                {
                    card.CardSuite = ECardSuite.Wild;
                }
                if (isFirstCard)
                {
                    firstCard = card;
                    isFirstCard = false;
                }
                else
                {
                    reShuffle.Add(card);
                }
            }

            foreach (var card in state.DeckOfCardsInPlay)
            {
                reShuffle.Add(card);
            }

            state.DeckOfCardsInPlay = Shuffle(reShuffle);
            state.PlayedCards.Push(firstCard);
        }
        return state;
    }

    // Maybe in another universe, where the console game consists of series of games. 
    
    // public string FindHighestValueDeck(GameState state)
    // {
    //     var playerValues = new Dictionary<string, int>();
    //     foreach (var player in state.PlayerDecks.Keys)
    //     {
    //         playerValues[player] = 0;
    //         foreach (var card in state.PlayerDecks[player])
    //         {
    //             var value = card.CardValue;
    //             switch (value)
    //             {
    //                 case ECardValue.Plus2 or ECardValue.Reverse or ECardValue.Skip:
    //                 case ECardValue.Draw4 or ECardValue.ChangeColor:
    //                     playerValues[player] += 20;
    //                     break;
    //                 default:
    //                     playerValues[player] +=
    //                         int.Parse(Strings.Replace(value.ToString(), "Value", "")!);
    //                     break;
    //             }
    //         }
    //     }
    //     int maxVal = 0;
    //     string playerMax = default!;
    //     foreach (var player in playerValues.Keys)
    //     {
    //         if (playerValues[player] > maxVal)
    //         {
    //             maxVal = playerValues[player];
    //             playerMax = player;
    //         }
    //     }
    //
    //     return playerMax;
    // }

    // Play for bots.
    public (GameCard?, GameState) MakeRandomMove(Player player, GameState state)
    {
        Rnd = new Random();
        var count = 0;
        GameCard card = default!;
        var gotPlay = false;
        
        while (count <= 10)
        {
            var index = Rnd.Next(0, state.PlayerDecks[player.Name].Count - 1);
            card = state.PlayerDecks[player.Name][index];
            count++;
            if (IsPlayPossible(card, player, state))
            {
                gotPlay = true;
                state = Play(card, player, state);
                state.PlayedCards.Push(card);
                state.PlayerDecks[player.Name].RemoveAt(index);
                break;
            }
        }

        if (!gotPlay)
        {
            state = AddCardsToPlayer(player, state);
            return (null, state);
        }
        
        if (card.CardSuite == ECardSuite.Wild)
        {
            // Sets random color as the current color in the game (Is state.CurrentColor even needed any more?).
            var lastCard = state.PlayedCards.Pop();
            lastCard.CardSuite = (ECardSuite)Rnd.Next((int)ECardSuite.Wild);
            state.PlayedCards.Push(lastCard);
        }

        return (card, state);
    }

    public string GetPlayerStackSizeString(GameState state, bool web)
    {
        var lineSeparator = web ? "<br/>" : "\n";
        var stringBuilder = new StringBuilder();

        for (var i = 0; i < state.PlayerDecks.Keys.Count; i++)
        {
            if (state.OrderCount == i)
            {
                stringBuilder.Append(
                    state.Players[i] + " | Stack size: " + 
                    state.PlayerDecks[state.Players[i].Name].Count + " <-");
                stringBuilder.Append(lineSeparator);

            }
            else
            {
                stringBuilder.Append(
                    state.Players[i] + " | Stack size: " + 
                    state.PlayerDecks[state.Players[i].Name].Count);
                stringBuilder.Append(lineSeparator);
            }
        }

        return stringBuilder.ToString();
    }

    public List<GameCard> GetPlayableCards(List<GameCard> cards, GameState state, Player player)
    {
        List<GameCard> playableCards = new();
        foreach (var card in cards)
        {
            if (IsPlayPossible(card, player, state))
            {
                playableCards.Add(card);
            }
        }

        return playableCards;
    }

    public List<GameCard> GetStackableCards(GameCard playedCard, Player player, GameState state)
    {
        List<GameCard> possiblePlays = new List<GameCard>();
        if (playedCard.CardValue != ECardValue.Draw4 && playedCard.CardValue != ECardValue.Plus2 && playedCard.CardValue != ECardValue.ChangeColor)
        {
            foreach (var card in state.PlayerDecks[player.Name])
            {
                if (card.CardValue == playedCard.CardValue)
                {
                    possiblePlays.Add(card);
                }
            }
        }

        return possiblePlays;
    }

    
    public bool IsPlayPossible(GameCard card, Player player, GameState state)
    {
        var lastCard = state.PlayedCards.Peek();

        if (card.CardSuite == ECardSuite.Wild)
        {
            if (card.CardValue == ECardValue.Draw4 && !state.CanPlayDrawWithoutRestrictions)
            {
                foreach (var cardVal in state.PlayerDecks[player.Name])
                {
                    if (cardVal.CardSuite != ECardSuite.Wild && IsPlayPossible(cardVal, player, state))
                    {
                        state.Reason = "Wild 4 is only playable when you do not have any other playable cards!";
                        return false;
                    }
                }
            }
            return true;
        }

        var cardSuite = card.CardSuite;
        if (cardSuite.Equals(lastCard.CardSuite) ||
            card.CardValue.Equals(lastCard.CardValue)) 
        {
            return true;
        }

        state.Reason = "Card has to be of the same type or the same value!";
        return false;
    }

    public GameState Play(GameCard card, Player player, GameState state, bool web = false)
    {
        if (card.CardSuite == ECardSuite.Wild)
        {
            state.Reason = null;
        }
        var lastCard = state.PlayedCards.Peek();
        var cardSuite = card.CardSuite;

        if (cardSuite == ECardSuite.Wild)
        {
            return PlayWild(card, player, state, web);
        }

        if (cardSuite == lastCard.CardSuite || card.CardValue == lastCard.CardValue)
        {
            return PlayNormal(card, player, state);
        }

        return state;
    }

    private GameState PlayNormal(GameCard card, Player player, GameState state)
    {
        switch (card.CardValue)
        {
            case ECardValue.Skip:
                state.OrderCount++;
                return state;
            case ECardValue.Plus2:
            {
                int index;
                if (state.Players.IndexOf(player) == state.Players.Count - 1)
                {
                    index = 0;
                }
                else
                {
                    index = state.Players.IndexOf(player) + 1;
                }

                AddCardsToPlayer(state.Players[index], state);
                AddCardsToPlayer(state.Players[index], state);
                if (state.SkipAfterDraw)
                {
                    state.OrderCount++;
                }

                return state;
            }
            case ECardValue.Reverse:
                state.Players.Reverse();
                state.OrderCount = state.Players.IndexOf(player);

                return state;
            default:
                return state;
        }
    }

    private GameState PlayWild(GameCard card, Player player, GameState state, bool web)
    {
        if (card.CardValue == ECardValue.Draw4)
        {
            int index;
            if (state.Players.IndexOf(player) == state.Players.Count - 1)
            {
                index = 0;
            }
            else
            {
                index = state.Players.IndexOf(player) + 1;
            }

            if (state.SkipAfterDraw && !web)
            {
                state.OrderCount++;
            }

            AddCardsToPlayer(state.Players[index], state);
            AddCardsToPlayer(state.Players[index], state);
            AddCardsToPlayer(state.Players[index], state);
            AddCardsToPlayer(state.Players[index], state);
            Console.WriteLine(state.Players[index]);
        }

        if (player.PlayerType == EPlayerType.Human)
        {
            state.ChangeColors = card;
        }

        return state;
    }

    public GameState PlayBotForWeb(GameState state)
    {
        var activePlayer = GetActivePlayer(state);
        if (activePlayer.PlayerType != EPlayerType.Human && state.PlayerDecks[activePlayer.Name].Count > 0)
        {
            state = MakeRandomMove(activePlayer, state).Item2;
            state = CheckForEndOfPlayers(state);
            state.OrderCount++;
            state = CheckForEndOfPlayers(state);
        }

        return state;
    }

    public string? Finished(GameState state)
    {
        foreach (var player in state.PlayerDecks.Keys)
        {
            if (state.PlayerDecks[player].Count == 0)
            {
                return player;
            }
        }

        return null;
    }

    public GameState InitializePlayerDecks(GameState state)
    {
        var players = state.Players;
        foreach (var player in players)
        {
            state.PlayerDecks[player.Name] = new List<GameCard>();
            for (int count = 0; count < state.HandSize; count++)
            {
                state = AddCardsToPlayer(player, state);
            }
        }
        return state;
    }

    public GameState AddCardsToPlayer(Player player, GameState state)
    {
        if (state.DeckOfCardsInPlay.Count < 1)
        {
            state = ReShuffle(state);
        }
        state.PlayerDecks[player.Name].Add(state.DeckOfCardsInPlay[^1]);
        state.DeckOfCardsInPlay.RemoveAt(state.DeckOfCardsInPlay.Count - 1);
        return state;
    }

    public GameState InitializeFullDeck(GameState state)
    {
        for (int cardSuite = 0; cardSuite <= (int)ECardSuite.Blue; cardSuite++)
        {
            for (int cardValue = 0; cardValue <= (int)ECardValue.Plus2; cardValue++)
            {
                for (int i = 0; i < state.AmountOfCardValues[(ECardValue)cardValue]; i++)
                {
                    state.DeckOfCardsInPlay.Add(new GameCard(cardSuite, cardValue));
                }
            }
        }
        for (int cardValue = (int)ECardValue.ChangeColor; cardValue <= (int)ECardValue.Draw4; cardValue++)
        {
            state.DeckOfCardsInPlay.Add(new GameCard(4, cardValue));
            state.DeckOfCardsInPlay.Add(new GameCard(4, cardValue));
            state.DeckOfCardsInPlay.Add(new GameCard(4, cardValue));
            state.DeckOfCardsInPlay.Add(new GameCard(4, cardValue));
        }

        state.DeckOfCardsInPlay = Shuffle(state.DeckOfCardsInPlay);
        return state;
    }

    public GameCard GetLastPlayedCard(GameState state)
    {
        return state.PlayedCards.Peek();
    }

    public List<T> Shuffle<T>(List<T> list)
    {
        var randomizedList = new List<T>();
        while (list.Count > 0)
        {
            var randomPositionInDeck = Rnd.Next(list.Count);
            randomizedList.Add(list[randomPositionInDeck]);
            list.RemoveAt(randomPositionInDeck);
        }

        return randomizedList;
    }

    public static bool CheckForUno(Player player, GameState state)
    {
        if (state.PlayerDecks[player.Name].Count == 1 && state.PlayerTypes[EPlayerType.Human] != 0)
        {
            return true;
        }
        return false;
    }

    public Player GetActivePlayer(GameState state)
    {
        return state.Players[state.OrderCount];
    }

    public GameState CheckDeUno(GameState state)
    {

        foreach (var player in state.Players)
        {
            if (state.PlayerDecks[player.Name].Count > 1)
            {
                if (state.UnoDict.ContainsKey(player.Name))
                {
                    if (state.UnoDict[player.Name])
                    {
                        state.UnoDict[player.Name] = false;
                    }
                    
                }
                else
                {
                    state.UnoDict[player.Name] = false;
                }
            }
        }
        
        return state;
    }
}
