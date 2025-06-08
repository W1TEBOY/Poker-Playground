using FluentAssertions;
using Poker.Core.Agents;
using Poker.Core.Interfaces;
using Poker.Core.Models;
using System;
using Xunit;

namespace Poker.Core.Tests.Models
{

    [CollectionDefinition("Player Tests")]
    public class PlayerTestsCollection
    {
    }

    [Collection("Player Tests")]
    public class PlayerTests
    {
        private readonly IPlayerStrategy _strategy = new CheckOrFold();
        private readonly string _name = "Test Player";
        private readonly int _startChips = 1000;
        private readonly Guid _id = Guid.NewGuid();
        private readonly int _playerCount = 6;
        private readonly int _currentPosition = 0;

        [Fact]
        public void Player_WithAllParams_ShouldHaveFullProperties( )
        {
            var player = new Player(_name, _startChips, _playerCount, _currentPosition, _strategy, _id);

            player.Name.Should().Be(_name);
            player.Chips.Should().Be(_startChips);
            player.Position.PlayerCount.Should().Be(_playerCount);
            player.Position.CurrentPosition.Should().Be(_currentPosition);
            player.Id.Should().Be(_id);

            player.Hand.Should().NotBeNull();
            player.Hand.Cards.Should().BeEmpty();
        }

        [Fact]
        public void Player_WithNullName_ShouldThrowArgumentNullException( )
        {
            Action act = ( ) => new Player(null, _startChips, _playerCount, _currentPosition, _strategy, _id);
            act.Should().Throw<ArgumentNullException>().WithMessage("Value cannot be null. (Parameter 'name')");
        }

        [Fact]
        public void Bet_WithValidBetSize_ShouldReduceChipsAndReturnBetAmount( )
        {
            var player = new Player(_name, _startChips, _playerCount, _currentPosition, _strategy, _id);
            int betSize = 100;

            int bet = player.Bet(betSize);

            bet.Should().Be(betSize);
            player.Chips.Should().Be(_startChips - betSize);
        }

        [Theory]
        [InlineData(12340)]
        [InlineData(5000)]
        [InlineData(1001)]
        public void Bet_WithBetSizeGreaterThanChips_ShouldReduceChipsToZeroAndReturnRemainingChips( int betSize )
        {
            var player = new Player(_name, _startChips, _playerCount, _currentPosition, _strategy, _id);

            int bet = player.Bet(betSize);

            bet.Should().Be(_startChips);
            player.Chips.Should().Be(0);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(100)]
        [InlineData(10000)]
        public void AddWinnings_WithPositivePotShare_ShouldIncreaseChipsByPotShare( int potShare )
        {
            var player = new Player(_name, _startChips, _playerCount, _currentPosition, _strategy, _id);

            player.AddWinnings(potShare);

            player.Chips.Should().Be(_startChips + potShare);
        }
    }
}