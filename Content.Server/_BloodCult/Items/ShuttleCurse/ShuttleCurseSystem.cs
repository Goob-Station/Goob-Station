using Content.Server.Chat.Systems;
using Content.Server.Popups;
using Content.Server.RoundEnd;
using Content.Server.Shuttles.Systems;
using Content.Shared.Interaction;
using Robust.Server.Audio;
using Robust.Shared.Player;
using Robust.Shared.Random;

namespace Content.Server.WhiteDream.BloodCult.Items.ShuttleCurse;

public sealed class ShuttleCurseSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;

    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly EmergencyShuttleSystem _emergencyShuttle = default!;
    [Dependency] private readonly RoundEndSystem _roundEnd = default!;
    [Dependency] private readonly PopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ShuttleCurseComponent, ActivateInWorldEvent>(OnActivate);
    }

    private void OnActivate(Entity<ShuttleCurseComponent> orb, ref ActivateInWorldEvent args)
    {
        if (args.Handled)
            return;

        var curseProvider = EnsureShuttleCurseProvider(orb);
        if (curseProvider is null)
        {
            _popup.PopupEntity(Loc.GetString("shuttle-curse-cant-activate"), orb, args.User);
            return;
        }

        if (curseProvider.CurrentUses >= curseProvider.MaxUses)
        {
            _popup.PopupEntity(Loc.GetString("shuttle-curse-max-charges"), orb, args.User);
            return;
        }

        if (_emergencyShuttle.EmergencyShuttleArrived)
        {
            _popup.PopupEntity(Loc.GetString("shuttle-curse-shuttle-arrived"), orb, args.User);
            return;
        }

        if (_roundEnd.ExpectedCountdownEnd is null)
        {
            _popup.PopupEntity(Loc.GetString("shuttle-curse-shuttle-not-called"), orb, args.User);
            return;
        }

        _roundEnd.DelayShuttle(orb.Comp.DelayTime);

        var cursedMessage = string.Concat(Loc.GetString(_random.Pick(orb.Comp.CurseMessages)),
            " ",
            Loc.GetString("shuttle-curse-success-global", ("time", orb.Comp.DelayTime.TotalMinutes)));

        _chat.DispatchGlobalAnnouncement(cursedMessage,
            Loc.GetString("shuttle-curse-system-failure"),
            colorOverride: Color.Gold);

        _popup.PopupEntity(Loc.GetString("shuttle-curse-success"), args.User, args.User);
        curseProvider.CurrentUses++;

        _audio.PlayEntity(orb.Comp.ScatterSound, Filter.Pvs(orb), orb, true);
        Del(orb);
    }

    private ShuttleCurseProviderComponent? EnsureShuttleCurseProvider(EntityUid orb)
    {
        var mapUid = Transform(orb).MapUid;
        return !mapUid.HasValue ? null : EnsureComp<ShuttleCurseProviderComponent>(mapUid.Value);
    }
}
