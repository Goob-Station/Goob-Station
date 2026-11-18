using Content.Goobstation.Shared.Bongle;
using Content.Shared.Popups;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.Bongle;

public sealed partial class SeethingSystem : EntitySystem
{
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private SharedAudioSystem _audio = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SeethingComponent, ComponentStartup>(OnStartup);
    }

    private void OnStartup(Entity<SeethingComponent> ent, ref ComponentStartup args)
    {
        var (_, comp) = ent;

        comp.ExpireTime = _timing.CurTime + comp.Duration;
        comp.NextPopup = _timing.CurTime + comp.PopupInterval;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<SeethingComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (_timing.CurTime >= comp.ExpireTime)
            {
                RemCompDeferred<SeethingComponent>(uid);
                continue;
            }

            if (_timing.CurTime >= comp.NextHeartbeat)
            {
                comp.NextHeartbeat = _timing.CurTime + comp.HeartbeatInterval;
                _audio.PlayEntity(comp.HeartbeatSound, uid, uid);
            }

            if (_timing.CurTime < comp.NextPopup)
                continue;

            comp.NextPopup = _timing.CurTime + comp.PopupInterval;

            if (comp.Popup is not null)
            {
                _popup.PopupEntity(Loc.GetString(comp.Popup.Value), uid, uid);
            }
        }
    }
}
