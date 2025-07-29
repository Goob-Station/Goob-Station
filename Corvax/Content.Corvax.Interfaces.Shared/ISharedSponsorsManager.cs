using System.Diagnostics.CodeAnalysis;
using Robust.Shared.Maths;
using Robust.Shared.Network;

namespace Content.Corvax.Interfaces.Shared;

public interface ISharedSponsorsManager
{
    public void Initialize();

    // Client
    public List<string> GetClientPrototypes();

    // Server
    public bool TryGetServerPrototypes(NetUserId userId, [NotNullWhen(true)] out List<string>? prototypes);
    public bool TryGetServerOocColor(NetUserId userId, [NotNullWhen(true)] out Color? color);
    public int GetServerExtraCharSlots(NetUserId userId);
    public bool HaveServerPriorityJoin(NetUserId userId);
}
