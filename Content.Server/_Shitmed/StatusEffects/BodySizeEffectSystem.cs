using Content.Shared._EinsteinEngines.HeightAdjust;
using Content.Shared._Shitmed.StatusEffects;
using Robust.Shared.Random;
using System.Numerics;

namespace Content.Server._Shitmed.StatusEffects;

public sealed class BodySizeEffectSystem : EntitySystem
{
    [Dependency] private readonly HeightAdjustSystem _height = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RandomBodySizeComponent, ComponentInit>(OnInit);
    }

    private void OnInit(Entity<RandomBodySizeComponent> ent, ref ComponentInit args)
    {
        var width = RandomSize(ent.Comp.MinWidth, ent.Comp.MaxWidth);
        var height = RandomSize(ent.Comp.MinHeight, ent.Comp.MaxHeight);

        _height.SetScale(ent.Owner, new Vector2(width, height));
    }

    private float RandomSize(float min, float max)
    {
        return _random.NextFloat(min, max + 1);
    }
}
