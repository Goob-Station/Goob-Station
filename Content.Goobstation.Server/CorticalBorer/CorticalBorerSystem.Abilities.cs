using Content.Goobstation.Common.Changeling;
using Content.Goobstation.Shared.CorticalBorer;
using Content.Goobstation.Shared.CorticalBorer.Components;
using Content.Goobstation.Shared.Devil;
using Content.Goobstation.Shared.SlaughterDemon;
using Content.Server.Medical;
using Content.Shared._White.Xenomorphs.Xenomorph;
using Content.Shared.Body.Components;
using Content.Shared.DoAfter;
using Content.Shared.IdentityManagement;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Popups;

namespace Content.Goobstation.Server.CorticalBorer;

public sealed partial class CorticalBorerSystem
{
    [Dependency] private readonly VomitSystem _vomit = default!;

    private void SubscribeAbilities()
    {
        SubscribeLocalEvent<CorticalBorerComponent, CorticalInfestEvent>(OnInfest);
        SubscribeLocalEvent<CorticalBorerComponent, CorticalInfestDoAfterEvent>(OnInfestDoAfter);

        SubscribeLocalEvent<CorticalBorerComponent, CorticalEjectEvent>(OnEjectHost);
        SubscribeLocalEvent<CorticalBorerComponent, CorticalTakeControlEvent>(OnTakeControl);

        SubscribeLocalEvent<CorticalBorerComponent, CorticalChemMenuActionEvent>(OnChemicalMenu);
        SubscribeLocalEvent<CorticalBorerComponent, CorticalCheckBloodEvent>(OnCheckBlood);


        SubscribeLocalEvent<CorticalBorerInfestedComponent, CorticalEndControlEvent>(OnEndControl);
        SubscribeLocalEvent<CorticalBorerInfestedComponent, CorticalLayEggEvent>(OnLayEgg);
    }

    private void OnChemicalMenu(Entity<CorticalBorerComponent> ent, ref CorticalChemMenuActionEvent args)
    {
        if(!TryComp<UserInterfaceComponent>(ent, out var uic))
            return;

        if (ent.Comp.Host is null)
        {
            Popup.PopupEntity(Loc.GetString("cortical-borer-no-host"), ent, ent, PopupType.Medium);
            return;
        }

        UpdateUiState(ent);
        UI.TryToggleUi((ent, uic), CorticalBorerDispenserUiKey.Key, ent);
        args.Handled = true;
    }

    private void OnInfest(Entity<CorticalBorerComponent> ent, ref CorticalInfestEvent args)
    {
        var (uid, comp) = ent;
        var target = args.Target;
        var targetIdentity = Identity.Entity(target, EntityManager);

        if (comp.Host is not null)
        {
            Popup.PopupEntity(Loc.GetString("cortical-borer-has-host"), uid, uid, PopupType.Medium);
            return;
        }

        if (HasComp<CorticalBorerInfestedComponent>(target))
        {
            Popup.PopupEntity(Loc.GetString("cortical-borer-host-already-infested", ("target", targetIdentity)), uid, uid, PopupType.Medium);
            return;
        }

        if (IsInvalidHost(target))
        {
            Popup.PopupEntity(Loc.GetString("cortical-borer-invalid-host", ("target", targetIdentity)), uid, uid, PopupType.Medium);
            return;
        }

        // target is on sugar for some reason, can't go in there
        if (!CanUseAbility(ent, target))
            return;

        var infestAttempt = new InfestHostAttempt();
        RaiseLocalEvent(target, infestAttempt);

        if (infestAttempt.Cancelled)
        {
            Popup.PopupEntity(Loc.GetString("cortical-borer-face-covered", ("target", targetIdentity)), uid, uid, PopupType.Medium);
            return;
        }

        Popup.PopupEntity(Loc.GetString("cortical-borer-start-infest", ("target", targetIdentity)), uid, uid, PopupType.Medium);

        var infestArgs = new DoAfterArgs(EntityManager, uid, TimeSpan.FromSeconds(3), new CorticalInfestDoAfterEvent(), uid, target)
        {
            DistanceThreshold = 1.5f,
            BreakOnDamage = true,
            BreakOnMove = true,
            BreakOnWeightlessMove = true,
            AttemptFrequency = AttemptFrequency.StartAndEnd,
            Hidden = true,
        };

        _doAfter.TryStartDoAfter(infestArgs);
    }

