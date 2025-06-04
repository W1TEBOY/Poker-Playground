using FluentAssertions;
using Poker.Core.Evaluation;
using Poker.Core.Models;
using System.Collections.Generic;
using Xunit;

namespace Poker.Core.Tests.Models
{
    [CollectionDefinition("HandValue Tests")]
    public class HandValueTestsCollection
    {
    }

    [Collection("HandValue Tests")]
    public class HandValueTests
    {
        private static HandValue CreateHighCard( IEnumerable<Card> cards )
        {
            // High card: HandRank.HighCard, cards list is the five highest cards, kickers empty
            var cardList = new List<Card>(cards);
            cardList.Sort(( a, b ) =>
            {
                int cmp = b.Value.CompareTo(a.Value);
                return cmp != 0 ? cmp : a.Suit.CompareTo(b.Suit);
            });
            var top5 = cardList.GetRange(0, 5);
            return new HandValue(HandRank.HighCard, new List<Card>(top5), new List<Card>());
        }

        private static HandValue CreatePair( Values pairValue, IEnumerable<Card> kickersSource )
        {
            // Pair: HandRank.Pair, two cards of pairValue, kickers are the three highest other cards
            var pairCards = new List<Card>
            {
                new Card(pairValue, Suits.Clubs),
                new Card(pairValue, Suits.Diamonds)
            };
            var kickers = new List<Card>(kickersSource);
            kickers.Sort(( a, b ) =>
            {
                int cmp = b.Value.CompareTo(a.Value);
                return cmp != 0 ? cmp : a.Suit.CompareTo(b.Suit);
            });
            var top3 = kickers.GetRange(0, 3);
            return new HandValue(HandRank.Pair, pairCards, new List<Card>(top3));
        }

        [Fact]
        public void CompareTo_DifferentRanks_ShouldOrderByRank( )
        {
            // Arrange: a HighCard vs a Pair
            var highCard = CreateHighCard(new[]
            {
                new Card(Values.Ace, Suits.Spades),
                new Card(Values.King, Suits.Hearts),
                new Card(Values.Queen, Suits.Diamonds),
                new Card(Values.Jack, Suits.Clubs),
                new Card(Values.Ten, Suits.Spades),
                new Card(Values.Nine, Suits.Hearts),
                new Card(Values.Eight, Suits.Diamonds)
            });

            var pair = CreatePair(
                Values.Five,
                new[]
                {
                    new Card(Values.Ace, Suits.Hearts),
                    new Card(Values.King, Suits.Diamonds),
                    new Card(Values.Queen, Suits.Clubs),
                    new Card(Values.Jack, Suits.Spades),
                    new Card(Values.Ten, Suits.Hearts)
                });

            // Act & Assert
            // HighCard rank is lower than Pair rank, so CompareTo should be negative
            (highCard < pair).Should().BeTrue();
            (pair > highCard).Should().BeTrue();
            highCard.CompareTo(pair).Should().BeNegative();
            pair.CompareTo(highCard).Should().BePositive();
        }

        [Fact]
        public void CompareTo_SameRank_DifferentCardValues_ShouldOrderByCardList( )
        {
            // Arrange: two Pair hands with different pair values
            var pairLow = CreatePair(
                Values.Four,
                new[]
                {
                    new Card(Values.Ace, Suits.Hearts),
                    new Card(Values.King, Suits.Diamonds),
                    new Card(Values.Queen, Suits.Clubs),
                    new Card(Values.Jack, Suits.Spades),
                    new Card(Values.Ten, Suits.Hearts)
                });

            var pairHigh = CreatePair(
                Values.Six,
                new[]
                {
                    new Card(Values.Ace, Suits.Spades),
                    new Card(Values.King, Suits.Spades),
                    new Card(Values.Queen, Suits.Spades),
                    new Card(Values.Jack, Suits.Spades),
                    new Card(Values.Ten, Suits.Spades)
                });

            // Act & Assert
            (pairLow < pairHigh).Should().BeTrue();
            (pairHigh > pairLow).Should().BeTrue();
            pairLow.CompareTo(pairHigh).Should().BeNegative();
            pairHigh.CompareTo(pairLow).Should().BePositive();
        }

