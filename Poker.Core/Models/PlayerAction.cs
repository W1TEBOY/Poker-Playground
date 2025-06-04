namespace Poker.Core.Models
{
    public class PlayerAction
    {
        public PlayType Play { get; set; }
        public int? Amount { get; set; }

        public PlayerAction( PlayType play, int? amount = null )
        {
            Play = play;
            Amount = amount;
        }
    }
}
