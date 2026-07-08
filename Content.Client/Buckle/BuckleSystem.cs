// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Client.Rotation;
using Content.Goobstation.Shared.Vehicles;
using Content.Shared.Buckle;
using Content.Shared.Buckle.Components;
using Content.Shared.Movement.Systems;
using Content.Shared.Rotation;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;

namespace Content.Client.Buckle;

internal sealed class BuckleSystem : SharedBuckleSystem
{
    [Dependency] private readonly RotationVisualizerSystem _rotationVisualizerSystem = default!;
    [Dependency] private readonly IEyeManager _eye = default!;
    [Dependency] private readonly SharedTransformSystem _xformSystem = default!;
    [Dependency] private readonly SpriteSystem _sprite = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BuckleComponent, AppearanceChangeEvent>(OnAppearanceChange);
        SubscribeLocalEvent<StrapComponent, MoveEvent>(OnStrapMoveEvent);
        SubscribeLocalEvent<BuckleComponent, BuckledEvent>(OnBuckledEvent);
        SubscribeLocalEvent<BuckleComponent, UnbuckledEvent>(OnUnbuckledEvent);
        SubscribeLocalEvent<BuckleComponent, AttemptMobCollideEvent>(OnMobCollide);
    }

    private void OnMobCollide(Entity<BuckleComponent> ent, ref AttemptMobCollideEvent args)
    {
        if (ent.Comp.Buckled)
        {
            args.Cancelled = true;
        }
    }

    private void OnStrapMoveEvent(EntityUid uid, StrapComponent component, ref MoveEvent args)
    {
        // I'm moving this to the client-side system, but for the sake of posterity let's keep this comment:
        // > This is mega cursed. Please somebody save me from Mr Buckle's wild ride

        // The nice thing is its still true, this is quite cursed, though maybe not omega cursed anymore.
        // This code is garbage, it doesn't work with rotated viewports. I need to finally get around to reworking
        // sprite rendering for entity layers & direction dependent sorting.

        // Future notes:
        // Right now this doesn't handle: other grids, other grids rotating, the camera rotation changing, and many other fun rotation specific things
        // The entire thing should be a concern of the engine, or something engine helps to implement properly.
        // Give some of the sprite rotations their own drawdepth, maybe as an offset within the rsi, or something like this
        // And we won't ever need to set the draw depth manually

        if (args.NewRotation == args.OldRotation)
            return;

        if (!TryComp<SpriteComponent>(uid, out var strapSprite))
            return;

        var angle = _xformSystem.GetWorldRotation(uid) + _eye.CurrentEye.Rotation; // Get true screen position, or close enough

        var isNorth = angle.GetCardinalDir() == Direction.North;
        foreach (var buckledEntity in component.BuckledEntities)
        {
            if (!TryComp<BuckleComponent>(buckledEntity, out var buckle))
                continue;

            if (!TryComp<SpriteComponent>(buckledEntity, out var buckledSprite))
                continue;

            // Goobstation start
            if (HasComp<VehicleComponent>(uid)) // let vehicle handle drawdepth
                return;
            buckle.OriginalDrawDepth ??= buckledSprite.DrawDepth;
            if (isNorth)
            {
                _sprite.SetDrawDepth((buckledEntity, buckledSprite), strapSprite.DrawDepth - 1);
            }
            else
            {
                _sprite.SetDrawDepth((buckledEntity, buckledSprite), strapSprite.DrawDepth + 1);
            }
            // Goobstation - end
        }
    }

    /// <summary>
    /// Lower the draw depth of the buckled entity without needing for the strap entity to rotate/move.
    /// Only do so when the entity is facing screen-local north
    /// </summary>
    private void OnBuckledEvent(Entity<BuckleComponent> ent, ref BuckledEvent args)
    {
        if (!TryComp<SpriteComponent>(args.Strap, out var strapSprite))
            return;

        if (!TryComp<SpriteComponent>(ent.Owner, out var buckledSprite))
            return;

        // Goobstation - Start
        var angle = _xformSystem.GetWorldRotation(args.Strap) + _eye.CurrentEye.Rotation;
        var isNorth = angle.GetCardinalDir() == Direction.North;

        ent.Comp.OriginalDrawDepth ??= buckledSprite.DrawDepth;
        _sprite.SetDrawDepth(
            (ent.Owner, buckledSprite),
        strapSprite.DrawDepth + (isNorth ? -1 : 1)
            );
        // Goobstation - end
    }

    /// <summary>
    /// Was the draw depth of the buckled entity lowered? Reset it upon unbuckling.
    /// </summary>
    private void OnUnbuckledEvent(Entity<BuckleComponent> ent, ref UnbuckledEvent args)
    {
        if (!TryComp<SpriteComponent>(ent.Owner, out var buckledSprite))
            return;

        if (!ent.Comp.OriginalDrawDepth.HasValue)
            return;

        _sprite.SetDrawDepth((ent.Owner, buckledSprite), ent.Comp.OriginalDrawDepth.Value);
        ent.Comp.OriginalDrawDepth = null;
    }

    private void OnAppearanceChange(EntityUid uid, BuckleComponent component, ref AppearanceChangeEvent args)
    {
        if (!TryComp<RotationVisualsComponent>(uid, out var rotVisuals))
            return;

        if (!Appearance.TryGetData<bool>(uid, BuckleVisuals.Buckled, out var buckled, args.Component) ||
            !buckled ||
            args.Sprite == null)
        {
            _rotationVisualizerSystem.SetHorizontalAngle((uid, rotVisuals), rotVisuals.DefaultRotation);
            return;
        }

        // Animate strapping yourself to something at a given angle
        // TODO: Dump this when buckle is better
        _rotationVisualizerSystem.AnimateSpriteRotation(uid, args.Sprite, rotVisuals.HorizontalRotation, 0.125f);
    }
}
