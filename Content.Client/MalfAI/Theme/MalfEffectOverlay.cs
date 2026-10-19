// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
//
// SPDX-License-Identifier: MIT

using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Client.UserInterface;
using Robust.Shared.IoC;
using Robust.Shared.Maths;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using System;
using System.Numerics;

namespace Content.Client.MalfAI.Theme;

/// <summary>
/// Effect type for the overlay system
/// </summary>
public enum MalfEffectType
{
    /// <summary>
    /// Uses the CameraStatic shader for authentic AI fog-of-war static
    /// </summary>
    ShaderStatic,

    /// <summary>
    /// Uses procedural drawing for static effects
    /// </summary>
    ProceduralStatic,

    /// <summary>
    /// Uses a sprite texture for animated effects
    /// </summary>
    SpriteAnimation,

    /// <summary>
    /// Custom shader effect
    /// </summary>
    CustomShader
}

/// <summary>
/// Render layer for the overlay system
/// </summary>
public enum MalfRenderLayer
{
    /// <summary>
    /// Renders behind all UI elements (underlay)
    /// </summary>
    Underlay,

    /// <summary>
    /// Renders above all UI elements (overlay)
    /// </summary>
    Overlay
}

/// <summary>
/// Configuration for Malf effect overlays
/// </summary>
public sealed class MalfEffectConfig
{
    public MalfEffectType Type { get; set; } = MalfEffectType.ShaderStatic;
    public MalfRenderLayer Layer { get; set; } = MalfRenderLayer.Overlay;
    public Color Color { get; set; } = new(1f, 1f, 1f, 0.08f);
    public float Scale { get; set; } = 1.0f;
    public float Speed { get; set; } = 1.0f;
    public string? ShaderPrototype { get; set; } = "CameraStatic";
    public string? SpriteTexture { get; set; }
    public bool Enabled { get; set; } = true;
}

/// <summary>
/// Unified overlay system for Malf AI effects that supports both underlays and overlays
/// with sprite-based or shader-based animations.
/// </summary>
public sealed class MalfEffectOverlay : Control
{
    [Dependency] private readonly IResourceCache _resourceCache = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    private MalfEffectConfig _config = new();
    private ShaderInstance? _shaderInstance;
    private Texture? _spriteTexture;
    private float _animationTime;

    public MalfEffectConfig Config
    {
        get => _config;
        set
        {
            _config = value;
            LoadResources();
        }
    }

    /// <summary>
    /// Creates a static overlay effect using the CameraStatic shader
    /// </summary>
    public static MalfEffectOverlay CreateStaticOverlay(float alpha = 0.08f)
    {
        var overlay = new MalfEffectOverlay();
        overlay.Config = new MalfEffectConfig
        {
            Type = MalfEffectType.ShaderStatic,
            Layer = MalfRenderLayer.Overlay,
            Color = new Color(1f, 1f, 1f, alpha),
            ShaderPrototype = "CameraStatic"
        };
        return overlay;
    }

    /// <summary>
    /// Creates a static underlay effect for backgrounds
    /// </summary>
    public static MalfEffectOverlay CreateStaticUnderlay(float alpha = 0.04f)
    {
        var overlay = new MalfEffectOverlay();
        overlay.Config = new MalfEffectConfig
        {
            Type = MalfEffectType.ShaderStatic,
            Layer = MalfRenderLayer.Underlay,
            Color = new Color(1f, 1f, 1f, alpha),
            ShaderPrototype = "CameraStatic"
        };
        return overlay;
    }

    /// <summary>
    /// Creates a sprite-based animated overlay
    /// </summary>
    public static MalfEffectOverlay CreateSpriteOverlay(string spriteTexture, Color color, float speed = 1.0f)
    {
        var overlay = new MalfEffectOverlay();
        overlay.Config = new MalfEffectConfig
        {
            Type = MalfEffectType.SpriteAnimation,
            Layer = MalfRenderLayer.Overlay,
            Color = color,
            Speed = speed,
            SpriteTexture = spriteTexture
        };
        return overlay;
    }

    /// <summary>
    /// Creates a sprite-based animated underlay for backgrounds
    /// </summary>
    public static MalfEffectOverlay CreateSpriteUnderlay(string spriteTexture, Color color, float speed = 1.0f, float scale = 1.0f)
    {
        var overlay = new MalfEffectOverlay();
        overlay.Config = new MalfEffectConfig
        {
            Type = MalfEffectType.SpriteAnimation,
            Layer = MalfRenderLayer.Underlay,
            Color = color,
            Speed = speed,
            Scale = scale,
            SpriteTexture = spriteTexture
        };
        return overlay;
    }

