using Content.Shared.Dataset;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._MACRO.StrangeMoods;

/// <summary>
///     Contains information on how an entity handles its moods,
///     and all possible moods that this entity can generate.
/// </summary>
[Virtual, DataDefinition]
[Serializable, NetSerializable]
public partial class StrangeMoodDefinition
{
    /// <summary>
    /// The prototype this definition was created from.
    /// </summary>
    [DataField]
    public ProtoId<StrangeMoodDefinitionPrototype>? ProtoId;

    /// <summary>
    /// The shared mood prototype that entities will look to follow.
    /// If null, the entity will not follow any shared moods.
    /// </summary>
    [DataField("shared")]
    public ProtoId<SharedMoodPrototype>? SharedMoodPrototype;

    /// <summary>
    /// The dataset prototypes that moods will be pulled from, as well as the amount of moods to be given.
    /// </summary>
    [DataField]
    public Dictionary<ProtoId<DatasetPrototype>, int> Datasets = [];

    /// <summary>
    /// The non-shared moods that are active.
    /// </summary>
    [DataField]
    public List<StrangeMood> Moods = [];

    /// <summary>
    /// Notification message shown in chat when the entity's moods change.
    /// </summary>
    [DataField("alertMessage")]
    public string MoodsChangedMessage = "strange-moods-update-notify";

    /// <summary>
    /// Notification sound played when the entity's moods change.
    /// </summary>
    [DataField("alertSound")]
    public SoundSpecifier? MoodsChangedSound = new SoundPathSpecifier("/Audio/_MACRO/StrangeMoods/moods_changed.ogg");

    /// <summary>
    /// The color of the mood change chat notification.
    /// </summary>
    [DataField("alertColor")]
    public Color MoodsChangedColor = Color.Orange;

    /// <summary>
    /// The action used to view moods.
    /// </summary>
    [DataField("viewAction")]
    public EntProtoId ActionViewMoods = "ActionViewMoods";
}

[Prototype]
public sealed partial class StrangeMoodDefinitionPrototype : StrangeMoodDefinition, IPrototype
{
    /// <inheritdoc/>
    [IdDataField]
    public string ID
    {
        get => ProtoId ?? "";
        set => ProtoId = value;
    }

    /// <summary>
    /// The name of the mood definition, used for admin tooling.
    /// </summary>
    [DataField(required: true)]
    public string Name = default!;

    /// <summary>
    /// The components that should be added to entities with these strange moods.
    /// ONLY USE COMPONENTS THAT ARE RELEVANT FOR INTERACTING WITH MOODS (I.E. THAVEN MOODS COMPONENT).
    /// </summary>
    [DataField]
    public ComponentRegistry? Components;
}