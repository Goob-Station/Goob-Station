// SPDX-FileCopyrightText: 2026 Janet Blackquill <uhhadd@gmail.com>
//
// SPDX-License-Identifier: LicenseRef-Wallening

using System.Numerics;
using Content.Goobstation.Common.CCVar;
using Content.Shared._ST.Interaction;
using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Prototypes;
using Content.Shared.Stealth.Components;
using Robust.Client.Animations;
using Robust.Client.GameObjects;
using Robust.Shared.Network;
using Robust.Shared.Configuration;
using Robust.Shared.Containers;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Client._ST.Interaction;

public sealed partial class StellarInteractionParticleSystem : EntitySystem
{
    [Dependency] private IRobustRandom _random = default!;
    [Dependency] private SpriteSystem _sprite = default!;
    [Dependency] private AppearanceSystem _appearance = default!;
    [Dependency] private AnimationPlayerSystem _animation = default!;
    [Dependency] private TransformSystem _xform = default!;
    [Dependency] private IConfigurationManager _cfg = default!;
    [Dependency] private SharedContainerSystem _container = default!;
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private IClientNetManager _net = default!;
    [Dependency] private IPrototypeManager _proto = default!;

    private static readonly TimeSpan ConfirmGrace = TimeSpan.FromMilliseconds(400);
    private static readonly TimeSpan PendingLifetime = TimeSpan.FromSeconds(1);

    private readonly List<PendingParticle> _pending = new();
    private readonly HashSet<(EntityUid, EntityUid)> _shownThisTick = new();
    private GameTick _shownTick;

    private float _inHandScale = 1f;

    private const string AnimateKey = "particle-animation";

    private static readonly Dictionary<StellarInteractionParticleType, EntProtoId> InteractionParticleIds = new ()
    {
        { StellarInteractionParticleType.Use, "StellarInteractionParticleUse" },
        { StellarInteractionParticleType.Pull, "StellarInteractionParticlePull" },
        { StellarInteractionParticleType.InHand, "StellarInteractionParticleUse" },
    };

    public override void Initialize()
    {
        base.Initialize();

        SubscribeAllEvent<StellarInteractionParticleEvent>(OnInteractionParticle);
        Subs.CVar(_cfg, GoobCVars.InHandInteractionParticleScale, v => _inHandScale = v, true);
    }

    public override void FrameUpdate(float frameTime)
    {
        base.FrameUpdate(frameTime);

        var now = _timing.RealTime;
        for (var i = _pending.Count - 1; i >= 0; i--)
        {
            var pending = _pending[i];
            if (pending.Particle is { } particle && now > pending.Deadline)
            {
                QueueDel(particle);
                pending.Particle = null;
            }

            if (now > pending.Expiry)
                _pending.RemoveAt(i);
        }
    }

    private void OnInteractionParticle(StellarInteractionParticleEvent ev)
    {
        var performer = GetEntity(ev.Performer);
        var target = GetEntity(ev.Target);

        if (_timing.CurTick != _shownTick)
        {
            _shownTick = _timing.CurTick;
            _shownThisTick.Clear();
        }

        if (!_shownThisTick.Add((performer, target)))
            return;

        if (!ev.IsClientEvent && TryConfirmPredicted(performer, target))
            return;

        var particle = SpawnParticle(ev, performer, target);

        if (!ev.IsClientEvent)
            return;

        var deadline = _timing.RealTime + TimeSpan.FromMilliseconds((_net.ServerChannel?.Ping ?? 0) * 2) + ConfirmGrace;
        _pending.Add(new PendingParticle(performer, target, particle, deadline, deadline + PendingLifetime));
    }

    private bool TryConfirmPredicted(EntityUid performer, EntityUid target)
    {
        for (var i = 0; i < _pending.Count; i++)
        {
            var pending = _pending[i];
            if (pending.Performer != performer || pending.Target != target)
                continue;

            _pending.RemoveAt(i);
            return true;
        }

        return false;
    }

