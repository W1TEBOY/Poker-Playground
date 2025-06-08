using FluentAssertions;
using Poker.Core.Agents;
using Poker.Core.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        [Fact]
        public void PlayHand_AllPlayersCheck_CompletesSuccessfully( )
        {
            // Arrange
            var engine = new PokerEngine();
            engine.SmallBlind = 1;
            engine.BigBlind = 2;
            int startChips = 100;
            int playerCount = 3;

            var checkOnlyStrategy = new CheckOrCall();
            var players = new List<Player>();
            for ( int i = 0; i < playerCount; i++ )
            {
                players.Add(new Player($"Player {i + 1}", startChips, playerCount, i, checkOnlyStrategy));
            }
            engine.Players = players;

            // Manually set active players and blind positions as NextHand() would typically do.
            // The default PokerEngine constructor calls NextHand(), but that's for its initial set of players.
            // We've replaced them, so we need to ensure the engine's state reflects this.
            engine.ActivePlayerIndices = Enumerable.Range(0, playerCount).ToList();
            engine.SmallBlindPosition = 0; // Player 1
            engine.BigBlindPosition = 1;   // Player 2
            // CurrentPlayerTurn should be after BB, so Player 3 (index 2)
            engine.CurrentPlayerTurn = 2;

            // Call NextHand() to properly initialize the hand state (e.g., post blinds, deal cards)
            // This is crucial because PlayHand() assumes a hand is already in progress.
            engine.NextHand();

            HandResult handResult = null;
            var exception = Record.Exception(( ) => handResult = engine.PlayHand());

            // Assert
            Assert.Null(exception); // Ensure PlayHand completes without throwing an exception
            Assert.NotNull(handResult);
            Assert.NotEmpty(handResult.Winners);
        }

        [Fact]
        public void PlayHand_BlindsFold_ChipsAreCollectedAndAwarded( )
        {
            // Arrange
            var engine = new PokerEngine();
            int playerCount = 3;

            var foldStrategy = new Fold();

            var p1 = new Player("P1_SB", 1000, playerCount, 0, foldStrategy); // Seat 0
            var p2 = new Player("P2_BB", 1000, playerCount, 1, foldStrategy); // Seat 1
            var p3 = new Player("P3_UTG", 1000, playerCount, 2, foldStrategy); // Seat 2

            engine.Players = new List<Player> { p1, p2, p3 };
            engine.ActivePlayerIndices = new List<int> { 0, 1, 2 };
            engine.SmallBlindPosition = 0;
            engine.BigBlindPosition = 1;
            engine.CurrentPlayerTurn = 2;

            // Act
            HandResult handResult = engine.PlayHand();

            // Assert
            engine.SmallBlindPosition.Should().Be(1); // SB advanced to P2 -> P3
            engine.BigBlindPosition.Should().Be(2);   // BB advanced to P3 -> P1

            handResult.Should().NotBeNull();
            handResult.Winners.Should().NotBeEmpty();
            handResult.Winners.Count().Should().Be(1);

            var winner = handResult.Winners[0];
            p3.Id.Should().Be(engine.Players[engine.BigBlindPosition].Id); // P3 (was BB) should win
            p3.Id.Should().Be(winner.Id); // Winner is P3

            p1.Chips.Should().Be(1000); // UTG folded, no contribution
            p2.Chips.Should().Be(995);  // SB posted 5
            p3.Chips.Should().Be(1005); // BB posted 10, won 15
        }

        [Fact]
        public void FiftyEngines_FiftyHandsEach_MaintainsTotalChips( )
        {
            const int engineCount = 50;
            const int handsPerEngine = 50;

            Parallel.For(0, engineCount, _ =>
            {
                var engine = new PokerEngine();
                for ( int i = 0; i < handsPerEngine && engine.Players.Count > 1; i++ )
                {
                    engine.PlayHand();
                    if ( engine.ActivePlayerIndices.Count == 1 )
                    {
                        break;
                    }
                }

                var totalChips = engine.Players.Sum(p => p.Chips);
                totalChips.Should().Be(6000);
            });
        }

    }
}
