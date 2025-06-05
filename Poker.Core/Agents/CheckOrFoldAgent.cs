using Poker.Core.Interfaces;
using Poker.Core.Models;

namespace Poker.Core.Agents
{
    public class CheckOrFoldAgent : IPlayerStrategy
    {
        /// <summary>
        /// The coward's strategy:
        /// - Always checks or folds, no matter the situation. 
        /// </summary>
        public PlayerAction Act( ActRequest state )
        {
            // 1) Be a coward
            if ( state.ToCall > 0 )
            {
                return new PlayerAction(PlayType.Fold);
            }

            return new PlayerAction(PlayType.Check);
        }
    }
}