namespace Content.Goobstation.Common.Silicons.Components;

/// <summary>
/// Used for law uploading console, when inserted it will update laws randomly,
/// then after some time when this set of laws wasn't changed it gives some research points to an RnD server.
/// </summary>
[RegisterComponent]
public sealed partial class ExperimentalLawProviderComponent : Component
{
    [DataField]
    public string RandomLawsets = "IonStormLawsets";

    [DataField]
    public float RewardTime = 120.0f;

    [DataField]
    public int RewardPoints = 5000;
}
