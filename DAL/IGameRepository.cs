using Domain;
namespace DAL;

public interface IGameRepository
{
    void SaveGame(Guid id, GameState game);
    GameState LoadGame(Guid id);
    List<(Guid id, DateTime ct, DateTime dt)> GetSavedGames();
    void DeleteGame(Guid id);
}