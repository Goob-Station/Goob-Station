using Content.Server.Explosion.EntitySystems;
using Content.Shared.Trigger;
using Content.Shared.Whitelist;

namespace Content.Server._Lavaland.Trigger;

public sealed class TriggerBlockerSystem : EntitySystem
{
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;

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

        if (map == null
            || _whitelist.IsWhitelistPass(ent.Comp.MapWhitelist, map.Value)
            || _whitelist.IsBlacklistFail(ent.Comp.MapBlacklist, map.Value))
            return;

        args.Cancelled = true;
    }
}
