// SPDX-License-Identifier: AGPL-3.0-or-later

// We keep this clone of the other system since I don't know yet if I'll need organ specific functions in the future.
// will delete or refactor as time goes on.
using Content.Shared._Shitmed.Body.Organ;
using Content.Shared.Body.Organ;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.Manager;
using Robust.Shared.Timing;
using System.Linq;
using Robust.Shared.Network;
using Content.Woundmed.Common.BodyEffects.Components;


namespace Content.Woundmed.Shared.BodyEffects.Systems;

public sealed partial class OrganEffectSystem : EntitySystem
{
    [Dependency] private ISerializationManager _serManager = default!;
    [Dependency] private IGameTiming _gameTiming = default!;

    // While I would love to kill this function, problem is that if we happen to have two parts that add the same
    // effect, removing one will remove both of them, since we cant tell what the source of a Component is.
    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        var query = EntityQueryEnumerator<OrganEffectComponent, OrganComponent>();
        var now = _gameTiming.CurTime;
        while (query.MoveNext(out var uid, out var comp, out var part))
        {
            if (now < comp.NextUpdate
                || !comp.Active.Any()
                || part.Body is not { } body
                || !part.Enabled)
                continue;

            comp.NextUpdate = now + comp.Delay;
            AddComponents(body, uid, comp.Active);
        }
    }

    [SubscribeLocalEvent]
    private void OnOrganComponentsModify(Entity<OrganComponent> organEnt,
        ref OrganComponentsModifyEvent ev)
    {
        if (_gameTiming.ApplyingState)
            return;

        if (organEnt.Comp.OnAdd != null)
        {
            if (ev.Add)
                AddComponents(ev.Body, organEnt, organEnt.Comp.OnAdd);
            else
                RemoveComponents(ev.Body, organEnt, organEnt.Comp.OnAdd);
        }

        if (organEnt.Comp.OnRemove != null)
        {
            if (ev.Add)
                RemoveComponents(ev.Body, organEnt, organEnt.Comp.OnRemove);
            else
                AddComponents(ev.Body, organEnt, organEnt.Comp.OnRemove);
        }
    }

    private void AddComponents(EntityUid body,
        EntityUid part,
        ComponentRegistry reg,
        OrganEffectComponent? effectComp = null)
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
        OrganEffectComponent? effectComp = null)
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
