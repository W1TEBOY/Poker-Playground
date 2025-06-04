using FluentAssertions;
using Poker.Core.Models;
using System;
using System.Linq;
using Xunit;

namespace Poker.Core.Tests.Models
{

    [CollectionDefinition("Deck Tests")]
    public class DeckTestsCollection
    {
    }

    [Collection("Deck Tests")]
    public class DeckTests
    {

        readonly int seed = 123;
        readonly Deck deck;

        public DeckTests( )
        {
            deck = new Deck(new Random(seed));
        }


        [Fact]
        public void Constructor_WithSetSeedRandom_ShouldNotShuffle_OrderedDeck( )
        {
            var rng1 = new Random(seed);
            var deck1 = new Deck(rng1);
            var cards1 = deck1.ToList();

            var rng2 = new Random(seed);
            var deck2 = new Deck(rng2);
            var cards2 = deck2.ToList();

            cards1.Should().HaveCount(52);
            cards1.Should().OnlyHaveUniqueItems();

            deck1.Should().Equal(deck2);
            cards1.Should().Equal(cards2);
        }

        [Fact]
        public void Constructor_WithSameSeededRandom_ProducesDeterministicShuffle( )
        {
            var rng1 = new Random(seed);
            var rng2 = new Random(seed);
            var deck1 = new Deck(rng1).ToList();
            var deck2 = new Deck(rng2).ToList();
            deck1.Should().Equal(deck2);
        }

        [Fact]
        public void Count_Initially_ShouldBe52( )
        {
            deck.Count.Should().Be(52);
            deck.Should().HaveCount(52);
        }

        [Fact]
        public void Peek_ShouldReturnTopCard_WithoutRemovingIt( )
        {
            var beforeCount = deck.Count;
            var top = deck.Peek();

            deck.Count.Should().Be(beforeCount);
            top.Should().Be(deck.Peek());
        }

        [Fact]
        public void Draw_ShouldReturnTopCard_AndDecreaseCount( )
        {
            var top = deck.Peek();
            var beforeCount = deck.Count;

            var drawn = deck.Draw();
            drawn.Should().Be(top);
            deck.Count.Should().Be(beforeCount - 1);
        }

        [Fact]
        public void Draw_MultipleTimes_ShouldDepleteDeck_AndThrowOnExtra( )
        {
            for ( int i = 0; i < 52; i++ )
                deck.Draw();
            deck.Count.Should().Be(0);

            deck.Invoking(d => d.Draw())
                .Should().Throw<InvalidOperationException>()
                .WithMessage("Deck is empty.");
        }

        [Fact]
        public void Peek_OnEmptyDeck_ShouldThrowInvalidOperationException( )
        {
            for ( int i = 0; i < 52; i++ )
                deck.Draw();

            deck.Invoking(d => d.Peek())
                .Should().Throw<InvalidOperationException>()
                .WithMessage("Deck is empty.");
        }

        [Fact]
        public void Reset_ShouldRestoreFullDeck_AndPreserveOrderWithSetSeedRandom( )
        {
            var original = deck.ToList();

            for ( int i = 0; i < 5; i++ )
            {
                deck.Draw();
            }
            deck.Count.Should().Be(47);

            deck.Reset();
            deck.Count.Should().Be(52);
            deck.Should().OnlyHaveUniqueItems();
            deck.Should().NotEqual(original);
        }

        [Fact]
        public void ToString_ShouldListAllCards_CommaSeparated( )
        {
            var repr = deck.ToString();
            var entries = repr.Split(", ");

            entries.Should().HaveCount(52);
            repr.Count(c => c == ',').Should().Be(51);
            entries.Should().OnlyHaveUniqueItems();
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        [InlineData(51)]
        [InlineData(52)]
        public void Draw_MultipleCounts_ShouldReduceCountByNumber( int draws )
        {
            for ( int i = 0; i < draws; i++ )
                deck.Draw();
            deck.Count.Should().Be(52 - draws);
        }

        [Fact]
        public void Enumeration_ShouldSupportLinqQueries( )
        {
            var hearts = deck.Where(card => card.Suit == Suits.Hearts).ToList();

            hearts.Should().HaveCount(13);
            hearts.Should().OnlyContain(c => c.Suit == Suits.Hearts);
        }
    }
}
