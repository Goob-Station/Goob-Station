// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Factory.Slots;
using Content.Shared.Prototypes;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Factory;

public sealed class AutomationSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;

    private EntityQuery<AutomationSlotsComponent> _slotsQuery;
    private EntityQuery<AutomatedComponent> _automatedQuery;

    private List<EntProtoId> _automatable = new();
    /// <summary>
    /// All entities with <see cref="AutomationSlotsComponent"/>, maintained on prototype reload.
    /// </summary>
    public IReadOnlyList<EntProtoId> Automatable => _automatable;

    public override void Initialize()
    {
        base.Initialize();

        _slotsQuery = GetEntityQuery<AutomationSlotsComponent>();
        _automatedQuery = GetEntityQuery<AutomatedComponent>();

        SubscribeLocalEvent<AutomationSlotsComponent, ComponentInit>(OnInit);

        SubscribeLocalEvent<AutomatedComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<AutomatedComponent, ComponentShutdown>(OnShutdown);

        SubscribeLocalEvent<PhysicsComponent, AnchorStateChangedEvent>(OnAnchorChanged);

        SubscribeLocalEvent<PrototypesReloadedEventArgs>(OnPrototypesReloaded);
        CacheEntities();
    }

    private void OnInit(Entity<AutomationSlotsComponent> ent, ref ComponentInit args)
    {
        foreach (var slot in ent.Comp.Slots)
        {
            slot.Owner = ent;
            slot.Initialize();
        }
    }

    private void OnMapInit(Entity<AutomatedComponent> ent, ref MapInitEvent args)
    {
        if (!TryComp<AutomationSlotsComponent>(ent, out var comp))
            return;

        foreach (var slot in comp.Slots)
        {
            slot.AddPorts();
        }
    }

    private void OnShutdown(Entity<AutomatedComponent> ent, ref ComponentShutdown args)
    {
        if (!TryComp<AutomationSlotsComponent>(ent, out var comp))
            return;

        foreach (var slot in comp.Slots)
        {
            slot.RemovePorts();
        }
    }

    private void OnAnchorChanged(Entity<PhysicsComponent> ent, ref AnchorStateChangedEvent args)
    {
        // force collision events so machines can react to objects getting unanchored
        // should get reset after a tick due to collision wake
        if (!args.Anchored)
            _physics.WakeBody(ent);
    }

    private void OnPrototypesReloaded(PrototypesReloadedEventArgs args)
    {
        if (!args.WasModified<EntityPrototype>())
            return;

        CacheEntities();
    }

    private void CacheEntities()
    {
        _automatable.Clear();
        var factory = EntityManager.ComponentFactory;
        foreach (var proto in _proto.EnumeratePrototypes<EntityPrototype>())
        {
            if (proto.HasComponent<AutomationSlotsComponent>(factory))
                _automatable.Add(proto.ID);
        }

        _automatable.Sort();
    }

    #region Public API

    public AutomationSlot? GetSlot(Entity<AutomationSlotsComponent?> ent, string port, bool input)
    {
        // entity has no automation slots to begin with
        if (!_slotsQuery.Resolve(ent, ref ent.Comp, false))
            return null;

        // automation isn't enabled
        if (!IsAutomated(ent))
            return null;

        foreach (var slot in ent.Comp.Slots)
        {
            string? id = input ? slot.Input : slot.Output;
            if (id == port)
                return slot;
        }

        return null;
    }

    public bool IsAutomated(EntityUid uid)
    {
        return _automatedQuery.HasComp(uid);
    }

    public bool HasSlot(Entity<AutomationSlotsComponent?> ent, string port, bool input)
    {
        return GetSlot(ent, port, input) != null;
    }

    #endregion
}
