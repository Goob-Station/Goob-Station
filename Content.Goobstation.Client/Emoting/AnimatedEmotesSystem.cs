using System.Linq;
using System.Numerics;
using Content.Client.Animations;
using Content.Client.DamageState;
using Content.Goobstation.Shared.Emoting;
using Content.Shared._Goobstation.Wizard.SupermatterHalberd;
using Content.Shared.Chat.Prototypes;
using Robust.Client.Animations;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Shared.Animations;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Goobstation.Client.Emoting;

public sealed partial class AnimatedEmotesSystem : SharedAnimatedEmotesSystem
{
    [Dependency] private readonly AnimationPlayerSystem _anim = default!;
    [Dependency] private readonly IPrototypeManager _prot = default!;
    [Dependency] private readonly RaysSystem _rays = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly TransformSystem _transform = default!;

    private const int TweakAnimationDurationMs = 1100; // 11 frames * 100ms per frame

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AnimatedEmotesComponent, ComponentHandleState>(OnHandleState);

        SubscribeLocalEvent<AnimatedEmotesComponent, AnimationFlipEmoteEvent>(OnFlip);
        SubscribeLocalEvent<AnimatedEmotesComponent, AnimationSpinEmoteEvent>(OnSpin);
        SubscribeLocalEvent<AnimatedEmotesComponent, AnimationJumpEmoteEvent>(OnJump);
        SubscribeLocalEvent<AnimatedEmotesComponent, AnimationTweakEmoteEvent>(OnTweak);
        SubscribeNetworkEvent<BibleFartSmiteEvent>(OnBibleSmite);
    }

    public void OnBibleSmite(BibleFartSmiteEvent args)
    {
        EntityUid uid = GetEntity(args.Bible);
        if (!_timing.IsFirstTimePredicted || uid == EntityUid.Invalid)
            return;

        var rays = _rays.DoRays(_transform.GetMapCoordinates(uid),
            Color.LightGoldenrodYellow,
            Color.AntiqueWhite,
            10,
            15,
            minMaxRadius: new Vector2(3f, 6f),
            minMaxEnergy: new Vector2(2f, 4f),
            proto: "EffectRayCharge",
            server: false);

        if (rays == null)
            return;

        var track = EnsureComp<TrackUserComponent>(rays.Value);
        track.User = uid;
    }

    public void PlayEmote(EntityUid uid, Animation anim, string animationKey = "emoteAnimKeyId")
    {
        if (_anim.HasRunningAnimation(uid, animationKey))
            return;

        _anim.Play(uid, anim, animationKey);
    }

    private void OnHandleState(EntityUid uid, AnimatedEmotesComponent component, ref ComponentHandleState args)
    {
        if (args.Current is not AnimatedEmotesComponentState state
        || !_prot.TryIndex<EmotePrototype>(state.Emote, out var emote))
            return;

        if (emote.Event != null)
            RaiseLocalEvent(uid, emote.Event);
    }

    private void OnFlip(Entity<AnimatedEmotesComponent> ent, ref AnimationFlipEmoteEvent args)
    {
        var a = new Animation
        {
            Length = TimeSpan.FromMilliseconds(500),
            AnimationTracks =
            {
                new AnimationTrackComponentProperty
                {
                    ComponentType = typeof(SpriteComponent),
                    Property = nameof(SpriteComponent.Rotation),
                    InterpolationMode = AnimationInterpolationMode.Linear,
                    KeyFrames =
                    {
                        new AnimationTrackProperty.KeyFrame(Angle.Zero, 0f),
                        new AnimationTrackProperty.KeyFrame(Angle.FromDegrees(180), 0.25f),
                        new AnimationTrackProperty.KeyFrame(Angle.FromDegrees(360), 0.25f),
                    }
                }
            }
        };
        PlayEmote(ent, a);
    }
    private void OnSpin(Entity<AnimatedEmotesComponent> ent, ref AnimationSpinEmoteEvent args)
    {
        var a = new Animation
        {
            Length = TimeSpan.FromMilliseconds(600),
            AnimationTracks =
            {
                new AnimationTrackComponentProperty
                {
                    ComponentType = typeof(TransformComponent),
                    Property = nameof(TransformComponent.LocalRotation),
                    InterpolationMode = AnimationInterpolationMode.Linear,
                    KeyFrames =
                    {
                        new AnimationTrackProperty.KeyFrame(Angle.FromDegrees(0), 0f),
                        new AnimationTrackProperty.KeyFrame(Angle.FromDegrees(90), 0.075f),
                        new AnimationTrackProperty.KeyFrame(Angle.FromDegrees(180), 0.075f),
                        new AnimationTrackProperty.KeyFrame(Angle.FromDegrees(270), 0.075f),
                        new AnimationTrackProperty.KeyFrame(Angle.Zero, 0.075f),
                        new AnimationTrackProperty.KeyFrame(Angle.FromDegrees(90), 0.075f),
                        new AnimationTrackProperty.KeyFrame(Angle.FromDegrees(180), 0.075f),
                        new AnimationTrackProperty.KeyFrame(Angle.FromDegrees(270), 0.075f),
                        new AnimationTrackProperty.KeyFrame(Angle.Zero, 0.075f),
                    }
                }
            }
        };
        PlayEmote(ent, a, "emoteAnimSpin");
    }
    private void OnJump(Entity<AnimatedEmotesComponent> ent, ref AnimationJumpEmoteEvent args)
    {
        var a = new Animation
        {
            Length = TimeSpan.FromMilliseconds(250),
            AnimationTracks =
            {
                new AnimationTrackComponentProperty
                {
                    ComponentType = typeof(SpriteComponent),
                    Property = nameof(SpriteComponent.Offset),
                    InterpolationMode = AnimationInterpolationMode.Cubic,
                    KeyFrames =
                    {
                        new AnimationTrackProperty.KeyFrame(Vector2.Zero, 0f),
                        new AnimationTrackProperty.KeyFrame(new Vector2(0, .35f), 0.125f),
                        new AnimationTrackProperty.KeyFrame(Vector2.Zero, 0.125f),
                    }
                }
            }
        };
        PlayEmote(ent, a);
    }
    private void OnTweak(Entity<AnimatedEmotesComponent> ent, ref AnimationTweakEmoteEvent args)
    {
        NetEntity netEntity = EntityManager.GetNetEntity(ent.Owner);

        if (!EntityManager.TryGetEntityData(netEntity, out _, out var metaData))
        {
            var sawmill = Logger.GetSawmill("tweak-emotes");
            sawmill.Warning($"EntityPrototype is null for entity {netEntity}");
            return;
        }

        if (metaData.EntityPrototype == null)
        {
            var sawmill = Logger.GetSawmill("tweak-emotes");
            sawmill.Warning($"EntityPrototype is null for entity {netEntity} (Type: {metaData.EntityName})");
            return;
        }

        var stateNumber = string.Concat(metaData.EntityPrototype.ID.Where(Char.IsDigit));
        if (string.IsNullOrEmpty(stateNumber))
        {
            stateNumber = "0";
        }

        var a = new Animation
        {
            Length = TimeSpan.FromMilliseconds(TweakAnimationDurationMs),
            AnimationTracks =
            {
                new AnimationTrackSpriteFlick
                {
                    LayerKey = DamageStateVisualLayers.Base,
                    KeyFrames =
                    {
                        new AnimationTrackSpriteFlick.KeyFrame(new RSI.StateId($"{metaData.EntityPrototype.SetName}-tweaking-{stateNumber}"), 0f)
                    }
                }
            }
        };
        PlayEmote(ent, a);
    }
}
