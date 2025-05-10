// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 gluesniffler <linebarrelerenthusiast@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Shared.Utility;

namespace Content.Goobstation.UIKit.UserInterface.Controls;

[Virtual]
public class StaticSpriteView : Control
{
    protected SpriteSystem? SpriteSystem;
    private SharedTransformSystem? _transform;
    protected readonly IEntityManager EntMan;

    private SpriteComponent? _cachedSprite;
    private readonly Angle _cachedWorldRotation = Angle.Zero;

    [ViewVariables]
    public SpriteComponent? Sprite => Entity?.Comp1;

    [ViewVariables]
    public Entity<SpriteComponent, TransformComponent>? Entity { get; private set; }

    [ViewVariables]
    public NetEntity? NetEnt { get; private set; }

    public bool IsVisible { get; set; } = true;

    /// <summary>
    /// This field configures automatic scaling of the sprite. This automatic scaling is done before
    /// applying the explicitly set scale <see cref="SunriseStaticSpriteView.Scale"/>.
    /// </summary>
    public StretchMode Stretch  { get; set; } = StretchMode.Fit;

    public enum StretchMode
    {
        /// <summary>
        /// Don't automatically scale the sprite. The sprite can still be scaled via <see cref="SunriseStaticSpriteView.Scale"/>
        /// </summary>
        None,

        /// <summary>
        /// Scales the sprite down so that it fits within the control. Does not scale the sprite up. Keeps the same
        /// aspect ratio. This automatic scaling is done before applying <see cref="SunriseStaticSpriteView.Scale"/>.
        /// </summary>
        Fit,

        /// <summary>
        /// Scale the sprite up or down so that it fills the whole control. Keeps the same aspect ratio. This
        /// automatic scaling is done before applying <see cref="SunriseStaticSpriteView.Scale"/>.
        /// </summary>
        Fill
    }

    /// <summary>
    /// Overrides the direction used to render the sprite.
    /// </summary>
    /// <remarks>
    /// If null, the world space orientation of the entity will be used. Otherwise the specified direction will be
    /// used.
    /// </remarks>
    public Direction? OverrideDirection { get; set; }

    #region Transform

    private Vector2 _scale = Vector2.One;
    private Angle _eyeRotation = Angle.Zero;
    private Angle? _worldRotation = Angle.Zero;

    public Angle EyeRotation
    {
        get => _eyeRotation;
        set
        {
            _eyeRotation = value;
            InvalidateMeasure();
        }
    }

    /// <summary>
    /// Used to override the entity's world rotation. Note that the desired size of the control will not
    /// automatically get updated as the entity's world rotation changes.
    /// </summary>
    public Angle? WorldRotation
    {
        get => _worldRotation;
        set
        {
            _worldRotation = value;
            InvalidateMeasure();
        }
    }

    /// <summary>
    /// Scale to apply when rendering the sprite. This is separate from the sprite's scale.
    /// </summary>
    public Vector2 Scale
    {
        get => _scale;
        set
        {
            _scale = value;
            InvalidateMeasure();
        }
    }

    /// <summary>
    /// Cached desired size. Differs from <see cref="Control.DesiredSize"/> as it it is not clamped by the
    /// minimum/maximum size options.
    /// </summary>
    private Vector2 _spriteSize;

    /// <summary>
    /// Determines whether or not the sprite's offset be applied to the control.
    /// </summary>
    public bool SpriteOffset { get; set; }

    #endregion

    public StaticSpriteView()
    {
        IoCManager.Resolve(ref EntMan);
        RectClipContent = true;
    }

    public StaticSpriteView(IEntityManager entMan)
    {
        EntMan = entMan;
        RectClipContent = true;
    }

    public StaticSpriteView(EntityUid? uid, IEntityManager entMan)
    {
        EntMan = entMan;
        RectClipContent = true;
        SetEntity(uid);
    }

    public StaticSpriteView(NetEntity uid, IEntityManager entMan)
    {
        EntMan = entMan;
        RectClipContent = true;
        SetEntity(uid);
    }

    public void SetEntity(NetEntity netEnt)
    {
        if (netEnt == NetEnt)
            return;

        if (EntMan.TryGetEntity(netEnt, out var uid))
        {
            SetEntity(uid);
        }
        else
        {
            // Подписаться на событие появления сущности
            Entity = null;
            NetEnt = netEnt;
        }
    }

