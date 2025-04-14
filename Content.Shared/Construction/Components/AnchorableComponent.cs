// SPDX-FileCopyrightText: 2019 DamianX <DamianX@users.noreply.github.com>
// SPDX-FileCopyrightText: 2020 ComicIronic <comicironic@gmail.com>
// SPDX-FileCopyrightText: 2020 Exp <theexp111@gmail.com>
// SPDX-FileCopyrightText: 2020 Julian Giebel <j.giebel@netrocks.info>
// SPDX-FileCopyrightText: 2020 Pieter-Jan Briers <pieterjan.briers@gmail.com>
// SPDX-FileCopyrightText: 2020 Víctor Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2020 Víctor Aguilera Puerto <zddm@outlook.es>
// SPDX-FileCopyrightText: 2020 chairbender <kwhipke1@gmail.com>
// SPDX-FileCopyrightText: 2020 juliangiebel <juliangiebel@live.de>
// SPDX-FileCopyrightText: 2020 py01 <60152240+collinlunn@users.noreply.github.com>
// SPDX-FileCopyrightText: 2020 py01 <pyronetics01@gmail.com>
// SPDX-FileCopyrightText: 2020 zumorica <zddm@outlook.es>
// SPDX-FileCopyrightText: 2021 20kdc <asdd2808@gmail.com>
// SPDX-FileCopyrightText: 2021 Acruid <shatter66@gmail.com>
// SPDX-FileCopyrightText: 2021 Alex Evgrashin <aevgrashin@yandex.ru>
// SPDX-FileCopyrightText: 2021 Metal Gear Sloth <metalgearsloth@gmail.com>
// SPDX-FileCopyrightText: 2021 Paul <ritter.paul1+git@googlemail.com>
// SPDX-FileCopyrightText: 2021 Paul Ritter <ritter.paul1@googlemail.com>
// SPDX-FileCopyrightText: 2021 ShadowCommander <10494922+ShadowCommander@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <gradientvera@outlook.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <zddm@outlook.es>
// SPDX-FileCopyrightText: 2021 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 cyclowns <cyclowns@protonmail.ch>
// SPDX-FileCopyrightText: 2021 mirrorcult <notzombiedude@gmail.com>
// SPDX-FileCopyrightText: 2022 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2022 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Ben <50087092+benev0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 BenOwnby <ownbyb@appstate.edu>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <drsmugleaf@gmail.com>
// SPDX-FileCopyrightText: 2023 keronshb <54602815+keronshb@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Construction.EntitySystems;
using Content.Shared.Tools;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Construction.Components
{
    [RegisterComponent, Access(typeof(AnchorableSystem)), NetworkedComponent, AutoGenerateComponentState]
    public sealed partial class AnchorableComponent : Component
    {
        [DataField]
        public ProtoId<ToolQualityPrototype> Tool { get; private set; } = "Anchoring";

        [DataField, AutoNetworkedField]
        public AnchorableFlags Flags = AnchorableFlags.Anchorable | AnchorableFlags.Unanchorable;

        [DataField]
        [ViewVariables(VVAccess.ReadWrite)]
        public bool Snap { get; private set; } = true;

        /// <summary>
        /// Base delay to use for anchoring.
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite)]
        [DataField]
        public float Delay = 1f;
    }

    [Flags]
    public enum AnchorableFlags : byte
    {
        None = 0,
        Anchorable = 1 << 0,
        Unanchorable = 1 << 1,
    }

    public abstract class BaseAnchoredAttemptEvent : CancellableEntityEventArgs
    {
        public EntityUid User { get; }
        public EntityUid Tool { get; }

        /// <summary>
        ///     Extra delay to add to the do_after.
        ///     Add to this, don't replace it.
        ///     Output parameter.
        /// </summary>
        public float Delay { get; set; } = 0f;

        protected BaseAnchoredAttemptEvent(EntityUid user, EntityUid tool)
        {
            User = user;
            Tool = tool;
        }
    }

    public sealed class AnchorAttemptEvent : BaseAnchoredAttemptEvent
    {
        public AnchorAttemptEvent(EntityUid user, EntityUid tool) : base(user, tool) { }
    }

    public sealed class UnanchorAttemptEvent : BaseAnchoredAttemptEvent
    {
        public UnanchorAttemptEvent(EntityUid user, EntityUid tool) : base(user, tool) { }
    }

    public abstract class BaseAnchoredEvent : EntityEventArgs
    {
        public EntityUid User { get; }
        public EntityUid Tool { get; }

        protected BaseAnchoredEvent(EntityUid user, EntityUid tool)
        {
            User = user;
            Tool = tool;
        }
    }

    /// <summary>
    ///     Raised just before the entity's body type is changed.
    /// </summary>
    public sealed class BeforeAnchoredEvent : BaseAnchoredEvent
    {
        public BeforeAnchoredEvent(EntityUid user, EntityUid tool) : base(user, tool) { }
    }

    /// <summary>
    ///     Raised when an entity with an anchorable component is anchored. Note that you may instead want the more
    ///     general <see cref="AnchorStateChangedEvent"/>. This event has the benefit of having user & tool information,
    ///     as a result of interactions mediated by the <see cref="AnchorableSystem"/>.
    /// </summary>
    public sealed class UserAnchoredEvent : BaseAnchoredEvent
    {
        public UserAnchoredEvent(EntityUid user, EntityUid tool) : base(user, tool) { }
    }

    /// <summary>
    ///     Raised just before the entity's body type is changed.
    /// </summary>
    public sealed class BeforeUnanchoredEvent : BaseAnchoredEvent
    {
        public BeforeUnanchoredEvent(EntityUid user, EntityUid tool) : base(user, tool) { }
    }

    /// <summary>
    ///     Raised when an entity with an anchorable component is unanchored. Note that you will probably also need to
    ///     subscribe to the more general <see cref="AnchorStateChangedEvent"/>, which gets raised BEFORE this one. This
    ///     event has the benefit of having user & tool information, whereas the more general event may be due to
    ///     explosions or grid-destruction or other interactions not mediated by the <see cref="AnchorableSystem"/>.
    /// </summary>
    public sealed class UserUnanchoredEvent : BaseAnchoredEvent
    {
        public UserUnanchoredEvent(EntityUid user, EntityUid tool) : base(user, tool) { }
    }
}