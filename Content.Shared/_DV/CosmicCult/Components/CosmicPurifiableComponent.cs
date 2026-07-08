using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.CosmicCult.Components;

[RegisterComponent]
[AutoGenerateComponentPause]
public sealed partial class CosmicPurifiableComponent : Component
{
    [DataField]
    public EntProtoId PurgeVFX = "CleanseEffectVFX";

    [DataField]
    public SoundSpecifier PurgeSound = new SoundPathSpecifier("/Audio/_DV/CosmicCult/cleanse_deconversion.ogg");

    [DataField]
    public TimeSpan CleanseTime = TimeSpan.FromSeconds(35);

    [DataField]
    public TimeSpan CleanseTimeChaplain = TimeSpan.FromSeconds(20);

    ///<summary>
    /// If true, only an entity with a BibleUserComponent can purge this entity
    ///</summary>
    [DataField]
    public bool BibleUserRequired;
}
