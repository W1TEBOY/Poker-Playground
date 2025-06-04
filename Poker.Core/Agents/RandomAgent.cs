using Poker.Core.Interfaces;
using Poker.Core.Models;
using System;
using System.Collections.Generic;

namespace Poker.Core.Agents
{
    public sealed class RandomAgent : IPlayerStrategy
    {
        private static readonly Random _rng = new();

        public PlayerAction Act( ActRequest state )
        {
            //----------------------------------------------------------------
            // 1️⃣  Determine which actions are legal in this spot
            //----------------------------------------------------------------
            var legal = new List<PlayType> { PlayType.Fold, PlayType.AllIn };

            if ( state.ToCall == 0 )
            {
                legal.Add(PlayType.Check);
                if ( state.YourStack >= state.MinRaise && state.MinRaise > 0 )
                    legal.Add(PlayType.Raise);         // opening raise
            }
            else // facing a bet
            {
                if ( state.YourStack >= state.ToCall )
                    legal.Add(PlayType.Call);

                if ( state.YourStack >= state.ToCall + state.MinRaise
                    && state.MinRaise > 0 )
                    legal.Add(PlayType.Raise);
            }

            //----------------------------------------------------------------
            // 2️⃣  Pick one legal action at random
            //----------------------------------------------------------------
            var pick = legal[_rng.Next(legal.Count)];

            //----------------------------------------------------------------
            // 3️⃣  Build the PlayerAction
            //----------------------------------------------------------------
            switch ( pick )
            {
                case PlayType.Check:
                    return new PlayerAction(PlayType.Check);

                case PlayType.Call:
                    return new PlayerAction(PlayType.Call);

                case PlayType.Raise:
                    {
                        int minTotal = state.ToCall + state.MinRaise; // rule-compliant
                        int maxTotal = state.YourStack;               // can’t bet more
                        int totalBet = _rng.Next(minTotal, maxTotal + 1); // inclusive upper
                        return new PlayerAction(PlayType.Raise, totalBet);
                    }

                case PlayType.AllIn:
                    return new PlayerAction(PlayType.AllIn);

                default: // Fold (fallback)
                    return new PlayerAction(PlayType.Fold);
            }
        }
    }
}
