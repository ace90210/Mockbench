using Microsoft.AspNetCore.SignalR;
using Mockbench.Shared.Models.General;

namespace Mockbench.Services.Hubs
{
    public class RequestHub : Hub
    {
        public async Task SendRequest(DateTime time, HttpRequestDto message)
        {
            await Clients.All.SendAsync("ReceiveMessage", time, message);
        }
    }
}
