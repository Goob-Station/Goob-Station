using Robust.Shared.Timing;

namespace Content.Client.Administration.UI.CustomControls;

public partial class CommandButton
{
    private IGameTiming _timing = default!;

    public bool NeedsConfirm { get; set; } = false;
    public TimeSpan ConfirmTimeout = TimeSpan.FromSeconds(1);
    private TimeSpan? _confirmTimeoutTime;
    private bool _isAwaitingConfirm = false;
    private string? _beforeConfirmText;

    protected override void FrameUpdate(FrameEventArgs args)
    {
        base.FrameUpdate(args);

        if (_timing is null)
            _timing = IoCManager.Resolve<IGameTiming>();

        if (_isAwaitingConfirm && _timing.CurTime - _confirmTimeoutTime >= TimeSpan.Zero)
            SetAwaitingConfirm(false);
    }

    private void SetAwaitingConfirm(bool confirming)
    {
        if (confirming)
        {
            _beforeConfirmText ??= Text; // Prevent overwriting it with the confirm dialogue
            _isAwaitingConfirm = true;
            _confirmTimeoutTime = _timing.CurTime + ConfirmTimeout;

            ModulateSelfOverride = Color.Red;
            Text = Loc.GetString("administration-ui-command-button-confirm");
        }
        else
        {
            _isAwaitingConfirm = false;
            _beforeConfirmText = null;

            ModulateSelfOverride = null;
            Text = _beforeConfirmText;
        }
    }

    private bool GoobConfirmPressCheck()
    {
        if (!NeedsConfirm)
            return true;

        if (!_isAwaitingConfirm)
        {
            SetAwaitingConfirm(true);

            return false;
        }

        SetAwaitingConfirm(false);

        return true;
    }
}
