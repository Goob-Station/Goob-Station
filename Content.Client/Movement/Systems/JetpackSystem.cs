// SPDX-FileCopyrightText: 2024 Aidenkrz <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Plykiya <58439124+Plykiya@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Tayrtahn <tayrtahn@gmail.com>
// SPDX-FileCopyrightText: 2023 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 rolfero <45628623+rolfero@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 slarticodefast <161409025+slarticodefast@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
using Content.Shared.Clothing.Components;
using Content.Shared.Clothing.EntitySystems;
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Systems;
using Robust.Client.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Physics.Components;
using Robust.Shared.Timing;

namespace Content.Client.Movement.Systems;

public sealed class JetpackSystem : SharedJetpackSystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly ClothingSystem _clothing = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedMapSystem _mapSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<JetpackComponent, AppearanceChangeEvent>(OnJetpackAppearance);
    }

    protected override bool CanEnable(EntityUid uid, JetpackComponent component)
    {
        // No predicted atmos so you'd have to do a lot of funny to get this working.
        return false;
    }

    private void OnJetpackAppearance(EntityUid uid, JetpackComponent component, ref AppearanceChangeEvent args)
    {
        Appearance.TryGetData<bool>(uid, JetpackVisuals.Enabled, out var enabled, args.Component);

        var state = "icon" + (enabled ? "-on" : "");
        args.Sprite?.LayerSetState(0, state);

        if (TryComp<ClothingComponent>(uid, out var clothing))
            _clothing.SetEquippedPrefix(uid, enabled ? "on" : null, clothing);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (!_timing.IsFirstTimePredicted)
            return;

        // TODO: Please don't copy-paste this I beg
        // make a generic particle emitter system / actual particles instead.
        var query = EntityQueryEnumerator<ActiveJetpackComponent>();

        while (query.MoveNext(out var uid, out var comp))
        {
            if (_timing.CurTime < comp.TargetTime)
                continue;

            comp.TargetTime = _timing.CurTime + TimeSpan.FromSeconds(comp.EffectCooldown);

            CreateParticles(uid);
        }
    }

    private void CreateParticles(EntityUid uid)
    {
        var uidXform = Transform(uid);
        // Don't show particles unless the user is moving.
        if (Container.TryGetContainingContainer((uid, uidXform, null), out var container) &&
            TryComp<PhysicsComponent>(container.Owner, out var body) &&
            body.LinearVelocity.LengthSquared() < 1f)
        {
            return;
        }

        var coordinates = uidXform.Coordinates;
        var gridUid = _transform.GetGrid(coordinates);

        if (TryComp<MapGridComponent>(gridUid, out var grid))
        {
            coordinates = new EntityCoordinates(gridUid.Value, _mapSystem.WorldToLocal(gridUid.Value, grid, _transform.ToMapCoordinates(coordinates).Position));
        }
        else if (uidXform.MapUid != null)
        {
            coordinates = new EntityCoordinates(uidXform.MapUid.Value, _transform.GetWorldPosition(uidXform));
        }
        else
        {
            return;
        }

        Spawn("JetpackEffect", coordinates);
    }
}
