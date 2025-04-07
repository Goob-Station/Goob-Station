// SPDX-FileCopyrightText: 2020 Julian Giebel <j.giebel@netrocks.info>
// SPDX-FileCopyrightText: 2020 Pieter-Jan Briers <pieterjan.briers@gmail.com>
// SPDX-FileCopyrightText: 2020 Vince <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Acruid <shatter66@gmail.com>
// SPDX-FileCopyrightText: 2021 Metal Gear Sloth <metalgearsloth@gmail.com>
// SPDX-FileCopyrightText: 2021 Paul <ritter.paul1+git@googlemail.com>
// SPDX-FileCopyrightText: 2021 Paul Ritter <ritter.paul1@googlemail.com>
// SPDX-FileCopyrightText: 2021 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2021 ShadowCommander <10494922+ShadowCommander@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <zddm@outlook.es>
// SPDX-FileCopyrightText: 2021 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 collinlunn <60152240+collinlunn@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Julian Giebel <juliangiebel@live.de>
// SPDX-FileCopyrightText: 2022 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 I.K <45953835+notquitehadouken@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2023 eoineoineoin <github@eoinrul.es>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 zero <hello@enumerate.dev>
// SPDX-FileCopyrightText: 2024 Ed <96445749+TheShuEd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 brainfood1183 <113240905+brainfood1183@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Audio;
using Content.Shared.Whitelist;
using Robust.Shared.Containers;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared.Disposal.Components;

[NetworkedComponent]
public abstract partial class SharedDisposalUnitComponent : Component
{
    public const string ContainerId = "disposals";

    /// <summary>
    /// Sounds played upon the unit flushing.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField("soundFlush")]
    public SoundSpecifier? FlushSound = new SoundPathSpecifier("/Audio/Machines/disposalflush.ogg");

    /// <summary>
    /// Blacklists (prevents) entities listed from being placed inside.
    /// </summary>
    [DataField]
    public EntityWhitelist? Blacklist;

    /// <summary>
    /// Whitelists (allows) entities listed from being placed inside.
    /// </summary>
    [DataField]
    public EntityWhitelist? Whitelist;

    /// <summary>
    /// Sound played when an object is inserted into the disposal unit.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField("soundInsert")]
    public SoundSpecifier? InsertSound = new SoundPathSpecifier("/Audio/Effects/trashbag1.ogg");

    /// <summary>
    /// State for this disposals unit.
    /// </summary>
    [DataField]
    public DisposalsPressureState State;

    // TODO: Just make this use vaulting.
    /// <summary>
    /// We'll track whatever just left disposals so we know what collision we need to ignore until they stop intersecting our BB.
    /// </summary>
    [ViewVariables, DataField]
    public List<EntityUid> RecentlyEjected = new();

    /// <summary>
    /// Next time the disposal unit will be pressurized.
    /// </summary>
    [DataField(customTypeSerializer:typeof(TimeOffsetSerializer))]
    public TimeSpan NextPressurized = TimeSpan.Zero;

    /// <summary>
    /// How long it takes to flush a disposals unit manually.
    /// </summary>
    [DataField("flushTime")]
    public TimeSpan ManualFlushTime = TimeSpan.FromSeconds(2);

    /// <summary>
    /// How long it takes from the start of a flush animation to return the sprite to normal.
    /// </summary>
    [DataField]
    public TimeSpan FlushDelay = TimeSpan.FromSeconds(3);

    /// <summary>
    /// Removes the pressure requirement for flushing.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public bool DisablePressure;

    /// <summary>
    /// Last time that an entity tried to exit this disposal unit.
    /// </summary>
    [ViewVariables]
    public TimeSpan LastExitAttempt;

    [DataField]
    public bool AutomaticEngage = true;

    [ViewVariables(VVAccess.ReadWrite)]
    [DataField]
    public TimeSpan AutomaticEngageTime = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Delay from trying to enter disposals ourselves.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField]
    public float EntryDelay = 0.5f;

    /// <summary>
    /// Delay from trying to shove someone else into disposals.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    public float DraggedEntryDelay = 2.0f;

    /// <summary>
    /// Container of entities inside this disposal unit.
    /// </summary>
    [ViewVariables] public Container Container = default!;

    // TODO: Network power shit instead fam.
    [ViewVariables, DataField]
    public bool Powered;

    /// <summary>
    /// Was the disposals unit engaged for a manual flush.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField]
    public bool Engaged;

    /// <summary>
    /// Next time this unit will flush. Is the lesser of <see cref="FlushDelay"/> and <see cref="AutomaticEngageTime"/>
    /// </summary>
    [ViewVariables, DataField(customTypeSerializer:typeof(TimeOffsetSerializer))]
    public TimeSpan? NextFlush;

    [Serializable, NetSerializable]
    public enum Visuals : byte
    {
        VisualState,
        Handle,
        Light
    }

    [Serializable, NetSerializable]
    public enum VisualState : byte
    {
        UnAnchored,
        Anchored,
        OverlayFlushing,
        OverlayCharging
    }

    [Serializable, NetSerializable]
    public enum HandleState : byte
    {
        Normal,
        Engaged
    }

    [Serializable, NetSerializable]
    [Flags]
    public enum LightStates : byte
    {
        Off = 0,
        Charging = 1 << 0,
        Full = 1 << 1,
        Ready = 1 << 2
    }

    [Serializable, NetSerializable]
    public enum UiButton : byte
    {
        Eject,
        Engage,
        Power
    }

    [Serializable, NetSerializable]
    public sealed class DisposalUnitBoundUserInterfaceState : BoundUserInterfaceState, IEquatable<DisposalUnitBoundUserInterfaceState>
    {
        public readonly string UnitName;
        public readonly string UnitState;
        public readonly TimeSpan FullPressureTime;
        public readonly bool Powered;
        public readonly bool Engaged;

        public DisposalUnitBoundUserInterfaceState(string unitName, string unitState, TimeSpan fullPressureTime, bool powered,
            bool engaged)
        {
            UnitName = unitName;
            UnitState = unitState;
            FullPressureTime = fullPressureTime;
            Powered = powered;
            Engaged = engaged;
        }

        public bool Equals(DisposalUnitBoundUserInterfaceState? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return UnitName == other.UnitName &&
                   UnitState == other.UnitState &&
                   Powered == other.Powered &&
                   Engaged == other.Engaged &&
                   FullPressureTime.Equals(other.FullPressureTime);
        }
    }

    /// <summary>
    ///     Message data sent from client to server when a disposal unit ui button is pressed.
    /// </summary>
    [Serializable, NetSerializable]
    public sealed class UiButtonPressedMessage : BoundUserInterfaceMessage
    {
        public readonly UiButton Button;

        public UiButtonPressedMessage(UiButton button)
        {
            Button = button;
        }
    }

    [Serializable, NetSerializable]
    public enum DisposalUnitUiKey : byte
    {
        Key
    }
}

[Serializable, NetSerializable]
public enum DisposalsPressureState : byte
{
    Ready,

    /// <summary>
    /// Has been flushed recently within FlushDelay.
    /// </summary>
    Flushed,

    /// <summary>
    /// FlushDelay has elapsed and now we're transitioning back to Ready.
    /// </summary>
    Pressurizing
}