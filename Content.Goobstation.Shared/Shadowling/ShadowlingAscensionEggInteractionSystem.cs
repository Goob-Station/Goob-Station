using Content.Shared._EE.Shadowling.Components;
using Content.Shared.Storage.Components;


namespace Content.Shared._EE.Shadowling;


/// <summary>
/// This handles Ascension Egg interactions
/// </summary>
public sealed class ShadowlingAscensionEggInteractionSystem : EntitySystem
{
    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        // The egg shouldn't be interactable by any means
        SubscribeLocalEvent<ShadowlingAscensionEggComponent, StorageOpenAttemptEvent>(OnEggOpenAttempt);
        SubscribeLocalEvent<ShadowlingAscensionEggComponent, StorageCloseAttemptEvent>(OnEggCloseAttempt);
    }

    private void OnEggOpenAttempt(EntityUid uid, ShadowlingAscensionEggComponent component, ref StorageOpenAttemptEvent args)
    {
        args.Cancelled = true;
    }

    private void OnEggCloseAttempt(EntityUid uid, ShadowlingAscensionEggComponent component, ref StorageCloseAttemptEvent args)
    {
        args.Cancelled = true;
    }
}
