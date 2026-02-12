// SPDX-FileCopyrightText: 2024 LankLTE <135308300+LankLTE@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 nikthechampiongr <32041239+nikthechampiongr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Conchelle <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 SX-7 <sn1.test.preria.2002@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Species.Components;
using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Content.Shared.Popups;
using Content.Shared.Stunnable;
using Content.Shared.Mind;
using Content.Shared.Zombies;
using Robust.Shared.Containers;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Timing;

namespace Content.Shared.Species.Systems;

public sealed partial class ReformSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;
    [Dependency] private readonly INetManager _netMan = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfterSystem = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly IPrototypeManager _protoManager = default!;
    [Dependency] private readonly SharedStunSystem _stunSystem = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly SharedMindSystem _mindSystem = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ReformComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<ReformComponent, ComponentShutdown>(OnCompRemove);

        SubscribeLocalEvent<ReformComponent, ReformEvent>(OnReform);
        SubscribeLocalEvent<ReformComponent, ReformDoAfterEvent>(OnDoAfter);

        SubscribeLocalEvent<ReformComponent, EntityZombifiedEvent>(OnZombified);
    }

    private void OnMapInit(EntityUid uid, ReformComponent comp, MapInitEvent args)
    {
        // When the map is initialized, give them the action
        if (comp.ActionPrototype != default && !_protoManager.TryIndex<EntityPrototype>(comp.ActionPrototype, out var actionProto))
            return;

        _actionsSystem.AddAction(uid, ref comp.ActionEntity, out var reformAction, comp.ActionPrototype);

        // See if the action should start with a delay, and give it that starting delay if so.
        if (comp.StartDelayed && reformAction != null && reformAction.UseDelay != null)
        {
            var start = _gameTiming.CurTime;
            var end = _gameTiming.CurTime + reformAction.UseDelay.Value;

            _actionsSystem.SetCooldown(comp.ActionEntity!.Value, start, end);
        }
    }

    private void OnCompRemove(EntityUid uid, ReformComponent comp, ComponentShutdown args)
    {
        _actionsSystem.RemoveAction(uid, comp.ActionEntity);
    }

    private void OnReform(EntityUid uid, ReformComponent comp, ReformEvent args)
    {
        // Stun them when they use the action for the amount of reform time.
        if (comp.ShouldStun)
            _stunSystem.TryStun(uid, TimeSpan.FromSeconds(comp.ReformTime), true);
        _popupSystem.PopupClient(Loc.GetString(comp.PopupText, ("name", uid)), uid, uid);

        // Create a doafter & start it
        var doAfter = new DoAfterArgs(EntityManager, uid, comp.ReformTime, new ReformDoAfterEvent(), uid)
        {
            BreakOnMove = true,
            BlockDuplicate = true,
            BreakOnDamage = true,
            CancelDuplicate = true,
            RequireCanInteract = false,
            MultiplyDelay = false, // Goobstation
        };

        _doAfterSystem.TryStartDoAfter(doAfter);
        args.Handled = true;
    }

    private void OnDoAfter(EntityUid uid, ReformComponent comp, ReformDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled || comp.Deleted)
            return;

        if (_netMan.IsClient)
            return;

        EntityUid child;

        // Madness: If you just goidaspawn entities when they're in a container - Transform(ent).Coordinates will just give 0,0 and the box as the parent
        // But you're not spawning them in the parent, you're spawning them next to it while thinking they're in the container
        // Which by a mysterious mean scams networking into thinking you NEVER LEFT the container, and frankly never even got back in into the world.
        // So anyone who was out of PVS range when you respawned will not and cannot see you as you're nonexistant.
        // Your entire state hinges on the idea that you're still in the container.

        if (_container.TryGetContainingContainer((uid, null, null), out var container))
        {
            child = TrySpawnInContainer(comp.ReformPrototype, container.Owner, container.ID, out var containedChild)
                ? containedChild.Value
                : SpawnNextToOrDrop(comp.ReformPrototype, container.Owner);
        }
        else
            child = Spawn(comp.ReformPrototype, Transform(uid).Coordinates);

        // This transfers the mind to the new entity
        if (_mindSystem.TryGetMind(uid, out var mindId, out var mind))
            _mindSystem.TransferTo(mindId, child, mind: mind);

        // Delete the old entity
        QueueDel(uid);
    }

    private void OnZombified(EntityUid uid, ReformComponent comp, ref EntityZombifiedEvent args)
    {
        _actionsSystem.RemoveAction(uid, comp.ActionEntity); // Zombies can't reform
    }

    public sealed partial class ReformEvent : InstantActionEvent { }

    [Serializable, NetSerializable]
    public sealed partial class ReformDoAfterEvent : SimpleDoAfterEvent { }
}
