namespace Poker.Core.Evaluation
{

    using Poker.Core.Models;
    using System.Collections.Generic;
    using System.Linq;

    public static class HandEvaluator
    {

        /// <summary>
        /// Returns a HandValue (rank + kickers) for the best 5-card hand  
        /// you can make from your holeCards + board.
        /// </summary>
        public static HandValue EvaluateBestHand(
            IReadOnlyList<Card> hand,
            IReadOnlyList<Card> board
            )
        {
            // combine hole cards + board cards into one list of 7
            var allCards = new List<Card>(7);
            allCards.AddRange(hand);
            allCards.AddRange(board);

            var output = Evaluate(allCards);
            output.Sort();
            return output;
        }


        private static HandValue Evaluate( List<Card> cards )
        {
            cards.Sort();
            Dictionary<Values, List<Card>> groups = cards
                .GroupBy(x => x.Value)
                .ToDictionary(g => g.Key, g => g.ToList());

            HandValue handValue = null;

            bool isFlush = _IsFlush(cards, out handValue);
            bool isStraight = _IsStraight(cards, out handValue);

            if ( _IsStraightOrRoyalFlush(cards, isFlush, isStraight, out handValue) )
            {
                return handValue;
            }
            if ( _IsFourOfAKind(cards, groups, out handValue) )
            {
                return handValue;
            }
            if ( _IsFullHouse(groups, out handValue) )
            {
                return handValue;
            }
            if ( _IsFlush(cards, out handValue) )
            {
                return handValue;
            }
            if ( _IsStraight(cards, out handValue) )
            {
                return handValue;
            }
            if ( _IsThreeOfAKind(cards, groups, out handValue) )
            {
                return handValue;
            }
            if ( _IsPairOrTwoPair(cards, groups, out handValue) )
            {
                return handValue;
            }

            Card bestCard = cards.Max();

            List<Card> kickers = _GetKickers(4, cards.Except(new[] { bestCard }).ToList());

            handValue = new HandValue(
                HandRank.HighCard,
                new List<Card> { bestCard },
                kickers
                );
            return handValue;
        }

        private static bool _IsPairOrTwoPair(
            List<Card> cards,
            Dictionary<Values, List<Card>> groups,
            out HandValue handValue
        )
        {
            handValue = null;

            // 1) Find all values that form exactly a pair, ordered from highest to lowest.
            var pairGroups = groups
                .Where(kvp => kvp.Value.Count == 2)
                .OrderByDescending(kvp => kvp.Key)
                .ToList();

            // No pairs at all → bail out
            if ( !pairGroups.Any() )
                return false;

            // Exactly one pair → build a "Pair" hand
            if ( pairGroups.Count == 1 )
            {
                var topPairValue = pairGroups[0].Key;
                var pairCards = pairGroups[0].Value; // 2 cards of that value

                var kickerCards = _GetKickers(3, cards.Where(c => c.Value != topPairValue).ToList());

                handValue = new HandValue(
                    HandRank.Pair,
                    pairCards,
                    kickerCards
                );
                return true;
            }

            // Two or more pairs → build a "TwoPair" using the top two by value
            else
            {
                var top1Pair = pairGroups[0];
                var top2Pair = pairGroups[1];

                // Gather the four cards that make up the two pairs
                var pairCards = new List<Card>(4)
                {
                    top1Pair.Value[0],
                    top1Pair.Value[1],
                    top2Pair.Value[0],
                    top2Pair.Value[1]
                };

                var top1Val = top1Pair.Key;
                var top2Val = top2Pair.Key;

                var kickerCards = _GetKickers(1, cards.Where(c => c.Value != top1Val && c.Value != top2Val).ToList());

                handValue = new HandValue(
                    HandRank.TwoPair,
                    pairCards,
                    kickerCards
                );
                return true;
            }
        }



        private static bool _IsThreeOfAKind(
            List<Card> cards,
            Dictionary<Values, List<Card>> groups,
            out HandValue handValue
        )
        {
            handValue = null;

            // find the three‐of‐a‐kind group without LINQ
            Values tripleValue = 0;
            List<Card> tripleCards = null;
            foreach ( var kvp in groups )
            {
                if ( kvp.Value.Count == 3 )
                {
                    tripleValue = kvp.Key;
                    tripleCards = kvp.Value;
                    break;
                }
            }
            if ( tripleCards == null )
                return false;

            var kickerCards = _GetKickers(2, cards.Where(c => c.Value != tripleValue).ToList());

            handValue = new HandValue(
                HandRank.ThreeOfAKind,
                tripleCards,
                kickerCards
            );

            return true;
        }


