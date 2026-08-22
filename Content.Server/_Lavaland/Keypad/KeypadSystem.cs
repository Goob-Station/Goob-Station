using Content.Shared._Lavaland.Keypad;
using Content.Server.DeviceLinking.Systems;
using Content.Shared.DeviceLinking;
using Robust.Server.GameObjects;
using Robust.Shared.Audio.Systems;

namespace Content.Server._Lavaland.Keypad;

public sealed class KeypadSystem : EntitySystem
{
    [Dependency] private readonly UserInterfaceSystem _ui = default!;
    [Dependency] private readonly DeviceLinkSystem _deviceLink = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<KeypadComponent, BoundUIOpenedEvent>(OnUiOpened);
        SubscribeLocalEvent<KeypadComponent, KeypadDigitMessage>(OnDigit);
        SubscribeLocalEvent<KeypadComponent, KeypadClearMessage>(OnClear);
        SubscribeLocalEvent<KeypadComponent, KeypadEnterMessage>(OnEnter);
    }

    // Set the keypad
    public void SetCode(EntityUid uid, string newCode, KeypadComponent? comp = null)
    {
        if (!Resolve(uid, ref comp))
            return;

        comp.Code = newCode;
        comp.MaxLength = Math.Max(comp.MaxLength, newCode.Length);
        comp.Entered = string.Empty;
        UpdateUi(uid, comp);
    }

    private void OnUiOpened(EntityUid uid, KeypadComponent comp, BoundUIOpenedEvent args)
    {
        // make sure the numbers match
        UpdateUi(uid, comp);
    }

    private void OnDigit(EntityUid uid, KeypadComponent comp, KeypadDigitMessage msg)
    {
        if (comp.Entered.Length >= comp.MaxLength)
            return;

        _audio.PlayPvs(comp.KeypadPressSound, uid);
        comp.Entered += msg.Digit.ToString();
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
        var correct = comp.Entered == comp.Code;

        if (correct)
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

        if (!_ui.HasUi(uid, KeypadUiKey.Key))
            return;

        _ui.SetUiState(uid, KeypadUiKey.Key, new KeypadUiState
        {
            EnteredLength = 0,
            MaxLength = comp.MaxLength,
            SuccessFlash = correct
        });
    }

    private void SendPulse(EntityUid uid, string port)
    {
        if (!HasComp<DeviceLinkSourceComponent>(uid))
        {
            Log.Warning($"Keypad {ToPrettyString(uid)} is missing DeviceLinkSourceComponent — cannot send pulse on port '{port}'.");
            return;
        }

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
            MaxLength = comp.MaxLength,
            SuccessFlash = false
        });
    }
}
