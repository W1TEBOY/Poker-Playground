using Poker.Core.Interfaces;
using Poker.Core.Models;

namespace Poker.Core.Agents
{
    public class BrainlessPotOdds : IPlayerStrategy
    {
        /// <summary>
        /// Play the Pot Odds... if you don't care about your hand
        /// </summary>
        public PlayerAction Act( ActRequest state )
        {
            double potOdds = state.ToCall / ((double) state.PotSize * state.ToCall);

            if ( potOdds > 0.25 )
            {
                return new PlayerAction(PlayType.Call, state.ToCall);
            }
            else if ( state.ToCall == 0 )
            {
                return new PlayerAction(PlayType.Check);
            }

            return new PlayerAction(PlayType.Fold);

        }
    }
}