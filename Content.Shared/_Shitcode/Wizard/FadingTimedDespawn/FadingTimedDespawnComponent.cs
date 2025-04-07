// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared._Goobstation.Wizard.FadingTimedDespawn;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
public sealed partial class FadingTimedDespawnComponent : Component
{
    /// <summary>
    /// How long the entity will exist before despawning
    /// </summary>
    [DataField]
    public float Lifetime = 5f;

    /// <summary>
    /// If it is above zero, entity will fade out slowly when despawning
    /// </summary>
    [DataField, AutoNetworkedField]
    public float FadeOutTime = 1f;

    /// <summary>
    /// Whether this entity started to fade out
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly), AutoNetworkedField]
    public bool FadeOutStarted;

    public const string AnimationKey = "fadeout";
}