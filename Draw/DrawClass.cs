using System.Text;
using Domain;
using UNOEngine;

namespace Draw;

public class DrawClass
{
    private readonly GameEngine _engine = new();

    public string DrawPlayerStack(List<GameCard> deck, Player player, GameState state)
    {
        var builder = new StringBuilder();
        var validKeys = new List<ConsoleKey> { ConsoleKey.Enter ,ConsoleKey.L, ConsoleKey.X};
        
        builder.Append("\n\nL: PLAY ALL\n");
        builder.Append("X: CONTINUE\n");
        builder.Append($"\nPOSSIBLE PLAYS FOR {player}:");

        var (key, currentIndex) = GetUserInput(validKeys, builder.ToString(), state, deck);
        return key == ConsoleKey.L ? "l" : key == ConsoleKey.X ? "x" : currentIndex.ToString();
    }

    public string DrawPlayerDeck(GameState state, List<GameCard> deck, Player player)
    {
        var builder = new StringBuilder();
        var validKeys = new List<ConsoleKey> { ConsoleKey.Enter ,ConsoleKey.T,ConsoleKey.X};
        
        builder.Append("\n\nT: TAKE");
        builder.Append("\nX: RETURN TO MAIN MENU\n");
        builder.Append($"\nYOUR DECK {player}:");

        var (key, currentIndex) = GetUserInput(validKeys, builder.ToString(), state, deck);
        return key == ConsoleKey.T ? "t" : key == ConsoleKey.X ? "x" : currentIndex.ToString();
    }
    
    public GameState ChangeColors(GameCard card, GameState state)
    {
        var colorOptions = new List<string> { "RED", "GREEN", "BLUE", "YELLOW" };
        var validKeys = new List<ConsoleKey> { ConsoleKey.Enter };
        var builder = new StringBuilder();
        builder.Append("\n\nWHAT COLOR DO YOU WANT?\n");

        var (_, currentIndex) = GetUserInput(validKeys, builder.ToString(),
            state, null , colorOptions);

        card.CardSuite = colorOptions[currentIndex] switch
        {
            "RED" => ECardSuite.Red,
            "GREEN" => ECardSuite.Green,
            "BLUE" => ECardSuite.Blue,
            "YELLOW" => ECardSuite.Yellow,
            _ => card.CardSuite
        };
        state.PlayedCards.Push(card);

        return state;
    }

    private void DrawHeader(GameState state)
    {
        Console.WriteLine($"Game ID: {state.GameId}, use this to reopen this game once closed.");
        Console.WriteLine($"\nTurn: {state.Round}\n\nPlayers:\n\n{_engine.GetPlayerStackSizeString(state, false)}");
    }

    private (ConsoleKey, int) GetUserInput(
        List<ConsoleKey> validKeys,
        string description,
        GameState state,
        List<GameCard>? cardOptions = null,
        List<string>? options = null)
    {
        var key = ConsoleKey.A;
        var currentIndex = 0;

        while (!validKeys.Contains(key))
        {
            Console.Clear();
            DrawHeader(state);
            PrintCard(state.PlayedCards.Peek());
            Console.WriteLine(description);
            // show cards
            if (cardOptions != null)
            {
                for (int i = 0; i < cardOptions.Count; i++)
                {
                    if (i == currentIndex)
                    {
                        Console.Write("<");
                        PrintCard(cardOptions[i], 1);
                        Console.Write(">");
                    }
                    else
                    {
                        PrintCard(cardOptions[i], 1);
                    }
                    Console.Write("   ");
                }
            }
            // show options (colors)
            else if (options != null)
            {
                for (int i = 0; i < options.Count; i++)
                {
                    if (i == currentIndex)
                    {
                        Console.Write("<" + options[i] + ">");
                    }
                    else
                    {
                        Console.Write(options[i]);
                    }
                    Console.Write("   ");
                }
            }
            else
            {
                throw new Exception("GetUserInput() method require one of the input " +
                                    "options(CardOptions, options) to be non null.");
            }
            
            // show error
            if (state.Reason != null)
            {
                Console.WriteLine("\n");
                Console.ForegroundColor = ConsoleColor.DarkMagenta;
                Console.WriteLine(state.Reason);
                state.Reason = null;
                Console.ResetColor();
            }

            Console.WriteLine("\n");
            key = Console.ReadKey().Key;
            currentIndex = HandleArrowKeys(cardOptions, options, key, currentIndex);
        }

        return (key, currentIndex);
    }

