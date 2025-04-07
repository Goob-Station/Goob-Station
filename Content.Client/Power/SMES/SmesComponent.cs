// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Client.Power.SMES;

[RegisterComponent]
public sealed partial class SmesComponent : Component
{
    /// <summary>
    /// The prefix used for the RSI states of the sprite layers indicating the charge level of the SMES.
    /// </summary>
    [DataField("chargeOverlayPrefix")]
    [ViewVariables(VVAccess.ReadWrite)]
    public string ChargeOverlayPrefix = "smes-og";

    /// <summary>
    /// The prefix used for the RSI states of the sprite layers indicating the input state of the SMES.
    /// Actually bundled together with the output indicator light.
    /// </summary>
    [DataField("inputOverlayPrefix")]
    [ViewVariables(VVAccess.ReadWrite)]
    public string InputOverlayPrefix = "smes-oc";

    /// <summary>
    /// The prefix used for the RSI states of the sprite layers indicating the output state of the SMES.
    /// Actually bundled together with the input indicator light.
    /// </summary>
    [DataField("outputOverlayPrefix")]
    [ViewVariables(VVAccess.ReadWrite)]
    public string OutputOverlayPrefix = "smes-op";
}