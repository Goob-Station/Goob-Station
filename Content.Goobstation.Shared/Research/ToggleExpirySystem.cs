using Content.Shared.Item.ItemToggle;
using Content.Shared.Item.ItemToggle.Components;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.Research;

public sealed partial class ToggleExpirySystem : EntitySystem
{
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private INetManager _net = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private ItemToggleSystem _toggle = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ToggleExpiryComponent, ItemToggleActivateAttemptEvent>(OnActivateAttempt);
        SubscribeLocalEvent<ToggleExpiryComponent, ItemToggledEvent>(OnToggled);
    }

    private void OnActivateAttempt(Entity<ToggleExpiryComponent> ent, ref ItemToggleActivateAttemptEvent args)
    {
        if (!ent.Comp.Expired)
            return;

        args.Cancelled = true;
        args.Popup = Loc.GetString(ent.Comp.ExpiredPopup);
    }

    private void OnToggled(Entity<ToggleExpiryComponent> ent, ref ItemToggledEvent args)
    {
        if (!_net.IsServer || !args.Activated || ent.Comp.Expired || ent.Comp.ExpireTime is not null)
            return;

        ent.Comp.ExpireTime = _timing.CurTime + ent.Comp.Duration;
        Dirty(ent);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (!_net.IsServer)
            return;

        var curTime = _timing.CurTime;
        var query = EntityQueryEnumerator<ToggleExpiryComponent, ItemToggleComponent>();
        while (query.MoveNext(out var uid, out var expiry, out var toggle))
        {
            if (expiry.ExpireTime is null || curTime < expiry.ExpireTime)
                continue;

            expiry.ExpireTime = null;
            expiry.Expired = true;
            Dirty(uid, expiry);

            if (!toggle.Activated)
                continue;

            _toggle.TryDeactivate((uid, toggle));
            _audio.PlayPvs(expiry.ExpireSound, uid);
        }
    }
}
