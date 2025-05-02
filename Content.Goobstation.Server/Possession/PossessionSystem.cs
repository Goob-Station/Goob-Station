// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Solstice <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.Changeling;
using Content.Goobstation.Shared.Religion;
using Content.Server.Stunnable;
using Content.Shared._Goobstation.Wizard.FadingTimedDespawn;
using Content.Shared.CombatMode.Pacification;
using Content.Shared.Coordinates;
using Content.Shared.Examine;
using Content.Shared.Ghost;
using Content.Shared.Heretic;
using Content.Shared.Mind;
using Content.Shared.Mindshield.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Content.Shared.Zombies;
using Robust.Server.Containers;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Map;
using Robust.Shared.Spawners;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.Possession;

public sealed partial class PossessionSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly StunSystem _stun = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly ContainerSystem _container = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<PossessedComponent, MapInitEvent>(OnInit);
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

            comp.PossessionTimeRemaining = comp.PossessionEndTime - _timing.CurTime;
        }
    }

    private void OnInit(EntityUid uid, PossessedComponent comp, MapInitEvent args)
    {
        if (!HasComp<WeakToHolyComponent>(uid))
            AddComp<WeakToHolyComponent>(uid).AlwaysTakeHoly = true;
        else
            comp.WasWeakToHoly = true;

        comp.PossessedContainer = _container.EnsureContainer<Container>(uid, "PossessedContainer");
    }
    private void OnComponentRemoved(EntityUid uid, PossessedComponent comp, ComponentRemove args)
    {
        MapCoordinates? coordinates = null;

        // Remove associated components.
        if (!comp.WasPacified)
            RemComp<PacifiedComponent>(comp.OriginalEntity);

        if (!comp.WasWeakToHoly)
            RemComp<WeakToHolyComponent>(comp.OriginalEntity);

        // Return the possessors mind to their body, and the target to theirs.
        if (!TerminatingOrDeleted(comp.PossessorMindId))
            _mind.TransferTo(comp.PossessorMindId, comp.PossessorOriginalEntity);
        if (!TerminatingOrDeleted(comp.OriginalMindId))
            _mind.TransferTo(comp.OriginalMindId, comp.OriginalEntity);

        if (!TerminatingOrDeleted(comp.OriginalEntity))
            coordinates = _transform.ToMapCoordinates(comp.OriginalEntity.ToCoordinates());

        // Paralyze, so you can't just magdump them.
        _stun.TryParalyze(uid, TimeSpan.FromSeconds(10), false);
        _popup.PopupEntity(Loc.GetString("possession-end-popup", ("target", uid)), uid, PopupType.LargeCaution);

        // Teleport to the entity, kinda like you're popping out of their head!
        if (!TerminatingOrDeleted(comp.PossessorOriginalEntity) && coordinates is not null)
            _transform.SetMapCoordinates(comp.PossessorOriginalEntity, coordinates.Value);

        _container.CleanContainer(comp.PossessedContainer);
    }

    private void OnExamined(EntityUid uid, PossessedComponent comp, ExaminedEvent args)
    {
        if (!args.IsInDetailsRange || args.Examined != args.Examiner)
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

        if (HasComp<PossessedComponent>(possessed))
        {
            _popup.PopupClient(Loc.GetString("possession-fail-target-already-possessed"), possessor, possessor);
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

        DoPossess(possessed, possessor, possessionDuration, pacifyPossessed, possessorMind, possessorMindComp);
        return true;
    }

    private void DoPossess(EntityUid possessed, EntityUid possessor, TimeSpan possessionDuration, bool pacifyPossessed, EntityUid possessorMind, MindComponent possessorMindComp)
    {
        _mind.TryGetMind(possessed, out var possessedMind, out var possessedMindComp);

        var possessedComp = EnsureComp<PossessedComponent>(possessed);

        if (pacifyPossessed)
        {
            if (!HasComp<PacifiedComponent>(possessed))
                EnsureComp<PacifiedComponent>(possessed);
            else
                possessedComp.WasPacified = true;
        }

        // Get the possession time.
        possessedComp.PossessionEndTime = _timing.CurTime + possessionDuration;

        // Store possessors original information.
        possessedComp.PossessorOriginalEntity = possessor;
        possessedComp.PossessorMindId = possessorMind;

        // Store possessed original info
        possessedComp.OriginalEntity = possessed;
        possessedComp.OriginalMindId = possessedMind;

        // Nobodies gonna know.
        var dummy = Spawn("FoodSnackLollypop", MapCoordinates.Nullspace);
        _container.Insert(dummy, possessedComp.PossessedContainer);
        _mind.TransferTo(possessedMind, dummy);

        // Transfer into target
        _mind.TransferTo(possessorMind, possessed);

        // SFX
        _popup.PopupEntity(Loc.GetString("possession-popup-self"), possessedMind, possessedMind, PopupType.LargeCaution);
        _popup.PopupEntity(Loc.GetString("possession-popup-others", ("target", possessed)), possessed, PopupType.MediumCaution);
        _audio.PlayPvs(possessedComp.PossessionSoundPath, possessed);
    }

    private bool CheckMindswapBlocker(Type type, string message, EntityUid possessed, EntityUid possessor)
    {
        if (!HasComp(possessed, type))
            return false;

        _popup.PopupClient(Loc.GetString($"possession-fail-{message}"), possessor, possessor);
        return true;
    }


}
