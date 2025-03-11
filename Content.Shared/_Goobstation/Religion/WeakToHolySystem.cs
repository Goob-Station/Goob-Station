using Content.Shared.Popups;
using Content.Shared.Mobs;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Content.Shared.Damage.Prototypes;
using System.Linq;
using Content.Shared.Damage;
using Robust.Shared.Audio.Systems;
using Content.Shared.StepTrigger.Systems;
using Content.Shared.Heretic;
using Robust.Shared.Utility;
using Content.Shared.Damage.Components;
using Robust.Shared.Network;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Events;
using Content.Shared.FixedPoint;

namespace Content.Shared._Goobstation.Religion;

public sealed partial class WeakToHolySystem : EntitySystem
{
    [Dependency] private readonly INetManager _netManager = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly DamageableSystem _damageableSystem = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<WeakToHolyComponent, ComponentInit>(OnCompInit);
        SubscribeLocalEvent<HereticRitualRuneComponent, StartCollideEvent>(OnCollide);
        SubscribeLocalEvent<HereticRitualRuneComponent, EndCollideEvent>(OnCollideEnd);
    }

    private void OnCompInit(Entity<WeakToHolyComponent> ent, ref ComponentInit args)
    {
        if (!_netManager.IsServer
            || TryComp<DamageableComponent>(ent, out var damageable) && damageable.DamageContainerID == "Biological")
        {
            _damageableSystem.ChangeDamageContainer(ent, "BiologicalMetaphysical");
        }


    }


    // passive healing on runes for aviu
    private void OnCollide(EntityUid uid, HereticRitualRuneComponent component, ref StartCollideEvent args)
    {
        var _heretic = EnsureComp<PassiveDamageComponent>(args.OtherEntity);

        if (!HasComp<WeakToHolyComponent>(args.OtherEntity) && _heretic.Damage.DamageDict.TryGetValue("Holy", out var holy)) {
            return;
        }

        var oldValue = _heretic.DamageCap;

        _heretic.Damage.DamageDict.TryAdd("Holy", -10);
        _heretic.DamageCap = new FixedPoint2(0); //why you no work
        DirtyEntity(args.OtherEntity);

    }

    private void OnCollideEnd(EntityUid uid, HereticRitualRuneComponent component, ref EndCollideEvent args)
    {
        var _heretic = EnsureComp<PassiveDamageComponent>(args.OtherEntity);
        var oldValue = _heretic.DamageCap;
        _heretic.DamageCap = oldValue;
        _heretic.Damage.DamageDict.Remove("Holy");
        DirtyEntity(args.OtherEntity);
    }
}
