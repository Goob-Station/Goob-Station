using Content.Goobstation.Shared.Keypad;
using Content.Server.DeviceLinking.Systems;
using Content.Shared.DeviceLinking;
using Robust.Server.GameObjects;
using Robust.Shared.Audio.Systems;

namespace Content.Goobstation.Server.Keypad;

public sealed class KeypadSystem : EntitySystem
{
    [Dependency] private readonly UserInterfaceSystem _ui = default!;
    [Dependency] private readonly DeviceLinkSystem _deviceLink = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<KeypadComponent, KeypadDigitMessage>(OnDigit);
        SubscribeLocalEvent<KeypadComponent, KeypadClearMessage>(OnClear);
        SubscribeLocalEvent<KeypadComponent, KeypadEnterMessage>(OnEnter);
    }

    private void OnDigit(EntityUid uid, KeypadComponent comp, KeypadDigitMessage msg)
    {
        if (comp.Entered.Length >= comp.MaxLength)
            return;

        _audio.PlayPvs(comp.KeypadPressSound, uid);

        comp.Entered += msg.Digit;
        UpdateUi(uid, comp);
    }

    private void OnClear(EntityUid uid, KeypadComponent comp, KeypadClearMessage msg)
    {
        _audio.PlayPvs(comp.ClearSound, uid);

        comp.Entered = string.Empty;
        UpdateUi(uid, comp);
    }

    private void OnEnter(EntityUid uid, KeypadComponent comp, KeypadEnterMessage msg)
    {
        if (comp.Entered == comp.Code)
        {
            _audio.PlayPvs(comp.AccessGrantedSound, uid);
            SendPulse(uid, "KeypadCorrect");
        }
        else
        {
            _audio.PlayPvs(comp.AccessDeniedSound, uid);
            SendPulse(uid, "KeypadIncorrect");
        }

        comp.Entered = string.Empty;
        UpdateUi(uid, comp);
    }

    private void SendPulse(EntityUid uid, string port)
    {
        // You literally didn't specify what the problem is so I'm assuming you're just a TryComp hater.
        if (!HasComp<DeviceLinkSourceComponent>(uid))
            throw new InvalidOperationException($"Entity {uid} missing DeviceLinkSourceComponent.");

        _deviceLink.SendSignal(uid, port, true);
        _deviceLink.SendSignal(uid, port, false);
    }

    private void UpdateUi(EntityUid uid, KeypadComponent comp)
    {
        if (!_ui.HasUi(uid, KeypadUiKey.Key))
            return;

        _ui.SetUiState(uid, KeypadUiKey.Key, new KeypadUiState
        {
            EnteredLength = comp.Entered.Length,
            MaxLength = comp.MaxLength
        });
    }
}
