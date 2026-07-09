// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Client.Rotation;
using Content.Goobstation.Shared.Vehicles;
using Content.Shared.Buckle;
using Content.Shared.Buckle.Components;
using Content.Shared.Movement.Systems;
using Content.Shared.Rotation;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Content.Shared.Tag; // DOWNSTREAM - TPirates: vehicle overlay fix (and chairs)
using Robust.Shared.Prototypes; // DOWNSTREAM - TPirates: vehicle overlay fix (and chairs)

namespace Content.Client.Buckle;

internal sealed class BuckleSystem : SharedBuckleSystem
{
    [Dependency] private readonly RotationVisualizerSystem _rotationVisualizerSystem = default!;
    [Dependency] private readonly IEyeManager _eye = default!;
    [Dependency] private readonly SharedTransformSystem _xformSystem = default!;
    [Dependency] private readonly SpriteSystem _sprite = default!;
    [Dependency] private readonly TagSystem _tag = default!; // DOWNSTREAM-TPirates: vehicle overlay fix (and chairs)
    private static readonly ProtoId<TagPrototype> ChairTag = "Chair"; // DOWNSTREAM-TPirates: vehicle overlay fix (and chairs)

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BuckleComponent, AppearanceChangeEvent>(OnAppearanceChange);
        SubscribeLocalEvent<StrapComponent, MoveEvent>(OnStrapMoveEvent);
        SubscribeLocalEvent<BuckleComponent, BuckledEvent>(OnBuckledEvent);
        SubscribeLocalEvent<BuckleComponent, UnbuckledEvent>(OnUnbuckledEvent);
        SubscribeLocalEvent<BuckleComponent, ComponentRemove>(OnBuckleRemove);
        SubscribeLocalEvent<BuckleComponent, AfterAutoHandleStateEvent>(OnAfterBuckleState);
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
        UpdateChairStrapDepth(uid, strapSprite, isNorth, component.BuckledEntities.Count > 0); // DOWNSTREAM-TPirates: vehicle overlay fix (and chairs)
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

        var strapComp = args.Strap.Comp;

        if (!TryComp<SpriteComponent>(ent.Owner, out var buckledSprite))
            return;

        // Goobstation - Start
        if (strapComp.SetVisible)
        {
            // Pirate: predicted buckles can run more than once; keep the real pre-hide value.
            ent.Comp.OriginalVisible ??= buckledSprite.Visible;
            _sprite.SetVisible((ent.Owner, buckledSprite), false);
        }

        var angle = _xformSystem.GetWorldRotation(args.Strap) + _eye.CurrentEye.Rotation;
        var isNorth = angle.GetCardinalDir() == Direction.North;
        UpdateChairStrapDepth(args.Strap, strapSprite, isNorth, true); // DOWNSTREAM-TPirates: vehicle overlay fix (and chairs)

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
        #region DOWNSTREAM-TPirates: vehicle overlay fix (and chairs)
        if (TryComp<SpriteComponent>(args.Strap, out var strapSprite))
        {
            var angle = _xformSystem.GetWorldRotation(args.Strap) + _eye.CurrentEye.Rotation;
            var isNorth = angle.GetCardinalDir() == Direction.North;
            UpdateChairStrapDepth(args.Strap, strapSprite, isNorth, args.Strap.Comp.BuckledEntities.Count > 0);
        }
        #endregion
        if (!TryComp<SpriteComponent>(ent.Owner, out var buckledSprite))
            return;

        if (args.Strap.Comp.SetVisible)
            RestoreHiddenSprite(ent, buckledSprite);

        RestoreDrawDepth(ent, buckledSprite);
    }

    private void OnBuckleRemove(Entity<BuckleComponent> ent, ref ComponentRemove args)
    {
        RestoreHiddenSprite(ent);
        RestoreDrawDepth(ent);
    }

    private void OnAfterBuckleState(Entity<BuckleComponent> ent, ref AfterAutoHandleStateEvent args)
    {
        if (ent.Comp.Buckled)
            return;

        RestoreHiddenSprite(ent);
        RestoreDrawDepth(ent);
    }

    private void RestoreHiddenSprite(Entity<BuckleComponent> ent, SpriteComponent? sprite = null)
    {
        if (ent.Comp.OriginalVisible is not { } originalVisible)
            return;

        if (!Resolve(ent.Owner, ref sprite, false))
            return;

        _sprite.SetVisible((ent.Owner, sprite), originalVisible);
        ent.Comp.OriginalVisible = null;
    }

    private void RestoreDrawDepth(Entity<BuckleComponent> ent, SpriteComponent? sprite = null)
    {
        if (ent.Comp.OriginalDrawDepth is not { } originalDrawDepth)
            return;

        if (!Resolve(ent.Owner, ref sprite, false))
            return;

        _sprite.SetDrawDepth((ent.Owner, sprite), originalDrawDepth);
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
    #region DOWNSTREAM-TPirates: vehicle overlay fix (and chairs)
    private void UpdateChairStrapDepth(EntityUid strap, SpriteComponent strapSprite, bool isNorth, bool occupied)
    {
        // For chair straps, move the chair itself over mobs when occupied and north-facing.
        // This keeps layering correct even if buckled mob depth gets reset by unrelated visuals.
        if (HasComp<VehicleComponent>(strap) || !_tag.HasTag(strap, ChairTag))
            return;

        var targetDepth = occupied && isNorth
            ? (int)Content.Shared.DrawDepth.DrawDepth.OverMobs
            : (int)Content.Shared.DrawDepth.DrawDepth.Objects;

        if (strapSprite.DrawDepth != targetDepth)
            _sprite.SetDrawDepth((strap, strapSprite), targetDepth);
    }
    #endregion
}