        [Fact]
        public void CompareTo_SameRank_SamePairValue_DifferentKickers_ShouldOrderByKickers( )
        {
            // Arrange: two Pair hands both with Pair of Sevens, but different kickers
            var kickersA = new[]
            {
                new Card(Values.Ace, Suits.Hearts),
                new Card(Values.King, Suits.Diamonds),
                new Card(Values.Queen, Suits.Clubs),
                new Card(Values.Jack, Suits.Spades),
                new Card(Values.Ten, Suits.Hearts)
            };

            var kickersB = new[]
            {
                new Card(Values.Ace, Suits.Spades),
                new Card(Values.King, Suits.Spades),
                new Card(Values.Queen, Suits.Spades),
                new Card(Values.Jack, Suits.Spades),
                new Card(Values.Nine, Suits.Hearts)
            };

            var pairA = CreatePair(Values.Seven, kickersA);
            var pairB = CreatePair(Values.Seven, kickersB);

            // Act & Assert
            // Compare kickers: Queen of Clubs vs Queen of Spades -> Clubs (lower suit) < Spades (higher suit)
            (pairA < pairB).Should().BeTrue();
            (pairB > pairA).Should().BeTrue();
            pairA.CompareTo(pairB).Should().BeNegative();
            pairB.CompareTo(pairA).Should().BePositive();
        }

        [Fact]
        public void Equals_And_OperatorEquals_ShouldBeTrue_ForIdenticalHands( )
        {
            // Arrange: two separate HandValue instances with identical cards and kickers
            var cards = new List<Card>
            {
                new Card(Values.Ten, Suits.Hearts),
                new Card(Values.Ten, Suits.Spades)
            };
            var kickers = new List<Card>
            {
                new Card(Values.Ace, Suits.Diamonds),
                new Card(Values.King, Suits.Clubs),
                new Card(Values.Queen, Suits.Diamonds)
            };

            var hand1 = new HandValue(HandRank.Pair, new List<Card>(cards), new List<Card>(kickers));
            var hand2 = new HandValue(HandRank.Pair, new List<Card>(cards), new List<Card>(kickers));

            // Act & Assert
            hand1.Equals(hand2).Should().BeTrue();
            (hand1 == hand2).Should().BeTrue();
            hand1.CompareTo(hand2).Should().Be(0);
            hand1.GetHashCode().Should().Be(hand2.GetHashCode());
        }

        [Fact]
        public void NotEquals_OperatorNotEquals_ShouldBeTrue_ForDifferentHands( )
        {
            // Arrange: two hands that differ by one kicker
            var cards = new List<Card>
            {
                new Card(Values.Ten, Suits.Hearts),
                new Card(Values.Ten, Suits.Spades)
            };
            var kickersA = new List<Card>
            {
                new Card(Values.Ace, Suits.Diamonds),
                new Card(Values.King, Suits.Clubs),
                new Card(Values.Queen, Suits.Diamonds)
            };
            var kickersB = new List<Card>
            {
                new Card(Values.Ace, Suits.Diamonds),
                new Card(Values.King, Suits.Clubs),
                new Card(Values.Jack, Suits.Diamonds)
            };

            var handA = new HandValue(HandRank.Pair, new List<Card>(cards), new List<Card>(kickersA));
            var handB = new HandValue(HandRank.Pair, new List<Card>(cards), new List<Card>(kickersB));

            // Act & Assert
            handA.Equals(handB).Should().BeFalse();
            (handA != handB).Should().BeTrue();
            handA.CompareTo(handB).Should().NotBe(0);
            handA.GetHashCode().Should().NotBe(handB.GetHashCode());
        }

        [Fact]
        public void Sort_FullHouse_ShouldOrderTripsThenPair( )
        {
            // Arrange: unsorted FullHouse cards
            var cards = new List<Card>
            {
                new Card(Values.Four, Suits.Spades),
                new Card(Values.Four, Suits.Clubs),
                new Card(Values.Three, Suits.Diamonds),
                new Card(Values.Three, Suits.Hearts),
                new Card(Values.Three, Suits.Clubs)
            };
            var kickers = new List<Card>(); // FullHouse has no kickers by design

            var fullHouse = new HandValue(HandRank.FullHouse, new List<Card>(cards), new List<Card>(kickers));

            // Act
            fullHouse.Sort();

            // Assert: after sorting, the three-of-a-kind (Threes) come first, ordered by suit (Clubs, Diamonds, Hearts),
            // then the pair (Fours) ordered by suit (Clubs, Spades)
            fullHouse.Cards.Should().Equal(
                new Card(Values.Three, Suits.Hearts),
                new Card(Values.Three, Suits.Clubs),
                new Card(Values.Three, Suits.Diamonds),
                new Card(Values.Four, Suits.Spades),
                new Card(Values.Four, Suits.Clubs)
            );
        }

