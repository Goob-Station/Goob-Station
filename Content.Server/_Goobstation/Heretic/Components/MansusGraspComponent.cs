using Content.Server.Heretic.EntitySystems;

namespace Content.Server.Heretic.Components;

[RegisterComponent, Access(typeof(MansusGraspSystem))]
public sealed partial class MansusGraspComponent : Component
{
    [DataField] public string? Path = null;

    [DataField] public TimeSpan CooldownAfterUse = TimeSpan.FromSeconds(10);
}
