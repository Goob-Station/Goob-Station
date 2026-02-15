namespace Content.Goobstation.Server.ContributorName;

/// <summary>
/// Gives an entity the name of a random GitHub contributor from `/Credits/GitHub.txt`.
/// </summary>
/// <remarks>
/// There is no possible way that this could backfire.
/// </remarks>
[RegisterComponent]
public sealed partial class ContributorNameComponent : Component
{
}
