namespace Poker.Core.Models
{
    /// <summary>
    /// All the possible actions a player can take on their turn.
    /// </summary>
    public enum PlayType
    {
        Fold,
        Check,
        Call,
        Raise,
        AllIn
    }
}
