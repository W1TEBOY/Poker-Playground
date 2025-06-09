using Poker.Core.Interfaces;
using Poker.Core.Models;

namespace Poker.Core.Agents
{
    public class FiscalResponsibility : IPlayerStrategy
    {
        /// <summary>
        /// You'll call, but not *too* much
        /// </summary>
        public PlayerAction Act( ActRequest state )
        {
            if ( state.ToCall > 0.1 * state.YourStack )
            {
                return new PlayerAction(PlayType.Fold);
            }
            else if ( state.ToCall == 0 )
            {
                return new PlayerAction(PlayType.Check);
            }

            if ( state.ToCall >= state.YourStack )
            {
                return new PlayerAction(PlayType.AllIn);
            }
            else
            {
                return new PlayerAction(PlayType.Call, state.ToCall);
            }
        }
    }
}