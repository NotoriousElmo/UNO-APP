using System.Text.Json;
using Domain;
using UNOEngine;

namespace DAL;

public class GameRepositoryFileSystem : IGameRepository
{
    private const string BasePath = @"C:\savedJSONGames";

    public void SaveGame(Guid id, GameState game)
    {
        var fileName = @"\" + id + ".json";
        File.WriteAllText(BasePath + fileName, 
            JsonSerializer.Serialize(game, JsonHelper.JsonSerializerOptions));
    }

    public GameState LoadGame(Guid id)
    {
        var filePath = BasePath + @"\" + id.ToString().Trim() + ".json";
        
        if (File.Exists(filePath)) {
            var state = JsonSerializer.Deserialize<GameState>(
                File.ReadAllText(filePath), JsonHelper.JsonSerializerOptions)!;
            
            var fixedStack = new Stack<GameCard>();
            foreach (var card in state.PlayedCards)
            {
                fixedStack.Push(card);
            }

            state.PlayedCards = fixedStack;
            return state;
        }
        throw new FileNotFoundException($"File doesn't exist {filePath}");

    }

    public void DeleteGame(Guid id)
    {
        var filePath = BasePath + @"\" + id.ToString().Trim() + ".json";
        File.Delete(filePath);
    }

    public List<(Guid, DateTime, DateTime)> GetSavedGames()
    {


    var fileList = Directory.EnumerateFiles(BasePath)
                    .Select(
                        path=> (
                            Guid.Parse(Path.GetFileNameWithoutExtension(path)),
                            File.GetCreationTime(path),
                            File.GetLastWriteTime(path)
                            )
                        ).ToList();

        return fileList;
    }
}