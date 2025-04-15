// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Fishbait <Fishbait@git.ml>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 fishbait <gnesse@gmail.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Actions;
using Content.Shared.Buckle.Components;
using Content.Shared.CombatMode;
using Content.Shared.DoAfter;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Stunnable;
using Robust.Shared.Containers;
using Robust.Shared.Serialization;
using Robust.Shared.Audio.Systems;


namespace Content.Goobstation.Shared.Vehicles.Clowncar;

/* TODO
 - Enter do after when entering the vehicle         //Done
 - Roll the dice action when emaged //not sure what to do whit this one
 - Explode if someone that has drank more than 30u of irish car bomb enters the car //done
 - Spread space lube on damage with a prob of 33% - //Done
 - Repair with bananas                              //Done
 - You can buckle nonclowns as a third party        //Done

 - Player feedback like popups  //
    and chat messages
    for bumping,
    crashing,
    repairing,
    irish bomb,
    lubing,
    emag,
    squishing,
    dice roll,
    and all other features

 - add a use of thank counter                       //Done

 no canon for now: coming in -vertion 2- one week away
    - Sometimes the toggle cannon action repeats
    - Cannon fires weird in rotated grids
    - When shooting a second time the server crashes
 */
public abstract partial class SharedClowncarSystem : EntitySystem
{
    [Dependency] private readonly IComponentFactory _factory = default!;

    [Dependency] protected readonly SharedAppearanceSystem AppearanceSystem = default!;
    [Dependency] private readonly SharedTransformSystem _transformSystem = default!;
    [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfterSystem = default!;
    [Dependency] private readonly SharedHandsSystem _handsSystem = default!;
    [Dependency] private readonly SharedStunSystem _stunSystem = default!;
    [Dependency] private readonly SharedAudioSystem _audioSystem = default!;
    [Dependency] private readonly SharedCombatModeSystem _combatSystem = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ClowncarComponent, EntInsertedIntoContainerMessage>(OnEntInserted);
        SubscribeLocalEvent<ClowncarComponent, StrappedEvent>(OnBuckle);
        SubscribeLocalEvent<ClowncarComponent, UnstrappedEvent>(OnUnBuckle);
        SubscribeLocalEvent<ClowncarComponent, ClowncarFireModeActionEvent>(OnClowncarFireModeAction);
        SubscribeLocalEvent<ClowncarComponent, EntRemovedFromContainerMessage>(OnEntRemoved);
    }

    /// <summary>
    /// Handles adding the "thank rider" action to passengers
    /// </summary>
    private void OnEntInserted(EntityUid uid, ClowncarComponent component, EntInsertedIntoContainerMessage args)
    {
        if (args.Container.ID != component.Container)
            return;

        if (!TryComp<VehicleComponent>(uid, out var _))
            return;
        EnsureComp<StunnedComponent>(args.Entity);
        _actionsSystem.AddAction(args.Entity, component.ThankRiderAction, uid);
    }


    /// <summary>
    /// Handles preventing collision with the rider and
    /// adding/removing the "toggle cannon" action from the rider when available,
    /// also deactivates the cannon when the rider unbuckles
    /// </summary>
    private void OnBuckle(EntityUid uid, ClowncarComponent component, ref StrappedEvent args)
    {
        _actionsSystem.AddAction(args.Buckle.Owner, component.QuietInTheBackAction, uid);
        component.ThankCounter = 0;
    }

    private void OnUnBuckle(EntityUid uid, ClowncarComponent component, ref UnstrappedEvent args)
    {
        foreach (var ( actionId, comp ) in _actionsSystem.GetActions(args.Buckle.Owner))
        {
            if (!TryComp(actionId, out MetaDataComponent? metaData))
                continue;
            if (metaData.EntityPrototype != null && metaData.EntityPrototype == component.QuietInTheBackAction)
                _actionsSystem.RemoveAction(actionId);
        }
    }

    private void ToggleCannon(EntityUid uid, ClowncarComponent component, EntityUid user, bool activated)
    {

    }

    /// <summary>
    /// Handles making people knock down each other when fired
    /// </summary>
    private void OnEntRemoved(EntityUid uid, ClowncarComponent component, EntRemovedFromContainerMessage args)
    {

        if (args.Container.ID != component.Container)
            return;

        foreach (var ( actionId, comp ) in _actionsSystem.GetActions(args.Entity))
        {
            if (!TryComp(actionId, out MetaDataComponent? metaData))
                continue;
            if (metaData.EntityPrototype != null && metaData.EntityPrototype == component.ThankRiderAction)
                _actionsSystem.RemoveAction(actionId);
        }
        RemComp<StunnedComponent>(args.Entity);
    }
}

[Serializable, NetSerializable]
public sealed partial class ClownCarDoAfterEvent : SimpleDoAfterEvent { }
[Serializable, NetSerializable]
public sealed partial class ClownCarEnterDriverSeatDoAfterEvent : SimpleDoAfterEvent { }
[Serializable, NetSerializable]
public sealed partial class ClownCarOpenTrunkDoAfterEvent : SimpleDoAfterEvent { }
public sealed partial class ThankRiderActionEvent : InstantActionEvent { }
public sealed partial class ClowncarFireModeActionEvent : InstantActionEvent { }
public sealed partial class QuietBackThereActionEvent : InstantActionEvent { }

[Serializable, NetSerializable]
public enum ClowncarVisuals : byte
{
    FireModeEnabled
}