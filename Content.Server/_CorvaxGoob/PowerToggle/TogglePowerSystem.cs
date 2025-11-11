using Content.Server.Power.Components;
using Content.Shared._CorvaxGoob.PowerToggle;
using Content.Shared.ActionBlocker;
using Content.Shared.Examine;
using Content.Shared.Interaction;
using Content.Shared.Sound;
using Content.Shared.Sound.Components;
using Robust.Server.Audio;
using Robust.Shared.Timing;

namespace Content.Server._CorvaxGoob.PowerToggle;

public sealed partial class TogglePowerSystem : SharedTogglePowerSystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedEmitSoundSystem _emitSoundSystem = default!;
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly SharedInteractionSystem _interaction = default!;
    [Dependency] private readonly ActionBlockerSystem _actionBlocker = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeNetworkEvent<TogglePowerMessage>(OnTogglePowerMessage);
        SubscribeLocalEvent<TogglePowerComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<TogglePowerComponent, ExaminedEvent>(OnExamined);
    }

    private void OnExamined(Entity<TogglePowerComponent> entity, ref ExaminedEvent args)
    {
        if (!args.IsInDetailsRange)
            return;

        if (entity.Comp.IsTurnedOn)
            args.PushMarkup(Loc.GetString("power-toggle-status-on"));
        else
            args.PushMarkup(Loc.GetString("power-toggle-status-off"));
    }

    private void OnMapInit(Entity<TogglePowerComponent> entity, ref MapInitEvent args)
    {
        if (!TryComp<ApcPowerReceiverComponent>(entity, out var apcPowerReceiver))
        {
            RemComp<TogglePowerComponent>(entity);
            return;
        }

        if (entity.Comp.ApplyPowerOnSpawn)
            SetPower(entity, entity.Comp.IsTurnedOn, false);
    }

    private void OnTogglePowerMessage(TogglePowerMessage args)
    {
        var entity = GetEntity(args.Entity);
        var user = GetEntity(args.User);

        if (!_interaction.InRangeUnobstructed(user, entity) || !_actionBlocker.CanInteract(user, entity))
            return;

        if (!TryComp<TogglePowerComponent>(entity, out var togglePower))
            return;

        if (_timing.CurTime < togglePower.NextToggle)
            return;

        togglePower.NextToggle = _timing.CurTime + togglePower.ToggleInterval;

        DoTogglePower((entity, togglePower));
    }

    public void DoTogglePower(Entity<TogglePowerComponent> entity) => SetPower(entity, !entity.Comp.IsTurnedOn);

    public void SetPower(Entity<TogglePowerComponent> entity, bool powered, bool playSound = true)
    {
        if (!TryComp<ApcPowerReceiverComponent>(entity, out var apcPowerReceiver))
            return;

        entity.Comp.IsTurnedOn = powered;

        if (playSound)
        {
            if (entity.Comp.IsTurnedOn && entity.Comp.TurnOnSound is not null)
                _audio.PlayPvs(entity.Comp.TurnOnSound, entity);

            if (!entity.Comp.IsTurnedOn && entity.Comp.TurnOffSound is not null)
                _audio.PlayPvs(entity.Comp.TurnOffSound, entity);
        }

        apcPowerReceiver.PowerDisabled = !powered;

        if (TryComp<SpamEmitSoundComponent>(entity, out var spamEmitSound))
            _emitSoundSystem.SetEnabled((entity.Owner, spamEmitSound), powered);

        Dirty(entity);
    }
}
