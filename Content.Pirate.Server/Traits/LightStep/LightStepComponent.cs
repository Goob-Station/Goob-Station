using Robust.Shared.Audio;

namespace Content.Pirate.Server.Traits.LightStep;

/// <summary>
/// Marker component for entities that walk silently.
/// </summary>
[RegisterComponent]
public sealed partial class LightStepComponent : Component
{
    [DataField] public float Volume = 0.1f;
    [ViewVariables(VVAccess.ReadWrite)]
    public SoundCollectionSpecifier CurrentStepCollection = new("FootstepFloor");
}
