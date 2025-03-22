using Robust.Shared.GameObjects;

namespace Content.Server._Goobstation.Teleportation.Components
{
    [RegisterComponent]
    public sealed partial class WarpParentOnTriggerComponent : Component
    {
        [DataField] public string WarpLocation { get; set; } = "CentComm";
    }
}
