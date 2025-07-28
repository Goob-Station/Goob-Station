using System.Net;

namespace Content.Corvax.Interfaces.Server;

public interface IServerVPNGuardManager
{
    public void Initialize();
    public Task<bool> IsConnectionVpn(IPAddress ip);
}
