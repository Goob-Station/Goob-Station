using Content.Goobstation.Shared.Voidwalker.Actions;
using Content.Shared.Stealth;
using Content.Shared.Throwing;

namespace Content.Goobstation.Shared.Voidwalker.Abilities.VoidWalk;

public sealed partial class VoidwalkerVoidWalkSystem : EntitySystem
{
    [Dependency] private readonly SharedVoidwalkerSystem _voidwalker = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly ThrowingSystem _throwing = default!;
    [Dependency] private readonly SharedStealthSystem _stealth = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<VoidwalkerVoidWalkComponent, VoidwalkerVoidWalkEvent>(OnVoidWalk);
    }

    private void OnVoidWalk(Entity<VoidwalkerVoidWalkComponent> entity, ref VoidwalkerVoidWalkEvent args)
    {
        if (!_voidwalker.TryUseAbility(entity.Owner, args))
            return;

        var vec = (_transform.ToMapCoordinates(args.Target).Position -
                   _transform.GetMapCoordinates(args.Performer).Position).Normalized() * entity.Comp.Distance;
        var speed = entity.Comp.Speed;

        _throwing.TryThrow(args.Performer, vec, speed, animated: false);
        _stealth.SetVisibility(entity, -1);
    }
}
