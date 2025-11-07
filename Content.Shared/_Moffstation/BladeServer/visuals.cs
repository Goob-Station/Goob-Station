using System.Linq;
using System.Numerics;
using Robust.Shared.Serialization;

namespace Content.Shared._Moffstation.BladeServer;

/// <summary>
/// This enum is used to identify dynamically added layers on the BladeServer sprite.
/// </summary>
[Serializable, NetSerializable]
public enum BladeServerVisuals : byte
{
    StripeLayer,
    StripeColor,
}

/// <summary>
/// This enum is used to identify dynamically added layers on the BladeServerRack sprite.
/// </summary>
[Serializable, NetSerializable]
public enum BladeServerRackVisuals : byte
{
    SlotsKey,
}

/// <summary>
/// This data class holds the visual state of BladeServers contained within a BladeServerRack, used to specify how
/// dynamically added layers on the latter represent the former. This is used as data in <see cref="AppearanceComponent"/>
/// with <see cref="BladeServerRackVisuals.SlotsKey"/>.
/// </summary>
[Serializable, NetSerializable]
public sealed class BladeServerRackSlotVisualData(Color? stripeColor, bool powered, Vector2 offset) : ICloneable
{
    /// <summary>
    /// The color of the stripe on a particular Blade Server. Corresponds to <see cref="BladeServerVisuals.StripeColor"/>.
    /// </summary>
    public readonly Color? StripeColor = stripeColor;

    /// <summary>
    /// Whether or not this BladeServer is powered.
    /// </summary>
    public readonly bool Powered = powered;

    /// <summary>
    /// The offset for this particular BladeServer's slot. This is used to visually offset each blade.
    /// </summary>
    public readonly Vector2 Offset = offset;

    public object Clone() => new BladeServerRackSlotVisualData(StripeColor, Powered, Offset);

    /// <summary>
    /// This class holds <see cref="BladeServerRackSlotVisualData"/> for ALL BladeServers in a BladeServerRack.
    /// </summary>
    /// <param name="slots"></param>
    [Serializable, NetSerializable]
    public sealed class Group(IEnumerable<BladeServerRackSlotVisualData?> slots) : ICloneable
    {
        public readonly List<BladeServerRackSlotVisualData?> Slots = slots.ToList();

        public object Clone() => new Group(Slots);
    }
}
