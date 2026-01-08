namespace Content.Goobstation.Shared.Loudspeaker.Components;

/// <summary>
///     Marks an entity that is holding equipped loudspeaker(s).
/// </summary>
[RegisterComponent]
public sealed partial class LoudspeakerHolderComponent : Component
{
    [DataField]
    public List<EntityUid> Loudspeakers = new();
}
