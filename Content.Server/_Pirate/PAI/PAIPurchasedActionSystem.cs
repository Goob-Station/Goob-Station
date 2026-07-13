// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Actions;
using Content.Shared.Actions.Components;
using Content.Shared.Mind.Components;
using Content.Shared.PAI;
using Content.Shared.Store.Components;

namespace Content.Server._Pirate.PAI;

public sealed class PAIPurchasedActionSystem : EntitySystem
{
    [Dependency] private readonly ActionContainerSystem _actionContainer = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StoreComponent, MindRemovedMessage>(OnMindRemoved, after: [typeof(ActionContainerSystem)]);
    }

    private void OnMindRemoved(Entity<StoreComponent> ent, ref MindRemovedMessage args)
    {
        if (!HasComp<PAIComponent>(ent.Owner) ||
            !TryComp<ActionsContainerComponent>(args.Mind.Owner, out var mindActions))
            return;

        // Installed software belongs to the pAI device, not the departing mind.
        foreach (var purchased in ent.Comp.BoughtEntities)
        {
            if (!mindActions.Container.Contains(purchased) ||
                !TryComp<ActionComponent>(purchased, out var action))
                continue;

            _actionContainer.TransferActionWithNewAttached(purchased, ent.Owner, ent.Owner, action);
        }
    }
}
