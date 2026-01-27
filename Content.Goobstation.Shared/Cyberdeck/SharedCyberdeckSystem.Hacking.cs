using System.Linq;
using Content.Goobstation.Common.Access;
using Content.Goobstation.Common.Cyberdeck.Components;
using Content.Shared._EinsteinEngines.Silicon.Components;
using Content.Shared.Access.Components;
using Content.Shared.DoAfter;
using Content.Shared.IdentityManagement;
using Content.Shared.Popups;
using Content.Shared.Silicons.Borgs.Components;
using Content.Shared.Silicons.StationAi;
using Robust.Shared.Utility;

namespace Content.Goobstation.Shared.Cyberdeck;

public abstract partial class SharedCyberdeckSystem
{
    private void InitializeHacking()
    {
        SubscribeLocalEvent<CyberdeckUserComponent, CyberdeckHackActionEvent>(OnStartHacking);
        SubscribeLocalEvent<CyberdeckHackableComponent, CyberdeckHackDoAfterEvent>(OnHacked);

        SubscribeLocalEvent<AccessReaderComponent, CyberdeckHackDeviceEvent>(OnAccessHacked);

        SubscribeLocalEvent<CyberdeckHackableComponent, StationAiLightEvent>(OnAiHacking, before: new [] { typeof(SharedStationAiSystem) } );
        SubscribeLocalEvent<CyberdeckHackableComponent, StationAiBoltEvent>(OnAiHacking, before: new [] { typeof(SharedStationAiSystem) } );
        SubscribeLocalEvent<CyberdeckHackableComponent, StationAiEmergencyAccessEvent>(OnAiHacking, before: new [] { typeof(SharedStationAiSystem) } );
        SubscribeLocalEvent<CyberdeckHackableComponent, StationAiElectrifiedEvent>(OnAiHacking, before: new [] { typeof(SharedStationAiSystem) } );

        SubscribeLocalEvent<BorgChassisComponent, BeforeCyberdeckHackEvent>(BeforeBorgHacked);
        SubscribeLocalEvent<BorgChassisComponent, AfterCyberdeckHackEvent>(AfterBorgHacked);
        SubscribeLocalEvent<SiliconComponent, AfterCyberdeckHackEvent>(AfterSiliconHacked);
    }

    private void OnStartHacking(Entity<CyberdeckUserComponent> ent, ref CyberdeckHackActionEvent args)
    {
        var (uid, component) = ent;

        if (args.Handled
            || args.Target == uid)
            return;

        args.Handled = true;

        EntityUid? target = null;

        // Starting with most specific cases, moving to most common ones for code safety
        // Prioritize containers over hands, because we want to be able to hack IPCs and borgs
        if (_containerQuery.TryComp(args.Target, out var containerComp))
        {
            // If it's a container, find anything hackable and hack it.
            // No, I won't stack loops inside an if statement, because birds will start migrating to such Nested code.
            foreach (var container in _container.GetAllContainers(args.Target, containerComp))
            {
                var containerTarget = container.ContainedEntities.FirstOrNull(_hackableQuery.HasComp);
                if (containerTarget == null)
                    continue;

                target = containerTarget.Value;
                break;
            }
        }

        if (_handsQuery.TryComp(args.Target, out var handsComp)
            && target == null)
        {
            // Check all hands for something that can be hacked
            var items = _hands.EnumerateHeld((args.Target, handsComp)).ToList();
            foreach (var item in items)
            {
                if (!_hackableQuery.HasComp(item))
                    continue;

                target = item;
                break;
            }
        }

        if (_hackableQuery.HasComp(args.Target))
            target = args.Target;

        // To be safe we get the component itself only here.
        if (!_hackableQuery.TryComp(target, out var hackable))
            return;

        if (_hands.GetActiveItem(uid) != null)
        {
            Popup.PopupClient(Loc.GetString("cyberdeck-needs-free-hand"), uid, uid);
            return;
        }

        // Make a popup and return if not enough charges
        if (component.ProviderEntity != null
            && !CheckCharges(uid, component.ProviderEntity.Value, hackable.Cost, target))
            return;

        // Balancing it via ref event that prevents you from hacking IPC batteries in 2 seconds.
        var beforeEv = new BeforeCyberdeckHackEvent();
        RaiseLocalEvent(target.Value, ref beforeEv);
        RaiseLocalEvent(args.Target, ref beforeEv);
        var penaltyTime = beforeEv.PenaltyTime;

        var ev = new DoAfterArgs(
            EntityManager,
            uid,
            hackable.HackingTime + penaltyTime,
            new CyberdeckHackDoAfterEvent(),
            target,
            target,
            component.ProviderEntity,
            uid)
        {
            BlockDuplicate = true,
            BreakOnDamage = true,
            //BreakOnMove = true, // Reenable if you want to nerf it
            BreakOnWeightlessMove = false,
            DistanceThreshold = 20f,
            Broadcast = false,
            Hidden = true,
            RequireCanInteract = false,
            ColorOverride = Color.Aquamarine,
        };

        if (!_doAfter.TryStartDoAfter(ev))
            return;

        var message = Loc.GetString("cyberdeck-start-hacking", ("target", Identity.Entity(target.Value, EntityManager, uid)));
        Popup.PopupClient(message, uid, uid);
        _audio.PlayLocal(component.UserHackingSound, uid, uid);

        var afterEv = new AfterCyberdeckHackEvent();
        RaiseLocalEvent(target.Value, ref afterEv);
        RaiseLocalEvent(args.Target, ref afterEv);
    }

