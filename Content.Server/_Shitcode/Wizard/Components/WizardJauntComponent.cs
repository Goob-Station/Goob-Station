// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Server._Goobstation.Wizard.Components;

[RegisterComponent]
public sealed partial class WizardJauntComponent : Component
{
    [DataField]
    public EntProtoId JauntStartEffect = "EtherealJauntStartEffect";

    [DataField]
    public EntProtoId JauntEndEffect = "EtherealJauntEndEffect";

    [DataField]
    public SoundSpecifier JauntStartSound = new SoundPathSpecifier("/Audio/Magic/ethereal_enter.ogg");

    [DataField]
    public SoundSpecifier JauntEndSound = new SoundPathSpecifier("/Audio/Magic/ethereal_exit.ogg");

    [DataField]
    public float DurationBetweenEffects = 2.8f;

    [DataField]
    public EntityUid? JauntEndEffectEntity;
}