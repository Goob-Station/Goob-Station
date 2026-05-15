// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Clothing;
using Content.Shared.Tag;
using Content.Trauma.Shared.AnimalAgeing;
using Content.Trauma.Shared.Ranching.Components;
using Robust.Shared.Timing;

namespace Content.Trauma.Server.Ranching;

public sealed class ReplaceOnItemEquippedSystem : EntitySystem
{
    [Dependency] readonly TagSystem _tag = default!;
    [Dependency] readonly SharedAnimalAgeingSystem _ageing = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<ReplaceOnItemEquippedComponent, ClothingDidEquippedEvent>(OnEquipped);
    }

    private void OnEquipped(Entity<ReplaceOnItemEquippedComponent> ent, ref ClothingDidEquippedEvent args)
    {
        if (args.Clothing.Comp.Slots != ent.Comp.Slots || !_tag.HasAllTags(args.Clothing.Owner, ent.Comp.RequiredTags))
            return;

        // Defer cuz it crashes becuase of shitcode
        var entCopy = ent.Comp.Ent;
        var ownerCopy = ent.Owner;
        Timer.Spawn(0, () =>
        {
            _ageing.CopyAndReplaceEntity(entCopy, ownerCopy);
        });
    }
}
