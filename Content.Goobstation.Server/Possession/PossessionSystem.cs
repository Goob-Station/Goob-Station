// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Solstice <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.Changeling;
using Content.Goobstation.Shared.Bible;
using Content.Goobstation.Shared.Devil;
using Content.Goobstation.Shared.Possession;
using Content.Goobstation.Shared.Religion;
using Content.Server.Actions;
using Content.Server.Polymorph.Components;
using Content.Server.Polymorph.Systems;
using Content.Server.Stunnable;
using Content.Shared._Goobstation.Wizard.FadingTimedDespawn;
using Content.Shared.Actions;
using Content.Shared.Administration.Logs;
using Content.Shared.CombatMode.Pacification;
using Content.Shared.Coordinates;
using Content.Shared.Database;
using Content.Shared.Examine;
using Content.Shared.Ghost;
using Content.Shared.Heretic;
using Content.Shared.Interaction.Events;
using Content.Shared.Mind;
using Content.Shared.Mindshield.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Content.Shared.Tag;
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
    [Dependency] private readonly ISharedAdminLogManager _admin = default!;
    [Dependency] private readonly ActionsSystem _action = default!;
    [Dependency] private readonly PolymorphSystem _polymorph = default!;
    [Dependency] private readonly TagSystem _tag = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<PossessedComponent, MapInitEvent>(OnInit);
        SubscribeLocalEvent<PossessedComponent, ComponentRemove>(OnComponentRemoved);

        SubscribeLocalEvent<PossessedComponent, ExaminedEvent>(OnExamined);

        SubscribeLocalEvent<PossessedComponent, EndPossessionEarlyEvent>(OnEarlyEnd);
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

    private void OnInit(Entity<PossessedComponent> possessed, ref MapInitEvent args)
    {
        if (!HasComp<WeakToHolyComponent>(possessed))
            AddComp<WeakToHolyComponent>(possessed).AlwaysTakeHoly = true;
        else
            possessed.Comp.WasWeakToHoly = true;

        if (possessed.Comp.HideActions)
            possessed.Comp.HiddenActions = _action.HideActions(possessed);

        _action.AddAction(possessed, ref possessed.Comp.ActionEntity, possessed.Comp.EndPossessionAction);

        _tag.AddTag(possessed, "CannotSuicideAny");

        possessed.Comp.PossessedContainer = _container.EnsureContainer<Container>(possessed, "PossessedContainer");
    }

    private void OnEarlyEnd(EntityUid uid, PossessedComponent comp, ref EndPossessionEarlyEvent args)
    {
        if (args.Handled)
            return;

        // if polymorphed, undo
        _polymorph.Revert(uid);
        RemCompDeferred(uid, comp);

        args.Handled = true;
    }
    private void OnComponentRemoved(Entity<PossessedComponent> possessed, ref ComponentRemove args)
    {
        MapCoordinates? coordinates = null;

        _action.RemoveAction(possessed, possessed.Comp.ActionEntity);

        if (possessed.Comp.HideActions)
            _action.UnHideActions(possessed, possessed.Comp.HiddenActions);

        if (possessed.Comp.PolymorphEntity && HasComp<PolymorphedEntityComponent>(possessed))
            _polymorph.Revert(possessed.Owner);

        _tag.RemoveTag(possessed, "CannotSuicideAny");

        // Remove associated components.
        if (!possessed.Comp.WasPacified)
            RemComp<PacifiedComponent>(possessed.Comp.OriginalEntity);

        if (!possessed.Comp.WasWeakToHoly)
            RemComp<WeakToHolyComponent>(possessed.Comp.OriginalEntity);

        // Return the possessors mind to their body, and the target to theirs.
        if (!TerminatingOrDeleted(possessed.Comp.PossessorMindId))
            _mind.TransferTo(possessed.Comp.PossessorMindId, possessed.Comp.PossessorOriginalEntity);
        if (!TerminatingOrDeleted(possessed.Comp.OriginalMindId))
            _mind.TransferTo(possessed.Comp.OriginalMindId, possessed.Comp.OriginalEntity);

        if (!TerminatingOrDeleted(possessed.Comp.OriginalEntity))
            coordinates = _transform.ToMapCoordinates(possessed.Comp.OriginalEntity.ToCoordinates());

        // Paralyze, so you can't just magdump them.
        _stun.TryParalyze(possessed, TimeSpan.FromSeconds(10), false);
        _popup.PopupEntity(Loc.GetString("possession-end-popup", ("target", possessed)), possessed, PopupType.LargeCaution);

        // Teleport to the entity, kinda like you're popping out of their head!
        if (!TerminatingOrDeleted(possessed.Comp.PossessorOriginalEntity) && coordinates is not null)
            _transform.SetMapCoordinates(possessed.Comp.PossessorOriginalEntity, coordinates.Value);

        _container.CleanContainer(possessed.Comp.PossessedContainer);
    }

    private void OnExamined(Entity<PossessedComponent> possessed, ref ExaminedEvent args)
    {
        if (!args.IsInDetailsRange
            || args.Examined != args.Examiner)
            return;

        var timeRemaining = Math.Floor(possessed.Comp.PossessionTimeRemaining.TotalSeconds);
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
    /// <param name="doesChaplainBlock">Is the chaplain immune to this possession?</param>
    /// <param name="HideActions">Should all actions be hidden during?</param>
    public bool TryPossessTarget(EntityUid possessed, EntityUid possessor, TimeSpan possessionDuration, bool pacifyPossessed, bool doesMindshieldBlock = false, bool doesChaplainBlock = true, bool hideActions = true, bool polymorphPossessor = true)
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

        if (doesChaplainBlock && HasComp<BibleUserComponent>(possessed))
        {
            _popup.PopupClient(Loc.GetString("possession-fail-target-chaplain"), possessor, possessor);
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
            (typeof(DevilComponent), "devil"),
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

        if (!_mind.TryGetMind(possessor, out var possessorMind, out _))
            return false;

        DoPossess(possessed, possessor, possessionDuration, possessorMind, pacifyPossessed, hideActions, polymorphPossessor);
        return true;
    }

    private void DoPossess(EntityUid? possessedNullable, EntityUid possessor, TimeSpan possessionDuration,  EntityUid possessorMind, bool pacifyPossessed, bool hideActions, bool polymorphPossessor)
    {
        if (possessedNullable is not { } possessed)
            return;

        var possessedComp = EnsureComp<PossessedComponent>(possessed);
        possessedComp.HideActions = hideActions;

        if (pacifyPossessed)
        {
            if (!HasComp<PacifiedComponent>(possessed))
                EnsureComp<PacifiedComponent>(possessed);
            else
                possessedComp.WasPacified = true;
        }

        possessedComp.PolymorphEntity = polymorphPossessor;
        if (polymorphPossessor)
            _polymorph.PolymorphEntity(possessor, possessedComp.Polymorph);

        // Get the possession time.
        possessedComp.PossessionEndTime = _timing.CurTime + possessionDuration;

        // Store possessors original information.
        possessedComp.PossessorOriginalEntity = possessor;
        possessedComp.PossessorMindId = possessorMind;

        // Store possessed original info
        possessedComp.OriginalEntity = possessed;

        if (_mind.TryGetMind(possessed, out var possessedMind, out _))
        {
            possessedComp.OriginalMindId = possessedMind;

            // Nobodies gonna know.
            var dummy = Spawn("FoodSnackLollypop", MapCoordinates.Nullspace);
            _container.Insert(dummy, possessedComp.PossessedContainer);

            _mind.TransferTo(possessedMind, dummy);
        }

        // Transfer into target
        _mind.TransferTo(possessorMind, possessed);

        // SFX
        _popup.PopupEntity(Loc.GetString("possession-popup-self"), possessedMind, possessedMind, PopupType.LargeCaution);
        _popup.PopupEntity(Loc.GetString("possession-popup-others", ("target", possessed)), possessed, PopupType.MediumCaution);
        _audio.PlayPvs(possessedComp.PossessionSoundPath, possessed);

        Log.Info($"{ToPrettyString(possessor)} possessed {ToPrettyString(possessed)}");
        _admin.Add(LogType.Mind, LogImpact.High, $"{ToPrettyString(possessor)} possessed {ToPrettyString(possessed)}");
    }

    private bool CheckMindswapBlocker(Type type, string message, EntityUid possessed, EntityUid possessor)
    {
        if (!HasComp(possessed, type))
            return false;

        _popup.PopupClient(Loc.GetString($"possession-fail-{message}"), possessor, possessor);
        return true;
    }


}
