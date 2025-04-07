using Content.Server.Body.Components;
using Content.Server.Body.Systems;
using Content.Server.Chat.Systems;
using Content.Shared.DoAfter;
using Content.Shared.Power.EntitySystems;
using Content.Shared._Shitmed.Autodoc.Components;
using Content.Shared._Shitmed.Autodoc.Systems;

namespace Content.Server._Shitmed.Autodoc.Systems;

public sealed class AutodocSystem : SharedAutodocSystem
{
    [Dependency] private readonly InternalsSystem _internals = default!;
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly SharedPowerReceiverSystem _power = default!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<ActiveAutodocComponent, AutodocComponent>();
        var now = Timing.CurTime;
        while (query.MoveNext(out var uid, out var active, out var comp))
        {
            if (now < active.NextUpdate)
                continue;

            active.NextUpdate = now + comp.UpdateDelay;
            if (HasComp<ActiveDoAfterComponent>(uid) || !_power.IsPowered(uid))
                continue;

            if (Proceed((uid, comp, active)))
                RemCompDeferred<ActiveAutodocComponent>(uid);
        }
    }

    protected override void WakePatient(EntityUid patient)
    {
        // incase they are using nitrous, disconnect it so they can get woken up later on
        if (TryComp<InternalsComponent>(patient, out var internals) && _internals.AreInternalsWorking(patient, internals))
            _internals.DisconnectTank((patient, internals));

        base.WakePatient(patient);
    }

    public override void Say(EntityUid uid, string msg)
    {
        _chat.TrySendInGameICMessage(uid, msg, InGameICChatType.Speak, hideChat: false, hideLog: true, checkRadioPrefix: false);
    }
}
