using Robust.Shared.Prototypes;
using Robust.Shared.Map;
using Robust.Client.GameObjects;
using Content.Shared.Repairable;
using Content.Shared._FarHorizons.Power.Generation.FissionGenerator;
using Content.Client.Popups;
using Content.Client.Examine;
using Content.Goobstation.Shared.Power._FarHorizons.Power.Generation.FissionGenerator;
using Robust.Client.Animations;

namespace Content.Client._FarHorizons.Power.Generation.FissionGenerator;

// Ported and modified from goonstation by Jhrushbe.
// CC-BY-NC-SA-3.0
// https://github.com/goonstation/goonstation/blob/ff86b044/code/obj/nuclearreactor/turbine.dm

public sealed class TurbineSystem : SharedTurbineSystem
{
    [Dependency] private readonly PopupSystem _popupSystem = default!;
    [Dependency] private readonly UserInterfaceSystem _userInterfaceSystem = default!;
    [Dependency] private readonly AnimationPlayerSystem _animationPlayer = default!;
    [Dependency] private readonly SpriteSystem _sprite = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TurbineComponent, ClientExaminedEvent>(TurbineExamined);

        SubscribeLocalEvent<TurbineComponent, AppearanceChangeEvent>(OnAppearanceChange);
        SubscribeLocalEvent<TurbineComponent, AnimationCompletedEvent>(OnAnimationCompleted);
    }

    protected override void OnRepairTurbineFinished(Entity<TurbineComponent> ent, ref RepairFinishedEvent args)
    {
        if (args.Cancelled)
            return;

        if (!TryComp(ent.Owner, out TurbineComponent? comp))
            return;

        _popupSystem.PopupClient(Loc.GetString("turbine-repair", ("target", ent.Owner), ("tool", args.Used!)), ent.Owner, args.User);
    }

    private void TurbineExamined(EntityUid uid, TurbineComponent comp, ClientExaminedEvent args) => Spawn(comp.ArrowPrototype, new EntityCoordinates(uid, 0, 0));

    private void OnAnimationCompleted(Entity<TurbineComponent> ent, ref AnimationCompletedEvent args) => PlayAnimation(ent);

    private void OnAppearanceChange(Entity<TurbineComponent> ent, ref AppearanceChangeEvent args) => PlayAnimation(ent);

    protected override void AccUpdate()
    {
        var query = EntityQueryEnumerator<TurbineComponent>();
        while (query.MoveNext(out var uid, out var component))
        {
            // Makes sure the anim doesn't get stuck at low RPM
            if (Math.Abs(component.RPM - component.AnimRPM) > component.BestRPM * 0.1)
                PlayAnimation((uid, component));
        }
    }

    private void PlayAnimation(Entity<TurbineComponent> ent)
    {
        if (ent.Comp.RPM < 1)
            return;

        if (!TryComp<SpriteComponent>(ent.Owner, out var sprite) || !_sprite.TryGetLayer((ent.Owner,sprite), TurbineVisualLayers.TurbineSpeed, out var layer, false))
            return;

        var state = "speedanim";
        if (_animationPlayer.HasRunningAnimation(ent.Owner, state))
            _animationPlayer.Stop(ent.Owner, state);

        if (_animationPlayer.HasRunningAnimation(ent.Owner, state)) // Despite what you'd think, this is not always true
            return;

        ent.Comp.AnimRPM = ent.Comp.RPM;
        var layerKey = TurbineVisualLayers.TurbineSpeed;
        var time = 0.5f * ent.Comp.BestRPM / ent.Comp.RPM;
        var timestep = time/12;
        var animation = new Animation
        {
            Length = TimeSpan.FromSeconds(time),
            AnimationTracks =
            {
                new AnimationTrackSpriteFlick
                {
                    LayerKey = layerKey,
                    KeyFrames =
                    {
                        new AnimationTrackSpriteFlick.KeyFrame("turbinerun_00", 0),
                        new AnimationTrackSpriteFlick.KeyFrame("turbinerun_01", timestep),
                        new AnimationTrackSpriteFlick.KeyFrame("turbinerun_02", timestep),
                        new AnimationTrackSpriteFlick.KeyFrame("turbinerun_03", timestep),
                        new AnimationTrackSpriteFlick.KeyFrame("turbinerun_04", timestep),
                        new AnimationTrackSpriteFlick.KeyFrame("turbinerun_05", timestep),
                        new AnimationTrackSpriteFlick.KeyFrame("turbinerun_06", timestep),
                        new AnimationTrackSpriteFlick.KeyFrame("turbinerun_07", timestep),
                        new AnimationTrackSpriteFlick.KeyFrame("turbinerun_08", timestep),
                        new AnimationTrackSpriteFlick.KeyFrame("turbinerun_09", timestep),
                        new AnimationTrackSpriteFlick.KeyFrame("turbinerun_10", timestep),
                        new AnimationTrackSpriteFlick.KeyFrame("turbinerun_11", timestep)
                    }
                }
            }
        };
        _sprite.LayerSetVisible(layer, true);
        _animationPlayer.Play(ent.Owner, animation, state);
    }
}
