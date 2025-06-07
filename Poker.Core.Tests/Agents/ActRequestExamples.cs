using Poker.Core.Models;
using System;
using System.Collections.Generic;

namespace Poker.Core.Tests.Agents
{
    public static class ActRequestFixtures
    {
        public static IEnumerable<object[]> GetTenCases( ) => new[]
        {
            new object[] { new ActRequest
            {
                HoleCards        = new [] { new Card(Values.Ace, Suits.Spades), new Card(Values.Ace, Suits.Hearts) },
                CommunityCards   = Array.Empty<Card>(),
                CurrentStreet    = Street.Preflop,
                ToCall           = 30,      // 3 bb open in front
                MinRaise         = 60,      // 3-bet size must be ≥ 6 bb
                AnyBetThisStreet = true,
                PotSize          = 45,      // SB+BB+open
                YourCurrentBet   = 0,
                YourStack        = 2000,
                NumActivePlayers = 6,
                YourSeatIndex    = 4,
                DealerIndex      = 2,
                SmallBlind       = 5,
                BigBlind         = 10,
                OtherActivePlayers = new Dictionary<Guid, PlayerSummary>
                {
                    { Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1"), new PlayerSummary { Chips = 1500, BetChips = 30 } },
                    { Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb2"), new PlayerSummary { Chips = 800,  BetChips = 30 } }
                }
            }},

            new object[] { new ActRequest
            {
                HoleCards        = new [] { new Card(Values.King, Suits.Clubs), new Card(Values.Queen, Suits.Clubs) },
                CommunityCards   = new []
                {
                    new Card(Values.Ten,  Suits.Hearts),
                    new Card(Values.Eight, Suits.Diamonds),
                    new Card(Values.Two,   Suits.Spades)
                },
                CurrentStreet    = Street.Flop,
                ToCall           = 0,       // can check back
                MinRaise         = 20,      // min-bet if you lead
                AnyBetThisStreet = false,
                PotSize          = 120,
                YourCurrentBet   = 0,
                YourStack        = 850,
                NumActivePlayers = 3,
                YourSeatIndex    = 1,
                DealerIndex      = 1,
                SmallBlind       = 5,
                BigBlind         = 10,
                OtherActivePlayers = new Dictionary<Guid, PlayerSummary>
                {
                    { Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1"), new PlayerSummary { Chips = 300, BetChips = 0 } },
                    { Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb2"), new PlayerSummary { Chips = 450, BetChips = 0 } }
                }
            }},

            new object[] { new ActRequest
            {
                HoleCards        = new [] { new Card(Values.Ace, Suits.Diamonds), new Card(Values.Eight, Suits.Diamonds) },
                CommunityCards   = new []
                {
                    new Card(Values.Seven, Suits.Diamonds),
                    new Card(Values.Two,   Suits.Diamonds),
                    new Card(Values.Nine,  Suits.Clubs)
                },
                CurrentStreet    = Street.Flop,
                ToCall           = 50,
                MinRaise         = 100,
                AnyBetThisStreet = true,
                PotSize          = 150,
                YourCurrentBet   = 0,
                YourStack        = 400,
                NumActivePlayers = 5,
                YourSeatIndex    = 0,
                DealerIndex      = 4,
                SmallBlind       = 5,
                BigBlind         = 10,
                OtherActivePlayers = new Dictionary<Guid, PlayerSummary>
                {
                    { Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1"), new PlayerSummary { Chips = 250, BetChips = 50 } },
                    { Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb2"), new PlayerSummary { Chips = 350, BetChips = 50 } }
                }
            }},

            new object[] { new ActRequest
            {
                HoleCards        = new [] { new Card(Values.Jack, Suits.Diamonds), new Card(Values.Ten, Suits.Hearts) },
                CommunityCards   = new []
                {
                    new Card(Values.Jack, Suits.Spades),
                    new Card(Values.Ten,  Suits.Diamonds),
                    new Card(Values.Four, Suits.Clubs),
                    new Card(Values.Seven,Suits.Diamonds)
                },
                CurrentStreet    = Street.Turn,
                ToCall           = 80,
                MinRaise         = 160,
                AnyBetThisStreet = true,
                PotSize          = 240,
                YourCurrentBet   = 0,
                YourStack        = 1500,
                NumActivePlayers = 4,
                YourSeatIndex    = 2,
                DealerIndex      = 3,
                SmallBlind       = 5,
                BigBlind         = 10,
                OtherActivePlayers = new Dictionary<Guid, PlayerSummary>
                {
                    { Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1"), new PlayerSummary { Chips = 1200, BetChips = 80 } },
                    { Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb2"), new PlayerSummary { Chips = 900,  BetChips = 80 } }
                }
            }},

            new object[] { new ActRequest
            {
                HoleCards        = new [] { new Card(Values.Nine, Suits.Spades),  new Card(Values.Nine, Suits.Hearts) },
                CommunityCards   = new []
                {
                    new Card(Values.Nine, Suits.Clubs),
                    new Card(Values.Six,  Suits.Diamonds),
                    new Card(Values.Six,  Suits.Hearts),
                    new Card(Values.Two,  Suits.Spades),
                    new Card(Values.King, Suits.Clubs)
                },
                CurrentStreet    = Street.River,
                ToCall           = 250,
                MinRaise         = 500,
                AnyBetThisStreet = true,
                PotSize          = 900,
                YourCurrentBet   = 0,
                YourStack        = 1200,
                NumActivePlayers = 3,
                YourSeatIndex    = 1,
                DealerIndex      = 1,
                SmallBlind       = 5,
                BigBlind         = 10,
                OtherActivePlayers = new Dictionary<Guid, PlayerSummary>
                {
                    { Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1"), new PlayerSummary { Chips = 1100, BetChips = 250 } },
                    { Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb2"), new PlayerSummary { Chips = 1300, BetChips = 250 } }
                }
            }},

            new object[] { new ActRequest
            {
                HoleCards        = new [] { new Card(Values.Five, Suits.Spades), new Card(Values.Five, Suits.Clubs) },
                CommunityCards   = new []
                {
                    new Card(Values.Ace, Suits.Spades),
                    new Card(Values.King,Suits.Diamonds),
                    new Card(Values.Queen,Suits.Diamonds)
                },
                CurrentStreet    = Street. Flop,
                ToCall           = 250,     // equals your stack
                MinRaise         = 500,
                AnyBetThisStreet = true,
                PotSize          = 520,
                YourCurrentBet   = 0,
                YourStack        = 250,
                NumActivePlayers = 9,
                YourSeatIndex    = 6,
                DealerIndex      = 7,
                SmallBlind       = 5,
                BigBlind         = 10,
                OtherActivePlayers = new Dictionary<Guid, PlayerSummary>
                {
                    { Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1"), new PlayerSummary { Chips =  600, BetChips = 250 } },
                    { Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb2"), new PlayerSummary { Chips =  400, BetChips = 250 } }
                }
            }},

            new object[] { new ActRequest
            {
                HoleCards        = new [] { new Card(Values.Eight, Suits.Spades), new Card(Values.Three, Suits.Spades) },
                CommunityCards   = new []
                {
                    new Card(Values.Eight, Suits.Hearts),
                    new Card(Values.Seven, Suits.Spades),
                    new Card(Values.Two,   Suits.Diamonds),
                    new Card(Values.Jack,  Suits.Spades)
                },
                CurrentStreet    = Street.Turn,
                ToCall           = 0,
                MinRaise         = 40,
                AnyBetThisStreet = false,
                PotSize          = 60,
                YourCurrentBet   = 0,
                YourStack        = 975,
                NumActivePlayers = 2,
                YourSeatIndex    = 0,
                DealerIndex      = 0,
                SmallBlind       = 5,
                BigBlind         = 10,
                OtherActivePlayers = new Dictionary<Guid, PlayerSummary>
                {
                    { Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1"), new PlayerSummary { Chips = 975, BetChips = 0 } },
                    { Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb2"), new PlayerSummary { Chips = 1025, BetChips = 0 } }
                }
            }},

            new object[] { new ActRequest
            {
                HoleCards        = new [] { new Card(Values.Seven, Suits.Diamonds), new Card(Values.Two, Suits.Hearts) },
                CommunityCards   = Array.Empty<Card>(),
                CurrentStreet    = Street.Preflop,
                ToCall           = 0,        // everyone folded to the BB
                MinRaise         = 20,
                AnyBetThisStreet = false,
                PotSize          = 15,       // blinds only
                YourCurrentBet   = 10,       // BB already posted
                YourStack        = 990,
                NumActivePlayers = 9,
                YourSeatIndex    = 8,        // BB
                DealerIndex      = 7,
                SmallBlind       = 5,
                BigBlind         = 10,
                OtherActivePlayers = new Dictionary<Guid, PlayerSummary>
                {
                    { Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1"), new PlayerSummary { Chips = 990, BetChips = 10 } },
                    { Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb2"), new PlayerSummary { Chips = 900, BetChips = 10 } }
                }
            }},

            new object[] { new ActRequest
            {
                HoleCards        = new [] { new Card(Values.Queen, Suits.Diamonds), new Card(Values.Jack, Suits.Diamonds) },
                CommunityCards   = new []
                {
                    new Card(Values.King, Suits.Diamonds),
                    new Card(Values.Ten,  Suits.Diamonds),
                    new Card(Values.Two,  Suits.Hearts)
                },
                CurrentStreet    = Street.Flop,
                ToCall           = 40,
                MinRaise         = 80,
                AnyBetThisStreet = true,
                PotSize          = 110,
                YourCurrentBet   = 0,
                YourStack        = 1400,
                NumActivePlayers = 4,
                YourSeatIndex    = 3,
                DealerIndex      = 0,
                SmallBlind       = 5,
                BigBlind         = 10,
                OtherActivePlayers = new Dictionary<Guid, PlayerSummary>
                {
                    { Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1"), new PlayerSummary { Chips = 1300, BetChips = 40 } },
                    { Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb2"), new PlayerSummary { Chips = 1200, BetChips = 40 } }
                }
            }},

            new object[] { new ActRequest
            {
                HoleCards        = new [] { new Card(Values.Six,  Suits.Clubs),    new Card(Values.Four, Suits.Clubs) },
                CommunityCards   = new []
                {
                    new Card(Values.Five, Suits.Clubs),
                    new Card(Values.Three,Suits.Diamonds),
                    new Card(Values.Two,  Suits.Spades),
                    new Card(Values.Seven,Suits.Hearts)
                },
                CurrentStreet    = Street.Turn,
                ToCall           = 0,
                MinRaise         = 100,
                AnyBetThisStreet = false,
                PotSize          = 260,
                YourCurrentBet   = 0,
                YourStack        = 800,
                NumActivePlayers = 5,
                YourSeatIndex    = 1,
                DealerIndex      = 2,
                SmallBlind       = 5,
                BigBlind         = 10,
                OtherActivePlayers = new Dictionary<Guid, PlayerSummary>
                {
                    { Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1"), new PlayerSummary { Chips = 500, BetChips = 40 } },
                    { Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb2"), new PlayerSummary { Chips = 500, BetChips = 40 } }
                }
            }}
        };
    }
}