// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 Scruq445 <storchdamien@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Vehicles;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
#region DOWNSTREAM-TPirates: vehicle overlay fix
using Robust.Client.Utility;
using Robust.Shared.Maths;
using System;
#endregion

namespace Content.Goobstation.Client.Vehicles;

public sealed class VehicleSystem : SharedVehicleSystem
{
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly IEyeManager _eye = default!;
    [Dependency] private readonly SpriteSystem _sprite = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!; // DOWNSTREAM-TPirates: vehicle overlay fix

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<VehicleComponent, AppearanceChangeEvent>(OnAppearanceChange);
        SubscribeLocalEvent<VehicleComponent, MoveEvent>(OnMove);
    }

    private void OnAppearanceChange(Entity<VehicleComponent> ent, ref AppearanceChangeEvent args)
    {
        if (args.Sprite == null
            || !_appearance.TryGetData(ent, VehicleState.Animated, out bool animated)
            || !TryComp<SpriteComponent>(ent, out var spriteComp))
            return;

        SpritePos(ent);

        if(!_sprite.TryGetLayer((ent,spriteComp),0,out var layer, false))
            return;

        _sprite.LayerSetAutoAnimated(layer, animated);
    }

    private void OnMove(Entity<VehicleComponent> ent, ref MoveEvent args)
    {
        SpritePos(ent);
    }

    private void SpritePos(Entity<VehicleComponent> ent)
    {
        if (!TryComp<SpriteComponent>(ent, out var spriteComp)
            || !_appearance.TryGetData(ent, VehicleState.DrawOver, out bool drawOver)) // DOWNSTREAM-TPirates: vehicle overlay fix
            return;

        _sprite.SetDrawDepth((ent, spriteComp), (int)Content.Shared.DrawDepth.DrawDepth.Objects);

        if (!drawOver || ent.Comp.RenderOver == VehicleRenderOver.None) // DOWNSTREAM-TPirates: vehicle overlay fix
            return;

        #region DOWNSTREAM-TPirates: vehicle overlay fix
        // Get the sprite's visual direction using the engine's logic
        var worldRot = _transform.GetWorldRotation(ent) + _eye.CurrentEye.Rotation;
        var dir = worldRot.GetDir();
        if (_sprite.TryGetLayer((ent, spriteComp), 0, out var layer, false) &&
            layer.ActualRsi is { } rsi &&
            rsi.TryGetState(layer.State, out var state))
        {
            var angle = worldRot.Reduced().FlipPositive();
            var rsiDir = SpriteComponent.Layer.GetDirection(state.RsiDirections, angle);
            dir = rsiDir.Convert();
        }
        #endregion
        var renderOverFlag = dir switch
        {
            Direction.North => VehicleRenderOver.North,
            Direction.NorthEast => VehicleRenderOver.NorthEast,
            Direction.East => VehicleRenderOver.East,
            Direction.SouthEast => VehicleRenderOver.SouthEast,
            Direction.South => VehicleRenderOver.South,
            Direction.SouthWest => VehicleRenderOver.SouthWest,
            Direction.West => VehicleRenderOver.West,
            Direction.NorthWest => VehicleRenderOver.NorthWest,
            _ => VehicleRenderOver.None,
        };

        if ((ent.Comp.RenderOver & renderOverFlag) == renderOverFlag)
            _sprite.SetDrawDepth((ent, spriteComp), (int) Content.Shared.DrawDepth.DrawDepth.OverMobs);
    }
}