    private static int HandleArrowKeys(List<GameCard>? cardOptions, List<string>? options,
        ConsoleKey key, int currentIndex)
    {
        if (options != null)
        {
            switch (key)
            {
                case ConsoleKey.LeftArrow:
                    currentIndex = currentIndex == 0 ? options.Count - 1 : currentIndex - 1;
                    break;
                case ConsoleKey.RightArrow:
                    currentIndex = currentIndex == options.Count - 1 ? 0 : currentIndex + 1;
                    break;
            }
        }
        else
        {
            switch (key)
            {
                case ConsoleKey.LeftArrow:
                    currentIndex = currentIndex == 0 ? cardOptions!.Count - 1 : currentIndex - 1;
                    break;
                case ConsoleKey.RightArrow:
                    currentIndex = currentIndex == cardOptions!.Count - 1 ? 0 : currentIndex + 1;
                    break;
            }
        }
        return currentIndex;
    }


    private void PrintCard(GameCard card, int? count = null)
    {
        var color = card.CardSuite;
        Console.ForegroundColor = color switch
        {
            ECardSuite.Blue => ConsoleColor.Blue,
            ECardSuite.Green => ConsoleColor.Green,
            ECardSuite.Red => ConsoleColor.Red,
            ECardSuite.Yellow => ConsoleColor.Yellow,
            ECardSuite.Wild => ConsoleColor.White,
            _ => Console.ForegroundColor
        };
        Console.Write(count == null ? $"Last Played Card: {card.AsCard()}" : card.AsCard());
        Console.ResetColor();
    }

    public void ShowPlayerPlaysCard(Player player, GameCard playedCard, GameState state)
    {
        Console.Clear();
        DrawHeader(state);
        Console.WriteLine($"\n\n{player} played:");
        PrintCard(playedCard);

        Console.WriteLine("\n");
        Console.WriteLine("X: RETURN TO MAIN MENU");
        Console.WriteLine("\n");
        
        DateTime startTime = DateTime.Now;
        ConsoleKey key = ConsoleKey.A;

        while ((DateTime.Now - startTime).TotalSeconds < 2)
        {
            Thread.Sleep(100);
            if (Console.KeyAvailable)
            {
                key = Console.ReadKey().Key;
                break;
            }
        }

        if (key == ConsoleKey.X)
        {
            throw new ApplicationException();
        }
        
    }
    
    public void ShowPlayerTakesCard(Player player, GameState state)
    {
        Console.Clear();
        DrawHeader(state);
        Console.WriteLine($"\n\n{player} took cards from the pile.");

        Console.WriteLine("\n");
        Console.WriteLine("X: RETURN TO MAIN MENU");
        Console.WriteLine("\n");
        
        DateTime startTime = DateTime.Now;
        ConsoleKey key = ConsoleKey.A;

        while ((DateTime.Now - startTime).TotalSeconds < 2)
        {
            Thread.Sleep(100);
            
            if (Console.KeyAvailable)
            {
                key = Console.ReadKey().Key;
                break;
            }
        }

        if (key == ConsoleKey.X)
        {
            throw new ApplicationException();
        }
    }

    public ConsoleKey DrawPlayerStartSignal(GameState state, Player player)
    {
        Console.Clear();
        DrawHeader(state);
        Console.WriteLine($"{player} PRESS ENTER TO START");
        return Console.ReadKey().Key;
    }

    public GameState ShowUno(Player player, GameState state)
    {
        var builder = new StringBuilder();
        var validKeys = new List<ConsoleKey> { ConsoleKey.Enter};
        builder.Append($"\nDid someone call out UNO for {player} before they could?\n");
        
        var (_, currentIndex) = GetUserInput(validKeys, builder.ToString(),
            state, null, new List<string> {"Yes", "No"});
        
        if (currentIndex == 0)
        {
            state = _engine.AddCardsToPlayer(player, state);
            state = _engine.AddCardsToPlayer(player, state);
        }
        return state;
    }

    public int ShowWinner(GameState state)
    {
        var builder = new StringBuilder();
        var validKeys = new List<ConsoleKey> { ConsoleKey.Enter};
        builder.Append("\n----------------------------------");
        builder.Append($"\nWinner is: {_engine.Finished(state)} with {state.Round} rounds!");
        builder.Append("\n----------------------------------\n");
        
        var (_, currentIndex) = GetUserInput(validKeys, builder.ToString(),
            state, null, new List<string> {"Continue"});
        
        return currentIndex;
    }
}
