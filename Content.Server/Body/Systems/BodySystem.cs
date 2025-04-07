// SPDX-FileCopyrightText: 2024 0x6273 <0x40@keemail.me>
// SPDX-FileCopyrightText: 2023 Doru991 <75124791+Doru991@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Jake Huxell <JakeHuxell@pm.me>
// SPDX-FileCopyrightText: 2021 Javier Guardia Fernández <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Jezithyr <jezithyr@gmail.com>
// SPDX-FileCopyrightText: 2022 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Matt <matt@isnor.io>
// SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 Plykiya <58439124+Plykiya@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Psychpsyo <60073468+Psychpsyo@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Rane <60792108+Elijahrane@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 ShadowCommander <10494922+ShadowCommander@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <gradientvera@outlook.com>
// SPDX-FileCopyrightText: 2023 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Winkarst <74284083+Winkarst-cpu@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 deathride58 <deathride58@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 keronshb <54602815+keronshb@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
using Content.Server.Body.Components;
using Content.Server.Ghost;
using Content.Server.Humanoid;
using Content.Shared._Shitmed.Body.Part;
using Content.Shared.Body.Components;
using Content.Shared.Body.Part;
using Content.Shared.Body.Systems;
using Content.Shared.Humanoid;
using Content.Shared.Mind;
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Events;
using Content.Shared.Movement.Systems;
using Robust.Shared.Audio;
using Robust.Shared.Timing;
using System.Numerics;

// Shitmed Change
using System.Linq;
using Content.Shared.Gibbing.Events;

namespace Content.Server.Body.Systems;

