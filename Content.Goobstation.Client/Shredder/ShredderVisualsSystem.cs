using Content.Goobstation.Shared.Shredder;
using Content.Shared.Fax;
using Robust.Client.Animations;
using Robust.Client.GameObjects;

namespace Content.Goobstation.Client.Shredder;

public sealed class ShredderVisualsSystem : EntitySystem
{
    [Dependency] private readonly AnimationPlayerSystem _player = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<ShredderComponent, AppearanceChangeEvent>(OnAppearanceChanged);
    }

    private void OnAppearanceChanged(Entity<ShredderComponent> ent, ref AppearanceChangeEvent args)
    {
        if (args.Sprite == null)
            return;

        if (_player.HasRunningAnimation(ent.Owner, "Shred"))
            return;

        if (_appearance.TryGetData(ent.Owner, ShredderVisuals.VisualState, out ShredderVisualsState visuals) &&
            visuals == ShredderVisualsState.Shredding)
        {
            _player.Play(ent.Owner,
                new Animation
                {
                    Length = TimeSpan.FromSeconds(5.7),
                    AnimationTracks =
                    {
                        new AnimationTrackSpriteFlick
                        {
                            LayerKey = FaxMachineVisuals.VisualState,
                            KeyFrames =
                            {
                                new AnimationTrackSpriteFlick.KeyFrame(ent.Comp.ShreddingState, 0f),
                                new AnimationTrackSpriteFlick.KeyFrame("icon", 5.6f),
                            },
                        },
                    },
                },
                "Shred");
        }
    }
}
