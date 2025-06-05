using Poker.Core.Interfaces;
using Poker.Core.Models;

namespace Poker.Core.Agents
{
    public class CheckOrCallAgent : IPlayerStrategy
    {
        public PlayerAction Act( ActRequest state )
        {
            if ( state.ToCall == 0 )
            {
                return new PlayerAction(PlayType.Check);
            }
            // If forced to call, then call.
            return new PlayerAction(PlayType.Call, state.ToCall);
        }
    }
}
