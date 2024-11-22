using Content.Shared.Damage;
using Content.Shared.FixedPoint;
using Content.Shared.Whitelist;
using Robust.Shared.Audio;

namespace Content.Server.Whetstone;

[RegisterComponent]
public sealed partial class WhetstoneComponent : Component
{
    [DataField]
    public int Uses = 1;

    [DataField]
    public DamageSpecifier DamageIncrease = new()
    {
        DamageDict = new Dictionary<string, FixedPoint2>
        {
            ["Slash"] = 4
        }
    };

    [DataField]
    public float MaximumIncrease = 25;

    [DataField]
    public EntityWhitelist Whitelist = new();

    [DataField]
    public EntityWhitelist Blacklist = new();

    [DataField]
    public SoundSpecifier SharpenAudio = new SoundPathSpecifier("/Audio/SimpleStation14/Items/Handling/sword_sheath.ogg");
}
