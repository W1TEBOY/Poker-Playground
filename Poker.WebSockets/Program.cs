using Poker.Core;  // your engine namespace
using Poker.WebSockets; // Add this namespace for PokerWebSocketHandler

var builder = WebApplication.CreateBuilder(args);

// register your poker engine
builder.Services.AddSingleton<PokerEngine>();

var app = builder.Build();

// enable raw WebSockets
app.UseWebSockets();

// map the /ws endpoint
app.Map("/ws", async context =>
{
    if ( !context.WebSockets.IsWebSocketRequest )
    {
        context.Response.StatusCode = 400;
        return;
    }

    using var ws = await context.WebSockets.AcceptWebSocketAsync();
    var engine = context.RequestServices.GetRequiredService<PokerEngine>();
    var handler = new PokerWebSocketHandler(engine);
    await handler.HandleAsync(ws);
});

// Use RunAsync instead of Run
await app.RunAsync();
