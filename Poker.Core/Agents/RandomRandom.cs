using Poker.Core.Interfaces;
using Poker.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Poker.Core.Agents
{
    public sealed class RandomRandom : IPlayerStrategy
    {
        private static readonly Random _rng = new();

        // persistent weights for each action
        private readonly Dictionary<PlayType, double> _weights;

        /// <summary>
        /// If Random wasn't enough:
        /// - Randomly chooses weights for each play, then keeps them persistent forever.
        /// - Odds per play will still be based on what's legal.
        /// </summary>
        public RandomRandom( )
        {
            // initialize one random weight per PlayType
            _weights = new Dictionary<PlayType, double>();
            foreach ( var action in new[]
                     {
                         PlayType.Fold,
                         PlayType.Check,
                         PlayType.Call,
                         PlayType.Raise,
                         PlayType.AllIn
                     } )
            {
                _weights[action] = _rng.NextDouble();
            }
        }

        public PlayerAction Act( ActRequest state )
        {
            //------------------------------------------------------------
            // 1️⃣  Determine which actions are legal in this spot
            //------------------------------------------------------------
            var legal = new List<PlayType> { PlayType.Fold, PlayType.AllIn };

            if ( state.MinRaise > 0 && state.YourStack >= state.ToCall + state.MinRaise )
                legal.Add(PlayType.Raise);

            if ( state.ToCall == 0 )
            {
                legal.Add(PlayType.Check);
            }
            else
            {
                if ( state.YourStack >= state.ToCall )
                    legal.Add(PlayType.Call);
            }

            //------------------------------------------------------------
            // 2️⃣  Sample one of the legal actions according to the fixed weights
            //------------------------------------------------------------
            var subset = legal
                .Select(a => (Action: a, Weight: _weights[a]))
                .ToArray();

            double totalWeight = subset.Sum(x => x.Weight);
            double pickPoint = _rng.NextDouble() * totalWeight;

            double cumul = 0;
            PlayType pickAction = subset[0].Action;  // fallback
            foreach ( var (Action, Weight) in subset )
            {
                cumul += Weight;
                if ( cumul >= pickPoint )
                {
                    pickAction = Action;
                    break;
                }
            }

            //------------------------------------------------------------
            // 3️⃣  Build the PlayerAction exactly as before
            //------------------------------------------------------------
            switch ( pickAction )
            {
                case PlayType.Check:
                    return new PlayerAction(PlayType.Check);

                case PlayType.Call:
                    return new PlayerAction(PlayType.Call);

                case PlayType.Raise:
                    {
                        int minTotal = state.ToCall + state.MinRaise;
                        int maxTotal = state.YourStack;
                        int totalBet = _rng.Next(minTotal, maxTotal + 1);
                        return new PlayerAction(PlayType.Raise, totalBet);
                    }

                case PlayType.AllIn:
                    return new PlayerAction(PlayType.AllIn);

                default:
                    return new PlayerAction(PlayType.Fold);
            }
        }
    }
}
