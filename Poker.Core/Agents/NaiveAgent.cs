using Poker.Core.Interfaces;
using Poker.Core.Models;

namespace Poker.Core.Agents
{
    public class NaiveAgent : IPlayerStrategy
    {
        /// <summary>
        /// A very naive strategy:
        /// - If there's nothing to call: 
        ///     • Preflop, always open‐bet the size of the big blind  
        ///     • Later streets, just check  
        /// - If there is something to call: 
        ///     • Call if cost <= 1/4 pot, otherwise fold  
        /// </summary>
        public PlayerAction Act( ActRequest state )
        {
            // 1) No bet to call
            if ( state.ToCall == 0 )
            {
                // if nobody's bet yet this street, open‐bet on Preflop
                if ( !state.AnyBetThisStreet && state.CurrentStreet == Street.Preflop )
                {
                    if ( state.ToCall + state.MinRaise >= state.YourStack )
                    {
                        return new PlayerAction(PlayType.AllIn);
                    }
                    else
                    {
                        return new PlayerAction(PlayType.Raise, state.ToCall + state.MinRaise);
                    }
                }
                // otherwise just check
                return new PlayerAction(PlayType.Check);
            }

            // 2) Facing a bet: very basic pot‐odds rule
            int pot = state.PotSize;
            int cost = state.ToCall;
            if ( cost <= pot / 4 )
            {
                return new PlayerAction(PlayType.Call);
            }
            else
            {
                return new PlayerAction(PlayType.Fold);
            }
        }
    }
}