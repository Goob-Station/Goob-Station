// SPDX-FileCopyrightText: 2025 Aviu00 <aviu00@protonmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.DoAfter;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class CombatDoAfterComponent : Component
{
    [ViewVariables, AutoNetworkedField]
    public ushort? DoAfterId;

    [ViewVariables, AutoNetworkedField]
    public EntityUid? DoAfterUser;

    // Only one trigger currently supported
    [NonSerialized, DataField(required: true)]
    public BaseCombatDoAfterSuccessEvent Trigger;

    // Required for throw trigger which activates after dropping item
    [DataField]
    public TimeSpan DropCancelDelay = TimeSpan.Zero;

    [DataField]
    public float Delay = 2f;

    [DataField]
    public float DelayVariation = 0.3f;

    [DataField]
    public float ActivationTolerance = 0.3f;

    [DataField]
    public bool Hidden;

    [DataField]
    public bool BreakOnMove;

    [DataField]
    public bool BreakOnWeightlessMove;

    [DataField]
    public bool BreakOnDamage;

    [DataField]
    public bool MultiplyDelay;

    [DataField]
    public Color? ColorOverride = Color.Red;

    [DataField]
    public Color? SuccessColorOverride = Color.Lime;

    [DataField]
    public bool AlwaysTriggerOnSelf = true;
}
