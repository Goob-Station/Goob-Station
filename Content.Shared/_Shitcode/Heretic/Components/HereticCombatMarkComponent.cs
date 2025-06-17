// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 whateverusername0 <whateveremail>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Utility;

namespace Content.Shared.Heretic;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
public sealed partial class HereticCombatMarkComponent : Component
{
    [DataField, AutoNetworkedField]
    public string Path = "Blade";

    [DataField]
    public float MaxDisappearTime = 15f;

    [DataField]
    public float DisappearTime = 15f;

    [DataField]
    public int Repetitions = 1;

    public TimeSpan Timer = TimeSpan.Zero;

    [DataField]
    public SoundSpecifier? TriggerSound = new SoundPathSpecifier("/Audio/_Goobstation/Heretic/repulse.ogg");

    [DataField]
    public ResPath ResPath = new("_Goobstation/Heretic/combat_marks.rsi");
}

public enum HereticCombatMarkKey : byte
{
    Key,
}
