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
    public class FourOfAKindTests
    {

        // 10 different 7‐card "High Card" hands (no pair, no straight, no flush, all values unique)
        public static IEnumerable<object[]> FourOfAKindSets =>
            new List<object[]>
            {
                new object[]
                {
                    new Card[]
                    {
                        new Card(Values.Two, Suits.Clubs),
                        new Card(Values.Two, Suits.Diamonds),
                        new Card(Values.Two, Suits.Hearts),
                        new Card(Values.Two, Suits.Spades),
                        new Card(Values.Ace, Suits.Clubs),
                        new Card(Values.King, Suits.Diamonds),
                        new Card(Values.Nine, Suits.Spades)
                    }
                },
                new object[]
                {
                    new Card[]
                    {
                        new Card(Values.Three, Suits.Clubs),
                        new Card(Values.Three, Suits.Diamonds),
                        new Card(Values.Three, Suits.Hearts),
                        new Card(Values.Three, Suits.Spades),
                        new Card(Values.Ace, Suits.Spades),
                        new Card(Values.Queen, Suits.Hearts),
                        new Card(Values.Seven, Suits.Clubs)
                    }
                },
                new object[]
                {
                    new Card[]
                    {
                        new Card(Values.Four, Suits.Clubs),
                        new Card(Values.Four, Suits.Diamonds),
                        new Card(Values.Four, Suits.Hearts),
                        new Card(Values.Four, Suits.Spades),
                        new Card(Values.King, Suits.Hearts),
                        new Card(Values.Jack, Suits.Diamonds),
                        new Card(Values.Eight, Suits.Spades)
                    }
                },
                new object[]
                {
                    new Card[]
                    {
                        new Card(Values.Five, Suits.Clubs),
                        new Card(Values.Five, Suits.Diamonds),
                        new Card(Values.Five, Suits.Hearts),
                        new Card(Values.Five, Suits.Spades),
                        new Card(Values.Ace, Suits.Hearts),
                        new Card(Values.Ten, Suits.Clubs),
                        new Card(Values.Six, Suits.Diamonds)
                    }
                },
                new object[]
                {
                    new Card[]
                    {
                        new Card(Values.Six, Suits.Clubs),
                        new Card(Values.Six, Suits.Diamonds),
                        new Card(Values.Six, Suits.Hearts),
                        new Card(Values.Six, Suits.Spades),
                        new Card(Values.Queen, Suits.Spades),
                        new Card(Values.Nine, Suits.Diamonds),
                        new Card(Values.Three, Suits.Hearts)
                    }
                },
                new object[]
                {
                    new Card[]
                    {
                        new Card(Values.Seven, Suits.Clubs),
                        new Card(Values.Seven, Suits.Diamonds),
                        new Card(Values.Seven, Suits.Hearts),
                        new Card(Values.Seven, Suits.Spades),
                        new Card(Values.King, Suits.Spades),
                        new Card(Values.Jack, Suits.Clubs),
                        new Card(Values.Four, Suits.Diamonds)
                    }
                },
                new object[]
                {
                    new Card[]
                    {
                        new Card(Values.Eight, Suits.Clubs),
                        new Card(Values.Eight, Suits.Diamonds),
                        new Card(Values.Eight, Suits.Hearts),
                        new Card(Values.Eight, Suits.Spades),
                        new Card(Values.Ace, Suits.Diamonds),
                        new Card(Values.Ten, Suits.Hearts),
                        new Card(Values.Five, Suits.Spades)
                    }
                },
                new object[]
                {
                    new Card[]
                    {
                        new Card(Values.Nine, Suits.Clubs),
                        new Card(Values.Nine, Suits.Diamonds),
                        new Card(Values.Nine, Suits.Hearts),
                        new Card(Values.Nine, Suits.Spades),
                        new Card(Values.King, Suits.Clubs),
                        new Card(Values.Queen, Suits.Diamonds),
                        new Card(Values.Two, Suits.Hearts)
                    }
                },
                new object[]
                {
                    new Card[]
                    {
                        new Card(Values.Ten, Suits.Clubs),
                        new Card(Values.Ten, Suits.Diamonds),
                        new Card(Values.Ten, Suits.Hearts),
                        new Card(Values.Ten, Suits.Spades),
                        new Card(Values.Jack, Suits.Spades),
                        new Card(Values.Eight, Suits.Clubs),
                        new Card(Values.Three, Suits.Diamonds)
                    }
                },
                new object[]
                {
                    new Card[]
                    {
                        new Card(Values.Jack, Suits.Clubs),
                        new Card(Values.Jack, Suits.Diamonds),
                        new Card(Values.Jack, Suits.Hearts),
                        new Card(Values.Jack, Suits.Spades),
                        new Card(Values.Ace, Suits.Clubs),
                        new Card(Values.Nine, Suits.Spades),
                        new Card(Values.Six, Suits.Hearts)
                    }
                }
            };




        [Theory]
        [MemberData(nameof(FourOfAKindSets))]
        public void EvaluateBestHand_ReturnsFourOfAKind( Card[] sevenCards )
        {
            var hole = new List<Card> { sevenCards[0], sevenCards[1] };
            var board = sevenCards.Skip(2).Take(5).ToList();

            // ––– Instead of picking sevenCards.Max(), explicitly pick "highest card of the pair" –––
            var pairValue =
                sevenCards
                  .GroupBy(c => c.Value)
                  .Where(g => g.Count() == 4)  // each data set is a single pair
                  .Select(g => g.Key)
                  .Single();

            Card bestFourOfAKindCard =
                sevenCards
                  .Where(c => c.Value == pairValue)
                  .OrderByDescending(c => c)
                  .First();

            // Evaluate with your implementation
            HandValue baseResult = HandEvaluator.EvaluateBestHand(hole, board);

            baseResult.Rank.Should().Be(HandRank.FourOfAKind);
            baseResult.Cards.Should().HaveCount(4);

            // Now we’re checking that the first card of the pair is "bestFourOfAKindCard"
            baseResult.Cards[0].Value.Should().Be(bestFourOfAKindCard.Value);

            baseResult.Kickers.Should().HaveCount(1);
            (baseResult.Cards.Count + baseResult.Kickers.Count).Should().Be(5);

            // Finally, check all hole/board permutations yield the same HandValue
            foreach ( var setOfCards in TestDataHelper.AllHoleBoardCombinations(new[] { sevenCards }) )
            {
                var holeCards = (Card[]) setOfCards[0];
                var boardCards = (Card[]) setOfCards[1];

                var result = HandEvaluator.EvaluateBestHand(holeCards.ToList(), boardCards.ToList());
                result.Should().Be(baseResult);
            }
        }

        [Theory]
        [MemberData(nameof(FourOfAKindSets))]
        public void EvaluateBestHand_FourOfAKind_PermutationsDontChangeResult( Card[] sevenCards )
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
