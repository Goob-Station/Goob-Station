using Content.Goobstation.Common.Changeling;
using Content.Goobstation.Shared.Devil;
using Content.Goobstation.Shared.Devil.Actions;
using Content.Server.Ghost;
using Content.Server.Polymorph.Systems;
using Content.Server.Stunnable;
using Content.Shared._Goobstation.Wizard.FadingTimedDespawn;
using Content.Shared.Actions;
using Content.Shared.CombatMode.Pacification;
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
    [Dependency] private readonly PolymorphSystem _poly = default!;

    private readonly EntProtoId _pentagramEffectProto = "Pentagram";

    public override void Initialize()
    {
        base.Initialize();
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

            comp.PossessionTimeRemaining = _timing.CurTime - comp.PossessionEndTime;
        }
    }
    private void OnComponentRemoved(EntityUid uid, PossessedComponent comp, ComponentRemove args)
    {
        if (!comp.WasPacified)
            RemComp<PacifiedComponent>(uid);

        // Return the possessors mind to their body, and the target to theirs.
        _mind.TransferTo(comp.PossessorMindId, comp.PossessorOriginalEntity);
        _mind.TransferTo(comp.OriginalMindId, uid);

        _stun.TryParalyze(uid, TimeSpan.FromSeconds(10), false);
        _popup.PopupEntity(Loc.GetString("possession-end-popup", ("target", uid)), uid, PopupType.LargeCaution);
    }

    private void OnExamined(EntityUid uid, PossessedComponent comp, ExaminedEvent args)
    {
        if (!args.IsInDetailsRange || _net.IsClient || comp.PossessorMindId == args.Examiner)
            return;

        var timeremaining = Math.Floor(comp.PossessionTimeRemaining.TotalSeconds);
        args.PushMarkup(Loc.GetString("possessed-component-examined", ("timeremaining", timeremaining)));
    }

    /// <summary>
    /// Attempts to temporarily possess a target.
    /// </summary>
    /// <param name="possessed">The entity being possessed.</param>
    /// <param name="possessor">The entity possessing the previous entity.</param>
    /// <param name="possessionDuration">How long does the possession last in seconds.</param>
    /// <param name="pacifyPossessed">Should the possessor be pacified while inside the possessed body?</param>
    /// <param name="doesMindshieldBlock">Does having a mindshield block being possessed?</param>
    public void TryPossessTarget(EntityUid possessed, EntityUid possessor, TimeSpan possessionDuration, bool pacifyPossessed, bool doesMindshieldBlock = false)
    {
        // Possessing a dead guy? What.
        if (_mobState.IsIncapacitated(possessed) || HasComp<ZombieComponent>(possessed))
        {
            _popup.PopupClient(Loc.GetString("possession-fail-target-dead"), possessor, possessor);
            return;
        }

        // if you ever wanted to prevent this
        if (doesMindshieldBlock && HasComp<MindShieldComponent>(possessed))
        {
            _popup.PopupClient(Loc.GetString("possession-fail-target-shielded"), possessor, possessor);
            return;
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
                return;
        }

        if (!_mind.TryGetMind(possessor, out var possessorMind, out var possessorMindComp))
            return;

        var possessedComp = EnsureComp<PossessedComponent>(possessed);

        if (pacifyPossessed)
            possessedComp.DoPacify = true;

        // Get the possession time.
        possessedComp.PossessionEndTime = _timing.CurTime + possessionDuration;

        // Store possessors original information.
        possessedComp.PossessorMindId = possessorMind;
        possessedComp.PossessorOriginalEntity = possessor;

        // Store targets original mind, and detach them.
        if (_mind.TryGetMind(possessed, out var possessedMind, out var possessedMindComp) && possessedMindComp.UserId != null)
        {
            possessedComp.OriginalMindId = possessedMind;
            _mind.TransferTo(possessedMind, null);
        }

        // Transfer into target
        _mind.TransferTo(possessorMind, possessed);

        if (!_net.IsServer)
            return;

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
