using Content.Goobstation.Common.Changeling;
using Content.Goobstation.Shared.Devil;
using Content.Goobstation.Shared.Devil.Actions;
using Content.Server.Ghost;
using Content.Server.Polymorph.Systems;
using Content.Server.Stunnable;
using Content.Shared._Goobstation.Wizard.FadingTimedDespawn;
using Content.Shared.CombatMode.Pacification;
using Content.Shared.Examine;
using Content.Shared.Ghost;
using Content.Shared.Heretic;
using Content.Shared.Mind;
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

    public void TryPossessTarget(DevilPossessionEvent args, bool hidePossessorEntity, bool pacifyPossessed)
    {
        // Possessing a dead guy? What.
        if (_mobState.IsIncapacitated(args.Target) || HasComp<ZombieComponent>(args.Target))
        {
            _popup.PopupClient(Loc.GetString("possession-fail-target-dead"), args.Performer, args.Target);
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
            if (CheckMindswapBlocker(item1, item2, args))
                return;
        }

        args.Handled = true;

        if (!_mind.TryGetMind(args.Performer, out var userMind, out var userMindComp))
            return;

        var possessedComp = EnsureComp<PossessedComponent>(args.Target);

        // I love generic systems.
        if (pacifyPossessed)
            possessedComp.DoPacify = true;

        // Get the possession time.
        if (TryComp<DevilComponent>(args.Performer, out var devilComponent))
            possessedComp.PossessionEndTime = _timing.CurTime + GetPossessionDuration(devilComponent);

        // Store possessors original information.
        possessedComp.PossessorMindId = userMind;
        possessedComp.PossessorOriginalEntity = args.Performer;

        // Store targets original mind, and detach them.
        if (_mind.TryGetMind(args.Target, out var targetMind, out var targetMindComp) && targetMindComp.UserId != null)
        {
            possessedComp.OriginalMindId = targetMind;
            _mind.TransferTo(targetMind, null);
        }

        // Transfer into target
        _mind.TransferTo(userMind, args.Target);

        // Jaunt the body so it can't be tampered with.
        // Easier than sending you to the paused map lol.
        if (hidePossessorEntity)
        {
            Spawn("PolymorphShadowJauntAnimation", Transform(possessedComp.PossessorOriginalEntity).Coordinates);
            Spawn(_pentagramEffectProto, Transform(possessedComp.PossessorOriginalEntity).Coordinates);

            if (devilComponent != null)
                _poly.PolymorphEntity(possessedComp.PossessorOriginalEntity, GetJauntEntity(devilComponent));
        }

        if (!_net.IsServer)
            return;

        // SFX
        _popup.PopupEntity(Loc.GetString("possession-popup-self"), targetMind, targetMind, PopupType.LargeCaution);
        _popup.PopupEntity(Loc.GetString("possession-popup-others", ("target", args.Target)), args.Target, PopupType.MediumCaution);
        _audio.PlayPvs(possessedComp.PossessionSoundPath, args.Target);
    }

    private bool CheckMindswapBlocker(Type type, string message, DevilPossessionEvent args)
    {
        if (!HasComp(args.Target, type))
            return false;

        _popup.PopupClient(Loc.GetString($"possession-fail-{message}"), args.Performer, args.Performer); // Change this later
        return true;
    }

    private static TimeSpan GetPossessionDuration(DevilComponent comp)
    {
        return comp.PowerLevel switch
        {
            2 => TimeSpan.FromSeconds(60),
            3 => TimeSpan.FromSeconds(90),
            _ => TimeSpan.FromSeconds(30),
        };
    }

    private ProtoId<PolymorphPrototype> GetJauntEntity(DevilComponent comp)
    {
        return comp.PowerLevel switch
        {
            2 => "ShadowJaunt60",
            3 => "ShadowJaunt90",
            _ => "ShadowJaunt30",
        };
    }
}
