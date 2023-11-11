using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Omegle.Client.HubClients;
using System;

namespace Omegle.Client.Pages
{
    public partial class Streamer : ComponentBase, IAsyncDisposable
    {
        [Inject] private IJSRuntime JavaScript { get; set; } = null!;
        [Inject] private NavigationManager NavigationManager { get; set; } = null!;

        private VideoHubClient? VideoHubClient = null;

        private IJSObjectReference? _localScript;
        private DotNetObjectReference<Streamer>? _streamerClass;
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                _streamerClass = DotNetObjectReference.Create(this);
                _localScript = await JavaScript.InvokeAsync<IJSObjectReference>("import", $"/Pages/{nameof(Streamer)}.razor.js");
                await _localScript.InvokeVoidAsync("setup", _streamerClass);
                VideoHubClient = new VideoHubClient(NavigationManager);
                VideoHubClient.OnInitConnection += VideoHubClient_OnInitConnection;
                VideoHubClient.OnReceiveOffer += VideoHubClient_OnReceiveOffer;
                VideoHubClient.OnReceiveAnswer += VideoHubClient_OnReceiveAnswer;
                VideoHubClient.OnReceiveIce += VideoHubClient_OnReceiveIce;
                await VideoHubClient.CheckSetupHubAsync();
                await VideoHubClient.JoinLobby();
            }
        }

        private async Task VideoHubClient_OnInitConnection(string connectionId)
        {
            if (_localScript is not null)
            {
                var offer = await _localScript.InvokeAsync<string>("createOffer", connectionId);
                await VideoHubClient!.SendOffer(connectionId, offer);
            }
        }

        private async Task VideoHubClient_OnReceiveOffer(string connectionId, string offer)
        {
            if (_localScript is not null)
            {
                var answer = await _localScript.InvokeAsync<string>("acceptOffer", connectionId, offer);
                await VideoHubClient!.SendAnswer(connectionId, answer);
            }
        }

        private async Task VideoHubClient_OnReceiveAnswer(string connectionId, string answer)
        {
            if (_localScript is not null)
            {
                await _localScript.InvokeVoidAsync("acceptAnswer", connectionId, answer);
            }
        }

        private async Task VideoHubClient_OnReceiveIce(string connectionId, string ice)
        {
            if (_localScript is not null)
            {
                await _localScript.InvokeVoidAsync("acceptIce", connectionId, ice);
            }
        }





        [JSInvokable]
        public async Task SendIce(string connectionId, string ice)
        {
            await VideoHubClient!.SendIce(connectionId, ice);
        }

        public async ValueTask DisposeAsync()
        {
            if (VideoHubClient is not null)
            {
                VideoHubClient.OnInitConnection -= VideoHubClient_OnInitConnection;
                VideoHubClient.OnReceiveOffer -= VideoHubClient_OnReceiveOffer;
                VideoHubClient.OnReceiveAnswer -= VideoHubClient_OnReceiveAnswer;
                VideoHubClient.OnReceiveIce -= VideoHubClient_OnReceiveIce;
            }
            if (_localScript is not null)
            {
                await _localScript.DisposeAsync();
            }
        }
    }
}