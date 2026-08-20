using Content.Goobstation.Common.CCVar;
using Content.Server.Nuke;
using Content.Shared.Nuke;
using Robust.Shared.Configuration;

namespace Content.Goobstation.Server.Twitch.Bits;

public sealed class TwitchBitsArmNukeSystem : EntitySystem, ITwitchBitsAction
{
    [Dependency] private readonly NukeSystem _nuke = default!;
    [Dependency] private readonly TwitchBitsSystem _twitchBits = default!;

    public string Id => "arm-nuke";
    public string DisplayName => "Arm Nuke";
    public string DisplayDescription => "Arm an available nuclear device using its current countdown.";
    public CVarDef<string> Sku => GoobCVars.TwitchBitsArmNukeSku;

    public override void Initialize()
    {
        base.Initialize();
        _twitchBits.RegisterAction(this);
    }

    public TwitchBitsActionValidity IsCurrentlyValid(EntityUid target, TwitchBitsActionContext context)
    {
        return TryFindNuke(out _, out _)
            ? TwitchBitsActionValidity.Valid
            : TwitchBitsActionValidity.Invalid("There is no unarmed nuclear device available.");
    }

    public bool Execute(EntityUid target, TwitchBitsActionContext context)
    {
        if (!TryFindNuke(out var uid, out var component))
            return false;

        _nuke.ArmBomb(uid, component);
        return component.Status == NukeStatus.ARMED;
    }

    private bool TryFindNuke(out EntityUid uid, out NukeComponent component)
    {
        var query = EntityQueryEnumerator<NukeComponent>();
        while (query.MoveNext(out uid, out var found))
        {
            if (found != null && !found.Exploded && found.Status != NukeStatus.ARMED)
            {
                component = found;
                return true;
            }
        }

        uid = EntityUid.Invalid;
        component = null!;
        return false;
    }
}
