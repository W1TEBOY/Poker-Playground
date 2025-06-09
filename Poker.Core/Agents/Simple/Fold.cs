using Poker.Core.Interfaces;
using Poker.Core.Models;

namespace Poker.Core.Agents
{
    public class Fold : IPlayerStrategy
    {
        /// <summary>
        /// The stupid coward's strategy:
        /// - Always folds, no matter the situation. 
        /// </summary>
        public PlayerAction Act( ActRequest state )
        {
            // 1) Be a stupid coward
            return new PlayerAction(PlayType.Fold);
        }
    }
}