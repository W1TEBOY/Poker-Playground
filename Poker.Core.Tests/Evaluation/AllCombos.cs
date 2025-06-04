using Poker.Core.Models;
using System.Collections.Generic;
using System.Linq;

namespace Poker.Core.Tests.Evaluation
{
    /// <summary>
    /// Given an IEnumerable of 7-card arrays, produce every possible
    /// “hole (2 cards) + board (remaining 5 cards)” split.
    /// </summary>
    public static class TestDataHelper
    {
        /// <summary>
        /// Input: a sequence of Card[7], each of which is “one 7-card hand.”
        /// Output: for each Card[7], yield all (i,j) pairs i<j as:
        ///   object[]{ Card[2] hole, Card[5] board }.
        /// </summary>
        public static IEnumerable<object[]> AllHoleBoardCombinations( IEnumerable<Card[]> sevenCardSets )
        {
            foreach ( var seven in sevenCardSets )
            {
                // There are 7 choose 2 = 21 ways to pick two distinct indices (i,j)
                for ( int i = 0; i < seven.Length; i++ )
                {
                    for ( int j = i + 1; j < seven.Length; j++ )
                    {
                        // hole = the two chosen cards
                        var hole = new[] { seven[i], seven[j] };

                        // board = the other five cards (all indices ≠ i, j)
                        var board = seven
                            .Where(( _, idx ) => idx != i && idx != j)
                            .ToArray(); // length == 5

                        yield return new object[] { hole, board };
                    }
                }
            }
        }
    }
}
