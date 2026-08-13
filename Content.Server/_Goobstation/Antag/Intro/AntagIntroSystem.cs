using Content.Goobstation.Shared.Antag.Intro;
using Content.Server.Antag;
using Content.Server.Antag.Components;
using Content.Shared.GameTicking;
using Robust.Shared.Enums;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Server._Goobstation.Antag.Intro;

/// <summary>
/// Holds an antagonist assignment back while the player it is meant for watches the opening.
/// </summary>
public sealed class AntagIntroSystem : EntitySystem
{
    [Dependency] private readonly AntagSelectionSystem _antag = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;

    private readonly List<Hold> _held = new();

    private sealed record Hold(
        Entity<AntagSelectionComponent> Rule,
        ICommonSession Session,
        AntagSelectionDefinition Def,
        bool IgnoreSpawner,
        ProtoId<AntagIntroPrototype> Intro,
        TimeSpan Expires);

    public override void Initialize()
    {
        base.Initialize();

        SubscribeNetworkEvent<AntagIntroFinishedEvent>(OnFinished);
        SubscribeLocalEvent<RoundRestartCleanupEvent>(_ => _held.Clear());
    }

    /// <summary>
    /// Takes an assignment off the servers hands and puts the opening on the players screen.
    /// </summary>
    public bool TryHold(Entity<AntagSelectionComponent> rule,
        ICommonSession? session,
        AntagSelectionDefinition def,
        bool ignoreSpawner)
    {
        if (session == null || def.AntagIntro is not { } id)
            return false;

        if (!_proto.TryIndex(id, out var intro))
        {
            Log.Error($"Antag definition in {ToPrettyString(rule)} names unknown intro {id}.");
            return false;
        }

        _held.Add(new Hold(rule, session, def, ignoreSpawner, id, _timing.CurTime + intro.Timeout));
        RaiseNetworkEvent(new AntagIntroPlayEvent(id), session);
        return true;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        for (var i = _held.Count - 1; i >= 0; i--)
        {
            var hold = _held[i];

            if (_timing.CurTime >= hold.Expires
                || hold.Session.Status == SessionStatus.Disconnected)
                Release(hold);
        }
    }

    private void OnFinished(AntagIntroFinishedEvent args, EntitySessionEventArgs session)
    {
        if (_held.Find(h => h.Session == session.SenderSession && h.Intro == args.Intro) is { } hold)
            Release(hold);
    }

    private void Release(Hold hold)
    {
        _held.Remove(hold);

        if (Exists(hold.Rule))
            _antag.MakeAntag(hold.Rule, hold.Session, hold.Def, hold.IgnoreSpawner, skipIntro: true);
    }
}
