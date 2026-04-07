using Content.Shared.Whitelist;

namespace Content.Goobstation.Shared.Disease.Components;

/// <summary>
/// This is used for spreading diseases on collide events
/// </summary>
[RegisterComponent]
public sealed partial class DiseaseOnCollideComponent : Robust.Shared.GameObjects.Component
{
    [DataField]
    public DiseaseSpreadSpecifier SpreadParams = new(1f, 1f, "Debug");

    /// <summary>
    /// Entities which we should infect on collide
    /// </summary>
    [DataField]
    public EntityWhitelist? Whitelist;

    /// <summary>
    /// Entities which we should not infect on collide
    /// </summary>
    [DataField]
    public EntityWhitelist? Blacklist;

    // minimum progress required to infect
    [DataField]
    public float InfectionProgressRequired = 0.1f;

    // Cooldown
    [DataField]
    public TimeSpan CooldownInterval = TimeSpan.FromSeconds(15);
    public TimeSpan Cooldown = TimeSpan.FromSeconds(0); // no datafield as it's for internal

}
