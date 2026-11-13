// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Chemistry.Components;
using Content.Shared.Database;
using Content.Shared.Dataset;
using Content.Shared.EntityEffects;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.EntityEffects;

public sealed partial class SayEffect : EntityEffectBase<SayEffect>
{
    [DataField]
    public ProtoId<LocalizedDatasetPrototype>? DatasetId = null;

    [DataField]
    public LocId? LocId = null;

    public override string? EntityEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => null;
}
