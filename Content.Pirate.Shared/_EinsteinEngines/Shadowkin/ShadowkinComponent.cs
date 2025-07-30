using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Content.Shared.Alert;

namespace Content.Shared.Shadowkin;

[RegisterComponent, NetworkedComponent]
public sealed partial class ShadowkinComponent : Component
{
    /// <summary>
    ///     Apply the SleepManaRegenMultiplier on SleepComponent if true.
    /// </summary>
    [DataField]
    public bool SleepManaRegen = true;

    /// <summary>
    ///     What do edit the ManaRegenMultiplier when on Sleep.
    /// </summary>
    [DataField]
    public float SleepManaRegenMultiplier = 4;

    /// <summary>
    ///     Set the Black-Eye Color.
    /// </summary>
    [DataField]
    public Color BlackEyeColor = Color.Black;

    public Color OldEyeColor = Color.LimeGreen;

    [DataField]
    public EntityUid? ShadowkinSleepAction;
}
