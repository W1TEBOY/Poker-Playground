namespace Poker.Core.Models
{
    public class Position
    {
        public int PlayerCount
        {
            get; set;
        }
        public int CurrentPosition
        {
            get; set;
        }

        public Position( int player_count, int current_position )
        {
            PlayerCount = player_count;
            CurrentPosition = current_position;
        }
    }
}