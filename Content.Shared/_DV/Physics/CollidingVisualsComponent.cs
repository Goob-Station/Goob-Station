using Content.Shared.Whitelist;
using Robust.Shared.Serialization;

namespace Content.Shared._DV.Physics;

/// <summary>
/// Changes an appearance data string depending on active collisions with fixtures.
/// </summary>
[RegisterComponent, Access(typeof(CollidingVisualsSystem))]
public sealed partial class CollidingVisualsComponent : Component
{
    /// <summary>
    /// A whitelist entities must match to be counted for collisions.
    /// </summary>
    [DataField]
    public EntityWhitelist? Whitelist;

    /// <summary>
    /// The string to use for appearance data when no fixtures are being collided with.
    /// </summary>
    [DataField]
    public string Default = "none";

    /// <summary>
    /// The list of fixtures to check for collisions, first one colliding is used so is most important.
    /// </summary>
    [DataField(required: true)]
    public List<string> Fixtures = new();

    /// <summary>
    /// Actively colliding fixtures.
    /// </summary>
    [DataField]
    public HashSet<string> Active = new();
}

[Serializable, NetSerializable]
public enum CollidingVisuals : byte
{
    Layer,
    Fixture
}
