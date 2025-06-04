using System;

namespace Poker.Core.Models
{
    public enum Values : byte
    {
        Two = 2,
        Three = 3,
        Four = 4,
        Five = 5,
        Six = 6,
        Seven = 7,
        Eight = 8,
        Nine = 9,
        Ten = 10,
        Jack = 11,
        Queen = 12,
        King = 13,
        Ace = 14
    }

    public enum Suits : byte
    {
        Diamonds = 1,
        Clubs = 2,
        Hearts = 3,
        Spades = 4
    }

    public readonly struct Card : IComparable<Card>, IEquatable<Card>
    {
        public Values Value
        {
            get;
        }
        public Suits Suit
        {
            get;
        }

        public Card( Values value, Suits suit )
        {
            Value = value;
            Suit = suit;
        }

        public int CompareTo( Card other )
        {
            int c = Value.CompareTo(other.Value);
            if ( c != 0 )
                return c;
            return Suit.CompareTo(other.Suit);
        }

        public override string ToString( )
            => $"{Value} of {Suit}";

        // IEquatable<T> implementation
        public bool Equals( Card other )
            => Value == other.Value && Suit == other.Suit;

#nullable enable

        public override bool Equals( object? obj )
            => obj is Card other && Equals(other);

#nullable disable

        public override int GetHashCode( )
            => HashCode.Combine(Value, Suit);

        // Equality operators
        public static bool operator ==( Card left, Card right )
            => left.Equals(right);

        public static bool operator !=( Card left, Card right )
            => !left.Equals(right);

        // Relational operators
        public static bool operator <( Card left, Card right )
            => left.CompareTo(right) < 0;

        public static bool operator <=( Card left, Card right )
            => left.CompareTo(right) <= 0;

        public static bool operator >( Card left, Card right )
            => left.CompareTo(right) > 0;

        public static bool operator >=( Card left, Card right )
            => left.CompareTo(right) >= 0;
    }
}