        private static bool _IsStraight(
            List<Card> cards,
            out HandValue handValue
        )
        {
            handValue = null;

            // 1) Build a lookup of the highest card for each rank
            var bestCard = new Card[15];        // indices 2..14
            var hasCard = new bool[15];
            foreach ( var c in cards )
            {
                int v = (int) c.Value;
                if ( !hasCard[v] || c.CompareTo(bestCard[v]) > 0 )
                {
                    bestCard[v] = c;
                    hasCard[v] = true;
                }
            }

            // 2) Scan from Ace (14) down to Five (5) looking for a 5‐card run
            for ( int top = 14; top >= 5; top-- )
            {
                if ( top > 5 )
                {
                    // Check ranks {top, top‐1, top‐2, top‐3, top‐4}
                    bool ok = true;
                    for ( int k = 0; k < 5; k++ )
                    {
                        if ( !hasCard[top - k] )
                        {
                            ok = false;
                            break;
                        }
                    }
                    if ( !ok )
                        continue;

                    // Build the straight in descending order
                    var run = new List<Card>(5);
                    for ( int k = 0; k < 5; k++ )
                    {
                        run.Add(bestCard[top - k]);
                    }

                    handValue = new HandValue(
                        HandRank.Straight,
                        run,
                        new List<Card>());

                    return true;
                }
                else
                {
                    // top == 5: check the wheel A‐2‐3‐4‐5
                    if ( hasCard[5] && hasCard[4] && hasCard[3] && hasCard[2] && hasCard[14] )
                    {
                        var run = new List<Card>(5)
                {
                    bestCard[5],
                    bestCard[4],
                    bestCard[3],
                    bestCard[2],
                    bestCard[14]
                };

                        handValue = new HandValue(
                            HandRank.Straight,
                            run,
                            new List<Card>());

                        return true;
                    }
                }
            }

            return false;
        }


        private static bool _IsFlush( List<Card> cards, out HandValue handValue )
        {
            handValue = null;

            var suitGroups = cards.GroupBy(c => c.Suit);
            var flushGroup = suitGroups.FirstOrDefault(g => g.Count() >= 5);

            if ( flushGroup != null )
            {
                var topFive = flushGroup
                    .OrderByDescending(c => c.Value)
                    .ThenBy(c => c.Suit)
                    .Take(5)
                    .ToList();

                handValue = new HandValue(
                    HandRank.Flush,
                    topFive,
                    new List<Card>()
                );
                return true;
            }

            return false;
        }


        private static bool _IsFullHouse(
            Dictionary<Values, List<Card>> groups,
            out HandValue handValue
        )
        {
            handValue = null;

            // find three-of-a-kind
            var tripleGroup = groups.FirstOrDefault(g => g.Value.Count == 3);
            if ( tripleGroup.Value == null )
                return false;

            // find a pair
            var pairGroup = groups.FirstOrDefault(g => g.Value.Count == 2);
            if ( pairGroup.Value == null )
                return false;

            List<Card> tripleCards = tripleGroup.Value;
            List<Card> pairCards = pairGroup.Value;

            // use exactly those five cards
            List<Card> usedCards = tripleCards
                .Concat(pairCards)
                .ToList();

            // full house has no extra kickers
            handValue = new HandValue(
                HandRank.FullHouse,
                usedCards,
                new List<Card>()
            );

            return true;
        }

        private static bool _IsFourOfAKind(
            List<Card> cards,
            Dictionary<Values, List<Card>> groups,
            out HandValue handValue
        )
        {
            handValue = null;

            KeyValuePair<Values, List<Card>> quads = groups.FirstOrDefault(g => g.Value.Count == 4);
            if ( quads.Value == null || quads.Value.Count != 4 )
                return false;

            List<Card> quadCards = quads.Value;

            List<Card> kickerCards = _GetKickers(1, cards.Except(quadCards).ToList());

            handValue = new HandValue(
                HandRank.FourOfAKind,
                quadCards,
                kickerCards
                );

            return true;
        }

