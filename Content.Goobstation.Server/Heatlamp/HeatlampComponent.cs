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
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public float Power = 200f;

    /// <summary>
    /// Current setting of the heater. If it is off or unpowered it won't heat anything.
    /// </summary>
    [DataField]
    public EntityHeaterSetting Setting = EntityHeaterSetting.Off;

    [ViewVariables(VVAccess.ReadWrite)]
    public float CurrentPowerDraw;

    /// <summary>
    /// An optional sound that plays when the setting is changed.
    /// </summary>
    [DataField]
    public SoundPathSpecifier? SettingSound;

    /// <summary>
    /// The entity to be heated.
    /// </summary>
    [ViewVariables]
    public EntityUid? User;

    /// <summary>
    /// When the next heating tick will occur.
    /// </summary>
    [ViewVariables]
    public TimeSpan NextTick;

    /// <summary>
    /// How long in between every heating tick.
    /// </summary>
    [DataField]
    public TimeSpan TickDelay = TimeSpan.FromSeconds(1);
}
