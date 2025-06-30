// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Utility;

namespace Content.Shared.Tools.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
public sealed partial class MultipleToolComponent : Component
{
    [DataDefinition]
    public sealed partial class ToolEntry
    {
        [DataField(required: true)]
        public PrototypeFlags<ToolQualityPrototype> Behavior = new();

        [DataField]
        public SoundSpecifier? UseSound;

        [DataField]
        public SoundSpecifier? ChangeSound;

        [DataField]
        public SpriteSpecifier? Sprite;
    }

    [DataField(required: true)]
    public ToolEntry[] Entries { get; private set; } = Array.Empty<ToolEntry>();

    [ViewVariables]
    [AutoNetworkedField]
    public uint CurrentEntry = 0;

    [ViewVariables]
    public string CurrentQualityName = string.Empty;

    [ViewVariables(VVAccess.ReadWrite)]
    public bool UiUpdateNeeded;

    [DataField]
    public bool StatusShowBehavior = true;
}