// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 cheetah1984 <davidc71114@gmail.com>
// SPDX-FileCopyrightText: 2025 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Containers.ItemSlots;
using Content.Shared.DeviceLinking;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Goobstation.Shared.Factory;

/// <summary>
/// Moves items from an input area or machine to an output area or machine.
/// Uses <see cref="ExclusiveMachineComponent"/> for I/O slots.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(RoboticArmSystem))]
[AutoGenerateComponentState(fieldDeltas: true), AutoGenerateComponentPause]
public sealed partial class RoboticArmComponent : Component
{
    #region Linking
    /// <summary>
    /// Signal port invoked after an item gets moved.
    /// </summary>
    [DataField]
    public ProtoId<SourcePortPrototype> MovedPort = "RoboticArmMoved";
    #endregion

    #region Item Slot
    /// <summary>
    /// Item slot that stores the held item.
    /// </summary>
    [DataField]
    public string ItemSlotId = "robotic_arm_item";

    /// <summary>
    /// The item slot cached on init.
    /// </summary>
    [ViewVariables]
    public ItemSlot ItemSlot = default!;

    /// <summary>
    /// The currently held item.
    /// </summary>
    [ViewVariables]
    public EntityUid? HeldItem => ItemSlot.Item;

    /// <summary>
    /// Whether an item is currently held.
    /// </summary>
    public bool HasItem => ItemSlot.HasItem;
    #endregion

    #region Input Items
    /// <summary>
    /// Fixture to look for input items with when no input machine is linked.
    /// </summary>
    [DataField]
    public string InputFixtureId = "robotic_arm_input";

    /// <summary>
    /// Items currently colliding with <see cref="InputFixtureId"/> and whether their CollisionWake was enabled.
    /// When items start to collide they get pushed to the end.
    /// When picking up items the last value is taken.
    /// This is essentially a FILO queue.
    /// </summary>
    [DataField, AutoNetworkedField]
    public List<(NetEntity, bool)> InputItems = new();
    #endregion

    #region Arm Moving
    /// <summary>
    /// How long it takes to move an item.
    /// </summary>
    [DataField]
    public TimeSpan MoveDelay = TimeSpan.FromSeconds(0.6);

    /// <summary>
    /// When the arm will next move to the input or output.
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    [AutoNetworkedField, AutoPausedField]
    public TimeSpan? NextMove;

    /// <summary>
    /// Sound played when moving an item.
    /// </summary>
    [DataField]
    public SoundSpecifier? MoveSound;
    #endregion

    #region Power

    /// <summary>
    /// Power used when idle.
    /// </summary>
    [DataField]
    public float IdlePowerDraw = 100f;

    /// <summary>
    /// Power used when moving items.
    /// </summary>
    [DataField]
    public float MovingPowerDraw = 500f;

    #endregion
}

[Serializable, NetSerializable]
public enum RoboticArmVisuals : byte
{
    HasItem
}

[Serializable, NetSerializable]
public enum RoboticArmLayers : byte
{
    Arm,
    Powered
}
