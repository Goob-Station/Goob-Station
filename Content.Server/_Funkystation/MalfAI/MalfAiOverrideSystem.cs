// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
// SPDX-FileCopyrightText: 2025 Goob-Station
//
// SPDX-License-Identifier: MIT

using Content.Server.Administration.Logs;
using Content.Shared.Database;
using Content.Shared.Emag.Components;
using Content.Shared.Emag.Systems;
using Content.Shared._Funkystation.MalfAI;
using Content.Shared._Funkystation.MalfAI.Actions;
using Content.Shared.Popups;
using Content.Shared.Silicons.StationAi;

namespace Content.Server._Funkystation.MalfAI;

/// <summary>
/// Handles the Malf AI Override Machine ability: remotely emags the targeted bot,
/// subverting its behaviour (e.g. medibots start injecting nocturine).
/// </summary>
public sealed class MalfAiOverrideSystem : EntitySystem
{
    [Dependency] private readonly IAdminLogManager _adminLog = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedTransformSystem _xforms = default!;

    private readonly HashSet<Entity<TransformComponent>> _entityBuffer = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MalfAiMarkerComponent, MalfAiOverrideMachineActionEvent>(OnOverride);
    }

    /// <summary>
    /// Popups must be positioned at the AI eye to be visible to the player (the brain is hidden in the core).
    /// </summary>
    private EntityUid? GetAiEyeForPopup(EntityUid ai)
    {
        var core = Transform(ai).ParentUid;
        if (!TryComp<StationAiCoreComponent>(core, out var coreComp))
            return null;

        return coreComp.RemoteEntity;
    }

    private void OnOverride(Entity<MalfAiMarkerComponent> ent, ref MalfAiOverrideMachineActionEvent args)
    {
        if (args.Handled)
            return;

        var popupTarget = GetAiEyeForPopup(ent.Owner) ?? ent.Owner;

        // Find emaggable entities in a very small radius around the clicked point,
        // closest to the cursor first.
        _entityBuffer.Clear();
        _lookup.GetEntitiesInRange(args.Target, 0.35f, _entityBuffer);

        var targetPos = _xforms.ToMapCoordinates(args.Target).Position;
        var candidates = new List<(EntityUid Uid, float Dist)>();
        foreach (var nearby in _entityBuffer)
        {
            if (nearby.Owner == ent.Owner)
                continue;

            candidates.Add((nearby.Owner, (_xforms.GetWorldPosition(nearby.Comp) - targetPos).LengthSquared()));
        }
        candidates.Sort((a, b) => a.Dist.CompareTo(b.Dist));

        foreach (var (target, _) in candidates)
        {

            // Already subverted.
            if (TryComp<EmaggedComponent>(target, out var emagged) &&
                (emagged.EmagType & EmagType.Interaction) != 0)
                continue;

            // Remote emag: raise the standard emag event; only entities that
            // handle it (medibots, cleanbots, vendors...) get subverted.
            var ev = new GotEmaggedEvent(ent.Owner, EmagType.Interaction);
            RaiseLocalEvent(target, ref ev);

            if (!ev.Handled)
                continue;

            if (!ev.Repeatable)
            {
                var comp = EnsureComp<EmaggedComponent>(target);
                comp.EmagType |= EmagType.Interaction;
                Dirty(target, comp);
            }

            _adminLog.Add(LogType.Emag, LogImpact.High,
                $"Malf AI {ToPrettyString(ent.Owner)} remotely emagged {ToPrettyString(target)}");

            _popup.PopupEntity(Loc.GetString("malfai-override-success"), popupTarget, ent.Owner);
            args.Handled = true;
            return;
        }

        _popup.PopupEntity(Loc.GetString("malfai-override-no-target"), popupTarget, ent.Owner);
    }
}
