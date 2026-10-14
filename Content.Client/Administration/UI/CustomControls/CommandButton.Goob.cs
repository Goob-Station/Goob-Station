using Robust.Shared.Timing;

namespace Content.Client.Administration.UI.CustomControls;

public partial class CommandButton
{
    private IGameTiming _timing = default!;

    public bool NeedsConfirm { get; set; } = false;
    public TimeSpan ConfirmTimeout = TimeSpan.FromSeconds(3);
    public TimeSpan ReadDelay = TimeSpan.FromSeconds(0.7);
    private TimeSpan? _readTime;
    private TimeSpan? _confirmTimeoutTime;
    private bool _isAwaitingConfirm = false;
    private string? _beforeConfirmText;

    protected override void FrameUpdate(FrameEventArgs args)
    {
        base.FrameUpdate(args);

        _timing ??= IoCManager.Resolve<IGameTiming>();

        Disabled = _isAwaitingConfirm && _timing.CurTime - _readTime < TimeSpan.Zero;

        if (_isAwaitingConfirm && _timing.CurTime - _confirmTimeoutTime >= TimeSpan.Zero)
            SetAwaitingConfirm(false);
    }

    private void SetAwaitingConfirm(bool confirming)
    {
        _isAwaitingConfirm = confirming;

        if (_isAwaitingConfirm)
        {
            _beforeConfirmText ??= Text; // Prevent overwriting it with the confirm dialogue
            _readTime = _timing.CurTime + ReadDelay;
            _confirmTimeoutTime = _timing.CurTime + ConfirmTimeout;

            ModulateSelfOverride = Color.Red;
            Text = Loc.GetString("administration-ui-command-button-confirm");
        }
        else
        {
            ModulateSelfOverride = null;
            Text = _beforeConfirmText ?? Text;

            _beforeConfirmText = null;
            _readTime = null;
            _confirmTimeoutTime = null;
        }
    }

    private bool GoobConfirmPressCheck()
    {
        if (!NeedsConfirm)
            return true;

        SetAwaitingConfirm(!_isAwaitingConfirm);
        return !_isAwaitingConfirm;
    }
}
