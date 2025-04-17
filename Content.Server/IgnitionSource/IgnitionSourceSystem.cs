// SPDX-FileCopyrightText: 2023 08A <git@08a.re>
// SPDX-FileCopyrightText: 2023 Chief-Engineer <119664036+Chief-Engineer@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Jezithyr <6192499+Jezithyr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2023 gus <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2024 Cojoke <83733158+Cojoke-dot@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 DrSmugleaf <10968691+DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 ShadowCommander <10494922+ShadowCommander@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Atmos.EntitySystems;
using Content.Shared.IgnitionSource;
using Content.Shared.Item.ItemToggle.Components;
using Content.Shared.Temperature;
using Robust.Server.GameObjects;

namespace Content.Server.IgnitionSource;

/// <summary>
/// This handles ignition, Jez basically coded this.
/// </summary>
public sealed class IgnitionSourceSystem : EntitySystem
{
    [Dependency] private readonly AtmosphereSystem _atmosphere = default!;
    [Dependency] private readonly TransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<IgnitionSourceComponent, IsHotEvent>(OnIsHot);
        SubscribeLocalEvent<ItemToggleHotComponent, ItemToggledEvent>(OnItemToggle);
        SubscribeLocalEvent<IgnitionSourceComponent, IgnitionEvent>(OnIgnitionEvent);
    }

    private void OnIsHot(Entity<IgnitionSourceComponent> ent, ref IsHotEvent args)
    {
        args.IsHot = ent.Comp.Ignited;
    }

    private void OnItemToggle(Entity<ItemToggleHotComponent> ent, ref ItemToggledEvent args)
    {
        if (TryComp<IgnitionSourceComponent>(ent, out var comp))
            SetIgnited((ent.Owner, comp), args.Activated);
    }

    private void OnIgnitionEvent(Entity<IgnitionSourceComponent> ent, ref IgnitionEvent args)
    {
        SetIgnited((ent.Owner, ent.Comp), args.Ignite);
    }

    /// <summary>
    /// Simply sets the ignited field to the ignited param.
    /// </summary>
    public void SetIgnited(Entity<IgnitionSourceComponent?> ent, bool ignited = true)
    {
        if (!Resolve(ent, ref ent.Comp, false))
            return;

        ent.Comp.Ignited = ignited;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<IgnitionSourceComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var comp, out var xform))
        {
            if (!comp.Ignited)
                continue;

            if (xform.GridUid is { } gridUid)
            {
                var position = _transform.GetGridOrMapTilePosition(uid, xform);
                _atmosphere.HotspotExpose(gridUid, position, comp.Temperature, 50, uid, true);
            }
        }
    }
}