    private EntityUid? SpawnParticle(StellarInteractionParticleEvent ev, EntityUid performer, EntityUid target)
    {
        var used = GetEntity(ev.Used);

        if (!Exists(performer) || !Exists(target))
            return null;

        if (TryComp<StealthComponent>(performer, out var stealth) && stealth.Enabled)
            return null;

        var type = ev.Type;
        if (type == StellarInteractionParticleType.Pull)
        {
            (performer, target) = (target, performer);
        }

        var performerXform = Transform(performer);
        var targetXform = Transform(target);
        if (performerXform.MapID == MapId.Nullspace || targetXform.MapID == MapId.Nullspace)
            return null;

        if (_container.IsEntityOrParentInContainer(performer, xform: performerXform))
            return null;

        if (_container.IsEntityOrParentInContainer(target, xform: targetXform))
        {
            if (type == StellarInteractionParticleType.Pull)
                return null;

            type = StellarInteractionParticleType.InHand;
        }

        if (type == StellarInteractionParticleType.InHand && _inHandScale <= 0f)
            return null;

        if ((performerXform.GridUid ?? performerXform.MapUid) is not { } frame)
            return null;

        var startPos = _xform.ToCoordinates(frame, _xform.GetMapCoordinates(performer, performerXform)).Position;
        var endPos = _xform.ToCoordinates(frame, _xform.GetMapCoordinates(target, targetXform)).Position;
        var inHandDelta = new Vector2(0, 0.85f);
        var particle = Spawn(InteractionParticleIds[type], new EntityCoordinates(frame, startPos));

        if (type == StellarInteractionParticleType.InHand)
        {
            used = target;
            _xform.SetParent(particle, performer);
        }

        if (used is { } usedEntity && Exists(usedEntity) && TryComp<SpriteComponent>(usedEntity, out var usedSprite))
        {
            // Force appearance update in case we changed the appearance after spawning.
            _appearance.OnChangeData(usedEntity, usedSprite);

            _sprite.CopySprite((usedEntity, usedSprite), particle);
            _sprite.SetDrawDepth(particle, (int) Shared.DrawDepth.DrawDepth.Effects);
        }
        else if (type == StellarInteractionParticleType.Use && GetHandSprite(performer) is { } handSprite)
        {
            _sprite.LayerSetSprite(particle, 0, handSprite);
        }

        var sprite = Comp<SpriteComponent>(particle);
        sprite.NoRotation = true;
        var spriteColor = sprite.Color;
        var animation = type switch
        {
            StellarInteractionParticleType.Use => GetUseAnimation(startPos, endPos, spriteColor),
            StellarInteractionParticleType.Pull => GetPullAnimation(startPos, endPos, spriteColor),
            StellarInteractionParticleType.InHand => GetUseAnimation(Vector2.Zero, inHandDelta, spriteColor, true, _inHandScale),
            _ => throw new ArgumentOutOfRangeException(nameof(ev), $"Interaction particle event has unknown particle type {type}"),
        };
        _animation.Play(particle, animation, AnimateKey);
        return particle;
    }

    private SpriteSpecifier? GetHandSprite(EntityUid performer)
    {
        if (!TryComp<HumanoidAppearanceComponent>(performer, out var humanoid)
            || !_proto.TryIndex(humanoid.Species, out var species))
            return null;

        return species.InteractionHandSprite;
    }

