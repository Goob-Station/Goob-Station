using Content.Shared._CorvaxGoob.QuantumTelepad;
using JetBrains.Annotations;
using Robust.Client.Animations;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;

namespace Content.Client._CorvaxGoob.QuantumTelepad;

public sealed class QuantumTelepadSystem : EntitySystem
{
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly SpriteSystem _sprite = default!;
    [Dependency] private readonly AnimationPlayerSystem _player = default!;

    private static readonly Robust.Client.Animations.Animation TelepadBeamAnimation = new()
    {
        Length = TimeSpan.FromSeconds(0.5),
        AnimationTracks =
        {
            new AnimationTrackSpriteFlick
            {
                LayerKey = TelepadLayers.Beam,
                KeyFrames =
                {
                    new AnimationTrackSpriteFlick.KeyFrame(new RSI.StateId("qpad-beam"), 0f)
                }
            }
        }
    };

    private const string TelepadBeamKey = "quantum-telepad-beam";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<QuantumTelepadComponent, AppearanceChangeEvent>(OnAppearance);
        SubscribeLocalEvent<QuantumTelepadComponent, AnimationCompletedEvent>(OnAnimComplete);
    }

    private void OnAppearance(Entity<QuantumTelepadComponent> entity, ref AppearanceChangeEvent args)
    {
        OnChangeData(entity, args.Component, args.Sprite);
    }

    private void OnAnimComplete(Entity<QuantumTelepadComponent> entity, ref AnimationCompletedEvent args)
    {
        if (!TryComp<AppearanceComponent>(entity, out var appearance))
            return;

        OnChangeData(entity, appearance, null);
    }

    private void OnChangeData(EntityUid uid, AppearanceComponent component, SpriteComponent? sprite = null)
    {
        if (!Resolve(uid, ref sprite))
            return;

        if (!TryComp<AnimationPlayerComponent>(uid, out var player))
            return;

        _appearance.TryGetData<QuantumTelepadState?>(uid, QuantumTelepadVisuals.State, out var state, component);

        if (state == QuantumTelepadState.ReceiveTeleporting)
            state = QuantumTelepadState.Teleporting;

        switch (state)
        {
            case QuantumTelepadState.Teleporting:
                if (!_player.HasRunningAnimation(uid, TelepadBeamKey))
                {
                    _sprite.LayerSetVisible((uid, sprite), TelepadLayers.Beam, true);
                    _sprite.LayerSetVisible((uid, sprite), TelepadLayers.Idle, false);
                    _player.Play((uid, player), TelepadBeamAnimation, TelepadBeamKey);
                }
                break;
            case QuantumTelepadState.Unlit:
                _sprite.LayerSetVisible((uid, sprite), TelepadLayers.Beam, false);
                _sprite.LayerSetVisible((uid, sprite), TelepadLayers.Idle, false);
                _player.Stop(uid, player, TelepadBeamKey);
                break;
            default:
                _sprite.LayerSetVisible((uid, sprite), TelepadLayers.Beam, false);
                _sprite.LayerSetVisible((uid, sprite), TelepadLayers.Idle, true);
                break;
        }
    }

    [UsedImplicitly]
    private enum TelepadLayers : byte
    {
        Idle,
        Beam,
    }
}
