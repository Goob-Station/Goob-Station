using Content.Shared.Emag.Systems;

namespace Content.Goobstation.Shared.Emag.Components;

/// <summary>
/// Marker for entity that can clean <see cref="EmagType.Jestographic"/>
/// </summary>
[RegisterComponent]
public sealed partial class CleanEmagComponent : Component
{
    /// <summary>
    /// How long is the clean do after
    /// </summary>
    [DataField]
    public TimeSpan CleanDuration = TimeSpan.FromSeconds(5);
}