        private static bool _IsStraightOrRoyalFlush(
            List<Card> cards,
            bool isFlush,
            bool isStraight,
            out HandValue handValue
        )
        {
            handValue = null;

            // 1) If we know it’s not both a flush and a straight, we can exit early.
            if ( !isFlush || !isStraight )
                return false;

            // We'll keep track of the best straight‐flush we find:
            int bestHighValue = 0;
            Suits bestSuit = Suits.Hearts;            // placeholder
            List<int> bestNeededRanks = null;         // the five rank values that make up that straight

            // 2) Group all cards by suit
            var cardsBySuit = new Dictionary<Suits, List<Card>>();
            foreach ( var c in cards )
            {
                if ( !cardsBySuit.ContainsKey(c.Suit) )
                    cardsBySuit[c.Suit] = new List<Card>();
                cardsBySuit[c.Suit].Add(c);
            }

            // 3) For each suit that has at least 5 cards, look for a straight within those cards
            foreach ( var kvp in cardsBySuit )
            {
                var suit = kvp.Key;
                var suitedCards = kvp.Value;

                if ( suitedCards.Count < 5 )
                    continue; // not enough cards of this suit to form a straight‐flush

                // 3a) Build a set of distinct integer ranks for this suit
                var distinctRanks = new HashSet<int>();
                foreach ( var c in suitedCards )
                    distinctRanks.Add((int) c.Value);

                // If there is an Ace (value = 14), also allow Ace to count as “1” for a wheel (A‐2‐3‐4‐5)
                bool hasAce = distinctRanks.Contains((int) Values.Ace);
                if ( hasAce )
                    distinctRanks.Add(1);

                // 3b) Convert the distinct ranks into a sorted list descending
                var sortedRanksDesc = distinctRanks.ToList();
                sortedRanksDesc.Sort(( a, b ) => b.CompareTo(a)); // largest first

                // 3c) For each possible “high” in that list (must be at least 5), check if we can build a 5‐card run
                foreach ( int high in sortedRanksDesc )
                {
                    if ( high < 5 )
                        break; // no need to check below 5, because you can’t have a 5‐card straight starting below 5

                    // Build the five consecutive ranks: high, high-1, high-2, high-3, high-4
                    var needed = new List<int>();
                    for ( int offset = 0; offset < 5; offset++ )
                    {
                        int r = high - offset;
                        // If this is “1”, it really means Ace in terms of picking cards later
                        needed.Add(r);
                    }

                    // Check if all those ranks exist in distinctRanks (treating “1” as Ace if needed)
                    bool allPresent = true;
                    foreach ( int r in needed )
                    {
                        if ( r == 1 )
                        {
                            // “1” is only valid if we added it because we had an Ace
                            if ( !hasAce )
                            {
                                allPresent = false;
                                break;
                            }
                        }
                        else if ( !distinctRanks.Contains(r) )
                        {
                            allPresent = false;
                            break;
                        }
                    }

                    if ( !allPresent )
                        continue;

                    // We found a valid straight of length 5 in this suit.
                    // Determine the “effective high value” for comparison:
                    // - If it’s a wheel (1-2-3-4-5), use high=5
                    // - Otherwise use the numeric high itself
                    int effectiveHigh = (high == 5 && hasAce && distinctRanks.Contains(14)) ? 5 : high;

                    // Keep the best (largest) effective high across all suits
                    if ( effectiveHigh > bestHighValue )
                    {
                        bestHighValue = effectiveHigh;
                        bestSuit = suit;
                        bestNeededRanks = new List<int>(needed);
                    }

                    // Once we’ve found a valid run starting at this high, no need to check smaller highs for this suit
                    break;
                }
            }

            // 4) If we never found any straight‐flush run, return false
            if ( bestHighValue == 0 || bestNeededRanks == null )
                return false;

            // 5) Reconstruct the actual Card objects for those five ranks, all from bestSuit.
            //    For rank “1” we pick an Ace (value=14).
            var cardsInStraightFlush = new List<Card>();
            foreach ( int r in bestNeededRanks )
            {
                int targetValue = (r == 1) ? (int) Values.Ace : r;

                // Among all cards of that suit and with that Value, just pick one.
                // (If there are duplicates, picking any is fine.)
                Card chosen = cards
                    .First(c => c.Suit == bestSuit && (int) c.Value == targetValue);
                cardsInStraightFlush.Add(chosen);
            }

            // 6) Sort those five cards from highest to lowest, but treat Ace‐low wheel specially:
            //    If bestHighValue == 5 and one of the cards is an Ace, move that Ace to the end of the sequence.
            cardsInStraightFlush.Sort(( c1, c2 ) =>
            {
                int v1 = (int) c1.Value;
                int v2 = (int) c2.Value;

                // If this is a wheel straight, treat Ace as 1
                if ( bestHighValue == 5 )
                {
                    if ( c1.Value == Values.Ace )
                        v1 = 1;
                    if ( c2.Value == Values.Ace )
                        v2 = 1;
                }

                // Sort descending by the adjusted value
                int cmp = v2.CompareTo(v1);
                if ( cmp != 0 )
                    return cmp;

                // If values tie (unlikely in a straightflush), just compare suits (arbitrary tiebreak)
                return c2.Suit.CompareTo(c1.Suit);
            });

            // 7) Check if it’s a wheel‐straight (A-2-3-4-5) or a royal (10-J-Q-K-A)
            bool isWheelStraight = (bestHighValue == 5) &&
                                   cardsInStraightFlush.Any(c => c.Value == Values.Ace);
            bool isRoyal = !isWheelStraight
                           && cardsInStraightFlush.Min(c => (int) c.Value) == 10
                           && cardsInStraightFlush.Max(c => (int) c.Value) == 14;

            // 8) Build the final HandValue
            var rank = isRoyal ? HandRank.RoyalFlush : HandRank.StraightFlush;
            handValue = new HandValue(
                rank,
                cardsInStraightFlush,
                new List<Card>()  // no extra kickers
            );

            return true;
        }



        private static List<Card> _GetKickers( int n, List<Card> cards )
        {
            // Ensure a new ordering without modifying original list
            return cards
                .OrderByDescending(c => c.Value)
                .ThenByDescending(c => c.Suit)
                .Take(n)
                .ToList();
        }

    }
}