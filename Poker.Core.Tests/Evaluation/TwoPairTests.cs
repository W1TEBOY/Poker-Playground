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
    public class TwoPairTests
    {

        public static IEnumerable<object[]> TwoPairSets =>
            new List<object[]>
            {
                new object[]
                {
                    new Card[]
                    {
                        new Card(Values.King, Suits.Hearts),
                        new Card(Values.King, Suits.Clubs),
                        new Card(Values.Ace, Suits.Diamonds),
                        new Card(Values.Ace, Suits.Spades),
                        new Card(Values.Seven, Suits.Spades),
                        new Card(Values.Four, Suits.Hearts),
                        new Card(Values.Nine, Suits.Diamonds)
                    }
                },
                new object[]
                {
                    new Card[]
                    {
                        new Card(Values.Ten, Suits.Diamonds),
                        new Card(Values.Ten, Suits.Spades),
                        new Card(Values.Three, Suits.Clubs),
                        new Card(Values.Three, Suits.Hearts),
                        new Card(Values.Six, Suits.Hearts),
                        new Card(Values.Queen, Suits.Diamonds),
                        new Card(Values.Eight, Suits.Spades)
                    }
                },
                new object[]
                {
                    new Card[]
                    {
                        new Card(Values.Ace, Suits.Clubs),
                        new Card(Values.Ace, Suits.Hearts),
                        new Card(Values.Two, Suits.Hearts),
                        new Card(Values.Two, Suits.Spades),
                        new Card(Values.Seven, Suits.Clubs),
                        new Card(Values.Nine, Suits.Hearts),
                        new Card(Values.Jack, Suits.Diamonds)
                    }
                },
                new object[]
                {
                    new Card[]
                    {
                        new Card(Values.Five, Suits.Spades),
                        new Card(Values.Five, Suits.Diamonds),
                        new Card(Values.Three, Suits.Spades),
                        new Card(Values.Three, Suits.Diamonds),
                        new Card(Values.Four, Suits.Clubs),
                        new Card(Values.Eight, Suits.Clubs),
                        new Card(Values.King, Suits.Hearts)
                    }
                },
                new object[]
                {
                    new Card[]
                    {
                        new Card(Values.Two, Suits.Clubs),
                        new Card(Values.Two, Suits.Diamonds),
                        new Card(Values.Nine, Suits.Clubs),
                        new Card(Values.Nine, Suits.Spades),
                        new Card(Values.Jack, Suits.Hearts),
                        new Card(Values.Queen, Suits.Spades),
                        new Card(Values.Six, Suits.Spades)
                    }
                },
                new object[]
                {
                    new Card[]
                    {
                        new Card(Values.Nine, Suits.Hearts),
                        new Card(Values.Nine, Suits.Spades),
                        new Card(Values.Four, Suits.Diamonds),
                        new Card(Values.Four, Suits.Hearts),
                        new Card(Values.Jack, Suits.Clubs),
                        new Card(Values.King, Suits.Diamonds),
                        new Card(Values.Three, Suits.Spades)
                    }
                },
                new object[]
                {
                    new Card[]
                    {
                        new Card(Values.Queen, Suits.Clubs),
                        new Card(Values.Queen, Suits.Hearts),
                        new Card(Values.Two, Suits.Diamonds),
                        new Card(Values.Two, Suits.Hearts),
                        new Card(Values.Five, Suits.Hearts),
                        new Card(Values.Eight, Suits.Diamonds),
                        new Card(Values.Jack, Suits.Spades)
                    }
                },
                new object[]
                {
                    new Card[]
                    {
                        new Card(Values.Six, Suits.Hearts),
                        new Card(Values.Six, Suits.Clubs),
                        new Card(Values.Two, Suits.Spades),
                        new Card(Values.Two, Suits.Diamonds),
                        new Card(Values.Four, Suits.Hearts),
                        new Card(Values.Nine, Suits.Diamonds),
                        new Card(Values.Jack, Suits.Spades)
                    }
                },
                new object[]
                {
                    new Card[]
                    {
                        new Card(Values.Jack, Suits.Diamonds),
                        new Card(Values.Jack, Suits.Spades),
                        new Card(Values.Three, Suits.Clubs),
                        new Card(Values.Three, Suits.Diamonds),
                        new Card(Values.Eight, Suits.Spades),
                        new Card(Values.King, Suits.Spades),
                        new Card(Values.Ace, Suits.Hearts)
                    }
                },
                new object[]
                {
                    new Card[]
                    {
                        new Card(Values.Three, Suits.Hearts),
                        new Card(Values.Three, Suits.Clubs),
                        new Card(Values.Four, Suits.Spades),
                        new Card(Values.Four, Suits.Diamonds),
                        new Card(Values.Nine, Suits.Clubs),
                        new Card(Values.Queen, Suits.Diamonds),
                        new Card(Values.Ace, Suits.Spades)
                    }
                }
            };


        [Theory]
        [MemberData(nameof(TwoPairSets))]
        public void EvaluateBestHand_ReturnsTwoPairs( Card[] sevenCards )
        {
            var hole = new List<Card> { sevenCards[0], sevenCards[1] };
            var board = sevenCards.Skip(2).Take(5).ToList();

            // ––– Identify the two pair values, highest first –––
            var pairValues =
                sevenCards
                  .GroupBy(c => c.Value)
                  .Where(g => g.Count() == 2)    // each data set contains exactly two distinct pairs
                  .Select(g => g.Key)
                  .OrderByDescending(v => v)
                  .ToArray();

            var highPairValue = pairValues[0];
            var lowPairValue = pairValues[1];

            // ––– Pick the top card of each pair (by suit) to verify ordering –––
            Card bestHighPairCard =
                sevenCards
                  .Where(c => c.Value == highPairValue)
                  .OrderByDescending(c => c)
                  .First();

            Card bestLowPairCard =
                sevenCards
                  .Where(c => c.Value == lowPairValue)
                  .OrderByDescending(c => c)
                  .First();

            // Evaluate with your implementation
            HandValue baseResult = HandEvaluator.EvaluateBestHand(hole, board);

            baseResult.Rank.Should().Be(HandRank.TwoPair);
            baseResult.Cards.Should().HaveCount(4);
            baseResult.Kickers.Should().HaveCount(1);
            (baseResult.Cards.Count + baseResult.Kickers.Count).Should().Be(5);

            // ––– Verify that the first two cards are the high pair, next two are the low pair –––
            baseResult.Cards[0].Value.Should().Be(bestHighPairCard.Value);
            baseResult.Cards[1].Value.Should().Be(bestHighPairCard.Value);
            baseResult.Cards[2].Value.Should().Be(bestLowPairCard.Value);
            baseResult.Cards[3].Value.Should().Be(bestLowPairCard.Value);
        }

        [Theory]
        [MemberData(nameof(TwoPairSets))]
        public void EvaluateBestHand_TwoPair_PermutationsDontChangeResult( Card[] sevenCards )
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
