using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;

namespace Omegle.Client.HubClients
{
    public class VideoHubClient
    {
        private HubConnection? _hubConnection;
        private readonly NavigationManager _navigationManager;
        public event Func<string, string, Task>? OnReceiveOffer;
        public event Func<string,string, Task>? OnReceiveAnswer;
        public event Func<string, string, bool, Task>? OnReceiveIce;

        private SemaphoreSlim _semaphore = new SemaphoreSlim(1);

        public VideoHubClient(NavigationManager navigationManager)
        {
            _navigationManager = navigationManager;
        }

        public async Task<HubConnection> CheckSetupHubAsync()
        {
            await _semaphore.WaitAsync();
            try
            {
                if (_hubConnection is not null && _hubConnection.State == HubConnectionState.Disconnected)
                {
                    await _hubConnection.StopAsync();
                    await _hubConnection.DisposeAsync();
                    _hubConnection = null;
                }

                if (_hubConnection is null)
                {
                    _hubConnection = new HubConnectionBuilder()
                        .WithUrl(_navigationManager.ToAbsoluteUri("/Hubs/VideoHub"))
                        .Build();

                    _hubConnection.On("ReceiveOffer", (string connectionId, string offer) =>
                    {
                        if (OnReceiveOffer is not null)
                        {
                            _ = OnReceiveOffer.Invoke(connectionId, offer);
                        }
                    });

                    _hubConnection.On("ReceiveAnswer", (string connectionId,string answer) =>
                    {
                        if (OnReceiveAnswer is not null)
                        {
                            _ = OnReceiveAnswer.Invoke(connectionId, answer);
                        }
                    });

                    _hubConnection.On("ReceiveIce", (string connectionId, string ice, bool destinationlocal) =>
                    {
                        if (OnReceiveIce is not null)
                        {
                            _ = OnReceiveIce.Invoke(connectionId, ice, destinationlocal);
                        }
                    });



                    await _hubConnection.StartAsync();
                }
                return _hubConnection;
            }
            finally { _semaphore.Release(); }
        }

        public async Task MakeOffer(string offer)
        {
            if (_hubConnection is not null)
            {
                await _hubConnection.SendAsync("MakeOffer", offer);
            }
        }

        public async Task SendAnswer(string connectionId, string answer)
        {
            if (_hubConnection is not null)
            {
                await _hubConnection.SendAsync("SendAnswer", connectionId, answer);
            }
        }

        public async Task SendIce(string connectionId, string ice, bool destinationlocal)
        {
            if (_hubConnection is not null)
            {
                await _hubConnection.SendAsync("SendIce", connectionId, ice, destinationlocal);
            }
        }



        public async ValueTask DisposeAsync()
        {
            if (_hubConnection is not null)
            {
                await _hubConnection.DisposeAsync();
            }
        }
    }
}
