// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Hood.Phone;
using Content.Shared.Containers.ItemSlots;
using JetBrains.Annotations;
using Robust.Client.UserInterface;

namespace Content.Client._Hood.Phone;

[UsedImplicitly]
public sealed class PhoneBoundUserInterface(EntityUid owner, Enum uiKey) : BoundUserInterface(owner, uiKey)
{
    private PhoneWindow? _window;

    protected override void Open()
    {
        base.Open();

        _window = this.CreateWindow<PhoneWindow>();
        _window.EjectSimPressed += OnEjectSimPressed;
        _window.SmsSendPressed += OnSmsSendPressed;
        _window.DialPressed += OnDialPressed;
        _window.AcceptCallPressed += OnAcceptCallPressed;
        _window.RejectCallPressed += OnRejectCallPressed;
        _window.HangupPressed += OnHangupPressed;
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (state is PhoneBoundUserInterfaceState phoneState)
            _window?.UpdateState(phoneState);
    }

    private void OnEjectSimPressed()
    {
        SendPredictedMessage(new ItemSlotButtonPressedEvent(PhoneComponent.SimSlotId, tryInsert: false));
    }

    private void OnSmsSendPressed(uint recipientNumber, string content)
    {
        SendMessage(new PhoneSendSmsMessage(Guid.NewGuid(), recipientNumber, content));
    }

    private void OnDialPressed(uint recipientNumber)
    {
        SendMessage(new PhoneDialMessage(recipientNumber));
    }

    private void OnAcceptCallPressed()
    {
        SendMessage(new PhoneAcceptCallMessage());
    }

    private void OnRejectCallPressed()
    {
        SendMessage(new PhoneRejectCallMessage());
    }

    private void OnHangupPressed()
    {
        SendMessage(new PhoneHangupMessage());
    }
}
