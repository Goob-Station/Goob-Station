using Content.Goobstation.Shared.Slasher.Systems;
using Content.Server.Radio.EntitySystems;
using Content.Shared.Radio;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.Slasher.Systems;

/// <summary>
/// Server side of SharedIdolSlasherCharmSystem.
/// </summary>
public sealed class IdolSlasherCharmSystem : SharedIdolSlasherCharmSystem
{
    private static readonly ProtoId<RadioChannelPrototype> CheerChannel = "Common";

    [Dependency] private readonly RadioSystem _radio = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;

    protected override void SendRadioCheer(EntityUid source, string message)
    {
        _radio.SendRadioMessage(source, message, _proto.Index(CheerChannel), source);
    }
}
