using Microsoft.AspNetCore.SignalR;
using System;

namespace Omegle.Hubs
{
    public class VideoHub : Hub
    {
        public const string HubUrl = "/Hubs/VideoHub";

        public static List<(string ConnectionId, string Offer)> Offers = new();



        public Task MakeOffer(string offer)
        {
            if (Offers.Any())
            {
                var otherOffer = Offers.First();
                Offers.Remove(otherOffer);
                Clients.Client(Context.ConnectionId).SendAsync("ReceiveOffer", otherOffer.ConnectionId, otherOffer.Offer);
                Clients.Client(otherOffer.ConnectionId).SendAsync("ReceiveOffer", Context.ConnectionId, offer);
            }
            else
            {
                Offers.Add((Context.ConnectionId, offer));
            }
            return Task.CompletedTask;
        }

        public async Task SendAnswer(string connectionId, string answer)
        {
            await Clients.Client(connectionId).SendAsync("ReceiveAnswer", Context.ConnectionId, answer);
        }

        public async Task SendIce(string connectionId, string ice, bool destinationlocal)
        {
            await Clients.Client(connectionId).SendAsync("ReceiveIce", Context.ConnectionId, ice, destinationlocal);
        }



        public override Task OnConnectedAsync()
        {
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            Offers.RemoveAll(x => x.ConnectionId == Context.ConnectionId);
            return base.OnDisconnectedAsync(exception);

        }

    }
}
