using ConsoleApp;
using DAL;
using Domain;
using MenuSystem;
using Microsoft.EntityFrameworkCore;
using UNOEngine;

int playerCount;
var handSize = 7;
var skipAfterDraw = true;
var canStackCards = true;
var canPlayDraw = false;
List<Player> setPlayers = new(); 
List<Player> players = new();

Dictionary<ECardValue, int> amountOfCardValue = new()
{
    [ECardValue.Draw4] = 4,
    [ECardValue.ChangeColor] = 4,
    [ECardValue.Plus2] = 2,
    [ECardValue.Reverse] = 2,
    [ECardValue.Skip] = 2, 
    [ECardValue.Value0] = 1,
    [ECardValue.Value1] = 2,
    [ECardValue.Value2] = 2,
    [ECardValue.Value3] = 2,
    [ECardValue.Value4] = 2,
    [ECardValue.Value5] = 2,
    [ECardValue.Value6] = 2,
    [ECardValue.Value7] = 2,
    [ECardValue.Value8] = 2,
    [ECardValue.Value9] = 2,
};

Dictionary<EPlayerType, int> playerTypes = new ()
{ 
    [EPlayerType.Human] = 1,
    [EPlayerType.Bot] = 1
};

var connectionString = "DataSource=<%temppath%>uno.db;Cache=Shared";
connectionString = connectionString.Replace("<%temppath%>", Path.GetTempPath());

var contextOptions = new DbContextOptionsBuilder<AppDbContext>()
    .UseSqlite(connectionString)
    .EnableDetailedErrors()
    .EnableSensitiveDataLogging()
    .Options;
using var db = new AppDbContext(contextOptions);
db.Database.Migrate();
  // var gameRepository = new GameRepositoryFileSystem();
var gameRepository = new GameRepositoryEF(db);
var engine = new GameEngine();
RunGame();

string? RunGame()
{
    var mainMenu = new Menu("UNO", new List<MenuItem>()
    {   new MenuItem()
        {
            MenuLabel = "Play Game",
            MethodToRun = Start
        }, 
        new MenuItem()
        {
            MenuLabel = "Options",
            MethodToRun = SetupNewGame
        },
        new MenuItem()
        {
            MenuLabel = "Load Game",
            MethodToRun = LoadGame
        },
        new MenuItem()
        {
            MenuLabel = "Delete Game",
            MethodToRun = DeleteGame
        }
    }, new List<MenuItem>()
    {
        new MenuItem()
        {
            MenuLabel = "Exit",
            MethodToRun = () =>
            {
                Environment.Exit(0);
                return null;
            }
        },
    });
    return mainMenu.Run();
}

string? SetupNewGame()
{
    var mainMenu = new Menu("Options", new List<MenuItem>()
    {
        new MenuItem()
        {
            MenuLabel = "Set Player Count and Types",
            MethodToRun = SetPlayerCount
        },
        new MenuItem()
        {
            MenuLabel = $"Set hand size: {handSize}",
            MethodToRun = SetHandSize
        },
        new MenuItem()
        {
            MenuLabel = "Set custom rules",
            MethodToRun = SetCustomRules
        },
        new MenuItem()
        {
            MenuLabel = "Set player names",
            MethodToRun = SetPlayerNames
        },
        new MenuItem()
        {
            MenuLabel = "Set the number of each card",
            MethodToRun = SetCardNumbers
        }
    }, new List<MenuItem>()
    {
        new MenuItem()
        {
            MenuLabel = "Back",
            MethodToRun = RunGame
        },
        new MenuItem()
        {
            MenuLabel = "Exit",
            MethodToRun = () =>
            {
                Environment.Exit(0);
                return null;
            }
        }
    });
    return mainMenu.Run();
}

string? SetCustomRules()
{
    var customRulesMenu = new Menu("Custom Rules", new List<MenuItem>()
    {   new MenuItem()
        {
            MenuLabel = $"Can stack cards when playing: {canStackCards}",
            MethodToRun = ChangeStackCards
        }, 
        new MenuItem()
        {
            MenuLabel = $"Skip next player after playing plus 2 or draw 4: {skipAfterDraw}",
            MethodToRun = ChangeSkipAfterDraw
        }, 
        new MenuItem()
        {
            MenuLabel = $"Allow to play draw 4 without restrictions: {canPlayDraw}",
            MethodToRun = ChangeCanPlayDraw
        }
    }, new List<MenuItem>()
    {
        new MenuItem()
        {
            MenuLabel = "Back",
            MethodToRun = SetupNewGame
        },
        new MenuItem()
        {
            MenuLabel = "Exit",
            MethodToRun = () =>
            {
                Environment.Exit(0);
                return null;
            }
        }
    });
    return customRulesMenu.Run();
}
 
