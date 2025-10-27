// SPDX-FileCopyrightText: 2025 AftrLite <61218133+AftrLite@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 gluesniffler <linebarrelerenthusiast@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Enums;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Client._DV.CosmicCult.Visuals;

public sealed class MonumentPlacementPreviewOverlay : Overlay
{
    private readonly IEntityManager _ent;
    private readonly IPlayerManager _player;
    private readonly SpriteSystem _sprite;
    private readonly MonumentPlacementPreviewSystem _preview;
    private readonly IGameTiming _timing;
    public override OverlaySpace Space => OverlaySpace.WorldSpaceEntities;
    private readonly ShaderInstance _saturationShader;
    private readonly ShaderInstance _unshadedShader;
    private readonly ShaderInstance _starsShader;
    public bool LockPlacement = false;
    private EntityCoordinates _lastPos = new();

    //for a slight fade in / out
    //ss14's formatting settings can take my needlessly public variables away from my cold, dead hands
    public float FadeInProgress = 0;
    public float FadeInTime = 0.25f;
    public bool FadingIn = true;

    public float FadeOutProgress = 0;
    public float FadeOutTime = 0.25f;
    public bool FadingOut = false;

    public float Alpha = 0;

    private readonly SpriteSpecifier _mainTex;
    private readonly SpriteSpecifier _outlineTex;
    private readonly SpriteSpecifier _starTex;

    //todo arbitrary sprite drawing overlay at some point
    //I don't want to have to make a new overlay for every "draw a sprite at x" thing
    //also I kinda want wrappers around the dog ass existing arbitrary rendering methods

    //evil huge ctor because doing iocmanager stuff was killing the client for some reason
    public MonumentPlacementPreviewOverlay(IEntityManager entityManager,
        IPlayerManager playerManager,
        IPrototypeManager protoMan,
        IGameTiming timing,
        int tier,
        ResPath? mainPath = null,
        string? mainState = null,
        string? outlineState = null,
        string? starState = null)
    {
        _ent = entityManager;
        _player = playerManager;
        _sprite = _ent.System<SpriteSystem>();
        _preview = _ent.System<MonumentPlacementPreviewSystem>();
        _timing = timing;

        // Set default values or use provided ones
        var mainRsiPath = mainPath ?? new ResPath("_DV/CosmicCult/Tileset/monument.rsi");
        var mainRsiState = mainState ?? $"stage{tier}";
        var outlineRsiState = outlineState ?? $"stage{tier}-placement-ghost-1";
        var starRsiState = starState ?? $"stage{tier}-placement-ghost-2";

        _saturationShader = protoMan.Index<ShaderPrototype>("SaturationShuffle").InstanceUnique();
        _saturationShader.SetParameter("tileSize", new Vector2(96, 96));
        _saturationShader.SetParameter("hsv", new Robust.Shared.Maths.Vector3(1.0f, 0.25f, 0.2f));

        _starsShader = protoMan.Index<ShaderPrototype>("MonumentPulse").InstanceUnique();
        _starsShader.SetParameter("tileSize", new Vector2(96, 96));

        _unshadedShader = protoMan.Index<ShaderPrototype>("unshaded").Instance(); //doesn't need a unique instance

        ZIndex = (int) Shared.DrawDepth.DrawDepth.Mobs; //make the overlay render at the same depth as the actual sprite. might want to make it 1 lower if things get wierd with it.

        //will fuck up if the wrong tier is passed in but it's not my problem if that happens
        _mainTex = new SpriteSpecifier.Rsi(mainRsiPath, mainRsiState);
        _outlineTex = new SpriteSpecifier.Rsi(mainRsiPath, outlineRsiState);
        _starTex = new SpriteSpecifier.Rsi(mainRsiPath, starRsiState);
    }

    //this might get wierd if the player managed to leave the grid they put the monument on? theoretically not a concern because it can't be placed too close to space.
    //shouldn't crash due to the comp checks, though.
    protected override void Draw(in OverlayDrawArgs args)
    {
        if (!_ent.TryGetComponent<TransformComponent>(_player.LocalEntity, out var transformComp)
            || !_ent.TryGetComponent<MapGridComponent>(transformComp.GridUid, out var grid)
            || !_ent.TryGetComponent<TransformComponent>(transformComp.ParentUid, out var parentTransform))
            return;

        var worldHandle = args.WorldHandle;

        //make effects look more nicer
        var time = (float) _timing.FrameTime.TotalSeconds;
        //make the fade in / out progress
        if (FadingIn)
        {
            FadeInProgress += time;
            if (FadeInProgress >= FadeInTime)
            {
                FadingIn = false;
                FadeInProgress = FadeInTime;
            }
            Alpha = FadeInProgress / FadeInTime;
        }

        if (FadingOut)
        {
            FadeOutProgress += time;
            if (FadeOutProgress >= FadeOutTime)
            {
                FadingOut = false;
                FadeOutProgress = FadeOutTime;
            }
            Alpha = 1 - FadeOutProgress / FadeOutTime;
        }

        //have the outline's alpha slightly "breathe"
        var outlineAlphaModulate = 0.75f + 0.25f * (float) Math.Sin(_timing.CurTime.TotalSeconds);

        //stuff to make the monument preview stick in place once the ability is confirmed
        Color color;
        if (!LockPlacement)
        {
            //set the colour based on if the target tile is valid or not
            color = _preview.VerifyPlacement(transformComp, out var snappedCoords) ? Color.White.WithAlpha(outlineAlphaModulate * Alpha) : Color.Gray.WithAlpha(outlineAlphaModulate * 0.5f * Alpha);
            _lastPos = snappedCoords; //update the position
        }
        else
        {
            //if the position is locked, then it has to be valid so always use the valid colour
            color = Color.White.WithAlpha(outlineAlphaModulate * Alpha);
        }

        worldHandle.SetTransform(parentTransform.LocalMatrix);

        //for the desaturated monument "shadow"
        worldHandle.UseShader(_saturationShader);
        worldHandle.DrawTexture(_sprite.Frame0(_mainTex), _lastPos.Position - new Vector2(1.5f, 0.5f), Color.White.WithAlpha(Alpha)); //needs the offset to render in the proper position. does not inherit the extra modulate

        //for the outline to pop
        worldHandle.UseShader(_unshadedShader);
        worldHandle.DrawTexture(_sprite.Frame0(_outlineTex), _lastPos.Position - new Vector2(1.5f, 0.5f), color);

        //some fancy schmancy things for the inside of the monument
        worldHandle.UseShader(_starsShader);
        worldHandle.DrawTexture(_sprite.Frame0(_starTex), _lastPos.Position - new Vector2(1.5f, 0.5f), color);
        worldHandle.UseShader(null);
    }
}
