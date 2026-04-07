using System.ComponentModel;
using Robust.Shared.Prototypes;
using Content.Shared.Whitelist;

namespace Content.Goobstation.Shared.Disease.Components;

/// <summary>
/// This is used for spreading diseases on collide events
/// </summary>
[RegisterComponent]
public sealed partial class DiseaseOnCollideComponent : Robust.Shared.GameObjects.Component
{
    /// <summary>
    /// Disease to give to entities hit with this
    /// If null, will spread diseases had by this entity
    /// </summary>
    [DataField]
    public EntProtoId? Disease;

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

    [DataField]
    public TimeSpan CooldownInterval = TimeSpan.FromSeconds(15);

    // not datafield as its for internal
    public TimeSpan Cooldown = TimeSpan.FromSeconds(0);

}
