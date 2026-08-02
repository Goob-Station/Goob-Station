// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.Traits;
using Content.Shared._Shitmed.Body.Components;
using Content.Shared.Body.Components;
using Content.Shared.Body.Part;

namespace Content.Goobstation.Shared.Traits.Assorted;

public sealed class LegsStartParalyzedSystem : EntitySystem
{
    [Dependency] private readonly EntityManager _entMan = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<LegsStartParalyzedComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<LegsStartParalyzedComponent, ComponentRemove>(OnRemoved);
    }

    private void OnMapInit(EntityUid uid, LegsStartParalyzedComponent component, MapInitEvent args)
    {
        if (!_entMan.TryGetComponent<BodyComponent>(uid, out var body))
            return;

        foreach (var legEntity in body.LegEntities)
            EnsureComp<LimbParalyzedComponent>(legEntity);
    }

    private void OnRemoved(EntityUid uid, LegsStartParalyzedComponent component, ComponentRemove args)
    {
        if (!TryComp<BodyComponent>(uid, out var body))
            return;

        foreach (var legEntity in body.LegEntities)
            RemComp<LimbParalyzedComponent>(legEntity);
    }
}
