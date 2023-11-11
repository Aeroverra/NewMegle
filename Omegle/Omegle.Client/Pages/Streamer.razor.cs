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
                VideoHubClient = new VideoHubClient(NavigationManager);
                VideoHubClient.OnReceiveOffer += VideoHubClient_OnReceiveOffer;
                VideoHubClient.OnReceiveAnswer += VideoHubClient_OnReceiveAnswer;
                VideoHubClient.OnReceiveIce += VideoHubClient_OnReceiveIce;
                await VideoHubClient.CheckSetupHubAsync();
                var offer = await _localScript.InvokeAsync<string>("createStream");
                await VideoHubClient.MakeOffer(offer);
            }
        }

        private async Task VideoHubClient_OnReceiveIce(string connectionId, string ice, bool destinationlocal)
        {
            if (_localScript is not null)
            {
                await _localScript.InvokeVoidAsync("acceptIce", _streamerClass, connectionId, ice, destinationlocal);
            }
        }

        private async Task VideoHubClient_OnReceiveAnswer(string connectionId, string answer)
        {
            if (_localScript is not null)
            {
                await _localScript.InvokeVoidAsync("acceptAnswer", _streamerClass, connectionId, answer);
            }
        }

        private async Task VideoHubClient_OnReceiveOffer(string connectionId, string offer)
        {
            if (_localScript is not null)
            {
                var answer = await _localScript.InvokeAsync<string>("acceptOffer", _streamerClass, connectionId, offer);
                await VideoHubClient!.SendAnswer(connectionId, answer);
            }
        }

        [JSInvokable]
        public async Task SendIce(string connectionId, string ice, bool destinationlocal)
        {
            await VideoHubClient!.SendIce(connectionId, ice, destinationlocal);
        }

        public async ValueTask DisposeAsync()
        {
            if (VideoHubClient is not null)
            {
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