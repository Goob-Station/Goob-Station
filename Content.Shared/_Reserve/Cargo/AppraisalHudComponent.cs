namespace Content.Server.Cargo.Components;

/// <summary>
/// Entities with this component have verb to get price of other entities
/// </summary>
[RegisterComponent]
public sealed partial class AppraisalHudComponent : Component
{
    public bool IsActive = false;
}
