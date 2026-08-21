using Content.Shared.Slippery;

namespace Content.Goobstation.Server.Twitch.Bits;

public sealed class TwitchBitsSlipSystem : EntitySystem, ITwitchBitsAction
{
    [Dependency] private readonly SlipperySystem _slippery = default!;
    [Dependency] private readonly TwitchBitsSystem _twitchBits = default!;

    public string Id => "slip";
    public string DisplayName => "Slip the Streamer";
    public string DisplayDescription => "Make the streamer's character slip where they stand.";
    public string Category => "Character";
    public string Sku => "ss14-slip";

    public override void Initialize()
    {
        base.Initialize();
        _twitchBits.RegisterAction(this);
    }

    public TwitchBitsActionValidity IsCurrentlyValid(EntityUid target, TwitchBitsActionContext context)
    {
        return _slippery.CanSlip(target, target)
            ? TwitchBitsActionValidity.Valid
            : TwitchBitsActionValidity.Invalid("The streamer's character cannot currently be slipped.");
    }

    public bool Execute(EntityUid target, TwitchBitsActionContext context)
    {
        if (!_slippery.CanSlip(target, target))
            return false;

        var existed = HasComp<SlipperyComponent>(target);
        var slippery = EnsureComp<SlipperyComponent>(target);
        _slippery.TrySlip(target, slippery, target, requiresContact: false, predicted: false);
        if (!existed)
            RemComp<SlipperyComponent>(target);
        return true;
    }
}
