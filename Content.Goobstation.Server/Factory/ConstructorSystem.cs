// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Factory;
using Content.Server.Construction;
using Content.Shared.Construction.Prototypes;
using Content.Shared.DoAfter;
using Robust.Shared.Maths;

namespace Content.Goobstation.Server.Factory;

public sealed class ConstructorSystem : SharedConstructorSystem
{
    [Dependency] private readonly ConstructionSystem _construction = default!;
    [Dependency] private readonly StartableMachineSystem _machine = default!;

    private EntityQuery<ActiveDoAfterComponent> _activeQuery;

    public override void Initialize()
    {
        base.Initialize();

        _activeQuery = GetEntityQuery<ActiveDoAfterComponent>();

        SubscribeLocalEvent<ConstructorComponent, MachineStartedEvent>(OnStarted);
    }

    private void OnStarted(Entity<ConstructorComponent> ent, ref MachineStartedEvent args)
    {
        // can't start if it's already building something
        if (_activeQuery.HasComp(ent))
            _machine.Failed(ent.Owner);
        else
            Construct(ent);
    }

    // async because construction shitcode
    private async void Construct(Entity<ConstructorComponent> ent)
    {
        var uid = ent.Owner;
        if (ent.Comp.Construction is not {} id)
        {
            _machine.Failed(uid);
            return;
        }

        _machine.Started(uid);

        var proto = Proto.Index(id);
        var completed = proto.Type switch
        {
            ConstructionType.Structure => await _construction.TryStartStructureConstruction(uid, id, OutputPosition(ent), Angle.Zero),
            ConstructionType.Item => await _construction.TryStartItemConstruction(id, uid)
        };

        if (completed)
            _machine.Completed(uid);
        else
            _machine.Failed(uid);
    }
}
