using Robust.Shared.GameStates;

namespace Content.Shared._Lavaland.Megafauna.Components.Idol;

/// <summary>
/// When this entity dies, it deals damage to its linked Producer
/// and then deletes itself.
/// </summary>
[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class IdolComponent : Component
{
    /// <summary>
    /// The Producer entity that summoned this idol.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid? Producer;

    /// <summary>
    /// How much damage to deal to the Producer when this idol dies.
    /// </summary>
    [DataField(required: true)]
    public float DamageOnDeath;
}
