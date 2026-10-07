// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Body.Part;
using Content.Woundmed.Common.BodyEffects;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.Manager;
using Robust.Shared.Timing;
using System.Linq;

namespace Content.Woundmed.Shared.BodyEffects;

public sealed partial class BodyPartEffectSystem : EntitySystem
{
    [Dependency] private ISerializationManager _serManager = default!;
    [Dependency] private IGameTiming _gameTiming = default!;

    // While I would love to kill this function, problem is that if we happen to have two parts that add the same
    // effect, removing one will remove both of them, since we cant tell what the source of a Component is.
    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<BodyPartEffectComponent, BodyPartComponent>();
        var now = _gameTiming.CurTime;
        while (query.MoveNext(out var uid, out var comp, out var part))
        {
            if (now < comp.NextUpdate || !comp.Active.Any() || part.Body is not { } body)
                continue;

            comp.NextUpdate = now + comp.Delay;
            AddComponents(body, uid, comp.Active);
        }
    }

    [SubscribeLocalEvent]
    private void OnPartAttached(Entity<BodyPartComponent> ent, ref BodyPartAddedEvent args)
    {
        if (ent.Comp.Body is null)
            return;

        if (ent.Comp.OnAdd != null)
            AddComponents(ent.Comp.Body.Value, ent, ent.Comp.OnAdd);
        else if (ent.Comp.OnRemove != null)
            RemoveComponents(ent.Comp.Body.Value, ent, ent.Comp.OnRemove);

        Dirty(ent);
    }

    [SubscribeLocalEvent]
    private void OnPartDetached(Entity<BodyPartComponent> ent, ref BodyPartRemovedEvent args)
    {
        if (ent.Comp.Body is null)
            return;

        if (ent.Comp.OnAdd != null)
            RemoveComponents(ent.Comp.Body.Value, ent, ent.Comp.OnAdd);
        else if (ent.Comp.OnRemove != null)
            AddComponents(ent.Comp.Body.Value, ent, ent.Comp.OnRemove);

        Dirty(ent);
    }

    private void AddComponents(EntityUid body,
        EntityUid part,
        ComponentRegistry reg,
        BodyPartEffectComponent? effectComp = null)
    {
        if (!Resolve(part, ref effectComp, logMissing: false))
            return;

        foreach (var (key, comp) in reg)
        {
            var compType = comp.Component.GetType();
            if (HasComp(body, compType))
                continue;

            var newComp = (Component) _serManager.CreateCopy(comp.Component, notNullableOverride: true);
            AddComp(body, newComp, true);

            effectComp.Active[key] = comp;
        }
    }

    private void RemoveComponents(EntityUid body,
        EntityUid part,
        ComponentRegistry reg,
        BodyPartEffectComponent? effectComp = null)
    {
        if (!Resolve(part, ref effectComp, logMissing: false))
            return;

        foreach (var (key, comp) in reg)
        {
            RemComp(body, comp.Component.GetType());
            effectComp.Active.Remove(key);
        }
    }
}