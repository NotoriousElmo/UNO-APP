using Domain;

namespace UNOEngine
{
    public class Player
    {
        public string Name { get; set; } = default!;
        public EPlayerType PlayerType { get; set; }
        public Guid Id { get; set; } = Guid.NewGuid();
        
        public override string ToString()
        {
            return $"{Name} ({PlayerType})";
        }
    }
}