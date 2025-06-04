using FluentAssertions;
using Poker.Core.Evaluation;
using Poker.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Poker.Core.Tests.Evaluation
{

    [Collection("Hand Evaluator Tests")]
    public class StraightFlushTests
    {

        // 10 different 7‐card "High Card" hands (no pair, no straight, no flush, all values unique)
        public static IEnumerable<object[]> StraightFlushSets =>
            new List<object[]>
            {
                new object[]
                {
                    new Card[]
                    {
                        new Card(Values.Five, Suits.Hearts),
                        new Card(Values.Six, Suits.Hearts),
                        new Card(Values.Seven, Suits.Hearts),
                        new Card(Values.Eight, Suits.Hearts),
                        new Card(Values.Nine, Suits.Hearts),
                        new Card(Values.Two, Suits.Hearts),
                        new Card(Values.Ace, Suits.Hearts)
                    }
                },
                new object[]
                {
                    new Card[]
                    {
                        new Card(Values.Ten, Suits.Spades),
                        new Card(Values.Jack, Suits.Spades),
                        new Card(Values.Queen, Suits.Spades),
                        new Card(Values.King, Suits.Spades),
                        new Card(Values.Nine, Suits.Spades),
                        new Card(Values.Three, Suits.Hearts),
                        new Card(Values.Four, Suits.Clubs)
                    }
                },
                new object[]
                {
                    new Card[]
                    {
                        new Card(Values.Ace, Suits.Diamonds),
                        new Card(Values.Two, Suits.Diamonds),
                        new Card(Values.Three, Suits.Diamonds),
                        new Card(Values.Four, Suits.Diamonds),
                        new Card(Values.Five, Suits.Diamonds),
                        new Card(Values.Six, Suits.Hearts),
                        new Card(Values.Seven, Suits.Clubs)
                    }
                },
                new object[]
                {
                    new Card[]
                    {
                        new Card(Values.Six, Suits.Clubs),
                        new Card(Values.Seven, Suits.Clubs),
                        new Card(Values.Eight, Suits.Clubs),
                        new Card(Values.Nine, Suits.Clubs),
                        new Card(Values.Ten, Suits.Clubs),
                        new Card(Values.Two, Suits.Hearts),
                        new Card(Values.Four, Suits.Spades)
                    }
                },
                new object[]
                {
                    new Card[]
                    {
                        new Card(Values.Three, Suits.Hearts),
                        new Card(Values.Four, Suits.Hearts),
                        new Card(Values.Five, Suits.Hearts),
                        new Card(Values.Six, Suits.Hearts),
                        new Card(Values.Seven, Suits.Hearts),
                        new Card(Values.King, Suits.Spades),
                        new Card(Values.Nine, Suits.Diamonds)
                    }
                },
                new object[]
                {
                    new Card[]
                    {
                        new Card(Values.Nine, Suits.Spades),
                        new Card(Values.Ten, Suits.Spades),
                        new Card(Values.Jack, Suits.Spades),
                        new Card(Values.Queen, Suits.Spades),
                        new Card(Values.King, Suits.Spades),
                        new Card(Values.Three, Suits.Clubs),
                        new Card(Values.Four, Suits.Hearts)
                    }
                },
                new object[]
                {
                    new Card[]
                    {
                        new Card(Values.Four, Suits.Diamonds),
                        new Card(Values.Five, Suits.Diamonds),
                        new Card(Values.Six, Suits.Diamonds),
                        new Card(Values.Seven, Suits.Diamonds),
                        new Card(Values.Eight, Suits.Diamonds),
                        new Card(Values.Ace, Suits.Hearts),
                        new Card(Values.Ten, Suits.Clubs)
                    }
                },
                new object[]
                {
                    new Card[]
                    {
                        new Card(Values.Seven, Suits.Clubs),
                        new Card(Values.Eight, Suits.Clubs),
                        new Card(Values.Nine, Suits.Clubs),
                        new Card(Values.Ten, Suits.Clubs),
                        new Card(Values.Jack, Suits.Clubs),
                        new Card(Values.Two, Suits.Diamonds),
                        new Card(Values.Three, Suits.Spades)
                    }
                },
                new object[]
                {
                    new Card[]
                    {
                        new Card(Values.Eight, Suits.Hearts),
                        new Card(Values.Nine, Suits.Hearts),
                        new Card(Values.Ten, Suits.Hearts),
                        new Card(Values.Jack, Suits.Hearts),
                        new Card(Values.Queen, Suits.Hearts),
                        new Card(Values.Four, Suits.Clubs),
                        new Card(Values.Six, Suits.Spades)
                    }
                },
                new object[]
                {
                    new Card[]
                    {
                        new Card(Values.Two, Suits.Spades),
                        new Card(Values.Three, Suits.Spades),
                        new Card(Values.Four, Suits.Spades),
                        new Card(Values.Five, Suits.Spades),
                        new Card(Values.Six, Suits.Spades),
                        new Card(Values.King, Suits.Diamonds),
                        new Card(Values.Ten, Suits.Hearts)
                    }
                }
            };



        [Theory]
        [MemberData(nameof(StraightFlushSets))]
        public void EvaluateBestHand_ReturnsStraightFlushes( Card[] sevenCards )
        {
            var hole = new List<Card> { sevenCards[0], sevenCards[1] };
            var board = sevenCards.Skip(2).Take(5).ToList();

            var flushSuit = sevenCards
                .GroupBy(c => c.Suit)
                .Where(g => g.Count() >= 5)
                .Select(g => g.Key)
                .Single();

            var flushCards = sevenCards
                .Where(c => c.Suit == flushSuit)
                .ToList();

            var distinctValues = flushCards
                .Select(c => (int) c.Value)
                .Distinct()
                .OrderBy(v => v)
                .ToList();

            int straightHigh = 0;
            for ( int i = 0; i <= distinctValues.Count - 5; i++ )
            {
                var window = distinctValues.Skip(i).Take(5).ToList();
                if ( window[4] - window[0] == 4 )
                {
                    straightHigh = window[4];
                }
            }

            if ( straightHigh == 0 &&
                distinctValues.Contains((int) Values.Ace) &&
                distinctValues.Contains(2) &&
                distinctValues.Contains(3) &&
                distinctValues.Contains(4) &&
                distinctValues.Contains(5) )
            {
                straightHigh = 5;
            }

            List<int> straightValues;
            if ( straightHigh == 5 &&
                distinctValues.Contains((int) Values.Ace) &&
                distinctValues.Contains(2) &&
                distinctValues.Contains(3) &&
                distinctValues.Contains(4) &&
                distinctValues.Contains(5) )
            {
                straightValues = new List<int> { 5, 4, 3, 2, 14 };
            }
            else
            {
                straightValues = Enumerable.Range(straightHigh - 4, 5)
                    .Reverse()
                    .ToList();
            }

            Func<Card, int> sortKey = c =>
            {
                if ( straightHigh == 5 && c.Value == Values.Ace )
                    return 1;
                return (int) c.Value;
            };

            var bestStraightFlushCards = straightValues
                .Select(v => flushCards
                    .Where(c => (int) c.Value == v)
                    .OrderByDescending(c => c)
                    .First())
                .OrderByDescending(sortKey)
                .ToList();

            HandValue baseResult = HandEvaluator.EvaluateBestHand(hole, board);

            baseResult.Rank.Should().Be(HandRank.StraightFlush);
            baseResult.Cards.Should().HaveCount(5);
            baseResult.Kickers.Should().HaveCount(0);
            (baseResult.Cards.Count + baseResult.Kickers.Count).Should().Be(5);

            for ( int i = 0; i < 5; i++ )
            {
                baseResult.Cards[i].Value.Should().Be(bestStraightFlushCards[i].Value);
                baseResult.Cards[i].Suit.Should().Be(flushSuit);
            }

            foreach ( var setOfCards in TestDataHelper.AllHoleBoardCombinations(new[] { sevenCards }) )
            {
                var holeCards = (Card[]) setOfCards[0];
                var boardCards = (Card[]) setOfCards[1];

                var result = HandEvaluator.EvaluateBestHand(holeCards.ToList(), boardCards.ToList());
                result.Should().Be(baseResult);
            }
        }

        [Theory]
        [MemberData(nameof(StraightFlushSets))]
        public void EvaluateBestHand_StraightFlush_PermutationsDontChangeResult( Card[] sevenCards )
        {
            var hole = new List<Card> { sevenCards[0], sevenCards[1] };
            var board = sevenCards.Skip(2).Take(5).ToList();
            var baseResult = HandEvaluator.EvaluateBestHand(hole, board);

            var allSplits = TestDataHelper
                .AllHoleBoardCombinations(new[] { sevenCards })
                .ToList(); // materialize before parallelizing

            try
            {
                Parallel.ForEach(allSplits, setOfCards =>
                {
                    var holeCards = ((Card[]) setOfCards[0]).ToList();
                    var boardCards = ((Card[]) setOfCards[1]).ToList();
                    var result = HandEvaluator.EvaluateBestHand(holeCards, boardCards);

                    result.Should().Be(baseResult);
                });
            }
            catch ( AggregateException ae )
            {
                // If any assertion failed inside Parallel.ForEach, rethrow the first inner exception
                throw ae.Flatten().InnerExceptions[0];
            }
        }

    }
}
