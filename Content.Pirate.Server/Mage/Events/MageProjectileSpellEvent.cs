using Content.Shared.Actions;
using Content.Shared.Chat;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;
using Content.Shared.Magic;

namespace Content.Pirate.Server.Mage.Events;

public sealed partial class MageProjectileSpellEvent : WorldTargetActionEvent//, ISpeakSpell
{
    /// <summary>
    /// What entity should be spawned.
    /// </summary>
    [DataField("prototype", required: true, customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    public string Prototype = default!;

    /// <summary>
    /// Gets the targeted spawn positions; may lead to multiple entities being spawned.
    /// </summary>
    [DataField("posData")] public MagicInstantSpawnData Pos = new TargetCasterPos();

    //[DataField("speech")] public string? Speech { get; set; }

    /// <summary>
    /// How much mana should be drained.
    /// </summary>
    [DataField("manaCost")]
    public float ManaCost = 30f;

    //public InGameICChatType ChatType { get; } = InGameICChatType.Speak;
}
