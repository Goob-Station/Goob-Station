using Content.Shared.Whitelist;
using Content.Shared.DoAfter;
using Content.Shared._Shitmed.Body.Part;

namespace Content.Goobstation.Shared.Xenobiology.Components;

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

    /// <summary>
    /// What type of body part composition can be eaten, if null - any.
    /// </summary>
    [DataField]
    public BodyPartComposition? BodyPartComposition = null;

    /// <summary>
    /// How long the do-after to separate body part or organ from corpse.
    /// </summary>
    [DataField]
    public TimeSpan EatCorpseDoAfterDuration = TimeSpan.FromSeconds(20);

    /// <summary>
    /// Stores last eat corpse DoAfter id, used for HTN stuff.
    /// </summary>
    [DataField]
    public DoAfterId? LastDoAfterId = null;
}
