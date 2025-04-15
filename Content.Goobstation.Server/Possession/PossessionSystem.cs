// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.Changeling;
using Content.Goobstation.Shared.Devil;
using Content.Goobstation.Shared.Devil.Actions;
using Content.Goobstation.Shared.Religion;
using Content.Server.Ghost;
using Content.Server.Polymorph.Systems;
using Content.Server.Stunnable;
using Content.Shared._Goobstation.Wizard.FadingTimedDespawn;
using Content.Shared.Actions;
using Content.Shared.CombatMode.Pacification;
using Content.Shared.Coordinates;
using Content.Shared.Examine;
using Content.Shared.Ghost;
using Content.Shared.Heretic;
using Content.Shared.Mind;
using Content.Shared.Mindshield.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Polymorph;
using Content.Shared.Popups;
using Content.Shared.Zombies;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Spawners;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.Possession;

public sealed partial class PossessionSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly StunSystem _stun = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<PossessedComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<PossessedComponent, ComponentRemove>(OnComponentRemoved);
        SubscribeLocalEvent<PossessedComponent, ExaminedEvent>(OnExamined);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<PossessedComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (_timing.CurTime >= comp.PossessionEndTime)
                RemComp<PossessedComponent>(uid);

            if (comp.DoPacify)
            {
                comp.WasPacified = true;
                EnsureComp<PacifiedComponent>(uid);
            }

            comp.PossessionTimeRemaining = comp.PossessionEndTime - _timing.CurTime;
        }
    }

    private void OnStartup(EntityUid uid, PossessedComponent comp, ComponentStartup args)
    {
        EnsureComp<WeakToHolyComponent>(uid);
    }
    private void OnComponentRemoved(EntityUid uid, PossessedComponent comp, ComponentRemove args)
    {
        // Remove associated components.
        if (comp.WasPacified)
            RemComp<PacifiedComponent>(comp.OriginalEntity);
        RemComp<WeakToHolyComponent>(comp.OriginalEntity);

        // Return the possessors mind to their body, and the target to theirs.
        if (!TerminatingOrDeleted(comp.PossessorOriginalEntity))
            _mind.TransferTo(comp.PossessorMindId, comp.PossessorOriginalEntity);
        if (!TerminatingOrDeleted(comp.OriginalEntity))
            _mind.TransferTo(comp.OriginalMindId, comp.OriginalEntity);

        // Uncross those beams. This shit is jank yo!
        if (TryComp<ActorComponent>(comp.PossessorOriginalEntity, out var possessorActorComponent))
            _mind.SetUserId(comp.PossessorMindId, possessorActorComponent.PlayerSession.UserId);

        // Paralyze, so you can't just magdump them.
        _stun.TryParalyze(uid, TimeSpan.FromSeconds(10), false);
        _popup.PopupEntity(Loc.GetString("possession-end-popup", ("target", uid)), uid, PopupType.LargeCaution);

        // Teleport to the entity, kinda like you're popping out of their head!
        if (TerminatingOrDeleted(comp.OriginalEntity))
            return;

        var coordinates = _transform.ToMapCoordinates(comp.OriginalEntity.ToCoordinates());

        if (!TerminatingOrDeleted(comp.PossessorOriginalEntity))
            _transform.SetMapCoordinates(comp.PossessorOriginalEntity, coordinates);
    }

    private void OnExamined(EntityUid uid, PossessedComponent comp, ExaminedEvent args)
    {
        if (!args.IsInDetailsRange || _net.IsClient || comp.PossessorMindId == args.Examiner)
            return;

        var timeRemaining = Math.Floor(comp.PossessionTimeRemaining.TotalSeconds);
        args.PushMarkup(Loc.GetString("possessed-component-examined", ("timeremaining", timeRemaining)));
    }

    /// <summary>
    /// Attempts to temporarily possess a target.
    /// </summary>
    /// <param name="possessed">The entity being possessed.</param>
    /// <param name="possessor">The entity possessing the previous entity.</param>
    /// <param name="possessionDuration">How long does the possession last in seconds.</param>
    /// <param name="pacifyPossessed">Should the possessor be pacified while inside the possessed body?</param>
    /// <param name="doesMindshieldBlock">Does having a mindshield block being possessed?</param>
    public bool TryPossessTarget(EntityUid possessed, EntityUid possessor, TimeSpan possessionDuration, bool pacifyPossessed, bool doesMindshieldBlock = false)
    {
        // Possessing a dead guy? What.
        if (_mobState.IsIncapacitated(possessed) || HasComp<ZombieComponent>(possessed))
        {
            _popup.PopupClient(Loc.GetString("possession-fail-target-dead"), possessor, possessor);
            return false;
        }

        // if you ever wanted to prevent this
        if (doesMindshieldBlock && HasComp<MindShieldComponent>(possessed))
        {
            _popup.PopupClient(Loc.GetString("possession-fail-target-shielded"), possessor, possessor);
            return false;
        }

        List<(Type, string)> blockers =
        [
            (typeof(ChangelingComponent), "changeling"),
            (typeof(HereticComponent), "heretic"),
            (typeof(GhoulComponent), "ghoul"),
            (typeof(GhostComponent), "ghost"),
            (typeof(SpectralComponent), "ghost"),
            (typeof(TimedDespawnComponent), "temporary"),
            (typeof(FadingTimedDespawnComponent), "temporary"),
        ];

        foreach (var (item1, item2) in blockers)
        {
            if (CheckMindswapBlocker(item1, item2, possessed, possessor))
                return false;
        }

        if (!_mind.TryGetMind(possessor, out var possessorMind, out var possessorMindComp))
            return false;

        _mind.TryGetMind(possessed, out var possessedMind, out var possessedMindComp);

        var possessedComp = EnsureComp<PossessedComponent>(possessed);

        if (pacifyPossessed)
            possessedComp.DoPacify = true;

        // Get the possession time.
        possessedComp.PossessionEndTime = _timing.CurTime + possessionDuration;

        // Store possessors original information.
        possessedComp.PossessorOriginalEntity = possessor;
        possessedComp.PossessorMindId = possessorMind;
        possessedComp.PossessorMindComponent = possessorMindComp;

        // Store possessed original info
        possessedComp.OriginalEntity = possessed;
        possessedComp.OriginalMindId = possessedMind;

        if (possessedMindComp != null)
            possessedComp.OriginalMindComponent = possessedMindComp;

        // Detach target
        _mind.TransferTo(possessedMind, null);

        // Transfer into target
        _mind.TransferTo(possessorMind, possessed);

        // SFX
        _popup.PopupEntity(Loc.GetString("possession-popup-self"), possessedMind, possessedMind, PopupType.LargeCaution);
        _popup.PopupEntity(Loc.GetString("possession-popup-others", ("target", possessed)), possessed, PopupType.MediumCaution);
        _audio.PlayPvs(possessedComp.PossessionSoundPath, possessed);

        return true;
    }

    private bool CheckMindswapBlocker(Type type, string message, EntityUid possessed, EntityUid possessor)
    {
        if (!HasComp(possessed, type))
            return false;

        _popup.PopupClient(Loc.GetString($"possession-fail-{message}"), possessor, possessor);
        return true;
    }


}
