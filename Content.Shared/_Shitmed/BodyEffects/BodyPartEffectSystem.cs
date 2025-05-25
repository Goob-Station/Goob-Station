// SPDX-FileCopyrightText: 2024 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Shitmed.Body.Events;
using Content.Shared.Body.Part;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.Manager;
using Robust.Shared.Timing;
using System.Linq;

namespace Content.Shared._Shitmed.BodyEffects;
public sealed partial class BodyPartEffectSystem : EntitySystem
{
    [Dependency] private readonly IComponentFactory _compFactory = default!;
    [Dependency] private readonly ISerializationManager _serManager = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BodyPartComponent, BodyPartAddedEvent>(OnPartAttached);
        SubscribeLocalEvent<BodyPartComponent, BodyPartRemovedEvent>(OnPartDetached);
    }

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

    private void OnPartAttached(EntityUid uid, BodyPartComponent part, ref BodyPartAddedEvent args)
    {
        if (part.Body is null)
            return;

        if (part.OnAdd != null)
            AddComponents(part.Body.Value, uid, part.OnAdd);
        else if (part.OnRemove != null)
            RemoveComponents(part.Body.Value, uid, part.OnRemove);

        Dirty(uid, part);
    }

    private void OnPartDetached(EntityUid uid, BodyPartComponent part, ref BodyPartRemovedEvent args)
    {
        if (part.Body is null)
            return;

        if (part.OnAdd != null)
            RemoveComponents(part.Body.Value, uid, part.OnAdd);
        else if (part.OnRemove != null)
            AddComponents(part.Body.Value, uid, part.OnRemove);

        Dirty(uid, part);
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
            EntityManager.AddComponent(body, newComp, true);

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