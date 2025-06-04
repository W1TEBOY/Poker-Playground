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
            get; set;
        }
        public int BigBlind { get; set; } = 10;
        public int BigBlindPosition { get; set; } = 5;
        public int CurrentMinBet { get; set; } = 0;
        public int CurrentPlayerTurn
        {
            get; set;
        }
        public int CurrentPot
        {
            get
            {
                // If _betsThisStreet is not yet initialized, treat as zero.
                return _betsThisStreet?.Values.Sum() ?? 0;
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
            _lastRaiseAmount = BigBlind; // Minimum raise amount is initially BB
            CurrentMinBet = BigBlind; // Cost to play is initially BB
            _anyBetThisStreet = false; // No bets yet other than blinds (which are handled by CurrentMinBet)

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
            CurrentPlayerId = Players[CurrentPlayerTurn].Id;

            if ( !Players.Any(p => p.Id == CurrentPlayerId) )
            {
                CurrentPlayerTurn = _AdvanceIndex(BigBlindPosition);
                CurrentPlayerId = Players[CurrentPlayerTurn].Id;
            }

            // reshuffle and clear board
            Deck.Reset();
            CommunityCards = new CommunityCards();

            // initialize betting
            _anyBetThisStreet = false;
            _betsThisStreet = Players.ToDictionary(p => p.Id, p => 0);

            var sbPlayer = Players[SmallBlindPosition];
            _betsThisStreet[sbPlayer.Id] = sbPlayer.Bet(SmallBlind); // Chips deducted here

            var bbPlayer = Players[BigBlindPosition];
            _betsThisStreet[bbPlayer.Id] = bbPlayer.Bet(BigBlind); // Chips deducted here

            // CurrentMinBet is already BigBlind
            // _lastRaiseAmount is already BigBlind
            // _anyBetThisStreet is false

            DealToPlayers();

            Console.WriteLine($"DEBUG: NextHand END. SB: {Players[SmallBlindPosition].Name} (bet: {_betsThisStreet[Players[SmallBlindPosition].Id]}, chips: {Players[SmallBlindPosition].Chips}), BB: {Players[BigBlindPosition].Name} (bet: {_betsThisStreet[Players[BigBlindPosition].Id]}, chips: {Players[BigBlindPosition].Chips}), UTG: {Players[CurrentPlayerTurn].Name} (bet: {_betsThisStreet[Players[CurrentPlayerTurn].Id]}, chips: {Players[CurrentPlayerTurn].Chips})");

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
            CurrentMinBet = BigBlind;
        }

        public void Flop( )
        {
            Deck.Draw(); // burn
            CommunityCards.Add(Deck.Draw());
            CommunityCards.Add(Deck.Draw());
            CommunityCards.Add(Deck.Draw());
            // Removed betting state logic: CurrentMinBet, _lastRaiseAmount, _anyBetThisStreet, _betsThisStreet reset, CurrentPlayerTurn
        }

        public void Turn( )
        {
            Deck.Draw();
            CommunityCards.Add(Deck.Draw());
            // Removed betting state logic
        }

        [SuppressMessage("CodeQuality", "S4144:Methods should not have identical implementations",
            Justification = "Turn and River are distinct streets with same dealing.")]
        public void River( )
        {
            Deck.Draw();
            CommunityCards.Add(Deck.Draw());
            // Removed betting state logic
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
                var req = new ActRequest
                {
                    HoleCards = player.Hand.Cards,
                    CommunityCards = CommunityCards.Cards,
                    CurrentStreet = _round,
                    ToCall = CurrentMinBet - _betsThisStreet[player.Id], // Use player.Id for safety
                    MinRaise = _lastRaiseAmount + CurrentMinBet - _betsThisStreet[player.Id], // Use player.Id for safety
                    AnyBetThisStreet = _anyBetThisStreet,
                    PotSize = CurrentPot,
                    YourCurrentBet = _betsThisStreet[player.Id], // Use player.Id for safety
                    YourStack = player.Chips,
                    NumActivePlayers = ActivePlayerIndices.Count,
                    YourSeatIndex = CurrentPlayerTurn,
                    DealerIndex = BigBlindPosition - 1 < 0 ? Players.Count - 1 : BigBlindPosition - 1,
                    SmallBlind = SmallBlind,
                    BigBlind = BigBlind
                };

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
            return new ActRequest
            {
                HoleCards = player.Hand.Cards,
                CommunityCards = CommunityCards.Cards,
                CurrentStreet = _round,
                ToCall = CurrentMinBet - _betsThisStreet[CurrentPlayerId],
                MinRaise = _lastRaiseAmount + CurrentMinBet - _betsThisStreet[CurrentPlayerId],
                AnyBetThisStreet = _anyBetThisStreet,
                PotSize = CurrentPot,
                YourCurrentBet = _betsThisStreet[CurrentPlayerId],
                YourStack = player.Chips,
                NumActivePlayers = ActivePlayerIndices.Count,
                YourSeatIndex = CurrentPlayerTurn,
                DealerIndex = BigBlindPosition - 1 < 0 ? Players.Count - 1 : BigBlindPosition - 1,
                SmallBlind = SmallBlind,
                BigBlind = BigBlind
            };
        }

        // add this helper to your class:
        /// <summary>
        /// True as soon as every player still in the hand has zero chips left.
        /// </summary>
        private bool _EveryoneAllIn( )
            => ActivePlayerIndices.All(i => Players[i].Chips == 0);

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
            // DO NOT drop their bet record from _betsThisStreet.
            // Their bets are part of the pot for the current street.
            // _betsThisStreet.Remove(playerId); // This line is removed.
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
                _Check(); // Player doesn't owe anything or owes less than 0 (should not happen if CurrentMinBet is well-managed)
                return;
            }

            _anyBetThisStreet = true; // A call means betting action is happening or continuing.

            if ( toCall >= Players[seat].Chips ) // Calling this amount means player is all-in
            {
                _AllIn(playerId); // _AllIn handles bet, _anyBetThisStreet, and turn advancement
                Console.WriteLine($"{Players[seat].Name} is All In via Call!.");
                return;
            }

            _betsThisStreet[playerId] += Players[seat].Bet(toCall);
            _AdvanceTurn();
        }

        private void _Raise( Guid playerId, int amount )
        {
            int seat = Players.FindIndex(p => p.Id == playerId);
            int already = _betsThisStreet[playerId];      // chips committed this street
            var player = Players[seat];

            int minRaiseTotal = _lastRaiseAmount + CurrentMinBet - _betsThisStreet[player.Id];   // total bet required

            if ( amount < minRaiseTotal )
            {
                throw new InvalidOperationException(
                    $"Raise must be at least {minRaiseTotal} (call {CurrentMinBet} + last raise {_lastRaiseAmount}).");
            }

            int contribution = amount - already;              // new chips put in pot
            _betsThisStreet[playerId] += player.Bet(contribution);

            _anyBetThisStreet = true;

            _lastRaiseAmount = amount - CurrentMinBet;       // size of THIS raise
            CurrentMinBet = amount;                       // amount to call now

            _AdvanceTurn();
        }

        private void _AllIn( Guid playerId )
        {
            int seat = Players.FindIndex(p => p.Id == playerId);
            int contribution = Players[seat].Chips;
            int total = _betsThisStreet[playerId] + contribution;
            _betsThisStreet[playerId] += Players[seat].Bet(contribution);
            _anyBetThisStreet = true;
            if ( total > CurrentMinBet )
            {
                _lastRaiseAmount = total - CurrentMinBet;
                CurrentMinBet = total;
            }
            _AdvanceTurn();
        }

        private void _AdvanceTurn( )
        {
            CurrentPlayerTurn = _AdvanceIndex(CurrentPlayerTurn);
            CurrentPlayerId = Players[CurrentPlayerTurn].Id;
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
            if (skipRound) return true; // Check skipRound AT THE VERY BEGINNING

            _round = street; // Set current street first

            // Deal cards for new street *before* setting up betting state for that street
            if (street == Street.Flop) { Flop(); } // Flop() now only deals cards
            else if (street == Street.Turn) { Turn(); } // Turn() now only deals cards
            else if (street == Street.River) { River(); } // River() now only deals cards

            if (street != Street.Preflop)
            {
                _anyBetThisStreet = false;
                _lastRaiseAmount = 0; // No prior raise amount for this new street.
                CurrentMinBet = 0;    // No bets yet on this new street.

                foreach (var pid_key in _betsThisStreet.Keys.ToList())
                {
                     _betsThisStreet[pid_key] = 0;
                }

                if (ActivePlayerIndices.Any()) {
                    CurrentPlayerTurn = SmallBlindPosition; // Start with SB post-flop

                    int initialTurnCheck = CurrentPlayerTurn;
                    bool foundNextPlayer = ActivePlayerIndices.Contains(CurrentPlayerTurn); // Check if SB itself is active

                    if (!foundNextPlayer) { // If SB not active, find next
                        CurrentPlayerTurn = (CurrentPlayerTurn + 1) % Players.Count;
                        while (CurrentPlayerTurn != initialTurnCheck) {
                            if (ActivePlayerIndices.Contains(CurrentPlayerTurn)) {
                                foundNextPlayer = true;
                                break;
                            }
                            CurrentPlayerTurn = (CurrentPlayerTurn + 1) % Players.Count;
                        }
                    }

                    if (foundNextPlayer) {
                         CurrentPlayerId = Players[CurrentPlayerTurn].Id;
                         Console.WriteLine($"DEBUG: {street} starting. Player turn set to {Players[CurrentPlayerTurn].Name} (seat {CurrentPlayerTurn}, chips {Players[CurrentPlayerTurn].Chips}). Bets reset.");
                    } else {
                        Console.WriteLine($"DEBUG: {street} starting. No active player found to start turn. ActiveIndices: {ActivePlayerIndices.Count}");
                        skipRound = true;
                    }
                } else {
                     Console.WriteLine($"DEBUG: {street} starting. No active players. ActiveIndices: {ActivePlayerIndices.Count}");
                     skipRound = true;
                }
            }
            else // Street.Preflop
            {
                // Values are set by NextHand():
                // _betsThisStreet contains SB and BB.
                // CurrentMinBet = BigBlind.
                // _lastRaiseAmount = BigBlind.
                // CurrentPlayerTurn = UTG. (Set by NextHand)
                _anyBetThisStreet = true; // Blinds are live, so betting action has started.

                // Debug line for preflop start state
                // Ensure players and their bets exist before trying to access them for logging
                string sbNameDbg = SmallBlindPosition < Players.Count ? Players[SmallBlindPosition].Name : "N/A";
                Guid sbIdDbg = SmallBlindPosition < Players.Count ? Players[SmallBlindPosition].Id : Guid.Empty;
                int sbBetDbg = (sbIdDbg != Guid.Empty && _betsThisStreet.ContainsKey(sbIdDbg)) ? _betsThisStreet[sbIdDbg] : -1;
                long sbChipsDbg = SmallBlindPosition < Players.Count ? Players[SmallBlindPosition].Chips : -1;

                string bbNameDbg = BigBlindPosition < Players.Count ? Players[BigBlindPosition].Name : "N/A";
                Guid bbIdDbg = BigBlindPosition < Players.Count ? Players[BigBlindPosition].Id : Guid.Empty;
                int bbBetDbg = (bbIdDbg != Guid.Empty && _betsThisStreet.ContainsKey(bbIdDbg)) ? _betsThisStreet[bbIdDbg] : -1;
                long bbChipsDbg = BigBlindPosition < Players.Count ? Players[BigBlindPosition].Chips : -1;

                string utgNameDbg = CurrentPlayerTurn < Players.Count ? Players[CurrentPlayerTurn].Name : "N/A";
                Guid utgIdDbg = CurrentPlayerTurn < Players.Count ? Players[CurrentPlayerTurn].Id : Guid.Empty;
                int utgBetDbg = (utgIdDbg != Guid.Empty && _betsThisStreet.ContainsKey(utgIdDbg)) ? _betsThisStreet[utgIdDbg] : -1;
                long utgChipsDbg = CurrentPlayerTurn < Players.Count ? Players[CurrentPlayerTurn].Chips : -1;

                Console.WriteLine($"DEBUG: RunBettingRound Preflop START. CurrentTurn: {utgNameDbg} (id: {utgIdDbg}, bet: {utgBetDbg}, chips: {utgChipsDbg}). SB: {sbNameDbg} (id: {sbIdDbg}, bet: {sbBetDbg}, chips: {sbChipsDbg}). BB: {bbNameDbg} (id: {bbIdDbg}, bet: {bbBetDbg}, chips: {bbChipsDbg}). CurrentMinBet: {CurrentMinBet}. LastRaise: {_lastRaiseAmount}");
            }

            if (skipRound) return true; // Skip if no active players post-flop or pre-existing skip

            // If, after street setup (e.g. Flop cards dealt), only one player or all-in, skip betting.
            if (ActivePlayerIndices.Count <= 1 || _EveryoneAllIn()) {
                if (_EveryoneAllIn() && street < Street.River) {
                    // If everyone is all-in before the river, advance to showdown to deal remaining cards.
                    // The PlayHand() method will call RunBettingRound for subsequent streets with skipRound=true.
                }
                return true;
            }

            int actionsThisSubRound = 0; // Actions since last bet/raise or since start of street.
            // playerMakingLastAggressiveAction is not strictly needed if we check all players' bets against CurrentMinBet.

            while (true) // Loop until betting round explicitly ends.
            {
                if (!(_round == street && ActivePlayerIndices.Count > 1 && !_EveryoneAllIn())) {
                    break; // Initial conditions for betting no longer met (e.g., street changed by fold, only one left)
                }

                // Termination Check (occurs BEFORE player acts for the current CurrentPlayerTurn)
                if (actionsThisSubRound >= ActivePlayerIndices.Count) { // Everyone has had a turn in the current "sub-round"
                    bool canEndRound = false;
                    if (!_anyBetThisStreet) { // No bets AT ALL this street (e.g. everyone checked around)
                        canEndRound = true;
                        Console.WriteLine($"Street {street} ends: All players checked (no actual bets placed). actionsThisSubRound: {actionsThisSubRound}");
                    } else { // There WAS some betting this street (_anyBetThisStreet is true)
                        // Check if all active players who are not all-in have bet CurrentMinBet
                        bool allNonAllInPlayersMatchedCurrentMinBet = true;
                        foreach (int activeSeatIdx in ActivePlayerIndices) {
                            Player p = Players[activeSeatIdx];
                            if (p.Chips > 0 && _betsThisStreet[p.Id] < CurrentMinBet) {
                                allNonAllInPlayersMatchedCurrentMinBet = false;
                                break;
                            }
                        }
                        if (allNonAllInPlayersMatchedCurrentMinBet) {
                            canEndRound = true;
                            Console.WriteLine($"Street {street} ends: All bets ({CurrentMinBet}) settled or players all-in. actionsThisSubRound: {actionsThisSubRound}");
                        }
                    }

                    if (canEndRound) {
                        _round = street + 1; // Advance to next street (or showdown if street was River)
                        break;
                    }
                }

                var actingPlayer = Players[CurrentPlayerTurn];
                var actingPlayerId = actingPlayer.Id; // Ensure we use this specific player's ID for consistency

                Console.WriteLine($"DEBUG: Betting Loop TOP. Street: {street}. Acting Player: {actingPlayer.Name} (id: {actingPlayerId}, turnIndex: {CurrentPlayerTurn}, chips: {actingPlayer.Chips}). Expected bet this street: {_betsThisStreet[actingPlayerId]}. CurrentMinBet: {CurrentMinBet}. Pot: {CurrentPot}.");

                var req = new ActRequest // BuildActRequest logic inlined and using actingPlayerId
                {
                    HoleCards = actingPlayer.Hand.Cards,
                    CommunityCards = CommunityCards.Cards,
                    CurrentStreet = _round,
                    ToCall = CurrentMinBet - _betsThisStreet[actingPlayerId],
                    MinRaise = _lastRaiseAmount + CurrentMinBet - _betsThisStreet[actingPlayerId],
                    AnyBetThisStreet = _anyBetThisStreet,
                    PotSize = CurrentPot,
                    YourCurrentBet = _betsThisStreet[actingPlayerId],
                    YourStack = actingPlayer.Chips,
                    NumActivePlayers = ActivePlayerIndices.Count,
                    YourSeatIndex = CurrentPlayerTurn, // This is actingPlayer's seat index
                    DealerIndex = BigBlindPosition - 1 < 0 ? Players.Count - 1 : BigBlindPosition - 1,
                    SmallBlind = SmallBlind,
                    BigBlind = BigBlind
                };
                Console.WriteLine($"DEBUG: ActRequest for {actingPlayer.Name}: ToCall={req.ToCall}, MinRaise={req.MinRaise}, YourCurrentBet={req.YourCurrentBet}, PotSize={req.PotSize}, YourStack={req.YourStack}");

                var move = actingPlayer.Act(req); // Use actingPlayer

                // Game Action Log using CurrentPlayerTurn, which is actingPlayer's turn.
                Console.WriteLine($"{Players[CurrentPlayerTurn].Name} (seat {CurrentPlayerTurn}, chips {Players[CurrentPlayerTurn].Chips}) Street:{street} ToCall:{req.ToCall} MinBet:{CurrentMinBet} CurrentPot:{CurrentPot} TheirBet:{req.YourCurrentBet} -> {move.Play} ({move.Amount})");

                ApplyMove(actingPlayerId, move.Play, move.Amount ?? 0); // ApplyMove using actingPlayerId
                actionsThisSubRound++;

                if (move.Play == PlayType.Raise) {
                    actionsThisSubRound = 1;
                }

                if (_round != street) {
                    break;
                }
            }

            // Safety: If loop exited for reasons other than advancing _round (e.g. unexpected break),
            // and the round wasn't naturally concluded by player count or all-in state, advance _round.
            if (_round == street && ActivePlayerIndices.Count > 1 && !_EveryoneAllIn()) {
                Console.WriteLine($"Warning: Betting loop for street {street} exited by other means with multiple players active and not all-in. Advancing street to {street + 1}.");
                _round = street + 1;
            }

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
            var contributions = showdownHands
                .Select(sh => (Player: sh.Player, Bet: _betsThisStreet[sh.Player.Id]))
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
                    // everyone whose total bet ≥ this level participates
                    var eligible = contributions
                        .Where(x => x.Bet >= levelBet)
                        .Select(x => x.Player)
                        .ToList();

                    int potAmount = levelSize * eligible.Count;
                    sidePots.Add((potAmount, eligible));
                    prevLevel = levelBet;
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
                _betsThisStreet[pid] = 0;
        }

    }
}