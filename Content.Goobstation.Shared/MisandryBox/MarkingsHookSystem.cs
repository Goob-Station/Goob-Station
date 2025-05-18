// SPDX-FileCopyrightText: 2025 Conchelle <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Shared._Shitmed.Humanoid.Events;
using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Markings;
using Content.Shared.MagicMirror;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.MisandryBox;

// If I get trolled by stateguard again I will declare a fatwa against electro
public sealed class MarkingsHookSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HumanoidAppearanceComponent, ProfileLoadFinishedEvent>(OnProfileLoad);
    }

    private void OnProfileLoad(EntityUid uid, HumanoidAppearanceComponent component, ProfileLoadFinishedEvent args)
    {
        foreach (var mark in component.MarkingSet.Markings.Values.SelectMany(markings => markings))
        {
            if (!_proto.TryIndex(mark.MarkingId, out MarkingPrototype? markProt))
                continue;

            foreach (var spec in markProt.Special)
            {
                spec.AfterEquip(uid);
            }
        }
    }
}
