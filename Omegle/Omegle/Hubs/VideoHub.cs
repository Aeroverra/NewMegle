using Microsoft.AspNetCore.SignalR;
using System;

namespace Omegle.Hubs
{
    public class VideoHub : Hub
    {
        public const string HubUrl = "/Hubs/VideoHub";

        public static List<string> Lobby = new();


        public async Task JoinLobby()
        {
            var otherConnectionId = Lobby.FirstOrDefault();
            if(otherConnectionId is not null)
            {
                Lobby.Remove(otherConnectionId);
                await Clients.Client(Context.ConnectionId).SendAsync("InitConnection", otherConnectionId);
            }
            else
            {
                Lobby.Add(Context.ConnectionId);
            }

        }


        public async Task SendOffer(string connectionId, string offer)
        {
            await Clients.Client(connectionId).SendAsync("ReceiveOffer", Context.ConnectionId, offer);
        }
        public async Task SendAnswer(string connectionId, string answer)
        {
            await Clients.Client(connectionId).SendAsync("ReceiveAnswer", Context.ConnectionId, answer);
        }

        public async Task SendIce(string connectionId, string ice)
        {
            await Clients.Client(connectionId).SendAsync("ReceiveIce", Context.ConnectionId, ice);
        }



        public override Task OnConnectedAsync()
        {
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            Lobby.RemoveAll(x => x == Context.ConnectionId);
            return base.OnDisconnectedAsync(exception);

        }

    }
}
