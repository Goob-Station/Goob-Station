// SPDX-FileCopyrightText: 2023 LankLTE <135308300+LankLTE@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Solstice <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 coderabbitai[bot] <136622811+coderabbitai[bot]@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Mobs;

namespace Content.Goobstation.Server.Implants.Components;

[RegisterComponent]
public sealed partial class StypticStimulatorImplantComponent : Component
{
    /// <summary>
    /// Next execution time. (Explanatory, I know.)
    /// </summary>
    [DataField]
    public TimeSpan NextExecutionTime = TimeSpan.Zero;

    /// <summary>
    /// How much to reduce the bleeding by every second.
    /// </summary>
    [DataField]
    public float BleedingModifier = -100f;

    [DataField]
    public List<MobState> OriginalAllowedMobStates = new();
}
