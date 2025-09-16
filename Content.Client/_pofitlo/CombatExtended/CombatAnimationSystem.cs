using Content.Client.Weapons.Melee.Components;
using Content.Shared._pofitlo.CombatExtended.FightAction;
using Content.Shared._pofitlo.CombatExtended.FightAction.Prototypes;
using Robust.Client.Animations;
using Robust.Client.GameObjects;
using Robust.Shared.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Maths;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using System;
using System.Numerics;
using Content.Client.Animations;
using Robust.Shared.Animations;

namespace Content.Client._pofitlo.CombatExtended;

public sealed class CombatStrategyAnimationSystem : EntitySystem
{
    [Dependency] private readonly AnimationPlayerSystem _animation = default!;
    [Dependency] private readonly SpriteSystem _sprite = default!;
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] protected readonly IGameTiming Timing = default!;

    private const string MeleeLungeKey = "melee-lunge";
    private const string SlashAnimationKey = "melee-slash";
    private const string ThrustAnimationKey = "melee-thrust";
    private const string FadeAnimationKey = "melee-fade";

    public void DoCombatStrategyAnimation(
        EntityUid user,
        EntityUid weapon,
        Vector2 localPos,
        string? animationPrototype,
        Angle spriteRotation,
        bool flippedAnimation,
        FightActionComponent fightActionComponent,
        bool predicted = true)
    {
        if (!Timing.IsFirstTimePredicted)
            return;

        var lunge = GetLungeAnimation(localPos);

        // Stop any existing lunges on the user.
        _animation.Stop(user, MeleeLungeKey);
        _animation.Play(user, lunge, MeleeLungeKey);

        if (localPos == Vector2.Zero || animationPrototype == null)
            return;

        if (!TryComp<TransformComponent>(user, out var userXform) || userXform.MapID == MapId.Nullspace)
            return;

        var animationUid = Spawn(animationPrototype, userXform.Coordinates);


        if (!TryComp<SpriteComponent>(animationUid, out var sprite)
            || !TryComp<WeaponArcVisualsComponent>(animationUid, out var arcComponent))
        {
            return;
        }


        _sprite.SetRotation((animationUid, sprite), localPos.ToWorldAngle());
        var distance = Math.Clamp(localPos.Length() / 2f, 0.2f, 1f);

        var xform = Transform(animationUid);
        TrackUserComponent track;

        CombatAnimationPrototype? animPrototype = null;
        if (fightActionComponent.CombatAnimationPrototype != null)
        {
            _prototypeManager.TryIndex(fightActionComponent.CombatAnimationPrototype, out animPrototype);
        }

        switch (fightActionComponent.Strategy)
        {
            case AttackStrategy.TailAttack:
                track = EnsureComp<TrackUserComponent>(animationUid);
                track.User = user;
                _animation.Play(animationUid, GetSlashAnimation(sprite, spriteRotation, animPrototype), SlashAnimationKey);
                if (arcComponent.Fadeout && animPrototype?.UseFadeout != false)
                    _animation.Play(animationUid, GetFadeAnimation(sprite, animPrototype?.FadeoutStartTime ?? 0.065f, (animPrototype?.FadeoutStartTime ?? 0.065f) + (animPrototype?.FadeoutDuration ?? 0.05f)), FadeAnimationKey);
                break;
            case AttackStrategy.Punch:
                var (mapPos, mapRot) = _transform.GetWorldPositionRotation(userXform);
                var worldPos = mapPos + (mapRot - userXform.LocalRotation).RotateVec(localPos);
                var newLocalPos = Vector2.Transform(worldPos, _transform.GetInvWorldMatrix(xform.ParentUid));
                _transform.SetLocalPositionNoLerp(animationUid, newLocalPos, xform);
                if (arcComponent.Fadeout && animPrototype?.UseFadeout != false)
                    _animation.Play(animationUid, GetFadeAnimation(sprite, animPrototype?.FadeoutStartTime ?? 0f, (animPrototype?.FadeoutStartTime ?? 0f) + (animPrototype?.FadeoutDuration ?? 0.15f)), FadeAnimationKey);
                break;
        }
    }

    private Animation GetLungeAnimation(Vector2 direction)
    {
        const float length = 0.1f;

        return new Animation
        {
            Length = TimeSpan.FromSeconds(length),
            AnimationTracks =
            {
                new AnimationTrackComponentProperty()
                {
                    ComponentType = typeof(SpriteComponent),
                    Property = nameof(SpriteComponent.Offset),
                    InterpolationMode = AnimationInterpolationMode.Linear,
                    KeyFrames =
                    {
                        new AnimationTrackProperty.KeyFrame(direction.Normalized() * 0.15f, 0f),
                        new AnimationTrackProperty.KeyFrame(Vector2.Zero, length)
                    }
                }
            }
        };
    }

    private Animation GetSlashAnimation(SpriteComponent sprite, Angle spriteRotation, CombatAnimationPrototype? animPrototype = null)
    {
        var length = animPrototype?.AnimationDuration ?? 0.15f;
        //var slashAngle = animPrototype?.Angle ?? 60f;
        var startRotation = sprite.Rotation + Angle.FromDegrees(animPrototype?.AngleStart ?? 0f);
        var endRotation = startRotation + Angle.FromDegrees(animPrototype?.AngleEnd ?? 60f);
        var startRotationOffset = startRotation.RotateVec(new Vector2(0f, 0.5f));
        var endRotationOffset = endRotation.RotateVec(new Vector2(0f, .5f));
        spriteRotation = Angle.FromDegrees(135f);
        startRotation += spriteRotation;
        endRotation += spriteRotation;

        var middleRotation = startRotation + Angle.FromDegrees(((animPrototype?.AngleEnd ?? 60f) + (animPrototype?.AngleStart ?? 0f)) * 0.5f);
        var middleRotationOffset = middleRotation.RotateVec(new Vector2(0f, 0.8f));
        var tLRotation = startRotation + Angle.FromDegrees(((animPrototype?.AngleEnd ?? 60f) + (animPrototype?.AngleStart ?? 0f)) * 0.25f);
        var tLRotationOffset = middleRotation.RotateVec(new Vector2(0f, 0.65f));
        var tHRotation = startRotation + Angle.FromDegrees(((animPrototype?.AngleEnd ?? 60f) + (animPrototype?.AngleStart ?? 0f)) * 0.75f);
        var tHRotationOffset = middleRotation.RotateVec(new Vector2(0f, 0.65f));

        return new Animation
        {
            Length = TimeSpan.FromSeconds(length),
            AnimationTracks =
            {
                new AnimationTrackComponentProperty()
                {
                    ComponentType = typeof(SpriteComponent),
                    Property = nameof(SpriteComponent.Rotation),
                    InterpolationMode = AnimationInterpolationMode.Linear,
                    KeyFrames =
                    {
                        new AnimationTrackProperty.KeyFrame(startRotation, 0f),
                        new AnimationTrackProperty.KeyFrame(endRotation, length)
                    }
                }, //TODO вынести в говорящие функции
                new AnimationTrackComponentProperty()
                {
                    ComponentType = typeof(SpriteComponent),
                    Property = nameof(SpriteComponent.Offset),
                    InterpolationMode = AnimationInterpolationMode.Linear,
                    KeyFrames =
                    {
                        new AnimationTrackProperty.KeyFrame(startRotationOffset, 0f),
                        //new AnimationTrackProperty.KeyFrame(tLRotationOffset, length * 0.25f),
                        new AnimationTrackProperty.KeyFrame(middleRotationOffset, length * 0.5f),
                        //new AnimationTrackProperty.KeyFrame(tHRotationOffset, length * 0.75f),
                        new AnimationTrackProperty.KeyFrame(endRotationOffset, length)
                    }
                }
            }
        };
    }
    private Animation GetFadeAnimation(SpriteComponent sprite, float startTime, float endTime)
    {
        return new Animation
        {
            Length = TimeSpan.FromSeconds(endTime),
            AnimationTracks =
            {
                new AnimationTrackComponentProperty()
                {
                    ComponentType = typeof(SpriteComponent),
                    Property = nameof(SpriteComponent.Color),
                    InterpolationMode = AnimationInterpolationMode.Linear,
                    KeyFrames =
                    {
                        new AnimationTrackProperty.KeyFrame(sprite.Color, startTime),
                        new AnimationTrackProperty.KeyFrame(sprite.Color.WithAlpha(0f), endTime)
                    }
                }
            }
        };
    }
}