string? Start()
{
    players = engine!.InitializePlayerNamesAndTypes(setPlayers, playerTypes);
    var game = new ConsoleGame<string>(gameRepository, playerTypes, handSize, skipAfterDraw, canStackCards, players, 
        amountOfCardValue, canPlayDraw);
    game.PlayGame();
    return RunGame(); 
}

string? ChangeStackCards()
{
    var userChoice = ConsoleKey.A;
    while (userChoice != ConsoleKey.Y && userChoice != ConsoleKey.N)
    {
        Console.Clear();
        Console.WriteLine("Should stacking of cards be allowed (Y/N):");
        userChoice = Console.ReadKey().Key;
    }
    canStackCards = userChoice == ConsoleKey.Y;
    return SetCustomRules();
}

string? ChangeSkipAfterDraw()
{
    var userChoice = ConsoleKey.A;
    while (userChoice != ConsoleKey.Y && userChoice != ConsoleKey.N)
    {
        Console.Clear();
        Console.WriteLine("Should the next player be skipped with plus 2 and draw 4 (Y/N):");
        userChoice = Console.ReadKey().Key;
    }
    skipAfterDraw = userChoice == ConsoleKey.Y;
    return SetCustomRules();
}
 
string? ChangeCanPlayDraw()
{
    var userChoice = ConsoleKey.A;
    while (userChoice != ConsoleKey.Y && userChoice != ConsoleKey.N)
    {
        Console.Clear();
        Console.WriteLine("Should draw 4 be playable without restrictions? (Y/N):");
        userChoice = Console.ReadKey().Key;
    }
    canPlayDraw = userChoice == ConsoleKey.Y;
    return SetCustomRules();
}

string? SetPlayerCount()
{
    string? userChoice;
    do
    {
        Console.Clear();
        Console.WriteLine("Type Player Count (2-10): ");
        userChoice = Console.ReadLine()?.Trim();
    } while (!int.TryParse(userChoice, out _) || 10 < int.Parse(userChoice) || int.Parse(userChoice) < 2);
    playerCount = int.Parse(userChoice);
    return SetPlayerTypes();
}

string? SetPlayerTypes()
{
    string? userChoice;
    do
    {
        Console.Clear();
        Console.WriteLine("Current Total Players: " + playerCount);
        Console.WriteLine("Human Player Count: ");
        userChoice = Console.ReadLine()?.Trim();
    } while (!int.TryParse(userChoice, out _) ||
             int.Parse(userChoice) < 0 || playerCount - int.Parse(userChoice) < 0);

    var intChoice = int.Parse(userChoice);
    playerTypes[EPlayerType.Human] = intChoice;
    playerTypes[EPlayerType.Bot] = playerCount - intChoice;
    
    return SetupNewGame();
}

string? SetHandSize()
{
    string? userChoice;
    do
    {
        Console.Clear();
        Console.WriteLine("Current hand size: " + handSize);
        Console.WriteLine("Type desired hand size (4 - 10): ");
        userChoice = Console.ReadLine()?.Trim();
    } while (!int.TryParse(userChoice, out _) ||
             int.Parse(userChoice) < 4 || int.Parse(userChoice) > 10);

    var intChoice = int.Parse(userChoice);
    handSize = intChoice;
    
    return SetupNewGame();
}

void WriteSavedGames(List<(Guid, DateTime, DateTime)> fileList, int currentIndex)
{
    for (int i = 0; i < fileList.Count; i++)
    {
        if (i == currentIndex)
        {
            Console.WriteLine("<" + fileList[i].Item1 + "   Created" + fileList[i].Item2 + "  Last modified: " + fileList[i].Item3 + ">");
        }
        else
        {
            Console.WriteLine(fileList[i].Item1 + "   Created" + fileList[i].Item2 + "  Last modified: " + fileList[i].Item3);
        }
    }
}