    private void OnHacked(Entity<CyberdeckHackableComponent> ent, ref CyberdeckHackDoAfterEvent args)
    {
        if (args.Cancelled
            || args.Handled
            || ent.Owner != args.Target
            || !TryHackDevice(args.User, ent.Owner))
            return;

        // This evil hacking events chain is required to handle charges properly if target has multiple components.
        // For example, hacking an Airlock will open it AND add IgnoreAccess, but it will take charges only once.
        var ev = new CyberdeckHackDeviceEvent(args.User);
        RaiseLocalEvent(ent.Owner, ref ev);

        // Oops. Compensate charges if we failed
        if (ev.Refund)
        {
            if (!_cyberdeckUserQuery.TryComp(args.User, out var userComp)
                || userComp.ProviderEntity == null)
                return;

            Charges.AddCharges(userComp.ProviderEntity.Value, ent.Comp.Cost);
        }
        else
        {
            // Spawn hacking effect entity that can be seen by the station AI
            var pos = Transform(ent).Coordinates;
            if (ent.Comp.AfterHackingEffect != null
                && _net.IsServer) // Visibility isn't predicted I guess, so for half a second you can see the trace appearing if it was spawned it prediction...
                SpawnAtPosition(ent.Comp.AfterHackingEffect, pos);
        }
    }

    private void OnAccessHacked(Entity<AccessReaderComponent> ent, ref CyberdeckHackDeviceEvent args)
    {
        var ignore = EnsureComp<IgnoreAccessComponent>(ent);
        ignore.Ignore.Add(args.User);
    }

    private void OnAiHacking<T>(Entity<CyberdeckHackableComponent> target, ref T args) where T : BaseStationAiAction
    {
        if (_cyberdeckUserQuery.HasComp(args.User))
            args.Cancelled = !TryHackDevice(args.User, target);
    }

    private void BeforeBorgHacked(Entity<BorgChassisComponent> ent, ref BeforeCyberdeckHackEvent args)
    {
        if (args.Handled)
            return;

        args.PenaltyTime += ent.Comp.CyberdeckPenaltyTime;
        args.Handled = true;
    }

    private void AfterSiliconHacked(Entity<SiliconComponent> ent, ref AfterCyberdeckHackEvent args)
    {
        if (args.Handled)
            return;

        _audio.PlayGlobal(ent.Comp.VictimHackedSound, args.Target);
        Popup.PopupEntity(Loc.GetString("cyberdeck-player-get-hacked"),
            ent.Owner,
            ent.Owner,
            PopupType.LargeCaution);

        args.Handled = true;
    }

    private void AfterBorgHacked(Entity<BorgChassisComponent> ent, ref AfterCyberdeckHackEvent args)
    {
        if (args.Handled)
            return;

        _audio.PlayGlobal(ent.Comp.VictimHackedSound, args.Target);
        Popup.PopupEntity(Loc.GetString("cyberdeck-player-get-hacked"),
            ent.Owner,
            ent.Owner,
            PopupType.LargeCaution);

        args.Handled = true;
    }
}
