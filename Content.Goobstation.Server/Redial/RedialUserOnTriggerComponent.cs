using Robust.Shared.GameObjects;
using Robust.Shared.Serialization.Manager.Attributes;

namespace Content.Goobstation.Server.Redial;

[RegisterComponent]
public sealed partial class RedialUserOnTriggerComponent : Component
{
    [DataField]
    public string Address = string.Empty;
}
