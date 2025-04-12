// SPDX-FileCopyrightText: 2022 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared.Revenant.Components;

/// <summary>
/// This is used for tracking lights that are overloaded
/// and are about to zap a player.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class RevenantOverloadedLightsComponent : Component
{
    [ViewVariables]
    public EntityUid? Target;

    [ViewVariables(VVAccess.ReadWrite)]
    public float Accumulator = 0;

    [ViewVariables(VVAccess.ReadWrite)]
    public float ZapDelay = 3f;

    [ViewVariables(VVAccess.ReadWrite)]
    public float ZapRange = 4f;

    [DataField("zapBeamEntityId",customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    public string ZapBeamEntityId = "LightningRevenant";

    public float? OriginalEnergy;
    public bool OriginalEnabled = false;
}