string? LoadGame()
{
    var fileList = gameRepository!.GetSavedGames();
    if (fileList.Count > 0)
    {
        ConsoleKeyInfo key;
        var currentIndex = 0;
        Console.Clear();
        Console.WriteLine("SAVED GAMES");
        Console.WriteLine();
        WriteSavedGames(fileList, currentIndex);
        Console.WriteLine();
        Console.WriteLine("Press X to return.");

        while ((key = Console.ReadKey()).Key != ConsoleKey.Enter && key.Key != ConsoleKey.X)
        {
            switch (key.Key)
            {
                case ConsoleKey.UpArrow:
                    currentIndex = currentIndex == 0 ? fileList.Count - 1 : currentIndex - 1;
                    break;
                case ConsoleKey.DownArrow:
                    currentIndex = currentIndex == fileList.Count - 1 ? 0 : currentIndex + 1;
                    break;
            }
            Console.Clear();
            Console.WriteLine("SAVED GAMES");
            Console.WriteLine();
            WriteSavedGames(fileList, currentIndex);
            Console.WriteLine();
            Console.WriteLine("Press X to return.");
        }
        if (key.Key == ConsoleKey.X)
        {
            return RunGame();
        }
        try {
            var savedGame = fileList[currentIndex];
            var loadState = gameRepository.LoadGame(savedGame.Item1);
            var game = new ConsoleGame<string>(gameRepository, playerTypes,
                    loadState.HandSize, loadState.SkipAfterDraw,
                    loadState.CanStackCards, loadState.Players, amountOfCardValue, canPlayDraw, loadState);
            game.PlayGame();
            return RunGame();
        } catch (FileNotFoundException) {
            return RunGame();
        }
    }
    Console.Clear();
    Console.ForegroundColor = ConsoleColor.DarkMagenta;
    Console.WriteLine("Did not find any saved games.");
    
    var startTime = DateTime.Now;
    while ((DateTime.Now - startTime).TotalSeconds < 1)
    {
        Thread.Sleep(100);
    }
    Console.ResetColor();
    return RunGame();
}
 
string? DeleteGame()
{
    var fileList = gameRepository!.GetSavedGames();
    if (fileList.Count > 0)
    {
        ConsoleKeyInfo key;
        var currentIndex = 0;
        Console.Clear();
        Console.WriteLine("SAVED GAMES");
        Console.WriteLine();
        WriteSavedGames(fileList, currentIndex);
        Console.WriteLine();
        Console.WriteLine("Press X to return.");

        while ((key = Console.ReadKey()).Key != ConsoleKey.Enter && key.Key != ConsoleKey.X)
        { 
            switch (key.Key)
            {
                case ConsoleKey.UpArrow:
                    currentIndex = currentIndex == 0 ? fileList.Count - 1 : currentIndex - 1;
                    break;
                case ConsoleKey.DownArrow:
                    currentIndex = currentIndex == fileList.Count - 1 ? 0 : currentIndex + 1;
                    break;
            }
            Console.Clear();
            Console.WriteLine("SAVED GAMES");
            Console.WriteLine();
            WriteSavedGames(fileList, currentIndex);
            Console.WriteLine();
            Console.WriteLine("Press X to return.");
            
        }

        if (key.Key == ConsoleKey.X)
        { 
            return RunGame();
        }
        try { 
            var savedGame = fileList[currentIndex];
            gameRepository.DeleteGame(savedGame.Item1);
            return DeleteGame();
        } catch (FileNotFoundException) { 
            return RunGame();
        }
    }
    return RunGame();
}
 
