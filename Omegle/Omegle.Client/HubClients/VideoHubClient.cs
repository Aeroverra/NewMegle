using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;

namespace Omegle.Client.HubClients
{
    public class VideoHubClient
    {
        private HubConnection? _hubConnection;
        private readonly NavigationManager _navigationManager;
        public event Func<string, string, Task>? OnReceiveOffer;
        public event Func<string, string, Task>? OnReceiveAnswer;
        public event Func<string, string, Task>? OnReceiveIce;
        public event Func<string, Task>? OnInitConnection;

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
                     
                    _hubConnection.On("InitConnection", (string connectionId) =>
                    {
                        if (OnInitConnection is not null)
                        {
                            _ = OnInitConnection.Invoke(connectionId);
                        }
                    });

                    _hubConnection.On("ReceiveOffer", (string connectionId, string offer) =>
                    {
                        if (OnReceiveOffer is not null)
                        {
                            _ = OnReceiveOffer.Invoke(connectionId, offer);
                        }
                    });

                    _hubConnection.On("ReceiveAnswer", (string connectionId, string answer) =>
                    {
                        if (OnReceiveAnswer is not null)
                        {
                            _ = OnReceiveAnswer.Invoke(connectionId, answer);
                        }
                    });

                    _hubConnection.On("ReceiveIce", (string connectionId, string ice) =>
                    {
                        if (OnReceiveIce is not null)
                        {
                            _ = OnReceiveIce.Invoke(connectionId, ice);
                        }
                    });



                    await _hubConnection.StartAsync();
                }
                return _hubConnection;
            }
            finally { _semaphore.Release(); }
        }

        public async Task JoinLobby()
        {
            if (_hubConnection is not null)
            {
                await _hubConnection.SendAsync("JoinLobby");
            }
        }

        public async Task SendOffer(string connectionId, string offer)
        {
            if (_hubConnection is not null)
            {
                await _hubConnection.SendAsync("SendOffer", connectionId, offer);
            }
        }

        public async Task SendAnswer(string connectionId, string answer)
        {
            if (_hubConnection is not null)
            {
                await _hubConnection.SendAsync("SendAnswer", connectionId, answer);
            }
        } 

        public async Task SendIce(string connectionId, string ice)
        {
            if (_hubConnection is not null)
            {
                await _hubConnection.SendAsync("SendIce", connectionId, ice);
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
