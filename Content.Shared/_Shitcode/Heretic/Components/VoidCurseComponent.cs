// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 whateverusername0 <whateveremail>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Shitcode.Heretic.SpriteOverlay;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;
using Robust.Shared.Utility;

namespace Content.Shared._Goobstation.Heretic.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true), AutoGenerateComponentPause]
public sealed partial class VoidCurseComponent : BaseSpriteOverlayComponent
{
    [DataField]
    public float Lifetime = 5f; // 8s on 1 stack, 20s on max stack

    [DataField]
    public float MaxLifetime = 5f;

    [DataField]
    public float LifetimeIncreasePerLevel = 3f;

    [DataField, AutoNetworkedField]
    public float Stacks;

    [DataField]
    public float MaxStacks = 5f;

    [DataField]
    public float Timer = 1f;

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField]
    public TimeSpan NextUpdate = TimeSpan.Zero;

    [DataField]
    public string OverlayStateNormal = "void_chill_partial";

    [DataField]
    public string OverlayStateMax = "void_chill_oh_fuck";

    public override Enum Key { get; set; } = VoidCurseKey.Key;

    [DataField]
    public override SpriteSpecifier? Sprite { get; set; } =
        new SpriteSpecifier.Rsi(new ResPath("_Goobstation/Heretic/void_overlay.rsi"), "void_chill_partial");
}

public enum VoidCurseKey : byte
{
    Key,
}
