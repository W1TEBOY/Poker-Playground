// Poker.WebSockets/PokerWebSocketHandler.cs
using Poker.Core;
using Poker.Core.Models;
using System.Net.WebSockets;
using System.Text;

namespace Poker.WebSockets
{
    /// <summary>
    /// Minimal WebSocket handler to compile.
    /// Replace with your real JSON serialization & loop logic.
    /// </summary>
    public class PokerWebSocketHandler
    {
        private readonly PokerEngine _engine;

        public PokerWebSocketHandler( PokerEngine engine )
        {
            _engine = engine;
        }

        public async Task HandleAsync( WebSocket socket )
        {
            var buffer = new byte[4096];
            var result = await socket.ReceiveAsync(buffer, CancellationToken.None);

            while ( !result.CloseStatus.HasValue )
            {
                // Echo or call into your engine
                var msg = Encoding.UTF8.GetString(buffer, 0, result.Count);

                // Convert "player1" to a Guid and msg to PlayType
                var playerId = Guid.Parse("player1"); // Replace with actual Guid for player1
                var playType = Enum.Parse<PlayType>(msg); // Replace with actual PlayType parsing logic

                _engine.ApplyMove(playerId, playType);

                var response = "Move applied successfully"; // Example response
                var outBytes = Encoding.UTF8.GetBytes(response);
                await socket.SendAsync(outBytes, WebSocketMessageType.Text, true, CancellationToken.None);

                result = await socket.ReceiveAsync(buffer, CancellationToken.None);
            }

            await socket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
        }
    }
}
