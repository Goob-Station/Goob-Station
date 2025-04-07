// SPDX-FileCopyrightText: 2022 Alex Evgrashin <aevgrashin@yandex.ru>
// SPDX-FileCopyrightText: 2022 Alexander Evgrashin <evgrashin.adl@gmail.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Server.Xenoarchaeology.XenoArtifacts.Triggers.Components;

/// <summary>
///     Activate artifact by touching, attacking or pulling it.
/// </summary>
[RegisterComponent]
public sealed partial class ArtifactInteractionTriggerComponent : Component
{
    /// <summary>
    ///     Should artifact be activated just by touching with empty hand?
    /// </summary>
    [DataField("emptyHandActivation")]
    [ViewVariables(VVAccess.ReadWrite)]
    public bool EmptyHandActivation = true;

    /// <summary>
    ///     Should artifact be activated by melee attacking?
    /// </summary>
    [DataField("attackActivation")]
    [ViewVariables(VVAccess.ReadWrite)]
    public bool AttackActivation = true;

    /// <summary>
    ///     Should artifact be activated by starting pulling it?
    /// </summary>
    [DataField("pullActivation")]
    [ViewVariables(VVAccess.ReadWrite)]
    public bool PullActivation = true;
}