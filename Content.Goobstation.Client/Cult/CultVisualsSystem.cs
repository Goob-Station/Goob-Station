using Content.Goobstation.Shared.Cult;
using Content.Shared.StatusIcon.Components;
using Robust.Client.GameObjects;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Utility;

namespace Content.Goobstation.Client.Cult;
public sealed partial class CultVisualsSystem : EntitySystem
{
    [Dependency] private readonly SpriteSystem _sprite = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IPrototypeManager _prot = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BloodCultistComponent, GetStatusIconsEvent>(OnGetStatusIcons);
        SubscribeLocalEvent<BloodCultistLeaderComponent, GetStatusIconsEvent>(OnLeaderGetStatusIcons);

        SubscribeLocalEvent<BloodCultVisualHaloComponent, ComponentStartup>(OnCultHaloAdded);
        SubscribeLocalEvent<BloodCultVisualHaloComponent, ComponentShutdown>(OnCultHaloRemoved);
    }

    private void OnGetStatusIcons(Entity<BloodCultistComponent> ent, ref GetStatusIconsEvent args)
    {
        if (HasComp<BloodCultistLeaderComponent>(ent))
            return;

        if (_prot.TryIndex(ent.Comp.StatusIcon, out var iconPrototype))
            args.StatusIcons.Add(iconPrototype);
    }

    private void OnLeaderGetStatusIcons(Entity<BloodCultistLeaderComponent> ent, ref GetStatusIconsEvent args)
    {
        if (_prot.TryIndex(ent.Comp.StatusIcon, out var iconPrototype))
            args.StatusIcons.Add(iconPrototype);
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
