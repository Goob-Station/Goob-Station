using Content.Shared.Whitelist;

namespace Content.Goobstation.Shared.Xenobiology;

[RegisterComponent]
public sealed partial class CorpseEaterComponent : Component
{
    /// <summary>
    /// Whitelist for organs that can be eaten by ability.
    /// </summary>
    [DataField]
    public EntityWhitelist? OrganWhitelist;

    /// <summary>
    /// Whitelist for body parts that can be eaten by ability.
    /// </summary>
    [DataField]
    public EntityWhitelist? BodyPartWhitelist;

    /// <summary>
    /// Blacklist for organs that can't be eaten by ability.
    /// </summary>
    [DataField]
    public EntityWhitelist? OrganBlacklist;

    /// <summary>
    /// Blacklist for body parts that can't be eaten by ability.
    /// </summary>
    [DataField]
    public EntityWhitelist? BodyPartBlacklist;
}
