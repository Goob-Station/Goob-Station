// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Goobstation.Maths.FixedPoint;
using Content.Shared._Gabystation.MalfAi;
using Content.Shared.Alert;
using Content.Shared.Store;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._Gabystation.MalfAi.Components;

/// <summary>
/// Marker component placed on the Station AI when it becomes a Malfunctioning AI antagonist.
/// Used to gate special interactions (e.g., APC CPU siphoning) without affecting visuals like EMAG.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(SharedMalfAiSystem))]
public sealed partial class MalfunctioningAiComponent : Component
{
    [DataField]
    public LocId StoreName = "store-preset-name-malfai";

    [DataField]
    public string[] StoreCategories = { "All", "MalfAI", "Deception", "Factory", "Disruption" };

    [DataField]
    public ProtoId<CurrencyPrototype> CurrencyId = "CPU";

    [DataField]
    public EntProtoId OpenStoreAction = "ActionMalfAiOpenStore";

    [DataField]
    public EntProtoId OpenBorgsUiAction = "ActionMalfAiOpenBorgsUi";

    [DataField]
    public ProtoId<AlertPrototype> CurrencyAlertId = "MalfCpu";

    [DataField, AutoNetworkedField]
    public FixedPoint2 CpuStore;
}
