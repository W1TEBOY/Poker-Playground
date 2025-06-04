using Poker.Core.Models;
using System;
using Xunit;

namespace Poker.Core.Tests.Models
{

    [CollectionDefinition("Hand Tests")]
    public class HandTestsCollection
    {
    }

    [Collection("Hand Tests")]
    public class HandTests
    {

        Card card1 = new Card(Values.Ace, Suits.Hearts);
        Card card2 = new Card(Values.King, Suits.Spades);
        Card card3 = new Card(Values.Queen, Suits.Diamonds);


        [Fact]
        public void Add_WithTwoCards_ShouldHaveTwoCards( )
        {
            var hand = new Hand();
            hand.Add(card1);
            hand.Add(card2);
            Assert.Equal(2, hand.Cards.Count);
        }

        [Fact]
        public void Add_WithMoreThanTwoCards_ShouldThrowException( )
        {
            var hand = new Hand();
            hand.Add(card1);
            hand.Add(card2);
            Assert.Throws<InvalidOperationException>(( ) => hand.Add(card3));
        }

        [Fact]
        public void ResetCards_ShouldClearAllCards( )
        {
            var hand = new Hand();
            hand.Add(card1);
            hand.Add(card2);
            Assert.Equal(2, hand.Cards.Count);

            hand.ResetCards();
            Assert.Empty(hand.Cards);
        }

        [Fact]
        public void ToString_ShouldReturnCardDescriptions( )
        {
            var hand = new Hand();
            hand.Add(card1);
            hand.Add(card2);
            string expected = $"{card1}, {card2}";
            Assert.Equal(expected, hand.ToString());
        }
    }
}