using Robust.Shared.GameStates;
using Robust.Shared.Serialization;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Hastur.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class HasturDevouringComponent : Component
{
    [DataField]
    public string Normal = "hasturM";

    [DataField]
    public string Devouring = "hastur_devour";

    [DataField]
    public TimeSpan DevourDuration = TimeSpan.FromSeconds(1);
}

[NetSerializable, Serializable]
public enum DevourVisuals : byte
{
    Devouring,
}

