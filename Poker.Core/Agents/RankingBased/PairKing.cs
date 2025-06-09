using Poker.Core.Evaluation;
using Poker.Core.Interfaces;
using Poker.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Poker.Core.Agents
{
    public class PairKing : IPlayerStrategy
    {
        readonly HandRank _minRank = HandRank.Pair;

        /// <summary>
        /// The coward's strategy:
        /// - Always checks or folds, no matter the situation. 
        /// </summary>
        public PlayerAction Act( ActRequest state )
        {
            List<Card> holeCards = state.HoleCards.ToList();
            List<Card> communityCards = state.CommunityCards.ToList();

            HandValue bestHand = HandEvaluator.EvaluateBestHand(holeCards, communityCards);

            int streetInt = (int) state.CurrentStreet;
            int reraiseAmount = (int) Math.Pow(2, streetInt);
            int totalMinRaise = state.ToCall + state.MinRaise;

            if ( bestHand.Rank >= _minRank )
            {
                if ( state.YourStack > totalMinRaise )
                {
                    if ( state.YourStack > totalMinRaise + reraiseAmount )
                    {
                        return new PlayerAction(PlayType.Raise, totalMinRaise + reraiseAmount);
                    }
                    else
                    {
                        return new PlayerAction(PlayType.Raise, totalMinRaise);
                    }
                }
                else
                {
                    return new PlayerAction(PlayType.AllIn, state.YourStack);
                }
            }

            // 1) Be a coward
            if ( state.ToCall > 0 )
            {
                // BASICALLY don't chicken out PreFlop *within reason*
                if ( state.CurrentStreet == Street.Preflop &&
                    state.YourStack > state.ToCall &&
                    state.YourCurrentBet + state.ToCall <= 10 * state.BigBlind )
                {
                    return new PlayerAction(PlayType.Call, state.ToCall);
                }
                else
                {
                    return new PlayerAction(PlayType.Fold);
                }
            }

            return new PlayerAction(PlayType.Check);
        }
    }
}