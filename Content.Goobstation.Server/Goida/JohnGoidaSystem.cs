using Content.Server.Spawners.Components;
using Content.Server.Station.Systems;
using Content.Shared.GameTicking;
using Content.Shared.Mind;
using Content.Shared.Popups;
using Robust.Shared.Map;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.Goida;

/// <summary>
/// When someone names their character <see cref="John"/> it replaces them with <see cref="Goida"/>, spawned at the assistant spawn point.
/// This is hardcoded to also make a curse of 220 popup.
/// Ghost roles etc shouldn't be affected.
/// </summary>
public sealed class JohnGoidaSystem : EntitySystem
{
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly StationSystem _station = default!;

    public const string John = "John Goida";
    public static readonly EntProtoId Goida = "MobGoidaBotValid";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PlayerBeforeSpawnEvent>(OnBeforeSpawn);
    }

    private void OnBeforeSpawn(PlayerBeforeSpawnEvent args)
    {
        if (args.Profile.Name != John)
            return;

        args.Handled = true;

        var mind = _mind.CreateMind(args.Player.UserId, args.Profile.Name);
        _mind.SetUserId(mind, args.Player.UserId);

        var mob = Spawn(Goida, GetSpawn(args.Station));
        _mind.TransferTo(mind, mob);

        // curse is killsign, goida
        _popup.PopupEntity(Loc.GetString("curse-of-220"), mob, PopupType.LargeCaution);
    }

    private EntityCoordinates GetSpawn(EntityUid station)
    {
        // forces you to spawn at a latejoin spawn point
        var query = EntityQueryEnumerator<SpawnPointComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var comp, out var xform))
        {
            if (_station.GetOwningStation(uid, xform) != station)
                continue;

            if (comp.SpawnType == SpawnPointType.LateJoin)
                return xform.Coordinates;
        }

        // how have you done this, fall back to an arbitrary player
        var goida = EntityQueryEnumerator<ActorComponent, TransformComponent>();
        while (goida.MoveNext(out _, out var fallback))
        {
            return fallback.Coordinates;
        }

        // goidabot in nullspace go use /suicide
        // you are definitely just doing this in dev if you manage to get here anyway
        Log.Error($"goidabot in nullspace for {ToPrettyString(station)}, curse of 220");
        return EntityCoordinates.Invalid;
    }
}
