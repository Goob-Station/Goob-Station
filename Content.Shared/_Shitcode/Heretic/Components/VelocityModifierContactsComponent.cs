using System.Numerics;
using Content.Shared.Whitelist;
using Robust.Shared.GameStates;

namespace Content.Shared._Shitcode.Heretic.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class VelocityModifierContactsComponent : Component
{
    [DataField, AutoNetworkedField]
    public float Modifier = 1.0f;

    [DataField, AutoNetworkedField]
    public bool IsActive = true;

    [DataField]
    public EntityWhitelist? Whitelist;

    [DataField]
    public EntityWhitelist? Blacklist;
}

[NetworkedComponent, RegisterComponent, AutoGenerateComponentState]
public sealed partial class VelocityModifiedByContactComponent : Component
{
    [DataField, AutoNetworkedField]
    public Vector2? OriginalVelocity;
}
