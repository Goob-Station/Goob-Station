using Content.Server.Body.Systems;
using Content.Server.DoAfter;
using Content.Server.Ghost.Roles.Components;
using Content.Server.Jittering;
using Content.Server.Popups;
using Content.Shared._White.Xenomorphs;
using Content.Shared._White.Xenomorphs.Larva;
using Content.Shared.DoAfter;
using Content.Shared.IdentityManagement;
using Content.Shared.Popups;
using Robust.Server.Containers;
using Robust.Shared.Containers;
using Robust.Shared.Player;
using Content.Shared.Damage; // Omu
using Content.Shared._Shitmed.Targeting; // Omu

namespace Content.Server._White.Xenomorphs.Larva;

public sealed class XenomorphLarvaSystem : EntitySystem
{
    [Dependency] private readonly BodySystem _body = default!;
    [Dependency] private readonly ContainerSystem _container = default!;
    [Dependency] private readonly DoAfterSystem _doAfter = default!;
    [Dependency] private readonly JitteringSystem _jitter = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly DamageableSystem _damageableSystem = default!; // Omu

    public override void Initialize()
    {
        SubscribeLocalEvent<XenomorphLarvaComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<XenomorphLarvaComponent, EntGotRemovedFromContainerMessage>(OnGotRemovedFromContainer);
        SubscribeLocalEvent<XenomorphLarvaComponent, TakeGhostRoleEvent>(OnTakeGhostRole);
        SubscribeLocalEvent<XenomorphLarvaComponent, LarvaBurstDoAfterEvent>(OnLarvaBurstDoAfter);
    }

    private void OnShutdown(EntityUid uid, XenomorphLarvaComponent component, ComponentShutdown args)
    {
        if (component.Victim.HasValue)
            RemComp<XenomorphLarvaVictimComponent>(component.Victim.Value);
    }

    private void OnGotRemovedFromContainer(EntityUid uid, XenomorphLarvaComponent component, EntGotRemovedFromContainerMessage args)
    {
        if (component.Victim.HasValue)
            RemComp<XenomorphLarvaVictimComponent>(component.Victim.Value);
    }

    private void OnTakeGhostRole(EntityUid uid, XenomorphLarvaComponent component, TakeGhostRoleEvent args)
    {
        if (component.Victim is not {} victim)
            return;

        var doAfterEventArgs = new DoAfterArgs(EntityManager, uid, component.BurstDelay, new LarvaBurstDoAfterEvent(), uid, target: component.Victim)
        {
            NeedHand = false,
            BreakOnDamage = false,
            BreakOnMove = false,
            Hidden = true,
            CancelDuplicate = true,
            BlockDuplicate = true,
            DuplicateCondition = DuplicateConditions.SameEvent
        };

        if (!_doAfter.TryStartDoAfter(doAfterEventArgs))
            return;

        _popup.PopupEntity(Loc.GetString("xenomorphs-burst-victim"), victim, victim, PopupType.MediumCaution);
        _popup.PopupEntity(Loc.GetString("xenomorphs-burst-other", ("victim", Identity.Entity(victim, EntityManager))), victim, Filter.PvsExcept(victim), true, PopupType.LargeCaution);

        _jitter.DoJitter(victim, component.BurstDelay, true);
    }

    private void OnLarvaBurstDoAfter(EntityUid uid, XenomorphLarvaComponent component, LarvaBurstDoAfterEvent args)
    {
        if (!_container.TryGetContainingContainer((uid, null, null), out var container)
            || component.Victim is not { } victim)
            return;

        _container.Remove(uid, container);
        var damage = new DamageSpecifier(); // Omu start
        damage.DamageDict.Add("Blunt", 120);
        damage.DamageDict.Add("Piercing", 80);
        _damageableSystem.TryChangeDamage(uid: victim, damage: damage, ignoreResistances: true, interruptsDoAfters: false, targetPart: TargetBodyPart.Chest);
        //_body.GibBody(victim); // Omu end
    }
}
