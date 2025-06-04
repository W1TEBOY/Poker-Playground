#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;

namespace Poker.Core.Evaluation
{
    using Poker.Core.Models;

    public sealed class HandValue : IComparable<HandValue>, IEquatable<HandValue>
    {
        public HandRank Rank
        {
            get;
        }
        public IReadOnlyList<Card> Cards
        {
            get => _cards;
        }
        public IReadOnlyList<Card> Kickers
        {
            get => _kickers;
        }

        private IReadOnlyList<Card> _cards;
        private IReadOnlyList<Card> _kickers;

        public HandValue( HandRank rank, List<Card> cards, List<Card> kickers )
        {
            Rank = rank;
            _cards = cards;
            _kickers = kickers;
        }

        public int CompareTo( HandValue? other )
        {
            if ( other is null )
                return 1;

            // 1) Rank comparison (e.g. Pair vs TwoPair vs ThreeOfKind, etc.)
            int rankCmp = Rank.CompareTo(other.Rank);
            if ( rankCmp != 0 )
                return rankCmp;

            // 2) Same Rank == Pair → compare the “pair cards” in descending order.
            //    (Since both have exactly 2 cards in _cards, those cards are equal‐ranks if they are the same pair.)
            for ( int i = 0; i < _cards.Count; i++ )
            {
                int c = _cards[i].CompareTo(other._cards[i]);
                if ( c != 0 )
                    return c;
            }

            // 3) If the pair‐ranks tie (e.g. both players have “Pair of Queens”), compare kicker by kicker:
            for ( int i = 0; i < _kickers.Count; i++ )
            {
                int k = _kickers[i].CompareTo(other._kickers[i]);
                if ( k != 0 )
                    return k;
            }

            return 0; // they’re exactly equal in rank and kickers
        }

        public void Sort( )
        {
            if ( Rank == HandRank.FullHouse )
            {
                var groups = _cards.GroupBy(c => c.Value);
                var tripCards = groups.First(g => g.Count() == 3)
                                      .OrderByDescending(c => c.Suit)
                                      .ToList();
                var pairCards = groups.First(g => g.Count() == 2)
                                      .OrderByDescending(c => c.Suit)
                                      .ToList();
                _cards = tripCards.Concat(pairCards).ToList();
            }
            else if ( Rank == HandRank.Straight || Rank == HandRank.StraightFlush )
            {
                var values = _cards.Select(c => c.Value).ToList();
                bool isWheel = values.Contains(Values.Ace)
                                && values.Contains(Values.Two)
                                && values.Contains(Values.Three)
                                && values.Contains(Values.Four)
                                && values.Contains(Values.Five);

                if ( isWheel )
                {
                    var order = new[] { Values.Five, Values.Four, Values.Three, Values.Two, Values.Ace };
                    var sortedCards = order
                        .Select(v => _cards.Where(c => c.Value == v)
                                           .OrderByDescending(c => c.Suit)
                                           .First())
                        .ToList();
                    _cards = sortedCards;
                }
                else
                {
                    _cards = _cards
                        .OrderByDescending(c => c.Value)
                        .ThenBy(c => c.Suit)
                        .ToList();
                }
            }
            else
            {
                _cards = _cards
                    .OrderByDescending(c => c.Value)
                    .ThenBy(c => c.Suit)
                    .ToList();
            }

            _kickers = _kickers
                .OrderByDescending(c => c.Value)
                .ThenBy(c => c.Suit)
                .ToList();
        }




        // IEquatable<T>
        public bool Equals( HandValue? other )
        {
            if ( other is null )
                return false;
            if ( ReferenceEquals(this, other) )
                return true;
            return CompareTo(other) == 0;
        }

        // override object.Equals
        public override bool Equals( object? obj ) =>
            Equals(obj as HandValue);

        // override GetHashCode—you must include everything used in Equals/CompareTo
        public override int GetHashCode( )
        {
            unchecked
            {
                int hash = Rank.GetHashCode();
                // Incorporate cards into hash using immutable Cards property
                foreach ( var card in Cards )
                    hash = (hash * 397) ^ card.GetHashCode();
                // Incorporate kickers into hash using immutable Kickers property
                foreach ( var kicker in Kickers )
                    hash = (hash * 397) ^ kicker.GetHashCode();
                return hash;
            }
        }

        // comparison operators in terms of CompareTo
        public static bool operator ==( HandValue? a, HandValue? b ) =>
            a is null ? b is null : a.Equals(b);

        public static bool operator !=( HandValue? a, HandValue? b ) =>
            !(a == b);

        public static bool operator <( HandValue? a, HandValue? b ) =>
            a is null
                ? b is not null
                : a.CompareTo(b) < 0;

        public static bool operator <=( HandValue? a, HandValue? b ) =>
            a is null || a.CompareTo(b) <= 0;

        public static bool operator >( HandValue? a, HandValue? b ) =>
            a is not null && a.CompareTo(b) > 0;

        public static bool operator >=( HandValue? a, HandValue? b ) =>
            a is null
                ? b is null
                : a.CompareTo(b) >= 0;
    }
}
#nullable disable
