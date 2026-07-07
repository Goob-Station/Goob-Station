// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Server._Goobstation.Wizard.Components;

[RegisterComponent]
public sealed partial class UseDelayBlockKnockdownComponent : Component
{
    [DataField]
    public string Delay = "default";

    [DataField]
    public bool ResetDelayOnSuccess = true;

    [DataField]
    public SoundSpecifier? KnockdownSound = new SoundPathSpecifier("/Audio/Effects/Lightning/lightningbolt.ogg");

    [DataField]
    public bool DoSparks = true;

    [DataField]
    public bool DoCustom;

    [DataField]
    public EntProtoId CustomEffect = "EffectHearts";
}
