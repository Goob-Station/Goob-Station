using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using System;

// in common because it can be in common
namespace Content.Goobstation.Common.ItemMiner;

[RegisterComponent]
public sealed partial class ItemMinerComponent : Component
{
    /// <summary>
    /// Time for next item to be generated at
    /// </summary>
    [DataField]
    public TimeSpan NextAt;

    /// <summary>
    /// Entity prototype to spawn
    /// </summary>
    [DataField(required: true)]
    public EntProtoId Proto;

    /// <summary>
    /// Amount of entities to spawn
    /// </summary>
    [DataField]
    public int Amount = 1;

    /// <summary>
    /// ID of item slot to add items into
    /// Spawns on floor if null
    /// Only supported for stackable entities
    /// </summary>
    [DataField]
    public string? ItemSlotId = "miner_slot";

    /// <summary>
    /// Miner working SFX
    /// </summary>
    [DataField]
    public SoundSpecifier MiningSound = new SoundPathSpecifier("/Audio/Ambience/Objects/server_fans.ogg");

    /// <summary>
    /// How often to produce the item
    /// </summary>
    [DataField]
    public TimeSpan Interval = TimeSpan.FromSeconds(10.0f);

    /// <summary>
    /// Whether to need to be anchored to run
    /// </summary>
    [DataField]
    public bool NeedsAnchored = true;

    /// <summary>
    /// Whether to need power to run
    /// </summary>
    [DataField]
    public bool NeedsPower = true;

    // if you want to add a planetary miner or other varieties of miner, don't add more stuff to this, make a new comp and use events
}
