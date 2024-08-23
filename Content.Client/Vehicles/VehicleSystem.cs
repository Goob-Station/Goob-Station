using System.Numerics;
using Content.Shared.Vehicles;
using Robust.Client.GameObjects;

namespace Content.Server.Vehicles;

public sealed class VehicleSystem : SharedVehicleSystem
{
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly SpriteSystem _sprites = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<VehicleComponent, AppearanceChangeEvent>(OnAppearanceChange);
        SubscribeLocalEvent<VehicleComponent, MoveEvent>(OnMove);
    }

    private void OnAppearanceChange(EntityUid uid, VehicleComponent comp, ref AppearanceChangeEvent args)
    {
        if (args.Sprite == null)
            return;

        if (!_appearance.TryGetData<bool>(uid, VehicleState.Animated, out bool animated))
            return;

        if (!TryComp<SpriteComponent>(uid, out var spriteComp))
            return;

        SpritePos(uid);
        spriteComp.LayerSetAutoAnimated(0, animated);
    }

    private void OnMove(EntityUid uid, VehicleComponent component, ref MoveEvent args)
    {
        SpritePos(uid);
    }

    private void SpritePos(EntityUid uid)
    {
        if (!TryComp<SpriteComponent>(uid, out var spriteComp))
            return;

        if (!_appearance.TryGetData<bool>(uid, VehicleState.DrawOver, out bool depth))
            return;

        var isNorth = Transform(uid).LocalRotation.GetCardinalDir() == Direction.North;

        if (depth)
        {
            spriteComp.DrawDepth = (int)Content.Shared.DrawDepth.DrawDepth.OverMobs;
        }
        else
        {
            spriteComp.DrawDepth = (int)Content.Shared.DrawDepth.DrawDepth.Objects;
        }

        if (isNorth && depth)
        {
            spriteComp.DrawDepth = (int)Content.Shared.DrawDepth.DrawDepth.Objects;
        }
    }
}
