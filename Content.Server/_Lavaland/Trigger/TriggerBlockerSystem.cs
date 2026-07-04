using Content.Server.Explosion.EntitySystems;
using Content.Shared.Trigger;
using Content.Shared.Whitelist;
using Content.Server.Station.Systems; // Pirates: death rattle update

namespace Content.Server._Lavaland.Trigger;

public sealed class TriggerBlockerSystem : EntitySystem
{
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;
    [Dependency] private readonly StationSystem _station = default!; // Pirates: death rattle update
    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<TriggerBlockerComponent, AttemptTriggerEvent>(OnTrigger);
    }

    private void OnTrigger(Entity<TriggerBlockerComponent> ent, ref AttemptTriggerEvent args)
    {
        if (args.Cancelled)
            return;

        var map = Transform(ent).MapUid;
        #region Pirates: death rattle update
        if (ent.Comp.RequireOffStation)
        {
            var target = args.User ?? ent.Owner;
            args.Cancelled = _station.GetOwningStation(target) != null;
            return;
        }
        #endregion

        if (map == null
            || _whitelist.IsWhitelistPass(ent.Comp.MapWhitelist, map.Value)
            || _whitelist.IsWhitelistFail(ent.Comp.MapBlacklist, map.Value))
            return;

        args.Cancelled = true;
    }
}
