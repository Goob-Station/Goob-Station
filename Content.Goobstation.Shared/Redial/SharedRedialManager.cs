using Robust.Shared.IoC;
using Robust.Shared.Network;

namespace Content.Goobstation.Shared.Redial;

public abstract class SharedRedialManager : IPostInjectInit
{
    [Dependency] protected readonly INetManager _netManager = default!;

    public void PostInject()
    {
        Initialize();
    }

    public virtual void Initialize()
    {

    }
}
