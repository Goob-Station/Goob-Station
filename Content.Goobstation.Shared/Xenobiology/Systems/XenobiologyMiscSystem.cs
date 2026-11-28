using Content.Goobstation.Common.Magic;
using Content.Goobstation.Shared.Xenobiology.Events;
using Content.Shared.Actions;
using Content.Shared.Body.Components;
using Content.Shared.Body.Systems;
using Content.Shared.Gibbing.Events;
using Content.Shared.Magic;
using Content.Shared.Mind;
using Content.Shared.Popups;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;

namespace Content.Goobstation.Shared.Xenobiology.Systems;

/// <summary>
/// Store something random about xenobiology here..
/// </summary>
public sealed partial class XenobiologyMiscSystem : EntitySystem
{
    [Dependency] private SharedMindSystem _mind = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private SharedBodySystem _body = default!;
    [Dependency] private INetManager _net = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private SharedMagicSystem _magic = default!;

    [SubscribeLocalEvent]
    private void OnMindTransference(MindTransferenceEvent ev)
    {
        if (ev.Handled)
            return;

        var user = ev.Performer;
        var target = ev.Target;

        if (!HasComp<BodyComponent>(user))
            return;

        var beforeEv = new BeforeMindSwappedEvent();
        RaiseLocalEvent(ev.Target, ref beforeEv);

        if (beforeEv.Cancelled)
        {
            _popup.PopupClient(Loc.GetString($"spell-fail-mindswap-{beforeEv.Message}"), user, user);
            Log.Debug("Triggered");
            return;
        }

        ev.Handled = true;

        if (!_mind.TryGetMind(user, out var userMindId, out var _)) // Should not be possible but it whatever
            return;

        if (_mind.TryGetMind(target, out var _, out var _)) // No transfer to mob that has mind
            return;

        _mind.TransferTo(userMindId, target);

        foreach (var component in ev.Components)
        {
            _magic.TransferComponent(component, user, target);
        }

        if (_net.IsServer)
        {
            _body.GibBody(user, gib: GibType.Drop);
            _audio.PlayEntity(ev.Sound, ev.Target, ev.Target);
            _audio.PlayEntity(ev.Sound, ev.Performer, ev.Performer);
        }
    }
}
