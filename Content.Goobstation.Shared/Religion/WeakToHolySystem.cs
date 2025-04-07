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

    private const string BiologicalContainer = "Biological";
    private const string BiologicalMetaphysicalContainer = "BiologicalMetaphysical";
    private const string HolyDamageType = "Holy";
    private const int HealingAmount = 10; // Positive for display, will be negated when applying.
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<WeakToHolyComponent, ComponentStartup>(OnCompStartup);
        SubscribeLocalEvent<HereticRitualRuneComponent, StartCollideEvent>(OnCollide);
        SubscribeLocalEvent<HereticRitualRuneComponent, EndCollideEvent>(OnCollideEnd);
        SubscribeLocalEvent<PassiveDamageComponent, ComponentRemove>(OnComponentRemoved);
    }

    private void OnCompStartup(Entity<WeakToHolyComponent> ent, ref ComponentStartup args)
    {
        if (!_netManager.IsServer || TryComp<DamageableComponent>(ent, out var damageable) && damageable.DamageContainerID == BiologicalContainer)
            _damageableSystem.ChangeDamageContainer(ent, BiologicalMetaphysicalContainer);
    }

    // Heal passively on heretic runes.
    private void OnCollide(EntityUid uid, HereticRitualRuneComponent component, ref StartCollideEvent args)
    {
        var heretic = EnsureComp<PassiveDamageComponent>(args.OtherEntity);

        if (!HasComp<WeakToHolyComponent>(args.OtherEntity))
            return;

        // Store the original DamageCap if it hasn't already been stored
        if (!_originalDamageCaps.ContainsKey(args.OtherEntity))
            _originalDamageCaps[args.OtherEntity] = heretic.DamageCap;

        heretic.Damage.DamageDict.TryAdd(HolyDamageType, -HealingAmount);
        heretic.DamageCap = FixedPoint2.New(0);
        DirtyEntity(args.OtherEntity);

    }

    private void OnCollideEnd(EntityUid uid, HereticRitualRuneComponent component, ref EndCollideEvent args)
    {
        TryReturnOriginalDamage(uid);
    }

    private void OnComponentRemoved(EntityUid uid, PassiveDamageComponent comp, ComponentRemove args)
    {
        TryReturnOriginalDamage(uid);
    }

    private void TryReturnOriginalDamage(EntityUid target)
    {
        if (!TryComp<PassiveDamageComponent>(target, out var heretic))
            return;

        // Restore the original DamageCap if it was stored
        if (_originalDamageCaps.TryGetValue(target, out var originalCap))
        {
            heretic.DamageCap = originalCap;
            _originalDamageCaps.Remove(target); // Clean up after restoring
        }

        heretic.Damage.DamageDict.Remove(HolyDamageType);
        DirtyEntity(target);
    }

    public override void Shutdown()
    {
        _originalDamageCaps.Clear();
        base.Shutdown();
    }
}