string? SetPlayerNames()
{

    players = engine!.InitializePlayerNamesAndTypes(setPlayers, playerTypes);

    List<Player> settablePlayers = new();

    foreach (var player in players)
    {
        if (player.PlayerType.Equals(EPlayerType.Human))
        {
            settablePlayers.Add(player);
        }
    }


    var currentIndex = 0;
    ConsoleKeyInfo key;
     
    Console.Clear();
    Console.WriteLine("CHANGE PLAYER NAMES");
    Console.WriteLine();
    WriteSettablePlayers(settablePlayers, currentIndex);
    Console.WriteLine();
    Console.WriteLine("Press X to return.");
     
    while ((key = Console.ReadKey()).Key != ConsoleKey.Enter && key.Key != ConsoleKey.X)
    {
        switch (key.Key)
        {
            case ConsoleKey.UpArrow:
                currentIndex = currentIndex == 0 ? settablePlayers.Count - 1 : currentIndex - 1;
                break;
            case ConsoleKey.DownArrow:
                currentIndex = currentIndex == settablePlayers.Count - 1 ? 0 : currentIndex + 1;
                break;
        }
        Console.Clear();
        Console.WriteLine("CHANGE PLAYER NAMES");
        Console.WriteLine();
        WriteSettablePlayers(settablePlayers, currentIndex);
        Console.WriteLine();
        Console.WriteLine("Press X to return.");

    }
    if (key.Key == ConsoleKey.X)
    {
        return SetupNewGame();
    }
    ChangePlayerName(settablePlayers, currentIndex);

    return RunGame();
}

string? ChangePlayerName(List<Player> settablePlayers, int playerIndex)
{
     
    var userChoice = "";

    while (userChoice.Length < 1)
    {
         
        Console.Clear();
        Console.WriteLine("CHANGE PLAYER NAME");
        Console.WriteLine();
        userChoice = Console.ReadLine()!.Trim();
    }

    if (setPlayers.Contains(settablePlayers[playerIndex]))
    {
        var index = setPlayers.IndexOf(settablePlayers[playerIndex]);
        setPlayers[index].Name = userChoice;
    }
    else
    {
        var newPlayer = new Player()
        {
            Name = userChoice,
            PlayerType = EPlayerType.Human
        };
        setPlayers.Add(newPlayer);
    }

    return SetPlayerNames();
}

void WriteSettablePlayers(List<Player> settablePlayers, int currentIndex)
{
    for (var count = 0; count < settablePlayers.Count; count++)
    {
        if (currentIndex == count)
        {
            Console.WriteLine($"<{settablePlayers[count]}>");
        }
        else
        {
            Console.WriteLine(settablePlayers[count]);
        }
    }
}

void WriteCards(int index)
{
    foreach (var cardVal in amountOfCardValue.Keys)
    {
        if (amountOfCardValue.Keys.ElementAtOrDefault(index).Equals(cardVal))
        {
            Console.WriteLine(cardVal + ": " + amountOfCardValue[cardVal] + " <-");
        }
        else
        {
            Console.WriteLine(cardVal + ": " + amountOfCardValue[cardVal]);
        }
    }
}

string? ChangeNumberOfCards(int index)
{

    var key = amountOfCardValue.Keys.ElementAtOrDefault(index);
     
    string? userChoice;
    do
    {
        Console.Clear();
        Console.WriteLine("Current amount of " + key + ": " + amountOfCardValue[key]);
        Console.WriteLine("Type desired amount of these cards (0 - 10): ");
        userChoice = Console.ReadLine()?.Trim();
    } while (!int.TryParse(userChoice, out _) ||
             int.Parse(userChoice) < 0 || int.Parse(userChoice) > 10);

    var intChoice = int.Parse(userChoice);
    amountOfCardValue[key] = intChoice;
    
    return SetCardNumbers();
}

string? SetCardNumbers()
{
    var currentIndex = 0;
    ConsoleKeyInfo key;
     
    Console.Clear();
    Console.WriteLine("CHANGE NUMBER OF CARDS");
    Console.WriteLine();
    WriteCards(currentIndex);
    Console.WriteLine();
    Console.WriteLine("Press X to return.");
     
    while ((key = Console.ReadKey()).Key != ConsoleKey.Enter && key.Key != ConsoleKey.X)
    {
        switch (key.Key)
        {
            case ConsoleKey.UpArrow:
                currentIndex = currentIndex == 0 ? amountOfCardValue.Count - 1 : currentIndex - 1;
                break;
            case ConsoleKey.DownArrow:
                currentIndex = currentIndex == amountOfCardValue.Count - 1 ? 0 : currentIndex + 1;
                break;
        }
        Console.Clear();
        Console.WriteLine("CHANGE PLAYER NAMES");
        Console.WriteLine();
        WriteCards(currentIndex);
        Console.WriteLine();
        Console.WriteLine("Press X to return.");

    }
    if (key.Key == ConsoleKey.X)
    {
        return SetupNewGame();
    }

    return ChangeNumberOfCards(currentIndex);
}
 

