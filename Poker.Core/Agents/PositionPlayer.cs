using Poker.Core.Interfaces;
using Poker.Core.Models;
using static System.Math;

namespace Poker.Core.Agents
{
    public class PositionPlayer : IPlayerStrategy
    {
        /// <summary>
        /// Be Aggressive within reason if you're a later position
        /// </summary>
        public PlayerAction Act( ActRequest state )
        {
            bool isLateEnough = (state.NumActivePlayers <= 3);

            if ( isLateEnough )
            {
                if ( state.ToCall == 0 )
                {
                    int toBet = Max(state.BigBlind, state.MinRaise);

                    if ( state.YourStack >= toBet )
                    {
                        return new PlayerAction(PlayType.Raise, toBet);
                    }
                    else
                    {
                        return new PlayerAction(PlayType.AllIn, state.YourStack);
                    }
                }
                // Stay in, but don't go nuts
                else if ( state.YourStack >= 2 * state.ToCall )
                {
                    return new PlayerAction(PlayType.Call, state.ToCall);
                }
                else
                {
                    return new PlayerAction(PlayType.Fold);
                }
            }

            // 1) Be a coward
            if ( state.ToCall > 0 )
            {
                return new PlayerAction(PlayType.Fold);
            }

            return new PlayerAction(PlayType.Check);
        }
    }
}