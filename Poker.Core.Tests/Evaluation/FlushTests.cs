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
    public class FlushTests
    {

        // 10 different 7‐card "High Card" hands (no pair, no straight, no flush, all values unique)
        public static IEnumerable<object[]> FlushSets =>
            new List<object[]>
            {
                new object[]
                {
                    new Card[]
                    {
                        new Card(Values.Two, Suits.Hearts),
                        new Card(Values.Five, Suits.Hearts),
                        new Card(Values.Seven, Suits.Hearts),
                        new Card(Values.Nine, Suits.Hearts),
                        new Card(Values.Jack, Suits.Hearts),
                        new Card(Values.King, Suits.Spades),
                        new Card(Values.Three, Suits.Clubs)
                    }
                },
                new object[]
                {
                    new Card[]
                    {
                        new Card(Values.Three, Suits.Spades),
                        new Card(Values.Six, Suits.Spades),
                        new Card(Values.Eight, Suits.Spades),
                        new Card(Values.Ten, Suits.Spades),
                        new Card(Values.Queen, Suits.Spades),
                        new Card(Values.Two, Suits.Hearts),
                        new Card(Values.Four, Suits.Diamonds)
                    }
                },
                new object[]
                {
                    new Card[]
                    {
                        new Card(Values.Four, Suits.Diamonds),
                        new Card(Values.Seven, Suits.Diamonds),
                        new Card(Values.Nine, Suits.Diamonds),
                        new Card(Values.King, Suits.Diamonds),
                        new Card(Values.Ace, Suits.Diamonds),
                        new Card(Values.Five, Suits.Clubs),
                        new Card(Values.Eight, Suits.Hearts)
                    }
                },
                new object[]
                {
                    new Card[]
                    {
                        new Card(Values.Five, Suits.Clubs),
                        new Card(Values.Eight, Suits.Clubs),
                        new Card(Values.Ten, Suits.Clubs),
                        new Card(Values.Jack, Suits.Clubs),
                        new Card(Values.Two, Suits.Clubs),
                        new Card(Values.Queen, Suits.Hearts),
                        new Card(Values.Three, Suits.Spades)
                    }
                },
                new object[]
                {
                    new Card[]
                    {
                        new Card(Values.Four, Suits.Hearts),
                        new Card(Values.Six, Suits.Hearts),
                        new Card(Values.Ten, Suits.Hearts),
                        new Card(Values.Queen, Suits.Hearts),
                        new Card(Values.King, Suits.Hearts),
                        new Card(Values.Three, Suits.Diamonds),
                        new Card(Values.Seven, Suits.Clubs)
                    }
                },
                new object[]
                {
                    new Card[]
                    {
                        new Card(Values.Two, Suits.Spades),
                        new Card(Values.Seven, Suits.Spades),
                        new Card(Values.Nine, Suits.Spades),
                        new Card(Values.Jack, Suits.Spades),
                        new Card(Values.Ace, Suits.Spades),
                        new Card(Values.Four, Suits.Hearts),
                        new Card(Values.Six, Suits.Diamonds)
                    }
                },
                new object[]
                {
                    new Card[]
                    {
                        new Card(Values.Three, Suits.Diamonds),
                        new Card(Values.Six, Suits.Diamonds),
                        new Card(Values.Eight, Suits.Diamonds),
                        new Card(Values.Jack, Suits.Diamonds),
                        new Card(Values.King, Suits.Diamonds),
                        new Card(Values.Two, Suits.Diamonds),
                        new Card(Values.Nine, Suits.Diamonds)
                    }
                },
                new object[]
                {
                    new Card[]
                    {
                        new Card(Values.Four, Suits.Clubs),
                        new Card(Values.Five, Suits.Clubs),
                        new Card(Values.Eight, Suits.Clubs),
                        new Card(Values.Queen, Suits.Clubs),
                        new Card(Values.Ace, Suits.Clubs),
                        new Card(Values.Seven, Suits.Clubs),
                        new Card(Values.Ten, Suits.Hearts)
                    }
                },
                new object[]
                {
                    new Card[]
                    {
                        new Card(Values.Three, Suits.Hearts),
                        new Card(Values.Five, Suits.Hearts),
                        new Card(Values.Eight, Suits.Hearts),
                        new Card(Values.Ten, Suits.Hearts),
                        new Card(Values.Ace, Suits.Hearts),
                        new Card(Values.Four, Suits.Clubs),
                        new Card(Values.Nine, Suits.Diamonds)
                    }
                },
                new object[]
                {
                    new Card[]
                    {
                        new Card(Values.Four, Suits.Spades),
                        new Card(Values.Six, Suits.Spades),
                        new Card(Values.Nine, Suits.Spades),
                        new Card(Values.Queen, Suits.Spades),
                        new Card(Values.King, Suits.Spades),
                        new Card(Values.Two, Suits.Diamonds),
                        new Card(Values.Seven, Suits.Hearts)
                    }
                }
            };


        [Theory]
        [MemberData(nameof(FlushSets))]
        public void EvaluateBestHand_ReturnsFlushes( Card[] sevenCards )
        {
            var hole = new List<Card> { sevenCards[0], sevenCards[1] };
            var board = sevenCards.Skip(2).Take(5).ToList();

            var flushSuit = sevenCards
                .GroupBy(c => c.Suit)
                .Where(g => g.Count() >= 5)
                .Select(g => g.Key)
                .Single();

            var bestFlushCards = sevenCards
                .Where(c => c.Suit == flushSuit)
                .OrderByDescending(c => c.Value)
                .Take(5)
                .ToList();

            HandValue baseResult = HandEvaluator.EvaluateBestHand(hole, board);

            baseResult.Rank.Should().Be(HandRank.Flush);
            baseResult.Cards.Should().HaveCount(5);
            baseResult.Kickers.Should().HaveCount(0);
            (baseResult.Cards.Count + baseResult.Kickers.Count).Should().Be(5);

            for ( int i = 0; i < 5; i++ )
            {
                baseResult.Cards[i].Value.Should().Be(bestFlushCards[i].Value);
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
        [MemberData(nameof(FlushSets))]
        public void EvaluateBestHand_Flush_PermutationsDontChangeResult( Card[] sevenCards )
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
