using Content.Client.Message;
using Content.Goobstation.Common.RecoveryPassword;
using Content.Goobstation.Shared.RecoveryPassword;
using Robust.Shared.Network;

namespace Content.Goobstation.Client.RecoveryPassword;

public sealed class RecoveryPasswordManager : IRecoveryPasswordManager, IPostInjectInit
{
    [Dependency] private readonly INetManager _net = default!;

    private RecoveryPasswordWindow? _window;
    public bool HasPassword { get; private set; }

    public bool Enabled { get; private set; }
    public int MinLength { get; private set; } = 8;

    public event Action? Updated;

    public void ToggleWindow()
    {
        if (_window != null)
        {
            _window.Close();
            _window = null;
            return;
        }

        if (HasPassword || !Enabled)
            return;

        _window = new RecoveryPasswordWindow();
        _window.OnClose += () => _window = null;
        _window.InfoLabel.SetMarkupPermissive(Loc.GetString("recovery-password-window-info"));
        _window.SaveButton.OnPressed += _ => OnSavePressed();
        _window.OpenCentered();
    }

    private void OnSavePressed()
    {
        if (_window == null)
            return;

        var password = _window.PasswordEdit.Text;
        var confirm = _window.ConfirmEdit.Text;

        if (string.IsNullOrEmpty(password) || password.Length < MinLength)
        {
            _window.StatusLabel.Text = Loc.GetString("recovery-password-error-too-short", ("min", MinLength));
            return;
        }

        if (password != confirm)
        {
            _window.StatusLabel.Text = Loc.GetString("recovery-password-error-mismatch");
            return;
        }

        _window.SaveButton.Disabled = true;
        _window.StatusLabel.Text = Loc.GetString("recovery-password-status-saving");
        _net.ClientSendMessage(new MsgSetRecoveryPassword { Password = password });
    }

    private void OnStatus(MsgRecoveryPasswordStatus msg)
    {
        HasPassword = msg.HasPassword;
        Enabled = msg.Enabled;
        MinLength = msg.MinLength;
        Updated?.Invoke();
    }

    private void OnResult(MsgRecoveryPasswordResult msg)
    {
        if (_window == null)
            return;

        if (msg.Result == RecoveryPasswordSetResult.Success)
        {
            _window.StatusLabel.Text = Loc.GetString("recovery-password-status-success");
            _window.PasswordEdit.Editable = false;
            _window.ConfirmEdit.Editable = false;
            return;
        }

        _window.SaveButton.Disabled = false;
        var key = msg.Result switch
        {
            RecoveryPasswordSetResult.Disabled => "recovery-password-error-disabled",
            RecoveryPasswordSetResult.AlreadySet => "recovery-password-error-already-set",
            RecoveryPasswordSetResult.TooShort => "recovery-password-error-too-short",
            RecoveryPasswordSetResult.TooLong => "recovery-password-error-too-long",
            _ => "recovery-password-error-generic",
        };
        _window.StatusLabel.Text = Loc.GetString(key, ("min", MinLength));
    }

    void IPostInjectInit.PostInject()
    {
        _net.RegisterNetMessage<MsgSetRecoveryPassword>();
        _net.RegisterNetMessage<MsgRecoveryPasswordStatus>(OnStatus);
        _net.RegisterNetMessage<MsgRecoveryPasswordResult>(OnResult);
    }
}
