using Content.Shared.Damage;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Terror.Components;

/// <summary>
/// Ticks damage and periodic stuns on infected, then bursts into
/// a spiderling with a long stun once delay is up. Cleared early by chugging pesticide
/// </summary>
[RegisterComponent]
public sealed partial class InfestedComponent : Component
{
    [DataField]
    public TimeSpan DamageInterval = TimeSpan.FromSeconds(10);

    public TimeSpan NextDamageTick;

    [DataField(required: true)]
    public DamageSpecifier Damage = new();

    [DataField]
    public TimeSpan StunInterval = TimeSpan.FromSeconds(30);

    public TimeSpan NextStunTick;

    [DataField]
    public TimeSpan StunDuration = TimeSpan.FromSeconds(2);

    /// <summary>
    /// How long until this bursts.
    /// </summary>
    [DataField]
    public TimeSpan BurstDelay = TimeSpan.FromSeconds(180);

    public TimeSpan BurstAt;

    /// <summary>
    /// Long stun dealt by the bursting, so the spiderling has a chance to get away or be taken by ghostrole.
    /// </summary>
    [DataField]
    public TimeSpan BurstStun = TimeSpan.FromSeconds(20);

    [DataField]
    public SoundSpecifier SpawnSound = new SoundPathSpecifier("/Audio/_Goobstation/Heretic/hiss/lowHiss3.ogg");

    [DataField]
    public List<EntProtoId> SpiderlingPrototypes = new()
    {
        "SpiderlingRed",
        "SpiderlingGray",
        "SpiderlingGreen",
    };
}
