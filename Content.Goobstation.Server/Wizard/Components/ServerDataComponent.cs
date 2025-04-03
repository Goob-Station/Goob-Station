using System.Numerics;

namespace Content.Goobstation.Server.Wizard.Components;

/// <summary>
/// This component is needed for accessing scale from server side. Required for HulkSystem
/// </summary>
[RegisterComponent]
public sealed partial class ScaleDataComponent : Component
{
    [DataField]
    public Vector2 Scale = Vector2.One;
}
