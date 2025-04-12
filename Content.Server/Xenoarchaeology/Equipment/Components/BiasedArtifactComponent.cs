// SPDX-FileCopyrightText: 2022 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Server.Xenoarchaeology.Equipment.Components;

/// <summary>
/// This is used for artifacts that are biased to move
/// in a particular direction via the <see cref="TraversalDistorterComponent"/>
/// </summary>
[RegisterComponent]
public sealed partial class BiasedArtifactComponent : Component
{
    [ViewVariables]
    public EntityUid Provider;
}