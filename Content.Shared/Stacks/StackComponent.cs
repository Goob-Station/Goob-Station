// SPDX-FileCopyrightText: 2020 DTanxxx <55208219+DTanxxx@users.noreply.github.com>
// SPDX-FileCopyrightText: 2020 Exp <theexp111@gmail.com>
// SPDX-FileCopyrightText: 2020 Git-Nivrak <59925169+Git-Nivrak@users.noreply.github.com>
// SPDX-FileCopyrightText: 2020 Swept <jamesurquhartwebb@gmail.com>
// SPDX-FileCopyrightText: 2020 Tyler Young <tyler.young@impromptu.ninja>
// SPDX-FileCopyrightText: 2020 Víctor Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2020 Víctor Aguilera Puerto <zddm@outlook.es>
// SPDX-FileCopyrightText: 2021 Acruid <shatter66@gmail.com>
// SPDX-FileCopyrightText: 2021 Metal Gear Sloth <metalgearsloth@gmail.com>
// SPDX-FileCopyrightText: 2021 Paul <ritter.paul1+git@googlemail.com>
// SPDX-FileCopyrightText: 2021 Paul Ritter <ritter.paul1@googlemail.com>
// SPDX-FileCopyrightText: 2021 ShadowCommander <10494922+ShadowCommander@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Swept <sweptwastaken@protonmail.com>
// SPDX-FileCopyrightText: 2021 Tarlan2 <76408146+Tarlan2@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <gradientvera@outlook.com>
// SPDX-FileCopyrightText: 2021 ike709 <ike709@github.com>
// SPDX-FileCopyrightText: 2021 ike709 <ike709@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2022 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2022 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.GameStates;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared.Stacks
{
    [RegisterComponent, NetworkedComponent]
    public sealed partial class StackComponent : Component
    {
        [ViewVariables(VVAccess.ReadWrite)]
        [DataField("stackType", required: true, customTypeSerializer: typeof(PrototypeIdSerializer<StackPrototype>))]
        public string StackTypeId { get; private set; } = default!;

        /// <summary>
        ///     Current stack count.
        ///     Do NOT set this directly, use the <see cref="SharedStackSystem.SetCount"/> method instead.
        /// </summary>
        [DataField("count")]
        public int Count { get; set; } = 30;

        /// <summary>
        ///     Max amount of things that can be in the stack.
        ///     Overrides the max defined on the stack prototype.
        /// </summary>
        [ViewVariables(VVAccess.ReadOnly)]
        [DataField("maxCountOverride")]
        public int? MaxCountOverride  { get; set; }

        /// <summary>
        ///     Set to true to not reduce the count when used.
        ///     Note that <see cref="Count"/> still limits the amount that can be used at any one time.
        /// </summary>
        [DataField("unlimited")]
        [ViewVariables(VVAccess.ReadOnly)]
        public bool Unlimited { get; set; }

        /// <summary>
        /// Lingering stacks will remain present even when there are no items.
        /// Instead, they will become transparent.
        /// </summary>
        [DataField("lingering"), ViewVariables(VVAccess.ReadWrite)]
        public bool Lingering;

        [DataField("throwIndividually"), ViewVariables(VVAccess.ReadWrite)]
        public bool ThrowIndividually { get; set; } = false;

        [ViewVariables]
        public bool UiUpdateNeeded { get; set; }

        /// <summary>
        /// Default IconLayer stack.
        /// </summary>
        [DataField("baseLayer")]
        [ViewVariables(VVAccess.ReadWrite)]
        public string BaseLayer = "";

        /// <summary>
        /// Determines if the visualizer uses composite or non-composite layers for icons. Defaults to false.
        ///
        /// <list type="bullet">
        /// <item>
        /// <description>false: they are opaque and mutually exclusive (e.g. sprites in a cable coil). <b>Default value</b></description>
        /// </item>
        /// <item>
        /// <description>true: they are transparent and thus layered one over another in ascending order first</description>
        /// </item>
        /// </list>
        ///
        /// </summary>
        [DataField("composite")]
        [ViewVariables(VVAccess.ReadWrite)]
        public bool IsComposite;

        /// <summary>
        /// Sprite layers used in stack visualizer. Sprites first in layer correspond to lower stack states
        /// e.g. <code>_spriteLayers[0]</code> is lower stack level than <code>_spriteLayers[1]</code>.
        /// </summary>
        [DataField("layerStates")]
        [ViewVariables(VVAccess.ReadWrite)]
        public List<string> LayerStates = new();
    }

    [Serializable, NetSerializable]
    public sealed class StackComponentState : ComponentState
    {
        public int Count { get; }
        public int? MaxCount { get; }

        public bool Lingering;

        public StackComponentState(int count, int? maxCount, bool lingering)
        {
            Count = count;
            MaxCount = maxCount;
            Lingering = lingering;
        }
    }
}