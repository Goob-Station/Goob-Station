// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

using Content.Shared.Clothing.Components;
using Robust.Client.Physics;

namespace Content.Client.Clothing.Systems;

public sealed partial class PilotedByClothingSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PilotedByClothingComponent, UpdateIsPredictedEvent>(OnUpdatePredicted);
    }

    private void OnUpdatePredicted(Entity<PilotedByClothingComponent> entity, ref UpdateIsPredictedEvent args)
    {
        args.BlockPrediction = true;
    }
}
