using Content.Goobstation.Shared.Shadowling.Components;
using Content.Shared.Storage.Components;

namespace Content.Goobstation.Shared.Shadowling.Systems;

/// <summary>
/// This handles egg interactions.
/// </summary>
public sealed class ShadowlingHatchingSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        // The egg shouldn't be interactable by any means
        SubscribeLocalEvent<HatchingEggComponent, StorageOpenAttemptEvent>(OnEggOpenAttempt);
        SubscribeLocalEvent<HatchingEggComponent, StorageCloseAttemptEvent>(OnEggCloseAttempt);
    }

    private void OnEggOpenAttempt(EntityUid uid, HatchingEggComponent component, ref StorageOpenAttemptEvent args)
    {
        args.Cancelled = true;
    }

    private void OnEggCloseAttempt(EntityUid uid, HatchingEggComponent component, ref StorageCloseAttemptEvent args)
    {
        args.Cancelled = true;
    }
}
