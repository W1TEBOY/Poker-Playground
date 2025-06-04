using FluentAssertions;
using Poker.Core.Interfaces;
using Poker.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Poker.Core.Tests.Agents
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
            foreach ( object[] reqObj in ActRequestFixtures.GetTenCases() )
            {
                var req = (ActRequest) reqObj[0];
                foreach ( var agentType in AgentTypes )
                    yield return new object[] { agentType, req };
            }
        }
    }

    [CollectionDefinition("Agents Tests")]
    public static class AgentTestsCollection { }

    /// <summary>
    /// Runs every concrete IPlayerStrategy against every ActRequest fixture.
    /// </summary>
    [Collection("Agents Tests")]
    public sealed class AllAgentsLegalTests
    {
        [Theory]
        [MemberData(nameof(AgentTestData.Cases), MemberType = typeof(AgentTestData))]
        public void Act_Should_Return_Legal_Move( Type agentType, ActRequest actRequest )
        {
            // ---------- arrange ----------
            var agent = (IPlayerStrategy) Activator.CreateInstance(agentType)!;

            // ---------- act ---------------
            PlayerAction action = agent.Act(actRequest);

            // ---------- assert ------------
            action.Should().NotBeNull();

            var play = action.Play;
            var amount = action.Amount ?? 0;

            switch ( play )
            {
                case PlayType.Fold:
                    break;  // always legal

                case PlayType.Check:
                    actRequest.ToCall.Should().Be(0,
                        $"{agentType.Name} attempted to CHECK while facing a bet.");
                    break;

                case PlayType.Call:
                    actRequest.YourStack.Should().BeGreaterThanOrEqualTo(actRequest.ToCall,
                        $"{agentType.Name} attempted to CALL without enough chips.");
                    break;

                case PlayType.Raise:
                    actRequest.YourStack.Should().BeGreaterThanOrEqualTo(amount,
                        $"{agentType.Name} attempted to RAISE more than its stack.");
                    amount.Should().BeGreaterThanOrEqualTo(
                        actRequest.ToCall + actRequest.MinRaise,
                        $"{agentType.Name} produced an under-size raise.");
                    break;

                case PlayType.AllIn:
                    // always permitted
                    break;

                default:
                    throw new InvalidOperationException(
                        $"Unknown PlayType {play} returned by {agentType.Name}.");
            }
        }
    }
}
