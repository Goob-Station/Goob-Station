using Content.Shared._Lavaland.Megafauna.Mercury.Components;
using Content.Shared.Examine;
using Content.Shared.Interaction.Events;
using Content.Shared.Popups;
using Content.Shared.Timing;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using System.Numerics;

namespace Content.Shared._Lavaland.Megafauna.Mercury.Systems;

public sealed class EtherDrinkerSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly UseDelaySystem _useDelay = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<EtherDrinkerComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<EtherDrinkerComponent, UseInHandEvent>(OnUseInHand);
        SubscribeLocalEvent<EtherDrinkerComponent, ExaminedEvent>(OnExamined);
    }

    private void OnExamined(EntityUid uid, EtherDrinkerComponent comp, ExaminedEvent args)
    {
        if (!TryComp<UseDelayComponent>(uid, out var delay) || !_useDelay.TryGetDelayInfo((uid, delay), out var info))
            return;

        var remaining = info.EndTime - _timing.CurTime;
        var chargePercent = MathHelper.Clamp01(1f - (float) (remaining / info.Length));

        args.PushText(Loc.GetString("ether-drinker-examine-charge", ("charge", (int) (chargePercent * 100f))));
    }

    private void OnMapInit(EntityUid uid, EtherDrinkerComponent comp, MapInitEvent args)
    {
        _useDelay.SetLength((uid, null), comp.BaseRechargeTime);
    }

    private void OnUseInHand(EntityUid uid, EtherDrinkerComponent comp, UseInHandEvent args)
    {
        if (args.Handled)
            return;

        if (!TryComp<UseDelayComponent>(uid, out var delay) || !_useDelay.TryGetDelayInfo((uid, delay), out var info))
            return;

        var remaining = info.EndTime - _timing.CurTime;
        var chargePercent = MathHelper.Clamp01(1f - (float) (remaining / info.Length));

        var strikes = (int) (chargePercent * 100f / comp.ChargePerStrike);
        if (strikes <= 0)
        {
            _popup.PopupPredicted(Loc.GetString("ether-drinker-no-charge"), args.User, args.User, PopupType.Small);
            return;
        }

        strikes = Math.Min(strikes, comp.MaxStrikes);
        var totalStrikes = chargePercent >= 1f ? strikes * 2 : strikes;

        var coords = Transform(uid).Coordinates;
        for (var i = 0; i < totalStrikes; i++)
        {
            var offset = new Vector2(_random.NextFloat(-comp.StrikeOffset, comp.StrikeOffset), _random.NextFloat(-comp.StrikeOffset, comp.StrikeOffset));
            PredictedSpawnAtPosition(comp.LightningPrototype, coords.Offset(offset));
        }

        _audio.PlayPredicted(comp.DischargeSound, uid, args.User);
        _popup.PopupPredicted(Loc.GetString("ether-drinker-discharge"), args.User, args.User, PopupType.Medium);

        _useDelay.ResetAllDelays((uid, delay));

        args.Handled = true;
    }
}