public sealed partial class BodySystem : SharedBodySystem // Shitmed change: made partial
{
    [Dependency] private readonly BloodstreamSystem _bloodstream = default!; // Shitmed Change
    [Dependency] private readonly GhostSystem _ghostSystem = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly HumanoidAppearanceSystem _humanoidSystem = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!; // Shitmed Change
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly SharedMindSystem _mindSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BodyComponent, MoveInputEvent>(OnRelayMoveInput);
        SubscribeLocalEvent<BodyComponent, ApplyMetabolicMultiplierEvent>(OnApplyMetabolicMultiplier);
    }

    private void OnRelayMoveInput(Entity<BodyComponent> ent, ref MoveInputEvent args)
    {
        // If they haven't actually moved then ignore it.
        if ((args.Entity.Comp.HeldMoveButtons &
             (MoveButtons.Down | MoveButtons.Left | MoveButtons.Up | MoveButtons.Right)) == 0x0)
        {
            return;
        }

        if (_mobState.IsDead(ent) && _mindSystem.TryGetMind(ent, out var mindId, out var mind))
        {
            mind.TimeOfDeath ??= _gameTiming.RealTime;
            _ghostSystem.OnGhostAttempt(mindId, canReturnGlobal: true, mind: mind);
        }
    }

    private void OnApplyMetabolicMultiplier(
        Entity<BodyComponent> ent,
        ref ApplyMetabolicMultiplierEvent args)
    {
        foreach (var organ in GetBodyOrgans(ent, ent))
        {
            RaiseLocalEvent(organ.Id, ref args);
        }
    }

    protected override void AddPart(
        Entity<BodyComponent?> bodyEnt,
        Entity<BodyPartComponent> partEnt,
        string slotId)
    {
        // TODO: Predict this probably.
        base.AddPart(bodyEnt, partEnt, slotId);

        if (TryComp<HumanoidAppearanceComponent>(bodyEnt, out var humanoid))
        {
            var layer = partEnt.Comp.ToHumanoidLayers();
            if (layer != null)
            {
                var layers = HumanoidVisualLayersExtension.Sublayers(layer.Value);
                _humanoidSystem.SetLayersVisibility(
                    bodyEnt, new[] { layer.Value }, visible: true, permanent: true, humanoid); // Shitmed Change
            }
        }
    }

    protected override void RemovePart(
        Entity<BodyComponent?> bodyEnt,
        Entity<BodyPartComponent> partEnt,
        string slotId)
    {
        base.RemovePart(bodyEnt, partEnt, slotId);

        if (!TryComp<HumanoidAppearanceComponent>(bodyEnt, out var humanoid))
            return;

        var layer = partEnt.Comp.ToHumanoidLayers();

        if (layer is null)
            return;

        var layers = HumanoidVisualLayersExtension.Sublayers(layer.Value);
        _humanoidSystem.SetLayersVisibility(
            bodyEnt, layers, visible: false, permanent: true, humanoid);
        _appearance.SetData(bodyEnt, layer, true); // Shitmed Change
    }

    public override HashSet<EntityUid> GibBody(
        EntityUid bodyId,
        bool gibOrgans = false,
        BodyComponent? body = null,
        bool launchGibs = true,
        Vector2? splatDirection = null,
        float splatModifier = 1,
        Angle splatCone = default,
        SoundSpecifier? gibSoundOverride = null,
        // Shitmed Change
        GibType gib = GibType.Gib,
        GibContentsOption contents = GibContentsOption.Drop)
    {
        if (!Resolve(bodyId, ref body, logMissing: false)
            || TerminatingOrDeleted(bodyId)
            || EntityManager.IsQueuedForDeletion(bodyId))
        {
            return new HashSet<EntityUid>();
        }

        var xform = Transform(bodyId);
        if (xform.MapUid is null)
            return new HashSet<EntityUid>();

        var gibs = base.GibBody(bodyId, gibOrgans, body, launchGibs: launchGibs,
            splatDirection: splatDirection, splatModifier: splatModifier, splatCone: splatCone,
            gib: gib, contents: contents); // Shitmed Change

        var ev = new BeingGibbedEvent(gibs);
        RaiseLocalEvent(bodyId, ref ev);

        QueueDel(bodyId);

        return gibs;
    }

    // Shitmed Change Start
    public override HashSet<EntityUid> GibPart(
        EntityUid partId,
        BodyPartComponent? part = null,
        bool launchGibs = true,
        Vector2? splatDirection = null,
        float splatModifier = 1,
        Angle splatCone = default,
        SoundSpecifier? gibSoundOverride = null)
    {
        if (!Resolve(partId, ref part, logMissing: false)
            || TerminatingOrDeleted(partId)
            || EntityManager.IsQueuedForDeletion(partId))
            return new HashSet<EntityUid>();

        if (Transform(partId).MapUid is null)
            return new HashSet<EntityUid>();

        var gibs = base.GibPart(partId, part, launchGibs: launchGibs,
            splatDirection: splatDirection, splatModifier: splatModifier, splatCone: splatCone);

        var ev = new BeingGibbedEvent(gibs);
        RaiseLocalEvent(partId, ref ev);

        if (gibs.Any())
            QueueDel(partId);

        return gibs;
    }

    public override bool BurnPart(EntityUid partId, BodyPartComponent? part = null)
    {
        if (!Resolve(partId, ref part, logMissing: false)
            || TerminatingOrDeleted(partId)
            || EntityManager.IsQueuedForDeletion(partId))
            return false;

        return base.BurnPart(partId, part);
    }

    protected override void ApplyPartMarkings(EntityUid target, BodyPartAppearanceComponent component)
    {
        return;
    }

    protected override void RemoveBodyMarkings(EntityUid target, BodyPartAppearanceComponent partAppearance, HumanoidAppearanceComponent bodyAppearance)
    {
        foreach (var (visualLayer, markingList) in partAppearance.Markings)
            foreach (var marking in markingList)
                _humanoidSystem.RemoveMarking(target, marking.MarkingId, sync: false, humanoid: bodyAppearance);

        Dirty(target, bodyAppearance);
    }

    protected override void PartRemoveDamage(Entity<BodyComponent?> bodyEnt, Entity<BodyPartComponent> partEnt)
    {
        var bleeding = partEnt.Comp.SeverBleeding;
        if (partEnt.Comp.IsVital)
            bleeding *= 2f;
        _bloodstream.TryModifyBleedAmount(bodyEnt, bleeding);
    }

    // Shitmed Change End
}