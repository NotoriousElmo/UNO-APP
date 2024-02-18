namespace Domain.Database;

public class Game : BaseEntity
{
    public DateTime CreatedTime { get; set; }
    public DateTime UpdatedTime { get; set; }

    public string State { get; set; } = default!;
    public ICollection<Player>? Players { get; set; }
}