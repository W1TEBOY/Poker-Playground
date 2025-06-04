using Poker.Core.Agents;
using Poker.Core.Models;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Poker.Core.Tests
{

    [Collection("Poker Engine Tests")]
    public class PokerEngineTests
    {

        [Fact]
        public void NextHand_WithPlayersWithoutChips_GetCut( )
        {
            // Arrange: build a “fake” engine with 3 players, one of whom is already busted.
            var engine = new PokerEngine();

            // Manually replace engine.Players so we have deterministic seats:
            var p1 = new Player("P1", startChips: 100, playerCount: 3, currentPosition: 0, new RandomAgent());
            var p2 = new Player("P2", startChips: 0, playerCount: 3, currentPosition: 1, new RandomAgent());  // busted
            var p3 = new Player("P3", startChips: 100, playerCount: 3, currentPosition: 2, new RandomAgent());

            engine.Players = new List<Player> { p1, p2, p3 };
            // Since one player is busted already, NextHand should remove p2 from the table.
            engine.SmallBlindPosition = 0;
            engine.BigBlindPosition = 1;  // note: sits on “p2,” but p2 has 0 chips.

            // Act:
            var (losers, _) = engine.NextHand();

            // Assert #1: losers list contains exactly p2.Id
            Assert.Single(losers);
            Assert.Contains(p2.Id, losers);

            // Assert #2: engine.Players no longer contains p2
            Assert.DoesNotContain(engine.Players, p => p.Id == p2.Id);
            // And now there should only be 2 players left
            Assert.Equal(2, engine.Players.Count);
        }

        [Fact]
        public void NextHand_MaintainsTotalChips( )
        {
            // Arrange: build a “fake” engine with 3 players, each starting with 100 chips
            var engine = new PokerEngine();
            engine.SmallBlind = 1;
            engine.BigBlind = 2;

            // Manually replace engine.Players so we have deterministic seats:
            var p1 = new Player("P1", startChips: 100, playerCount: 3, currentPosition: 0, new RandomAgent());
            var p2 = new Player("P2", startChips: 100, playerCount: 3, currentPosition: 1, new RandomAgent());
            var p3 = new Player("P3", startChips: 100, playerCount: 3, currentPosition: 2, new RandomAgent());

            engine.Players = new List<Player> { p1, p2, p3 };

            // Calculate total “bankroll” before NextHand: sum of all player chips + pot
            var totalBefore = engine.Players.Sum(p => p.Chips) + engine.CurrentPot;

            // Ensure blinds are pointing at valid seats (they’ll be deducted in NextHand)
            engine.SmallBlindPosition = 0;
            engine.BigBlindPosition = 1;

            // Act: start a new hand
            var (_, _) = engine.NextHand();

            // Assert: total chips (players’ stacks + pot) remains unchanged
            var totalAfter = engine.Players.Sum(p => p.Chips) + engine.CurrentPot;
            Assert.Equal(totalBefore, totalAfter);
        }

        [Fact]
        public void NextHand_BlindPositionsWrapAround( )
        {
            var engine = new PokerEngine();
            engine.SmallBlind = 1;
            engine.BigBlind = 2;

            var p1 = new Player("P1", startChips: 100, playerCount: 3, currentPosition: 0, new RandomAgent());
            var p2 = new Player("P2", startChips: 100, playerCount: 3, currentPosition: 1, new RandomAgent());
            var p3 = new Player("P3", startChips: 100, playerCount: 3, currentPosition: 2, new RandomAgent());

            engine.Players = new List<Player> { p1, p2, p3 };

            engine.SmallBlindPosition = 1;
            engine.BigBlindPosition = 2;

            engine.NextHand();

            Assert.Equal(2, engine.SmallBlindPosition);
            Assert.Equal(0, engine.BigBlindPosition);
        }

    }
}
