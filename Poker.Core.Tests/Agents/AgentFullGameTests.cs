using FluentAssertions;
using Poker.Core.Interfaces;
using Poker.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Poker.Core.Tests.Agents
{

    [Collection("Poker Engine Tests")]
    public class AgentFullGameTests
    {

        internal static class AgentTestData
        {
            private static readonly Type[] AgentTypes =
                typeof(IPlayerStrategy).Assembly            // assembly that contains your agents
                    .GetTypes()
                    .Where(t => typeof(IPlayerStrategy).IsAssignableFrom(t)  // implements the interface
                             && t.IsClass
                             && !t.IsAbstract)
                    .ToArray();

            public static IEnumerable<object[]> Cases( )
            {
                foreach ( var agentType in AgentTypes )
                    yield return new object[] { agentType };
            }
        }

        [Theory]
        [MemberData(nameof(AgentTestData.Cases), MemberType = typeof(AgentTestData))]
        public void NextHand_WithPlayersWithoutChips_GetCut( Type agentType )
        {
            // Arrange: build a “fake” engine with 3 players, one of whom is already busted.
            var engine = new PokerEngine();

            var agent = (IPlayerStrategy) Activator.CreateInstance(agentType)!;

            // Manually replace engine.Players so we have deterministic seats:
            var p1 = new Player("P1", startChips: 100, playerCount: 3, currentPosition: 0, agent);
            var p2 = new Player("P2", startChips: 0, playerCount: 3, currentPosition: 1, agent);
            var p3 = new Player("P3", startChips: 100, playerCount: 3, currentPosition: 2, agent);

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

        [Theory]
        [MemberData(nameof(AgentTestData.Cases), MemberType = typeof(AgentTestData))]
        public void NextHand_MaintainsTotalChips( Type agentType )
        {
            // Arrange: build a “fake” engine with 3 players, each starting with 100 chips
            var engine = new PokerEngine();
            engine.SmallBlind = 1;
            engine.BigBlind = 2;

            var agent = (IPlayerStrategy) Activator.CreateInstance(agentType)!;

            // Manually replace engine.Players so we have deterministic seats:
            var p1 = new Player("P1", startChips: 100, playerCount: 3, currentPosition: 0, agent);
            var p2 = new Player("P2", startChips: 100, playerCount: 3, currentPosition: 1, agent);
            var p3 = new Player("P3", startChips: 100, playerCount: 3, currentPosition: 2, agent);

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

        [Theory]
        [MemberData(nameof(AgentTestData.Cases), MemberType = typeof(AgentTestData))]
        public void NextHand_BlindPositionsWrapAround( Type agentType )
        {
            var engine = new PokerEngine();
            engine.SmallBlind = 1;
            engine.BigBlind = 2;

            var agent = (IPlayerStrategy) Activator.CreateInstance(agentType)!;

            var p1 = new Player("P1", startChips: 100, playerCount: 3, currentPosition: 0, agent);
            var p2 = new Player("P2", startChips: 100, playerCount: 3, currentPosition: 1, agent);
            var p3 = new Player("P3", startChips: 100, playerCount: 3, currentPosition: 2, agent);

            engine.Players = new List<Player> { p1, p2, p3 };

            engine.SmallBlindPosition = 1;
            engine.BigBlindPosition = 2;

            engine.NextHand();

            Assert.Equal(2, engine.SmallBlindPosition);
            Assert.Equal(0, engine.BigBlindPosition);
        }



        [Theory]
        [MemberData(nameof(AgentTestData.Cases), MemberType = typeof(AgentTestData))]
        public void FiftyEngines_FiftyHandsEach_MaintainsTotalChips( Type agentType )
        {
            const int engineCount = 50;
            const int handsPerEngine = 50;

            var agent = (IPlayerStrategy) Activator.CreateInstance(agentType)!;

            Parallel.For(0, engineCount, _ =>
            {
                var engine = new PokerEngine();
                foreach ( Player p in engine.Players )
                {
                    p._strategy = agent;
                }

                for ( int i = 0; i < handsPerEngine && engine.Players.Count > 1; i++ )
                {
                    engine.PlayHand();
                    if ( engine.ActivePlayerIndices.Count == 1 )
                    {
                        break;
                    }
                }

                var totalChips = engine.Players.Sum(p => p.Chips);
                totalChips.Should().Be(6000, $"Agent Type: {agentType}");
            });
        }

    }
}
