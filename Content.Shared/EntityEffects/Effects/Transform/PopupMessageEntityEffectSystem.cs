// < Trauma >
using Content.Shared.IdentityManagement;
using Content.Shared.Random.Helpers;
using Robust.Shared.Timing;
// </Trauma>
using Content.Shared.Popups;
using Robust.Shared.Network;
using Robust.Shared.Random;
using Robust.Shared.Serialization;

namespace Content.Shared.EntityEffects.Effects.Transform;

/// <summary>
/// Creates a text popup to appear at this entity's coordinates.
/// </summary>
/// <inheritdoc cref="EntityEffectSystem{T,TEffect}"/>
public sealed partial class PopupMessageEntityEffectSystem : EntityEffectSystem<TransformComponent, PopupMessage>
{
    // <Trauma>
    [Dependency] private IGameTiming _timing = default!;
    // </Trauma>
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    protected override void Effect(Entity<TransformComponent> entity, ref EntityEffectEvent<PopupMessage> args)
    {
        // <Trauma> - predict these
        if (!_timing.IsFirstTimePredicted || _net.IsServer && args.Predicted)
            return;

        var rand = SharedRandomExtensions.PredictedRandom(_timing, GetNetEntity(entity));
        var msg = Loc.GetString(rand.Pick(args.Effect.Messages), ("entity",
            Identity.Entity(entity, EntityManager))); // Trauma - don't doxx from popups

        switch ((args.Effect.Method, args.Effect.Type))
        {
            case (PopupMethod.PopupEntity, PopupRecipients.Local):
                _popup.PopupEntity(msg, entity, entity, args.Effect.VisualType);
                break;
            case (PopupMethod.PopupEntity, PopupRecipients.Pvs):
                _popup.PopupEntity(msg, entity, args.Effect.VisualType);
                break;
            case (PopupMethod.PopupCoordinates, PopupRecipients.Local):
                _popup.PopupCoordinates(msg, Transform(entity).Coordinates, entity, args.Effect.VisualType);
                break;
            case (PopupMethod.PopupCoordinates, PopupRecipients.Pvs):
                _popup.PopupCoordinates(msg, Transform(entity).Coordinates, args.Effect.VisualType);
                break;
        }
    }
}

/// <inheritdoc cref="EntityEffect"/>
public sealed partial class PopupMessage : EntityEffectBase<PopupMessage>
{
    /// <summary>
    /// Array of messages that can popup.
    /// Only one is chosen when the effect is applied.
    /// </summary>
    [DataField(required: true)]
    public LocId[] Messages = default!; // Trauma - Use LocId

    /// <summary>
    /// Whether to just the entity we're affecting, or everyone around them.
    /// </summary>
    [DataField]
    public PopupRecipients Type = PopupRecipients.Local;

    /// <summary>
    /// Which popup API method to use.
    /// Use PopupCoordinates in case the entity will be deleted while the popup is shown.
    /// </summary>
    [DataField]
    public PopupMethod Method = PopupMethod.PopupEntity;

    /// <summary>
    /// Size of the popup.
    /// </summary>
    [DataField]
    public PopupType VisualType = PopupType.Small;
}

[Serializable, NetSerializable]
public enum PopupRecipients : byte
{
    Pvs,
    Local,
}

[Serializable, NetSerializable]
public enum PopupMethod : byte
{
    PopupEntity,
    PopupCoordinates,
}
