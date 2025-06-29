// SPDX-FileCopyrightText: 2025 AftrLite <61218133+AftrLite@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Solstice <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <linebarrelerenthusiast@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Collections.Immutable;
using Content.Server._DV.CosmicCult.Components;
using Content.Goobstation.Shared.Bible;
using Content.Goobstation.Shared.Religion; // Goobstation - Bible
using Content.Server.Popups;
using Content.Shared._DV.CosmicCult;
using Content.Shared._DV.CosmicCult.Components;
using Content.Shared._DV.CosmicCult.Components.Examine;
using Content.Shared.DoAfter;
using Content.Shared.Effects;
using Content.Shared.IdentityManagement;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Content.Shared.NPC;
using Content.Shared.Stunnable;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Player;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._DV.CosmicCult.Abilities;

public sealed class CosmicBlankSystem : EntitySystem
{
    [Dependency] private readonly CosmicCultSystem _cult = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedColorFlashEffectSystem _color = default!;
    [Dependency] private readonly SharedCosmicCultSystem _cosmicCult = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly DivineInterventionSystem _divineIntervention = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CosmicCultComponent, EventCosmicBlank>(OnCosmicBlank);
        SubscribeLocalEvent<CosmicCultComponent, EventCosmicBlankDoAfter>(OnCosmicBlankDoAfter);
    }

    private void OnCosmicBlank(Entity<CosmicCultComponent> uid, ref EventCosmicBlank args)
    {
        if (_cosmicCult.EntityIsCultist(args.Target)
            || HasComp<CosmicBlankComponent>(args.Target)
            || HasComp<ActiveNPCComponent>(args.Target))
        {
            _popup.PopupEntity(Loc.GetString("cosmicability-generic-fail"), uid, uid);
            return;
        }

        if (_divineIntervention.TouchSpellDenied(args.Target))
            return;

        if (args.Handled)
            return;

        var doargs = new DoAfterArgs(EntityManager, uid, uid.Comp.CosmicBlankDelay, new EventCosmicBlankDoAfter(), uid, args.Target)
        {
            DistanceThreshold = 1.5f,
            Hidden = false,
            BreakOnDamage = true,
            BreakOnMove = true,
            BreakOnDropItem = true,
        };

        args.Handled = true;
        _doAfter.TryStartDoAfter(doargs);
        _popup.PopupEntity(Loc.GetString("cosmicability-blank-begin", ("target", Identity.Entity(uid, EntityManager))), uid, args.Target);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var shuntQuery = EntityQueryEnumerator<InVoidComponent>();
        while (shuntQuery.MoveNext(out var uid, out var comp))
        {
            if (_timing.CurTime < comp.ExitVoidTime
                || !_mind.TryGetMind(uid, out var mindEnt, out var mind))
                continue;

            mind.PreventGhosting = false;
            _mind.TransferTo(mindEnt, comp.OriginalBody);
            RemComp<CosmicBlankComponent>(comp.OriginalBody);
            RemComp<CosmicCultExamineComponent>(comp.OriginalBody);
            _popup.PopupEntity(Loc.GetString("cosmicability-blank-return"), comp.OriginalBody, comp.OriginalBody);
            QueueDel(uid);
        }
    }

    private void OnCosmicBlankDoAfter(Entity<CosmicCultComponent> uid, ref EventCosmicBlankDoAfter args)
    {
        if (args.Args.Target is not { } target
            || args.Cancelled
            || args.Handled)
            return;

        args.Handled = true;

        if (!TryComp<MindContainerComponent>(target, out var mindContainer)
            || !mindContainer.HasMind)
            return;

        EnsureComp<CosmicBlankComponent>(target);
        var examine = EnsureComp<CosmicCultExamineComponent>(target);
        examine.CultistText = "cosmic-examine-text-abilityblank";

        _popup.PopupEntity(Loc.GetString("cosmicability-blank-success",
            ("target", Identity.Entity(target, EntityManager))), uid, uid);
        var tgtpos = Transform(target).Coordinates;
        var mindEnt = mindContainer.Mind.Value;
        var mind = Comp<MindComponent>(mindEnt);
        var comp = uid.Comp;
        mind.PreventGhosting = true;

        var spawnPoints = EntityManager
            .GetAllComponents(typeof(CosmicVoidSpawnComponent))
            .ToImmutableList();

        if (spawnPoints.IsEmpty)
            return;

        _audio.PlayPvs(comp.BlankSFX, uid, AudioParams.Default.WithVolume(6f));
        Spawn(comp.BlankVFX, tgtpos);
        var newSpawn = _random.Pick(spawnPoints);
        var spawnTgt = Transform(newSpawn.Uid).Coordinates;
        var mobUid = Spawn(comp.SpawnWisp, spawnTgt);
        EnsureComp<InVoidComponent>(mobUid, out var inVoid);
        inVoid.OriginalBody = target;
        inVoid.ExitVoidTime = _timing.CurTime + comp.CosmicBlankDuration;
        _mind.TransferTo(mindEnt, mobUid);
        _stun.TryKnockdown(target, comp.CosmicBlankDuration + TimeSpan.FromSeconds(2), true);
        _popup.PopupEntity(Loc.GetString("cosmicability-blank-transfer"), mobUid, mobUid);
        _audio.PlayPvs(comp.BlankSFX, spawnTgt, AudioParams.Default.WithVolume(6f));
        _color.RaiseEffect(Color.CadetBlue,
            new List<EntityUid>() { target },
            Filter.Pvs(target, entityManager: EntityManager));
        Spawn(comp.BlankVFX, spawnTgt);
        _cult.MalignEcho(uid);
    }
}
