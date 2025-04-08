// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Whitelist;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Goobstation.Wizard.SupermatterHalberd;

[RegisterComponent, NetworkedComponent]
public sealed partial class SupermatterHalberdComponent : Component
{
    [DataField]
    public TimeSpan ExecuteDelay = TimeSpan.FromSeconds(1);

    [DataField]
    public SoundSpecifier ExecuteSound = new SoundPathSpecifier("/Audio/_Goobstation/Wizard/supermatter.ogg");

    [DataField]
    public EntProtoId AshProto = "Ash";

    [DataField]
    public EntProtoId ExecuteEffect = "SupermatterFlashEffect";

    [DataField]
    public EntityWhitelist ObliterateWhitelist;
}