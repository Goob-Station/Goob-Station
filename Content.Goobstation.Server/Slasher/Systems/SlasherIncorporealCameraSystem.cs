using Content.Goobstation.Shared.Slasher.Events;
using Content.Server.SurveillanceCamera;
using Content.Shared.Interaction;
using Content.Shared.Physics;

namespace Content.Goobstation.Server.Slasher.Systems;

/// <summary>
/// Checks whether any active surveillance cameras have unobstructed line of sight to the slasher.
/// </summary>
public sealed class SlasherIncorporealCameraSystem : EntitySystem
{
    [Dependency] private readonly SharedInteractionSystem _interaction = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SlasherIncorporealCameraCheckEvent>(OnCameraCheck);
    }

    private void OnCameraCheck(ref SlasherIncorporealCameraCheckEvent args)
    {
        if (args.Cancelled)
            return;

        var slasher = GetEntity(args.Slasher);
        var range = args.Range;

        foreach (var other in _lookup.GetEntitiesInRange(slasher, range))
        {
            if (other == slasher)
                continue;

            // Require an active surveillance camera.
            if (!TryComp<SurveillanceCameraComponent>(other, out var cam) || !cam.Active)
                continue;

            if (_interaction.InRangeUnobstructed(other, slasher, range, CollisionGroup.Opaque))
            {
                args.Cancelled = true;
                return;
            }
        }
    }
}