    public void SetEntity(EntityUid? uid)
    {
        if (Entity?.Owner == uid)
            return;

        if (!EntMan.TryGetComponent(uid, out SpriteComponent? sprite)
            || !EntMan.TryGetComponent(uid, out TransformComponent? xform))
        {
            Entity = null;
            NetEnt = null;
            return;
        }

        // Создаем глубокую копию спрайта
        _cachedSprite = new SpriteComponent();
        _cachedSprite.CopyFrom(sprite); // Используем встроенный метод копирования

        Entity = new(uid.Value, sprite, xform);
        NetEnt = EntMan.GetNetEntity(uid);
    }
    protected override Vector2 MeasureOverride(Vector2 availableSize)
    {
        // TODO Make this get called when sprite bounds/properties update?
        UpdateSize();
        return _spriteSize;
    }

    private void UpdateSize()
    {
        if (!ResolveEntity(out _, out var sprite, out _))
            return;

        var spriteBox = sprite.CalculateRotatedBoundingBox(default,  _worldRotation ?? Angle.Zero, _eyeRotation)
            .CalcBoundingBox();

        if (!SpriteOffset)
        {
            // re-center the box.
            spriteBox = spriteBox.Translated(-spriteBox.Center);
        }

        // Scale the box (including any offset);
        var scale = _scale * EyeManager.PixelsPerMeter;
        var bl = spriteBox.BottomLeft * scale;
        var tr = spriteBox.TopRight * scale;

        // This view will be centered on (0,0). If the sprite was shifted by (1,2) the actual size of the control
        // would need to be at least (2,4).
        tr = Vector2.Max(tr, Vector2.Zero);
        bl = Vector2.Min(bl, Vector2.Zero);
        tr = Vector2.Max(tr, -bl);
        bl = Vector2.Min(bl, -tr);
        var box = new Box2(bl, tr);

        DebugTools.Assert(box.Contains(Vector2.Zero));
        DebugTools.Assert(box.TopLeft.EqualsApprox(-box.BottomRight));

        if (_worldRotation != null
            && _eyeRotation == Angle.Zero) // TODO This shouldn't need to be here, but I just give up at this point I am going fucking insane looking at rotating blobs of pixels. I doubt anyone will ever even use rotated sprite views.?
        {
            _spriteSize = box.Size;
            return;
        }

        // Size does not auto-update with world rotation. So if it is not fixed by _worldRotation we will just take
        // the maximum possible size.
        var size = box.Size;
        var longestSide = MathF.Max(size.X, size.Y);
        var longestRotatedSide = Math.Max(longestSide, (size.X + size.Y) / MathF.Sqrt(2));
        _spriteSize = new Vector2(longestRotatedSide, longestRotatedSide);
    }

    protected override void Draw(IRenderHandle renderHandle)
    {
        if (!ResolveEntity(out var uid, out _, out var xform) || _cachedSprite == null)
            return;

        SpriteSystem ??= EntMan.System<SpriteSystem>();
        _transform ??= EntMan.System<TransformSystem>();

        var stretchVec = Stretch switch
        {
            StretchMode.Fit => Vector2.Min(Size / _spriteSize, Vector2.One),
            StretchMode.Fill => Size / _spriteSize,
            _ => Vector2.One,
        };
        var stretch = MathF.Min(stretchVec.X, stretchVec.Y);

        var offset = SpriteOffset
            ? Vector2.Zero
            : - (-_eyeRotation).RotateVec(_cachedSprite.Offset * _scale) * new Vector2(1, -1) * EyeManager.PixelsPerMeter;

        var position = PixelSize / 2 + offset * stretch * UIScale;
        var scale = Scale * UIScale * stretch;

        var world = renderHandle.DrawingHandleWorld;
        var oldModulate = world.Modulate;
        world.Modulate *= Modulate * ActualModulateSelf;

        renderHandle.DrawEntity(
            uid,
            position,
            scale,
            _cachedWorldRotation, // Используем сохраненный поворот
            _eyeRotation,
            OverrideDirection,
            _cachedSprite, // Кэшированный спрайт
            xform
        );

        world.Modulate = oldModulate;
    }

    private bool ResolveEntity(
        out EntityUid uid,
        [NotNullWhen(true)] out SpriteComponent? sprite,
        [NotNullWhen(true)] out TransformComponent? xform)
    {
        sprite = _cachedSprite; // Возвращаем кэшированный спрайт
        xform = null; // Не используем текущий transform

        if (NetEnt != null && Entity == null && EntMan.TryGetEntity(NetEnt, out var ent))
            SetEntity(ent);

        if (Entity != null)
        {
            uid = Entity.Value.Owner;
            return !EntMan.Deleted(uid);
        }

        uid = default;
        return false;
    }
}
