using Poker.Core.Evaluation;
using System.Collections.Generic;

namespace Poker.Core.Models
{
    /// <summary>
    /// The outcome of one complete hand:
    ///  - the final board cards,
    ///  - each player's evaluated best 5‐card hand,
    ///  - and the list of winner(s).
    /// </summary>
    public record HandResult(
        IReadOnlyList<Card> Board,
        IReadOnlyList<(Player Player, HandValue Value)> ShowdownHands,
        IReadOnlyList<Player> Winners
    );
}