    private Animation GetUseAnimation(Vector2 startPosition, Vector2 endPosition, Color color, bool spriteOffset = false, float scale = 1f)
    {
        var startRotation = _random.NextAngle(Angle.FromDegrees(-40), Angle.FromDegrees(40));
        var endRotation = Angle.Zero;
        var startScale = new Vector2(0.3f, 0.3f) * scale;
        var endScale = new Vector2(1f, 1f) * scale;
        var rotationLength = TimeSpan.FromMilliseconds(600);

        var offsetLength = TimeSpan.FromMilliseconds(250);

        var startColor = color.WithAlpha(color.A * 0.7f);
        var endColor = color.WithAlpha(0f);
        var colorLength = rotationLength + offsetLength;

        // use anim lerps transform local position
        // but inhand just lerps sprite offset (since it gets parented to the performer)
        var posTrack = spriteOffset
            ? new AnimationTrackComponentProperty()
            {
                ComponentType = typeof(SpriteComponent),
                Property = nameof(SpriteComponent.Offset),
                KeyFrames =
                {
                    new AnimationTrackProperty.KeyFrame(startPosition, 0f),
                    new AnimationTrackProperty.KeyFrame(endPosition, (float)offsetLength.TotalSeconds, Easings.OutBack),
                },
            }
            : new AnimationTrackComponentProperty()
            {
                ComponentType = typeof(TransformComponent),
                Property = nameof(TransformComponent.LocalPosition),
                KeyFrames =
                {
                    new AnimationTrackProperty.KeyFrame(startPosition, 0f),
                    new AnimationTrackProperty.KeyFrame(endPosition, (float)offsetLength.TotalSeconds, Easings.OutBack),
                },
            };

        return new Animation()
        {
            Length = colorLength,

            AnimationTracks =
            {
                new AnimationTrackComponentProperty()
                {
                    ComponentType = typeof(SpriteComponent),
                    Property = nameof(SpriteComponent.Rotation),
                    KeyFrames =
                    {
                        new AnimationTrackProperty.KeyFrame(startRotation, 0f),
                        new AnimationTrackProperty.KeyFrame(endRotation, (float)rotationLength.TotalSeconds, Easings.OutBack),
                    },
                },
                new AnimationTrackComponentProperty()
                {
                    ComponentType = typeof(SpriteComponent),
                    Property = nameof(SpriteComponent.Scale),
                    KeyFrames =
                    {
                        new AnimationTrackProperty.KeyFrame(startScale, 0f),
                        new AnimationTrackProperty.KeyFrame(endScale, (float)rotationLength.TotalSeconds, Easings.OutBack),
                    },
                },
                new AnimationTrackComponentProperty()
                {
                    ComponentType = typeof(SpriteComponent),
                    Property = nameof(SpriteComponent.Color),
                    KeyFrames =
                    {
                        new AnimationTrackProperty.KeyFrame(startColor, 0f),
                        new AnimationTrackProperty.KeyFrame(startColor, (float)rotationLength.TotalSeconds),
                        new AnimationTrackProperty.KeyFrame(endColor, (float)offsetLength.TotalSeconds, Easings.InOutCirc),
                    },
                },
                posTrack
            },
        };
    }

    private Animation GetPullAnimation(Vector2 startPosition, Vector2 endPosition, Color color)
    {
        var rotationLength = TimeSpan.FromMilliseconds(8f * (1000f / 12f));

        var offsetLength = TimeSpan.FromMilliseconds(4f * (1000f / 12f));

        var endColor = color.WithAlpha(0f);

        return new Animation
        {
            Length = rotationLength,

            AnimationTracks =
            {
                new AnimationTrackComponentProperty()
                {
                    ComponentType = typeof(TransformComponent),
                    Property = nameof(TransformComponent.LocalPosition),
                    KeyFrames =
                    {
                        new AnimationTrackProperty.KeyFrame(startPosition, 0f),
                        new AnimationTrackProperty.KeyFrame(endPosition, (float)rotationLength.TotalSeconds, Easings.InOutCirc),
                    },
                },
                new AnimationTrackComponentProperty()
                {
                    ComponentType = typeof(SpriteComponent),
                    Property = nameof(SpriteComponent.Color),
                    KeyFrames =
                    {
                        new AnimationTrackProperty.KeyFrame(color, 0f),
                        new AnimationTrackProperty.KeyFrame(color, (float)offsetLength.TotalSeconds),
                        new AnimationTrackProperty.KeyFrame(endColor, (float)rotationLength.TotalSeconds, Easings.InOutCirc),
                    },
                },
            },
        };
    }

    private sealed class PendingParticle(EntityUid performer, EntityUid target, EntityUid? particle, TimeSpan deadline, TimeSpan expiry)
    {
        public readonly EntityUid Performer = performer;
        public readonly EntityUid Target = target;
        public EntityUid? Particle = particle;
        public readonly TimeSpan Deadline = deadline;
        public readonly TimeSpan Expiry = expiry;
    }
}
