using Poker.Core.Agents;
using Poker.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Poker.Core.Tests
{
    [Collection("Poker Engine Tests")]
    public class PokerEngineAdditionalTests
    {
        private static PokerEngine CreateEngine( )
        {
            var engine = new PokerEngine
            {
                SmallBlind = 1,
                BigBlind = 2,
                Players = new List<Player>
                {
                    new Player("P1", 100, 3, 0, new CheckOrFoldAgent()),
                    new Player("P2", 100, 3, 1, new CheckOrFoldAgent()),
                    new Player("P3", 100, 3, 2, new CheckOrFoldAgent())
                },
                SmallBlindPosition = 0,
                BigBlindPosition = 1,
                Deck = new Deck(new Random(42))
            };
            return engine;
        }

        [Fact]
        public void DealToPlayers_DealsTwoUniqueCardsToEachPlayer( )
        {
            var engine = CreateEngine();
            engine.DealToPlayers();

            Assert.All(engine.Players, p => Assert.Equal(2, p.Hand.Cards.Count));
            var unique = engine.Players.SelectMany(p => p.Hand.Cards).Distinct().Count();
            Assert.Equal(6, unique);
            Assert.Equal(52 - 6, engine.Deck.Count);
            Assert.Equal(engine.BigBlind, engine.CurrentMinBet);
        }

        [Fact]
        public void BoardCards_AreDealtCorrectly( )
        {
            var engine = CreateEngine();

            engine.Flop();
            Assert.Equal(3, engine.CommunityCards.Cards.Count);
            Assert.Equal(52 - 4, engine.Deck.Count);

            engine.Turn();
            Assert.Equal(4, engine.CommunityCards.Cards.Count);
            Assert.Equal(52 - 6, engine.Deck.Count);

            engine.River();
            Assert.Equal(5, engine.CommunityCards.Cards.Count);
            Assert.Equal(52 - 8, engine.Deck.Count);
        }

        [Fact]
        public void BuildActRequest_ReturnsExpectedValues( )
        {
            var engine = CreateEngine();
            engine.NextHand();
            var player = engine.Players[engine.CurrentPlayerTurn];
            var req = engine.BuildActRequest(player);

            Assert.Equal(engine.BigBlind, req.ToCall);
            Assert.Equal(engine.BigBlind * 2, req.MinRaise);
            Assert.False(req.AnyBetThisStreet);
            Assert.Equal(engine.CurrentPot, req.PotSize);
            Assert.Equal(0, req.YourCurrentBet);
            Assert.Equal(player.Chips, req.YourStack);
            Assert.Equal(engine.CurrentPlayerTurn, req.YourSeatIndex);
        }

        [Fact]
        public void ApplyMove_Fold_RemovesPlayerFromHand( )
        {
            var engine = CreateEngine();
            engine.NextHand();
            var seat = engine.CurrentPlayerTurn; // first actor
            var player = engine.Players[seat];

            engine.ApplyMove(player.Id, PlayType.Fold);

            Assert.DoesNotContain(seat, engine.ActivePlayerIndices);
            Assert.Equal(1, engine.CurrentPlayerTurn);
            Assert.Equal(3, engine.CurrentPot);
        }

        [Fact]
        public void ApplyMove_Raise_UpdatesPotAndMinBet( )
        {
            var engine = CreateEngine();
            engine.NextHand();
            var player = engine.Players[engine.CurrentPlayerTurn];

            engine.ApplyMove(player.Id, PlayType.Raise, 6);

            Assert.Equal(6, engine.CurrentMinBet);
            Assert.Equal(1 + 2 + 6, engine.CurrentPot);
            Assert.Equal(94, player.Chips);
        }

        [Fact]
        public void PlayHand_AllFold_BigBlindWinsPot( )
        {
            var engine = CreateEngine();
            var result = engine.PlayHand();
            var bigBlind = engine.Players[engine.BigBlindPosition];

            Assert.Contains(bigBlind, result.Winners);
            Assert.Equal(101, bigBlind.Chips);
            Assert.Equal(engine.SmallBlind + engine.BigBlind, engine.CurrentPot);
        }
    }
}

