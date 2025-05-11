// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
//
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


namespace Content.Shared._Shitmed.BodyEffects;
public sealed partial class OrganEffectSystem : EntitySystem
{
    [Dependency] private readonly IComponentFactory _compFactory = default!;
    [Dependency] private readonly ISerializationManager _serManager = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly INetManager _net = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<OrganComponent, OrganComponentsModifyEvent>(OnOrganComponentsModify);
    }

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
            AddComponents(body, uid, comp.Active, comp, false);
        }
    }

    private void OnOrganComponentsModify(Entity<OrganComponent> organEnt,
        ref OrganComponentsModifyEvent ev)
    {
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
                AddComponents(ev.Body, organEnt, organEnt.Comp.OnRemove);
            else
                RemoveComponents(ev.Body, organEnt, organEnt.Comp.OnRemove);
        }
    }

    private void AddComponents(EntityUid body,
        EntityUid part,
        ComponentRegistry reg,
        OrganEffectComponent? effectComp = null,
        bool? removeExisting = true)
    {
        if (!Resolve(part, ref effectComp, logMissing: false))
            return;

        EntityManager.AddComponents(body, reg, removeExisting ?? true);
        foreach (var (key, comp) in reg)
        {
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

        EntityManager.RemoveComponents(body, reg);
        foreach (var key in reg.Keys)
        {
            effectComp.Active.Remove(key);
        }
    }
}