    /// <summary>
    /// Creates a scrolling error backdrop for Malf AI UIs
    /// </summary>
    public static MalfEffectOverlay CreateErrorBackdrop(float alpha = 0.15f, float speed = 0.5f)
    {
        var overlay = new MalfEffectOverlay();
        overlay.Config = new MalfEffectConfig
        {
            Type = MalfEffectType.SpriteAnimation,
            Layer = MalfRenderLayer.Underlay,
            Speed = speed,
            Scale = 1,
            SpriteTexture = "/Textures/error.rsi/error.png"
        };
        return overlay;
    }

    public MalfEffectOverlay()
    {
        IoCManager.InjectDependencies(this);

        // Make this control fill its parent and be mouse-transparent
        HorizontalExpand = true;
        VerticalExpand = true;
        MouseFilter = MouseFilterMode.Ignore;

        LoadResources();
    }

    private void LoadResources()
    {
        if (!_config.Enabled)
            return;

        switch (_config.Type)
        {
            case MalfEffectType.ShaderStatic:
            case MalfEffectType.CustomShader:
                if (!string.IsNullOrEmpty(_config.ShaderPrototype))
                {
                    _shaderInstance = _prototypeManager.Index<ShaderPrototype>(_config.ShaderPrototype).Instance();
                }
                break;

            case MalfEffectType.SpriteAnimation:
                if (!string.IsNullOrEmpty(_config.SpriteTexture))
                {
                    try
                    {
                        _spriteTexture = _resourceCache.GetResource<TextureResource>(_config.SpriteTexture).Texture;
                    }
                    catch
                    {
                        // Fallback to procedural if sprite loading fails
                        _config.Type = MalfEffectType.ProceduralStatic;
                    }
                }
                break;
        }
    }

    protected override void FrameUpdate(FrameEventArgs args)
    {
        base.FrameUpdate(args);

        if (!_config.Enabled)
            return;

        // Update animation time
        _animationTime += (float)args.DeltaSeconds * _config.Speed;
    }

    protected override void Draw(DrawingHandleScreen handle)
    {
        base.Draw(handle);

        if (!_config.Enabled)
            return;

        var box = PixelSizeBox;

        switch (_config.Type)
        {
            case MalfEffectType.ShaderStatic:
            case MalfEffectType.CustomShader:
                DrawShaderEffect(handle, box);
                break;

            case MalfEffectType.SpriteAnimation:
                DrawSpriteEffect(handle, box);
                break;

            case MalfEffectType.ProceduralStatic:
                DrawProceduralStatic(handle, box);
                break;
        }
    }

    private void DrawShaderEffect(DrawingHandleScreen handle, UIBox2 box)
    {
        if (_shaderInstance == null)
            return;

        handle.UseShader(_shaderInstance);
        handle.DrawRect(box, _config.Color);
        handle.UseShader(null);
    }

    private void DrawSpriteEffect(DrawingHandleScreen handle, UIBox2 box)
    {
        if (_spriteTexture == null)
        {
            DrawProceduralStatic(handle, box);
            return;
        }

        // Calculate texture coordinates with animation offset
        var offsetX = (_animationTime * 20) % _spriteTexture.Width;
        var offsetY = (_animationTime * 15) % _spriteTexture.Height;

        // Draw tiled sprite texture with transparency
        var scaledSize = new Vector2(_spriteTexture.Width * _config.Scale,
                                   _spriteTexture.Height * _config.Scale);

        for (float x = box.Left - offsetX; x < box.Right + scaledSize.X; x += scaledSize.X)
        {
            for (float y = box.Top - offsetY; y < box.Bottom + scaledSize.Y; y += scaledSize.Y)
            {
                var drawBox = new UIBox2(x, y, x + scaledSize.X, y + scaledSize.Y);

                // Manual clipping to fit within the control area
                var clippedLeft = Math.Max(drawBox.Left, box.Left);
                var clippedTop = Math.Max(drawBox.Top, box.Top);
                var clippedRight = Math.Min(drawBox.Right, box.Right);
                var clippedBottom = Math.Min(drawBox.Bottom, box.Bottom);

                if (clippedLeft < clippedRight && clippedTop < clippedBottom)
                {
                    var clippedBox = new UIBox2(clippedLeft, clippedTop, clippedRight, clippedBottom);
                    handle.DrawTextureRect(_spriteTexture, clippedBox, _config.Color);
                }
            }
        }
    }

    private void DrawProceduralStatic(DrawingHandleScreen handle, UIBox2 box)
    {
        // Simple procedural static using small rectangles
        var random = new Random((int)(_animationTime * 1000));
        var pixelSize = 2.0f;

        for (float x = box.Left; x < box.Right; x += pixelSize)
        {
            for (float y = box.Top; y < box.Bottom; y += pixelSize)
            {
                if (random.NextDouble() > 0.92) // Only draw 8% of pixels for sparse static
                {
                    var intensity = (float)random.NextDouble() * _config.Color.A;
                    var pixelColor = new Color(_config.Color.R, _config.Color.G, _config.Color.B, intensity);
                    handle.DrawRect(new UIBox2(x, y, x + pixelSize, y + pixelSize), pixelColor);
                }
            }
        }
    }
}
