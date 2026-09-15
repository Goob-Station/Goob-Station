// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Actions;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Oskarrr.WildLandsTribe;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class WarCryComponent : Component
{
    [DataField, AutoNetworkedField]
    public float Range = 8f;

    [DataField]
    public List<string> StatusEffectsToRemove = new()
    {
        "Stun",
        "KnockedDown",
        "SlowedDown",
        "Stutter",
        "SeeingRainbows",
    };
}

public sealed partial class AborigineWarCryEvent : InstantActionEvent;
