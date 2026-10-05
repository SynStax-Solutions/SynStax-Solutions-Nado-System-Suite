using Microsoft.AspNetCore.SignalR;

namespace Nado_Graphics_MS_Backend.Hubs
{
    //This hub provides the SignalR connection used for live Job Card updates.
    public class JobCardHub : Hub
    {
        //This class currently uses the default SignalR hub behaviour.
        //Job Card services send update notifications through this hub.
    }
}
