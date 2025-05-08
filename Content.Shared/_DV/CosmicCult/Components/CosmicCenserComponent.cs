using Content.Shared.Damage;
using Content.Shared.Tools;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.CosmicCult.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class CosmicCenserComponent : Component
{
    /// <summary>
    /// The tool required to deconvert someone
    /// </summary>
    [DataField]
    public ProtoId<ToolQualityPrototype> ToolRequired = "Censer";

    /// <summary>
    /// The length of time it takes to deconvert someone.
    /// </summary>
    [DataField]
    public TimeSpan DeconversionTime = TimeSpan.FromSeconds(15);

    /// <summary>
    /// The damage to deal on a failed deconversion
    /// </summary>
    [DataField]
    public DamageSpecifier FailedDeconversionDamage = new() {
        DamageDict = new() {
            { "Asphyxiation", 65 },
            { "Caustic", 15 }
        }
    };

    [DataField] public SoundSpecifier SizzleSound = new SoundPathSpecifier("/Audio/Effects/lightburn.ogg");

    [DataField] public SoundSpecifier CleanseSound = new SoundPathSpecifier("/Audio/_DV/CosmicCult/cleanse_deconversion.ogg");

    [DataField] public SoundSpecifier MalignSound = new SoundPathSpecifier("/Audio/_DV/CosmicCult/glyph_trigger.ogg");

    [DataField] public EntProtoId CleanseVFX = "NoosphericVFX2";

    [DataField] public EntProtoId ReboundVFX = "NoosphericVFX1";

    [DataField] public EntProtoId MalignVFX = "CosmicGenericVFX";
}

[RegisterComponent, NetworkedComponent]
public sealed partial class CosmicCenserTargetComponent : Component;
