using System.Collections.Generic;

namespace Poker.Core.Models;

public class ActRequest
{
    public IReadOnlyList<Card> HoleCards { get; init; }
    public IReadOnlyList<Card> CommunityCards { get; init; }
    public Street CurrentStreet { get; init; }
    public int ToCall { get; init; }
    public int MinRaise { get; init; }
    public bool AnyBetThisStreet { get; init; }
    public int PotSize { get; init; }
    public int YourCurrentBet { get; init; }
    public int YourStack { get; init; }
    public int NumActivePlayers { get; init; }
    public int YourSeatIndex { get; init; }
    public int DealerIndex { get; init; }
    public int SmallBlind { get; init; }
    public int BigBlind { get; init; }
}