        [Fact]
        public void Sort_StraightWheel_ShouldOrderAsFiveThroughAceLow( )
        {
            // Arrange: A-2-3-4-5 of mixed suits
            var cards = new List<Card>
            {
                new Card(Values.Ace, Suits.Hearts),
                new Card(Values.Two, Suits.Clubs),
                new Card(Values.Three, Suits.Diamonds),
                new Card(Values.Four, Suits.Spades),
                new Card(Values.Five, Suits.Hearts)
            };
            var kickers = new List<Card>();

            var wheelStraight = new HandValue(HandRank.Straight, new List<Card>(cards), new List<Card>(kickers));

            // Act
            wheelStraight.Sort();

            // Assert: should be sorted as 5,4,3,2,A (suit order descending within same value)
            wheelStraight.Cards.Should().Equal(
                new Card(Values.Five, Suits.Hearts),
                new Card(Values.Four, Suits.Spades),
                new Card(Values.Three, Suits.Diamonds),
                new Card(Values.Two, Suits.Clubs),
                new Card(Values.Ace, Suits.Hearts)
            );
        }

        [Fact]
        public void Sort_StraightNormal_ShouldOrderDescendingByValueThenSuit( )
        {
            // Arrange: 8-7-6-5-4 of mixed suits
            var cards = new List<Card>
            {
                new Card(Values.Six, Suits.Hearts),
                new Card(Values.Eight, Suits.Clubs),
                new Card(Values.Four, Suits.Spades),
                new Card(Values.Seven, Suits.Diamonds),
                new Card(Values.Five, Suits.Hearts)
            };
            var kickers = new List<Card>();

            var straight = new HandValue(HandRank.Straight, new List<Card>(cards), new List<Card>(kickers));

            // Act
            straight.Sort();

            // Assert: descending by value: 8,7,6,5,4; within same value by suit
            straight.Cards.Should().Equal(
                new Card(Values.Eight, Suits.Clubs),
                new Card(Values.Seven, Suits.Diamonds),
                new Card(Values.Six, Suits.Hearts),
                new Card(Values.Five, Suits.Hearts),
                new Card(Values.Four, Suits.Spades)
            );
        }

        [Fact]
        public void Sort_Kickers_ShouldOrderDescendingByValueThenSuit( )
        {
            // Arrange: arbitrary hand with unsorted kickers
            var cards = new List<Card>
            {
                new Card(Values.Ten, Suits.Hearts),
                new Card(Values.Ten, Suits.Spades)
            };
            var kickers = new List<Card>
            {
                new Card(Values.Two, Suits.Hearts),
                new Card(Values.Ace, Suits.Diamonds),
                new Card(Values.King, Suits.Clubs)
            };

            var hand = new HandValue(HandRank.Pair, new List<Card>(cards), new List<Card>(kickers));

            // Act
            hand.Sort();

            // Assert: kickers sorted as Ace of Diamonds, King of Clubs, Two of Hearts
            hand.Kickers.Should().Equal(
                new Card(Values.Ace, Suits.Diamonds),
                new Card(Values.King, Suits.Clubs),
                new Card(Values.Two, Suits.Hearts)
            );
        }

        [Fact]
        public void CompareTo_Null_ShouldReturnPositive( )
        {
            // Arrange
            var someHand = CreateHighCard(new[]
            {
                new Card(Values.King, Suits.Spades),
                new Card(Values.Queen, Suits.Hearts),
                new Card(Values.Jack, Suits.Diamonds),
                new Card(Values.Ten, Suits.Clubs),
                new Card(Values.Nine, Suits.Spades),
                new Card(Values.Eight, Suits.Hearts),
                new Card(Values.Seven, Suits.Diamonds)
            });

            // Act & Assert
            someHand.CompareTo(null).Should().BePositive();
            (someHand > null).Should().BeTrue();
            (null < someHand).Should().BeTrue();
            (null == someHand).Should().BeFalse();
            (null != someHand).Should().BeTrue();
        }
    }
}
