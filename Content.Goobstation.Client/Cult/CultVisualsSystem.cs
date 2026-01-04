using Content.Goobstation.Shared.Cult;
using Content.Shared.Humanoid;
using Robust.Client.GameObjects;
using Robust.Shared.Random;
using Robust.Shared.Utility;

namespace Content.Goobstation.Client.Cult;
public sealed partial class CultVisualsSystem : EntitySystem
{
    [Dependency] private readonly SpriteSystem _sprite = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BloodCultVisualEyesComponent, ComponentStartup>(OnCultEyesAdded);
        SubscribeLocalEvent<BloodCultVisualEyesComponent, ComponentShutdown>(OnCultEyesRemoved);
        SubscribeLocalEvent<BloodCultVisualHaloComponent, ComponentStartup>(OnCultHaloAdded);
        SubscribeLocalEvent<BloodCultVisualHaloComponent, ComponentShutdown>(OnCultHaloRemoved);
    }

    private void OnCultEyesAdded(Entity<BloodCultVisualEyesComponent> ent, ref ComponentStartup args)
    {
        if (!TryComp<HumanoidAppearanceComponent>(ent, out var huac))
            return;

        ent.Comp.LastEyeColor = huac.EyeColor;
        huac.EyeColor = ent.Comp.EyeColor;
    }

    private void OnCultEyesRemoved(Entity<BloodCultVisualEyesComponent> ent, ref ComponentShutdown args)
    {
        if (!TryComp<HumanoidAppearanceComponent>(ent, out var huac) || ent.Comp.LastEyeColor == null)
            return;

        huac.EyeColor = (Color) ent.Comp.LastEyeColor;
    }

    private void OnCultHaloAdded(Entity<BloodCultVisualHaloComponent> ent, ref ComponentStartup args)
    {
        if (!TryComp<SpriteComponent>(ent, out var sprite) || _sprite.LayerMapTryGet(ent.Owner, BloodCultHaloKey.Key, out _, false))
            return;

        var halo = new SpriteSpecifier.Rsi(ent.Comp.Sprite, $"halo{_random.Next(0, 6)}");
        var layer = _sprite.AddRsiLayer((ent, sprite), halo.RsiState, halo.RsiPath);
        _sprite.LayerMapSet(ent.Owner, BloodCultHaloKey.Key, layer);
        sprite.LayerSetShader(layer, "unshaded");
    }

    private void OnCultHaloRemoved(Entity<BloodCultVisualHaloComponent> ent, ref ComponentShutdown args)
    {
        _sprite.LayerMapRemove(ent.Owner, BloodCultHaloKey.Key);
    }
}
