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

        [Fact]
        public void PlayHand_AllPlayersCheck_CompletesSuccessfully( )
        {
            // Arrange
            var engine = new PokerEngine();
            engine.SmallBlind = 1;
            engine.BigBlind = 2;
            int startChips = 100;
            int playerCount = 3;

            var checkOnlyStrategy = new CheckOrCallAgent();
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
            engine.SmallBlind = 5;
            engine.BigBlind = 10;
            int startChips = 100;
            int playerCount = 3;

            var foldStrategy = new FoldAgent();
            // CheckOnlyStrategy is already defined in this file from a previous task.

            var p1 = new Player("P1_SB", startChips, playerCount, 0, foldStrategy); // SB
            var p2 = new Player("P2_BB", startChips, playerCount, 1, foldStrategy); // BB
            var p3 = new Player("P3_UTG", startChips, playerCount, 2, foldStrategy); // UTG

            engine.Players = new List<Player> { p1, p2, p3 };
            engine.ActivePlayerIndices = new List<int> { 0, 1, 2 };
            engine.SmallBlindPosition = 0; // P1
            engine.BigBlindPosition = 1;   // P2
            engine.CurrentPlayerTurn = 2;   // P3 UTG's turn initially
                                            // This is who the test THINKS is UTG initially based on p1=SB, p2=BB.
                                            // However, NextHand() will determine the actual SB, BB, and UTG for the hand it sets up.
                                            // The engine.SmallBlindPosition etc. are inputs to NextHand for where the blinds were *last hand*.

            // Let p1, p2, p3 be players at conceptual seats 0, 1, 2.
            // If engine.SmallBlindPosition is 0 (p1 was last SB), NextHand() will make:
            // p2 (seat 1) the new SB.
            // p3 (seat 2) the new BB.
            // p1 (seat 0) the new UTG.

            int initialP1Chips = p1.Chips; // 100
            int initialP2Chips = p2.Chips; // 100
            int initialP3Chips = p3.Chips; // 100

            // Act
            // engine.SmallBlindPosition = 0 means P1 was SB in the *previous* hand (or initial setup for the first hand).
            // PlayHand() will internally call NextHand().
            // NextHand() will advance blinds: P2 (idx 1) becomes SB, P3 (idx 2) becomes BB. P1 (idx 0) becomes UTG.
            // P1 chips: 100. P2 chips: 100-5=95. P3 chips: 100-10=90. Pot = 15. UTG is P1 (idx 0).
            // All players use FoldStrategy.
            // P1 (UTG) folds.
            // P2 (SB) folds.
            // P3 (BB) wins the pot.
            HandResult handResult = engine.PlayHand();

            // Assert state after PlayHand() which includes one NextHand() call.
            // Chip assertions are based on the single NextHand call inside PlayHand.
            // Initial chips were 100 for all.
            // P1 is UTG, pays no blind, folds: 100.
            // P2 is SB, pays 5, folds: 95.
            // P3 is BB, pays 10, wins pot (15): 90 + 15 = 105.

            // Assert
            Assert.NotNull(handResult);
            Assert.NotEmpty(handResult.Winners);
            Assert.Single(handResult.Winners); // Should be only one winner

            var winner = handResult.Winners[0];
            Assert.Equal(p3.Id, winner.Id); // P3 (new BB) should be the winner.

            // Final chip counts reflect the outcome of the hand.
            Assert.Equal(initialP1Chips, p1.Chips); // P1 was UTG, folded, no betting change.
            Assert.Equal(initialP2Chips - engine.SmallBlind, p2.Chips); // P2 was SB, paid 5, folded.
            Assert.Equal(initialP3Chips - engine.BigBlind + (engine.SmallBlind + engine.BigBlind), p3.Chips); // P3 was BB, paid 10, won pot (15).
        }
    }
}
