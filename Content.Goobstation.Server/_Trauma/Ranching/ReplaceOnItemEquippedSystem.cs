// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Clothing;
using Content.Shared.Tag;
using Content.Goobstation.Shared._Trauma.AnimalAgeing;
using Content.Goobstation.Shared._Trauma.Ranching.Components;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server._Trauma.Ranching;

public sealed partial class ReplaceOnItemEquippedSystem : EntitySystem
{
    [Dependency] private TagSystem _tag = default!;
    [Dependency] private SharedAnimalAgeingSystem _ageing = default!;

    private readonly List<(EntProtoId Ent, EntityUid Owner)> _pending = new();

    public override void Initialize()
    {
        SubscribeLocalEvent<ReplaceOnItemEquippedComponent, ClothingDidEquippedEvent>(OnEquipped);
    }

    public override void FrameUpdate(float frameTime)
    {
        if (_pending.Count == 0)
            return;

        foreach (var (ent, owner) in _pending)
        {
            _ageing.CopyAndReplaceEntity(ent, owner);
        }

        _pending.Clear();
    }

    private void OnEquipped(Entity<ReplaceOnItemEquippedComponent> ent, ref ClothingDidEquippedEvent args)
    {
        if (args.Clothing.Comp.Slots != ent.Comp.Slots || !_tag.HasAllTags(args.Clothing.Owner, ent.Comp.RequiredTags))
            return;

        _pending.Add((ent.Comp.Ent, ent.Owner));
    }
}
