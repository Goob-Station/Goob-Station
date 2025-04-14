// SPDX-FileCopyrightText: 2023 LankLTE <135308300+LankLTE@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Mobs;

namespace Content.Goobstation.Server.Implants.Components;

[RegisterComponent]
public sealed partial class StypticStimulatorImplantComponent : Component
{
    [DataField]
    public List<MobState> OriginalAllowedStates;

    [DataField]
    public TimeSpan NextExecutionTime = TimeSpan.Zero;

    /// <summary>
    /// How much to reduce the bleeding by every second.
    /// </summary>
    [DataField]
    public float BleedingModifier = -25f;

}
