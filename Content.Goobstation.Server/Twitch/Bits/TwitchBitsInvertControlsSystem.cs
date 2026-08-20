using System.Linq;
using Content.Goobstation.Common.CCVar;
using Content.Goobstation.Shared.MisandryBox;
using Content.Shared.Movement.Components;
using Robust.Shared.Configuration;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.Twitch.Bits;

public sealed class TwitchBitsInvertControlsSystem : EntitySystem, ITwitchBitsAction
{
    private static readonly TimeSpan Duration = TimeSpan.FromSeconds(60);

    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly TwitchBitsSystem _twitchBits = default!;

    private readonly Dictionary<EntityUid, TimeSpan> _active = new();

    public string Id => "invert-controls";
    public string DisplayName => "Invert Controls";
    public string DisplayDescription => "Reverse the streamer's movement controls for 60 seconds.";
    public CVarDef<string> Sku => GoobCVars.TwitchBitsInvertControlsSku;

    public override void Initialize()
    {
        base.Initialize();
        _twitchBits.RegisterAction(this);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        foreach (var (target, expiresAt) in _active.ToArray())
        {
            if (Exists(target) && _timing.RealTime < expiresAt)
                continue;

            if (Exists(target) && HasComp<InputSwapComponent>(target))
                RemComp<InputSwapComponent>(target);

            _active.Remove(target);
        }
    }

    public TwitchBitsActionValidity IsCurrentlyValid(EntityUid target, TwitchBitsActionContext context)
    {
        if (!HasComp<MovementSpeedModifierComponent>(target))
            return TwitchBitsActionValidity.Invalid("The streamer's current character cannot have its controls inverted.");

        if (HasComp<InputSwapComponent>(target) && !_active.ContainsKey(target))
            return TwitchBitsActionValidity.Invalid("The streamer's controls are already inverted by another effect.");

        return TwitchBitsActionValidity.Valid;
    }

    public bool Execute(EntityUid target, TwitchBitsActionContext context)
    {
        if (!HasComp<InputSwapComponent>(target))
            AddComp<InputSwapComponent>(target);

        var start = _active.GetValueOrDefault(target) > _timing.RealTime
            ? _active[target]
            : _timing.RealTime;
        _active[target] = start + Duration;
        return true;
    }
}
