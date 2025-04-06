using Content.Shared.Popups;
using Robust.Shared.Prototypes;
using Content.Shared.Damage;
using Robust.Shared.Audio.Systems;
using Content.Shared.Heretic;
using Content.Shared.Damage.Components;
using Robust.Shared.Network;
using Robust.Shared.Physics.Events;
using Content.Shared.FixedPoint;

namespace Content.Goobstation.Shared.Religion;

/// <summary>
/// Allows an entity to heal on heretic runes, and be weak to holy. (Rubin code, porting before nullrods are finished.)
/// I don't know if most of this actually works, but the central task of just being weak to holy does so....
/// </summary>
public sealed partial class WeakToHolySystem : EntitySystem
{
    [Dependency] private readonly INetManager _netManager = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly DamageableSystem _damageableSystem = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;

    private readonly Dictionary<EntityUid, FixedPoint2> _originalDamageCaps = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<WeakToHolyComponent, ComponentStartup>(OnCompStartup);
        SubscribeLocalEvent<HereticRitualRuneComponent, StartCollideEvent>(OnCollide);
        SubscribeLocalEvent<HereticRitualRuneComponent, EndCollideEvent>(OnCollideEnd);
    }

    private void OnCompStartup(Entity<WeakToHolyComponent> ent, ref ComponentStartup args)
    {
        if (!_netManager.IsServer || TryComp<DamageableComponent>(ent, out var damageable) && damageable.DamageContainerID == "Biological")
            _damageableSystem.ChangeDamageContainer(ent, "BiologicalMetaphysical");
    }

    // Heal passively on heretic runes.
    private void OnCollide(EntityUid uid, HereticRitualRuneComponent component, ref StartCollideEvent args)
    {
        var heretic = EnsureComp<PassiveDamageComponent>(args.OtherEntity);

        if (!HasComp<WeakToHolyComponent>(args.OtherEntity) && heretic.Damage.DamageDict.TryGetValue("Holy", out var holy))
            return;

        // Store the original DamageCap if it hasn't already been stored
        if (!_originalDamageCaps.ContainsKey(args.OtherEntity))
            _originalDamageCaps[args.OtherEntity] = heretic.DamageCap;

        heretic.Damage.DamageDict.TryAdd("Holy", -10);
        heretic.DamageCap = FixedPoint2.New(0);
        DirtyEntity(args.OtherEntity);

    }

    private void OnCollideEnd(EntityUid uid, HereticRitualRuneComponent component, ref EndCollideEvent args)
    {
        if (!TryComp<PassiveDamageComponent>(args.OtherEntity, out var heretic))
            return;

        // Restore the original DamageCap if it was stored
        if (_originalDamageCaps.TryGetValue(args.OtherEntity, out var originalCap))
        {
            heretic.DamageCap = originalCap;
            _originalDamageCaps.Remove(args.OtherEntity); // Clean up after restoring
        }

        heretic.Damage.DamageDict.Remove("Holy");
        DirtyEntity(args.OtherEntity);
    }
}