    // anything with bloodstream, BUT NOT THIS!!!
    private bool IsInvalidHost(EntityUid target)
    {
        return !HasComp<BloodstreamComponent>(target) ||
               HasComp<CorticalBorerComponent>(target) ||
               HasComp<DevilComponent>(target) ||
               HasComp<SlaughterDemonComponent>(target) ||
               HasComp<ChangelingComponent>(target) ||
               HasComp<XenomorphComponent>(target);
    }

    private void OnInfestDoAfter(Entity<CorticalBorerComponent> ent, ref CorticalInfestDoAfterEvent args)
    {
        if (args.Handled)
            return;

        if (args.Args.Target is not { } target)
            return;

        if (args.Cancelled || HasComp<CorticalBorerInfestedComponent>(target))
            return;

        if (!CanUseAbility(ent, target))
            return;

        if (!HasComp<BloodstreamComponent>(target) || HasComp<CorticalBorerComponent>(target))
            return;

        InfestTarget(ent, target);
        args.Handled = true;
    }

    private void OnEjectHost(Entity<CorticalBorerComponent> ent, ref CorticalEjectEvent args)
    {
        if (args.Handled)
            return;

        var (uid, comp) = ent;

        if (comp.Host is null)
        {
            Popup.PopupEntity(Loc.GetString("cortical-borer-no-host"), uid, uid, PopupType.Medium);
            return;
        }

        if (TryEjectBorer(ent))
            args.Handled = true;
    }

    private void OnCheckBlood(Entity<CorticalBorerComponent> ent, ref CorticalCheckBloodEvent args)
    {
        if (args.Handled)
            return;

        if (ent.Comp.Host is null)
        {
            Popup.PopupEntity(Loc.GetString("cortical-borer-no-host"), ent, ent, PopupType.Medium);
            return;
        }

        if (TryToggleCheckBlood(ent))
            args.Handled = true;
    }

    private void OnTakeControl(Entity<CorticalBorerComponent> ent, ref CorticalTakeControlEvent args)
    {
        if (args.Handled)
            return;

        if (ent.Comp.Host is null)
        {
            Popup.PopupEntity(Loc.GetString("cortical-borer-no-host"), ent, ent, PopupType.Medium);
            return;
        }

        // Host is dead, you can't take control
        if (TryComp<MobStateComponent>(ent.Comp.Host, out var mobState) &&
            mobState.CurrentState == MobState.Dead)
        {
            Popup.PopupEntity(Loc.GetString("cortical-borer-dead-host"), ent, ent, PopupType.Medium);
            return;
        }

        if (!TryComp<CorticalBorerInfestedComponent>(ent.Comp.Host, out var infestedComp))
            return;

        if (!CanUseAbility(ent, ent.Comp.Host.Value))
            return;

        // idk how you would cause this...
        if (ent.Comp.ControlingHost)
        {
            Popup.PopupEntity(Loc.GetString("cortical-borer-already-control"), ent, ent, PopupType.Medium);
            return;
        }

        if (!TryUseAbility(ent, ent, args.Action))
            return;

        TakeControlHost(ent, infestedComp);

        args.Handled = true;
    }

    private void OnEndControl(Entity<CorticalBorerInfestedComponent> host, ref CorticalEndControlEvent args)
    {
        if (args.Handled)
            return;

        EndControl(host);

        args.Handled = true;
    }

    private void OnLayEgg(Entity<CorticalBorerInfestedComponent> host, ref CorticalLayEggEvent args)
    {
        if (args.Handled)
            return;

        var borer = host.Comp.Borer;

        if (!TryUseAbility(borer, host, args.Action))
            return;

        _vomit.Vomit(host, -20, -20); // half as much chem vomit, a lot that is coming up is the egg
        LayEgg(borer);

        args.Handled = true;
    }
}
