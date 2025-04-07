// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2024 Hannah Giovanna Dawson <karakkaraz@gmail.com>
// SPDX-FileCopyrightText: 2024 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Power.EntitySystems;
using Content.Server.Xenoarchaeology.Equipment.Components;
using Content.Shared.Examine;
using Content.Shared.Placeable;
using Robust.Shared.Timing;

namespace Content.Server.Xenoarchaeology.Equipment.Systems;

public sealed class TraversalDistorterSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<TraversalDistorterComponent, MapInitEvent>(OnInit);
        SubscribeLocalEvent<TraversalDistorterComponent, ExaminedEvent>(OnExamine);

        SubscribeLocalEvent<TraversalDistorterComponent, ItemPlacedEvent>(OnItemPlaced);
        SubscribeLocalEvent<TraversalDistorterComponent, ItemRemovedEvent>(OnItemRemoved);
    }

    private void OnInit(EntityUid uid, TraversalDistorterComponent component, MapInitEvent args)
    {
        component.NextActivation = _timing.CurTime;
    }

    /// <summary>
    /// Switches the state of the traversal distorter between up and down.
    /// </summary>
    /// <param name="uid">The distorter's entity</param>
    /// <param name="component">The component on the entity</param>
    /// <returns>If the distorter changed state</returns>
    public bool SetState(EntityUid uid, TraversalDistorterComponent component, bool isDown)
    {
        if (!this.IsPowered(uid, EntityManager))
            return false;

        if (_timing.CurTime < component.NextActivation)
            return false;

        component.NextActivation = _timing.CurTime + component.ActivationDelay;

        component.BiasDirection = isDown ? BiasDirection.Down : BiasDirection.Up;

        return true;
    }

    private void OnExamine(EntityUid uid, TraversalDistorterComponent component, ExaminedEvent args)
    {
        string examine = string.Empty;
        switch (component.BiasDirection)
        {
            case BiasDirection.Up:
                examine = Loc.GetString("traversal-distorter-desc-up");
                break;
            case BiasDirection.Down:
                examine = Loc.GetString("traversal-distorter-desc-down");
                break;
        }

        args.PushMarkup(examine);
    }

    private void OnItemPlaced(EntityUid uid, TraversalDistorterComponent component, ref ItemPlacedEvent args)
    {
        var bias = EnsureComp<BiasedArtifactComponent>(args.OtherEntity);
        bias.Provider = uid;
    }

    private void OnItemRemoved(EntityUid uid, TraversalDistorterComponent component, ref ItemRemovedEvent args)
    {
        var otherEnt = args.OtherEntity;
        if (TryComp<BiasedArtifactComponent>(otherEnt, out var bias) && bias.Provider == uid)
            RemComp(otherEnt, bias);
    }
}