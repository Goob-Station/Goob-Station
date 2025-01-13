using Robust.Shared.Physics.Events;

namespace Content.Shared.Turnstile;

/// <summary>
/// This handles...
/// </summary>
public sealed class TurnstileSystem : EntitySystem
{

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<TurnstileComponent,StartCollideEvent>(OnCollide);
        SubscribeLocalEvent<TurnstileComponent,EndCollideEvent>(EndCollide);
    }
    private void OnCollide(EntityUid ent, TurnstileComponent comp, ref StartCollideEvent args)
    {

        var newComp = EnsureComp<MovementBlockerComponent>(args.OtherEntity);
        newComp.Turnstile = ent;
    }
    private void EndCollide(EntityUid ent, TurnstileComponent comp, ref EndCollideEvent args)
    {
        RemComp<MovementBlockerComponent>(args.OtherEntity);
    }


}
