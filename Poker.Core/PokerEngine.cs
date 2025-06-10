namespace Poker.Core
{
    using Poker.Core.Evaluation;
    using Poker.Core.Interfaces;
    using Poker.Core.Models;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Stub poker engine – flesh this out with CFR logic, game state, etc.
    /// </summary>
    public class PokerEngine
    {
        public Deck Deck
        {
            get; set;
        }
        public CommunityCards CommunityCards
        {
            get; set;
        }
        public Guid CurrentPlayerId
        {
            get
            {
                return Players[CurrentPlayerTurn].Id;
            }
        }
        public int BigBlind { get; set; } = 10;
        public int BigBlindPosition { get; set; } = 5;
        public int CurrentMinBet
        {
            get
            {
                return _betsThisHand.Values.Any() ? _betsThisHand.Values.Max() : BigBlind;
            }
        }
        public int CurrentPlayerTurn
        {
            get; set;
        }
        public int CurrentPot
        {
            get
            {
                // Sum of all chips committed by players this hand.
                return _betsThisHand?.Values.Sum() ?? 0;
            }
        }
        public int MaxPlayers { get; } = 6;
        public int SmallBlind { get; set; } = 5;
        public int SmallBlindPosition { get; set; } = 4;
        public int StartChips { get; } = 1000;
        public List<int> ActivePlayerIndices
        {
            get; set;
        }
        public List<Player> Players
        {
            get; set;
        }

        private bool _anyBetThisStreet;
        private Dictionary<Guid, int> _betsThisStreet;
        private Dictionary<Guid, int> _betsThisHand = new Dictionary<Guid, int>();

        private Street _round = Street.Preflop;
        private int _lastRaiseAmount;

        public PokerEngine( )
        {
            // discover and instantiate all IPlayerStrategy implementations
            IPlayerStrategy[] allStrats = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => typeof(IPlayerStrategy).IsAssignableFrom(t)
                            && t.IsClass
                            && !t.IsAbstract)
                .Select(t => (IPlayerStrategy) Activator.CreateInstance(t)!)
                .ToArray();

            var rng = new Random();

            // initialize players with random strategies
            Players = Enumerable.Range(0, MaxPlayers)
                .Select(i =>
                {
                    var strat = allStrats[rng.Next(allStrats.Length)];
                    return new Player(
                        name: $"Player {i + 1}",
                        startChips: StartChips,
                        playerCount: MaxPlayers,
                        currentPosition: i,
                        strategy: strat
                    );
                })
                .ToList();

            // initial deck and board
            Deck = new Deck();
            CommunityCards = new CommunityCards();

        }

        public (List<Guid> losers, int nextPlayerTurn) NextHand( )
        {
            // 1) reset the street & raise tracker
            _round = Street.Preflop;
            _lastRaiseAmount = BigBlind;
            _anyBetThisStreet = false;

            // ─── 2) PRUNE busted players ───────────────────────
            var losers = Players
              .Where(p => p.Chips == 0)
              .Select(p => p.Id)
              .ToList();
            Players.RemoveAll(p => losers.Contains(p.Id));

            // ─── 3) REBUILD active seats ───────────────────────
            ActivePlayerIndices = Enumerable.Range(0, Players.Count)
                                            .ToList();

            // ─── 4) RESTORE or ADVANCE the big-blind ───────────
            SmallBlindPosition = _NextActiveFrom(SmallBlindPosition);
            BigBlindPosition = _NextActiveFrom(SmallBlindPosition);

            // ─── 5) Now recompute blinds & next actor ──────────
            CurrentPlayerTurn = _AdvanceIndex(BigBlindPosition);


            if ( !Players.Any(p => p.Id == CurrentPlayerId) )
            {
                CurrentPlayerTurn = _AdvanceIndex(BigBlindPosition);

            }

            // initialize betting
            _anyBetThisStreet = false;
            _betsThisStreet = Players.ToDictionary(p => p.Id, p => 0);
            _betsThisHand = Players.ToDictionary(p => p.Id, p => 0);

            // reshuffle and clear board
            Deck.Reset();
            CommunityCards = new CommunityCards();

            var sbPlayer = Players[SmallBlindPosition];
            var sbBet = sbPlayer.Bet(SmallBlind);
            _betsThisStreet[sbPlayer.Id] = sbBet;
            _betsThisHand[sbPlayer.Id] += sbBet;

            var bbPlayer = Players[BigBlindPosition];
            var bbBet = bbPlayer.Bet(BigBlind);
            _betsThisStreet[bbPlayer.Id] = bbBet;
            _betsThisHand[bbPlayer.Id] += bbBet;

            DealToPlayers();

            // return losers and next actor
            return (losers, CurrentPlayerTurn);
        }

        /// <summary>
        /// Given the current seat (an index into Players),
        /// return the *next* live seat (also an index into Players),
        /// and update CurrentPlayerTurn/CurrentPlayerId accordingly.
        /// </summary>
        private int _AdvanceIndex( int currentSeat )
        {
            if ( ActivePlayerIndices == null || ActivePlayerIndices.Count == 0 )
                throw new InvalidOperationException("No players to advance to.");

            // 1) find your position in the active‐seat list
            int pos = ActivePlayerIndices.IndexOf(currentSeat);

            // 2) advance one slot (wrap with modulo)
            int nextPos = (pos + 1) % ActivePlayerIndices.Count;

            // 3) pull the actual seat‐index from the active list
            int nextSeat = ActivePlayerIndices[nextPos];

            return nextSeat;
        }


        public void DealToPlayers( )
        {
            foreach ( var player in Players )
            {
                player.Hand.ResetCards();
            }
            Players.ForEach(player =>
            {
                player.Hand.Add(Deck.Draw());
                player.Hand.Add(Deck.Draw());
            });
        }

        public void Flop( )
        {
            Deck.Draw(); // burn
            CommunityCards.Add(Deck.Draw());
            CommunityCards.Add(Deck.Draw());
            CommunityCards.Add(Deck.Draw());
            Console.WriteLine($"At the Flop, we have: {CommunityCards}");
        }

        public void Turn( )
        {
            Deck.Draw();
            CommunityCards.Add(Deck.Draw());
            Console.WriteLine($"At the Turn, we have: {CommunityCards}");
        }

        [SuppressMessage("CodeQuality", "S4144:Methods should not have identical implementations",
            Justification = "Turn and River are distinct streets with same dealing.")]
        public void River( )
        {
            Deck.Draw();
            CommunityCards.Add(Deck.Draw());
            Console.WriteLine($"At the River, we have: {CommunityCards}");
        }

        /// <summary>
        /// Called to drive each player's turn until showdown.
        /// </summary>
        public void RunHand( )
        {
            int checks = 0;
            // ensures NextHand/deal called already
            while ( _round != Street.Showdown || (!_anyBetThisStreet && checks >= ActivePlayerIndices.Count) )
            {
                var player = Players[CurrentPlayerTurn];

                var req = BuildActRequest(player);

                var action = player.Act(req);
                ApplyMove(player.Id, action.Play, action.Amount ?? 0);
                if ( action.Play == PlayType.Check && !_anyBetThisStreet )
                {
                    checks += 1;
                }
            }
        }

        public ActRequest BuildActRequest( Player player )
        {
            Dictionary<Guid, PlayerSummary> otherActivePlayersChips = new Dictionary<Guid, PlayerSummary>();

            foreach ( int activePlayer in ActivePlayerIndices )
            {
                Guid activePlayerId = Players[activePlayer].Id;
                if ( activePlayerId == player.Id )
                {
                    continue;
                }

                otherActivePlayersChips[activePlayerId] = new PlayerSummary
                {
                    Chips = Players[activePlayer].Chips,
                    BetChips = _betsThisStreet[activePlayerId]
                };
            }


            return new ActRequest
            {
                HoleCards = player.Hand.Cards,
                CommunityCards = CommunityCards.Cards,
                CurrentStreet = _round,
                ToCall = CurrentMinBet - _betsThisHand[CurrentPlayerId],
                MinRaise = _lastRaiseAmount + CurrentMinBet - _betsThisHand[CurrentPlayerId],
                AnyBetThisStreet = _anyBetThisStreet,
                PotSize = CurrentPot,
                YourCurrentBet = _betsThisHand[CurrentPlayerId],
                YourStack = player.Chips,
                NumActivePlayers = ActivePlayerIndices.Count,
                YourSeatIndex = CurrentPlayerTurn,
                DealerIndex = BigBlindPosition - 1 < 0 ? Players.Count - 1 : BigBlindPosition - 1,
                SmallBlind = SmallBlind,
                BigBlind = BigBlind,
                OtherActivePlayers = otherActivePlayersChips
            };
        }

        // add this helper to your class:
        /// <summary>
        /// True as soon as every player except the richest still in the hand has zero chips left.
        /// </summary>
        private bool _EveryoneAllIn( )
            => ActivePlayerIndices.Count(i => Players[i].Chips > 0) < 2;

        // replace your existing ApplyMove with this:
        public void ApplyMove( Guid playerId, PlayType play, int amount = 0 )
        {
            // 1) do the action
            switch ( play )
            {
                case PlayType.Fold:
                    _Fold(playerId);
                    break;
                case PlayType.Check:
                    _Check();
                    break;
                case PlayType.Call:
                    _Call(playerId);
                    break;
                case PlayType.Raise:
                    _Raise(playerId, amount);
                    break;
                case PlayType.AllIn:
                    _AllIn(playerId);
                    break;
                default:
                    throw new ArgumentException($"Unknown play '{play}'");
            }

            // 2) if only one left OR everyone's all-in, immediately go to showdown
            if ( ActivePlayerIndices.Count <= 1 || _EveryoneAllIn() )
            {
                _round = Street.Showdown;
            }
        }



        private void _Fold( Guid playerId )
        {
            // Advance the turn BEFORE removing the active player index
            _AdvanceTurn();

            // remove from the “in-hand” list
            ActivePlayerIndices.RemoveAll(i => Players[i].Id == playerId);

        }


        private void _Check( )
        {
            // whose turn is it?
            var actor = Players[CurrentPlayerTurn];
            var playerId = actor.Id;

            // how much do they still owe?
            int actorBet = _betsThisStreet[playerId];
            int toCall = CurrentMinBet - actorBet;

            if ( toCall > 0 )
            {
                // they can’t really “check”, so force a call
                _Call(playerId);
                Console.WriteLine($"{Players[CurrentPlayerTurn].Name} can't REALLY Check, so they will call.");
            }
            else
            {
                // true check
                _AdvanceTurn();
            }
        }


        private void _Call( Guid playerId )
        {
            int seat = Players.FindIndex(p => p.Id == playerId);
            int toCall = CurrentMinBet - _betsThisStreet[playerId];
            if ( toCall <= 0 )
            {
                _Check();
                return;
            }

            _anyBetThisStreet = true;

            if ( toCall >= Players[seat].Chips )
            {
                _AllIn(playerId);
                Console.WriteLine($"{Players[seat].Name} is All In (even though they tried to call like a dumbass)!.");
                return;
            }
            var bet = Players[seat].Bet(toCall);
            _betsThisStreet[playerId] += bet;
            _betsThisHand[playerId] += bet;
            _AdvanceTurn();
        }

        private void _Raise( Guid playerId, int amount )
        {
            int seat = Players.FindIndex(p => p.Id == playerId);
            int already = _betsThisHand[playerId];      // chips committed this street
            var player = Players[seat];

            int minRaiseTotal = _lastRaiseAmount + CurrentMinBet - _betsThisHand[player.Id];   // total bet required

            if ( amount < minRaiseTotal )
            {
                throw new InvalidOperationException(
                    $"Raise must be at least {minRaiseTotal} (call {CurrentMinBet} + last raise {_lastRaiseAmount}).  Amount raised: {amount}");
            }

            int oldMinBet = _betsThisHand.Values.Any() ? _betsThisHand.Values.Max() : 0;

            int contribution = amount - already;              // new chips put in pot
            var bet = player.Bet(contribution);
            _betsThisStreet[playerId] += bet;
            _betsThisHand[playerId] += bet;

            int newMinBet = _betsThisHand.Values.Any() ? _betsThisHand.Values.Max() : 0;
            _lastRaiseAmount = Math.Max(newMinBet - oldMinBet, BigBlind);       // size of THIS raise

            _anyBetThisStreet = true;

            _AdvanceTurn();
        }

        private void _AllIn( Guid playerId )
        {
            int seat = Players.FindIndex(p => p.Id == playerId);
            int contribution = Players[seat].Chips;
            int total = _betsThisHand[playerId] + contribution;
            var bet = Players[seat].Bet(contribution);
            _betsThisStreet[playerId] += bet;
            _betsThisHand[playerId] += bet;
            _anyBetThisStreet = true;
            if ( total > CurrentMinBet )
            {
                _lastRaiseAmount = total - CurrentMinBet;
            }
            _AdvanceTurn();
        }

        private void _AdvanceTurn( )
        {
            CurrentPlayerTurn = _AdvanceIndex(CurrentPlayerTurn);

        }

        private int _NextActiveFrom( int start )
        {
            var n = Players.Count;
            var seat = (start + 1) % n;
            while ( !ActivePlayerIndices.Contains(seat) )
                seat = (seat + 1) % n;
            return seat;
        }

        public HandResult PlayHand( )
        {
            bool skipRound = false;

            NextHand();
            skipRound = RunBettingRound(Street.Preflop);
            skipRound = RunBettingRound(Street.Flop, skipRound);
            skipRound = RunBettingRound(Street.Turn, skipRound);
            RunBettingRound(Street.River, skipRound);
            return ResolveShowdown();
        }

        private HandResult ResolveShowdown( )
        {
            // 1) If everyone else folded, single player wins immediately
            if ( ActivePlayerIndices.Count == 1 )
            {
                var soloSeat = ActivePlayerIndices.Single();
                var solo = Players[soloSeat];
                var hv = HandEvaluator.EvaluateBestHand(solo.Hand.Cards, CommunityCards.Cards);

                // award whole pot to them
                solo.AddWinnings(CurrentPot);

                return new HandResult(
                    Board: CommunityCards.Cards,
                    ShowdownHands: new List<(Player, HandValue)> { (solo, hv) },
                    Winners: new List<Player> { solo }
                );
            }

            // 2) Build a list of (player, handValue) for all remaining players
            var showdownHands = ActivePlayerIndices
                .Select(seat =>
                {
                    var p = Players[seat];
                    var hv = HandEvaluator.EvaluateBestHand(p.Hand.Cards, CommunityCards.Cards);
                    return (Player: p, Value: hv);
                })
                .ToList();

            // 3) Find the best handValue
            var bestValue = showdownHands.Max(x => x.Value);

            // 4) Collect all players whose handValue equals bestValue
            var winners = showdownHands
                .Where(x => x.Value.CompareTo(bestValue) == 0)
                .Select(x => x.Player)
                .ToList();

            // 5) Distribute the main- and side-pots
            _DistributePots(showdownHands);

            // 6) Return the result
            return new HandResult(
                Board: CommunityCards.Cards,
                ShowdownHands: showdownHands,
                Winners: winners
            );
        }


        private bool RunBettingRound( Street street, bool skipRound = false )
        {
            PlayerAction move;

            // 0) Optional skip
            if ( skipRound )
                return true;

            // 1) Advance street + deal cards
            _round = street;
            if ( street == Street.Flop )
                Flop();
            else if ( street == Street.Turn )
                Turn();
            else if ( street == Street.River )
                River();

            // 2) Reset betting state
            _anyBetThisStreet = false;
            _lastRaiseAmount = BigBlind;
            if ( street != Street.Preflop )
            {
                foreach ( var pid in _betsThisStreet.Keys.ToList() )
                    _betsThisStreet[pid] = 0;
            }

            // 3) Pick first to act
            if ( street == Street.Preflop )
            {
                // Preflop: UTG = seat after BB
                CurrentPlayerTurn = (BigBlindPosition + 1) % Players.Count;
            }
            else
            {
                // Post-flop: SB starts
                CurrentPlayerTurn = SmallBlindPosition;
            }

            // If that seat isn’t active, find the next one
            if ( ActivePlayerIndices.Any() )
            {
                int start = CurrentPlayerTurn;
                while ( !ActivePlayerIndices.Contains(CurrentPlayerTurn) )
                {
                    CurrentPlayerTurn = (CurrentPlayerTurn + 1) % Players.Count;
                    if ( CurrentPlayerTurn == start )
                    {
                        // no one active
                        skipRound = true;
                        break;
                    }
                }
            }
            else
            {
                skipRound = true;
            }

            // 4) Preflop special: pretend there was already a bet so BB/CHeck logic works
            if ( street == Street.Preflop )
                _anyBetThisStreet = true;

            // 5) Early exits
            if ( skipRound || ActivePlayerIndices.Count <= 1 || _EveryoneAllIn() )
                return true;

            // 6) SCUFFED SOLUTION: track last non-fold actions
            var lastActions = new Queue<PlayType>();

            // 7) Main loop
            int iterationSafeguard = 0;
            const int iterationLimit = 1000;
            while ( _round == street && ActivePlayerIndices.Count > 1 && !_EveryoneAllIn() )
            {
                var player = Players[CurrentPlayerTurn];
                var req = BuildActRequest(player);
                if ( req.YourStack == 0 ) // if a player is already AllIn, just move onto the next
                {
                    move = new PlayerAction(PlayType.Check);
                }
                else
                {
                    move = player.Act(req);
                }

                if ( move.Amount < 0 )
                    throw new Exception("Move amount cannot be negative.");

                Console.WriteLine($"{player.Name} decided to {move.Play}!");
                ApplyMove(player.Id, move.Play, move.Amount ?? 0);

                // Enqueue non-folds and trim to current active count
                if ( move.Play != PlayType.Fold )
                {
                    lastActions.Enqueue(move.Play);
                    while ( lastActions.Count > ActivePlayerIndices.Count )
                        lastActions.Dequeue();
                }

                // If we now have exactly one action per active player and they're all checks, end the street
                if ( lastActions.Count == ActivePlayerIndices.Count
                    && lastActions.All(a => a == PlayType.Check) )
                {
                    _round = street + 1;
                    break;
                }

                if ( ++iterationSafeguard > iterationLimit )
                {
                    // Break out to avoid a potential infinite loop
                    _round = street + 1;
                    break;
                }
            }

            // 8) Final safety: if we never advanced street naturally, force it
            if ( _round == street && ActivePlayerIndices.Count > 1 && !_EveryoneAllIn() )
                _round = street + 1;

            return (ActivePlayerIndices.Count == 1 || _EveryoneAllIn());
        }



        /// <summary>
        /// After showdown, builds side-pots from each player’s contributed bets
        /// and then pays each pot to the best hand among the players eligible for it.
        /// </summary>
        private void _DistributePots(
            List<(Player Player, HandValue HandValue)> showdownHands )
        {
            // 1) Build a working list of (player, contributed amount)
            var contributions = Players
                .Select(p => (
                    Player: p,
                    Bet: _betsThisHand.ContainsKey(p.Id) ? _betsThisHand[p.Id] : 0,
                    Eligible: showdownHands.Any(sh => sh.Player.Id == p.Id)))
                .Where(x => x.Bet > 0)
                .OrderBy(x => x.Bet)
                .ToList();

            var sidePots = new List<(int Amount, List<Player> Eligible)>();
            int prevLevel = 0;

            // 2) Slice out successive “levels” to form side-pots
            for ( int i = 0; i < contributions.Count; i++ )
            {
                int levelBet = contributions[i].Bet;
                int levelSize = levelBet - prevLevel;
                if ( levelSize > 0 )
                {
                    int potAmount = levelSize * contributions.Count(x => x.Bet >= levelBet);

                    var eligible = contributions
                        .Where(x => x.Bet >= levelBet && x.Eligible)
                        .Select(x => x.Player)
                        .ToList();

                    if ( eligible.Count == 0 )
                    {
                        if ( sidePots.Count > 0 )
                        {
                            var last = sidePots[^1];
                            sidePots[^1] = (last.Amount + potAmount, last.Eligible);
                        }
                        else
                        {
                            // Shouldn't happen, but keep the pot to avoid losing chips
                            sidePots.Add((potAmount, eligible));
                        }
                    }
                    else
                    {
                        sidePots.Add((potAmount, eligible));
                        prevLevel = levelBet;
                    }
                }
            }

            // 3) Award each side-pot in turn
            foreach ( var (Amount, Eligible) in sidePots )
            {
                // find the winning hand among those eligible
                var bestValue = showdownHands
                    .Where(sh => Eligible.Contains(sh.Player))
                    .Max(sh => sh.HandValue);

                var winners = showdownHands
                    .Where(sh => Eligible.Contains(sh.Player) && sh.HandValue.CompareTo(bestValue) == 0)
                    .Select(sh => sh.Player)
                    .ToList();

                if ( winners.Count == 0 && Amount > 0 )
                {
                    throw new Exception("Side pots must be winners.");
                }

                int share = Amount / winners.Count;
                int remainder = Amount % winners.Count;

                foreach ( var w in winners )
                    w.AddWinnings(share);

                // give any odd chip(s) to the first winner
                if ( remainder > 0 )
                    winners[0].AddWinnings(remainder);
            }

            // 4) reset pot & per‐hand bets
            foreach ( var pid in _betsThisStreet.Keys.ToList() )
            {
                _betsThisStreet[pid] = 0;
                _betsThisHand[pid] = 0;
            }
        }

    }
}