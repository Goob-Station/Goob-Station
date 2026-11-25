using Content.Shared._Funkystation.Explosion.EntitySystems;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Funkystation.Explosion.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(SharedExplosionEffectSystem))]
public sealed partial class ExplosionEffectComponent : Component
{
    /// <summary>
    ///     A list of entities spawned at the epicenter, all at the same time
    /// </summary>
    [DataField, AutoNetworkedField]
    public List<EntProtoId> VisualEffects = new()
    {
        "ExplosionEffectGrenade",
        "ExplosionEffectGrenadeShockWave",
        "ExplosionEffectGrenadeSmoke",
        "ExplosionEffectGrenadeEmbers",
        "ExplosionEffectGrenadeGlowingEmbers"
    };

    [DataField, AutoNetworkedField]
    public List<EntProtoId> ShrapnelEffects = new() { "ExplosionEffectShrapnel1", "ExplosionEffectShrapnel2" };

    [DataField, AutoNetworkedField]
    public int MinShrapnel = 5;

    [DataField, AutoNetworkedField]
    public int MaxShrapnel = 9;

    [DataField, AutoNetworkedField]
    public float ShrapnelSpeed = 5f;
}
