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
    public class RoyalFlushTests
    {

        // 10 different 7‐card "High Card" hands (no pair, no straight, no flush, all values unique)
        public static IEnumerable<object[]> RoyalFlushSets =>
            new List<object[]>
            {
                new object[]
                {
                    new Card[]
                    {
                        new Card(Values.Ten, Suits.Hearts),
                        new Card(Values.Jack, Suits.Hearts),
                        new Card(Values.Queen, Suits.Hearts),
                        new Card(Values.King, Suits.Hearts),
                        new Card(Values.Ace, Suits.Hearts),
                        new Card(Values.Nine, Suits.Hearts),
                        new Card(Values.Three, Suits.Diamonds)
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
                        new Card(Values.Ace, Suits.Spades),
                        new Card(Values.Nine, Suits.Hearts),
                        new Card(Values.Five, Suits.Clubs)
                    }
                },
                new object[]
                {
                    new Card[]
                    {
                        new Card(Values.Ten, Suits.Diamonds),
                        new Card(Values.Jack, Suits.Diamonds),
                        new Card(Values.Queen, Suits.Diamonds),
                        new Card(Values.King, Suits.Diamonds),
                        new Card(Values.Ace, Suits.Diamonds),
                        new Card(Values.Six, Suits.Spades),
                        new Card(Values.Seven, Suits.Hearts)
                    }
                },
                new object[]
                {
                    new Card[]
                    {
                        new Card(Values.Ten, Suits.Clubs),
                        new Card(Values.Jack, Suits.Clubs),
                        new Card(Values.Queen, Suits.Clubs),
                        new Card(Values.King, Suits.Clubs),
                        new Card(Values.Ace, Suits.Clubs),
                        new Card(Values.Eight, Suits.Diamonds),
                        new Card(Values.Nine, Suits.Hearts)
                    }
                }
            };


        [Theory]
        [MemberData(nameof(RoyalFlushSets))]
        public void EvaluateBestHand_ReturnsRoyalFlushes( Card[] sevenCards )
        {
            var hole = new List<Card> { sevenCards[0], sevenCards[1] };
            var board = sevenCards.Skip(2).Take(5).ToList();

            var flushSuit = sevenCards
                .GroupBy(c => c.Suit)
                .Where(g => g.Count() >= 5)
                .Select(g => g.Key)
                .Single();

            var neededValues = new[] { Values.Ace, Values.King, Values.Queen, Values.Jack, Values.Ten };
            var bestRoyalFlushCards = neededValues
                .Select(v => sevenCards
                    .Single(c => c.Suit == flushSuit && c.Value == v))
                .OrderByDescending(c => c.Value)
                .ToList();

            HandValue baseResult = HandEvaluator.EvaluateBestHand(hole, board);

            baseResult.Rank.Should().Be(HandRank.RoyalFlush);
            baseResult.Cards.Should().HaveCount(5);
            baseResult.Kickers.Should().HaveCount(0);
            (baseResult.Cards.Count + baseResult.Kickers.Count).Should().Be(5);

            for ( int i = 0; i < 5; i++ )
            {
                baseResult.Cards[i].Value.Should().Be(bestRoyalFlushCards[i].Value);
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
        [MemberData(nameof(RoyalFlushSets))]
        public void EvaluateBestHand_RoyalFlush_PermutationsDontChangeResult( Card[] sevenCards )
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
