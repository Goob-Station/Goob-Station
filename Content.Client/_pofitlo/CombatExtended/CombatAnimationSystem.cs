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
        ProtoId<CombatAnimationPrototype>? combatAnimProto,
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
        //TrackUserComponent track;

        if (combatAnimProto == null || !_prototypeManager.TryIndex(combatAnimProto, out var animPrototype))
            return;

        var animationType = animPrototype.AnimationType;

        SetAnimationType(animationType, user, animationUid, sprite, spriteRotation, animPrototype, arcComponent, localPos, userXform, xform);

        // switch (fightActionComponent.Strategy)
        // {
        //     case AttackStrategy.TailAttack:
        //         track = EnsureComp<TrackUserComponent>(animationUid);
        //         track.User = user;
        //         _animation.Play(animationUid, GetSlashAnimation(sprite, spriteRotation, animPrototype), SlashAnimationKey);
        //         if (arcComponent.Fadeout && animPrototype?.UseFadeout != false)
        //             _animation.Play(animationUid, GetFadeAnimation(sprite, animPrototype?.FadeoutStartTime ?? 0.065f, (animPrototype?.FadeoutStartTime ?? 0.065f) + (animPrototype?.FadeoutDuration ?? 0.05f)), FadeAnimationKey);
        //         break;
        //     case AttackStrategy.Punch:
        //         var (mapPos, mapRot) = _transform.GetWorldPositionRotation(userXform);
        //         var worldPos = mapPos + (mapRot - userXform.LocalRotation).RotateVec(localPos);
        //         var newLocalPos = Vector2.Transform(worldPos, _transform.GetInvWorldMatrix(xform.ParentUid));
        //         _transform.SetLocalPositionNoLerp(animationUid, newLocalPos, xform);
        //         if (arcComponent.Fadeout && animPrototype?.UseFadeout != false)
        //             _animation.Play(animationUid, GetFadeAnimation(sprite, animPrototype?.FadeoutStartTime ?? 0f, (animPrototype?.FadeoutStartTime ?? 0f) + (animPrototype?.FadeoutDuration ?? 0.15f)), FadeAnimationKey);
        //         break;
        // }
    }

    private void SetAnimationType(String animationType, EntityUid user, EntityUid animationUid, SpriteComponent sprite, Angle spriteRotation, CombatAnimationPrototype animPrototype, WeaponArcVisualsComponent arcComponent, Vector2 localPos, TransformComponent userXform, TransformComponent xform)
    {
        if (animationType == "Slash")
        {
            var track = EnsureComp<TrackUserComponent>(animationUid);
            track.User = user;
            _animation.Play(animationUid, GetSlashAnimation(sprite, spriteRotation, animPrototype), SlashAnimationKey);
            if (arcComponent.Fadeout && animPrototype?.UseFadeout != false)
                _animation.Play(animationUid, GetFadeAnimation(sprite, animPrototype?.FadeoutStartTime ?? 0.065f, (animPrototype?.FadeoutStartTime ?? 0.065f) + (animPrototype?.FadeoutDuration ?? 0.05f)), FadeAnimationKey);

            return;
        }
        if (animationType == "Thrust")
        {
            var (mapPos, mapRot) = _transform.GetWorldPositionRotation(userXform);
            var worldPos = mapPos + (mapRot - userXform.LocalRotation).RotateVec(localPos);
            var newLocalPos = Vector2.Transform(worldPos, _transform.GetInvWorldMatrix(xform.ParentUid));
            _transform.SetLocalPositionNoLerp(animationUid, newLocalPos, xform);
            if (arcComponent.Fadeout && animPrototype?.UseFadeout != false)
                _animation.Play(animationUid, GetFadeAnimation(sprite, animPrototype?.FadeoutStartTime ?? 0f, (animPrototype?.FadeoutStartTime ?? 0f) + (animPrototype?.FadeoutDuration ?? 0.15f)), FadeAnimationKey);      //TODO
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

    private Animation GetSlashAnimation(SpriteComponent sprite, Angle spriteRotation, CombatAnimationPrototype animPrototype)
    {
        var startRotation = sprite.Rotation + Angle.FromDegrees(animPrototype.AngleStart);
        var endRotation = startRotation + Angle.FromDegrees(animPrototype.AngleEnd);
        spriteRotation = Angle.FromDegrees(135f); // TODO вынести в логику
        startRotation += spriteRotation;
        endRotation += spriteRotation; // TODO вынести либо в функции, либо избавиться
        return new Animation
        {
            Length = TimeSpan.FromSeconds(animPrototype.AnimationDuration),
            AnimationTracks =
            {
                new AnimationTrackComponentProperty()
                {
                    ComponentType = typeof(SpriteComponent),
                    Property = nameof(SpriteComponent.Rotation),
                    KeyFrames =
                    {
                        new AnimationTrackProperty.KeyFrame(startRotation, 0f),
                        new AnimationTrackProperty.KeyFrame(endRotation, animPrototype.AnimationDuration)
                    }
                }, //TODO вынести в говорящие функции
                new AnimationTrackComponentProperty()
                {
                    ComponentType = typeof(SpriteComponent),
                    Property = nameof(SpriteComponent.Offset),
                    KeyFrames = GetArcKeyFrames(sprite.Rotation, animPrototype, 10, 0.5f)
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

    private List<AnimationTrackProperty.KeyFrame> GetArcKeyFrames(Angle startAngle, CombatAnimationPrototype animPrototype, int countOfPoint, float offsetRad)
    {
        var length = animPrototype.AnimationDuration;
        List<AnimationTrackProperty.KeyFrame> frames = [];

        var startRotation = startAngle + Angle.FromDegrees(animPrototype.AngleStart);
        var startRotationOffset = startRotation.RotateVec(new Vector2(0f, offsetRad));

        frames.Add(new AnimationTrackProperty.KeyFrame(startRotationOffset, 0f));

        float timeSection = 1f / (countOfPoint - 1);

        float absSumOfAngles = Math.Abs(animPrototype.AngleEnd) + Math.Abs(animPrototype.AngleStart);
        float angleStep = absSumOfAngles / countOfPoint;

        for (float i = 1; i < countOfPoint; i++)
        {
            float stepMultiple = i / (countOfPoint - 1);
            var rotation = startAngle + Angle.FromDegrees(animPrototype.AngleStart + angleStep * i);
            var rotationOffset = rotation.RotateVec(new Vector2(0f, 0.5f));
            frames.Add(new AnimationTrackProperty.KeyFrame(rotationOffset, length / countOfPoint));
        }

        return frames;
    }
}
