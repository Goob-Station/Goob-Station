using Content.Shared.Temperature;
using Robust.Shared.Audio;

namespace Content.Goobstation.Server.Heatlamp;

[RegisterComponent]
public sealed partial class HeatlampComponent : Component
{
    /// <summary>
    /// Power used when heating at the high setting.
    /// Low and medium are 33% and 66% respectively.
    /// </summary>
    [DataField]
    public float Power = 10f;

    /// <summary>
    /// How much the power used is multiplied by before being turned into heat.
    /// </summary>
    [DataField]
    public float PowerToHeatMultiplier = 1000f;

    /// <summary>
    /// Current setting of the heater. If it is off or unpowered it won't heat anything.
    /// </summary>
    [DataField]
    public EntityHeaterSetting Setting = EntityHeaterSetting.Off;

    /// <summary>
    /// Should the efficiency be lowered when contained? (E.G, in a bag)
    /// </summary>
    [DataField]
    public bool LowerEfficiencyWhenContained;

    /// <summary>
    /// What amount is the efficiency multiplied by when contained.
    /// </summary>
    [DataField]
    public float ContainerMultiplier = 0.3f;

    [ViewVariables(VVAccess.ReadWrite)]
    public float CurrentPowerDraw;

    /// <summary>
    /// An optional sound that plays when the setting is changed.
    /// </summary>
    [DataField]
    public SoundSpecifier SettingSound = new SoundPathSpecifier("/Audio/Machines/button.ogg");

    /// <summary>
    /// The entity to be heated.
    /// </summary>
    [ViewVariables]
    public EntityUid? User;

}
