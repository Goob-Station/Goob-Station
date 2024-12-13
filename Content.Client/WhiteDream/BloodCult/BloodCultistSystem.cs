using System.Numerics;
using Content.Shared.StatusIcon.Components;
using Content.Shared.WhiteDream.BloodCult;
using Content.Shared.WhiteDream.BloodCult.BloodCultist;
using Content.Shared.WhiteDream.BloodCult.Components;
using Content.Shared.WhiteDream.BloodCult.Constructs;
using Robust.Client.GameObjects;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Utility;

namespace Content.Client.WhiteDream.BloodCult;

public sealed class BloodCultistSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<PentagramComponent, ComponentStartup>(OnPentagramAdded);
        SubscribeLocalEvent<PentagramComponent, ComponentShutdown>(OnPentagramRemoved);

        SubscribeLocalEvent<ConstructComponent, GetStatusIconsEvent>(GetConstructIcon);
        SubscribeLocalEvent<BloodCultistComponent, GetStatusIconsEvent>(GetCultistIcon);
        SubscribeLocalEvent<BloodCultLeaderComponent, GetStatusIconsEvent>(GetCultLeaderIcon);
    }

    private void OnPentagramAdded(EntityUid uid, PentagramComponent component, ComponentStartup args)
    {
        if (!TryComp<SpriteComponent>(uid, out var sprite) || sprite.LayerMapTryGet(PentagramKey.Key, out _))
            return;

        var adj = sprite.Bounds.Height / 2 + 1.0f / 32 * 10.0f;

        var randomState = _random.Pick(component.States);

        var layer = sprite.AddLayer(new SpriteSpecifier.Rsi(component.RsiPath, randomState));

        sprite.LayerMapSet(PentagramKey.Key, layer);
        sprite.LayerSetOffset(layer, new Vector2(0.0f, adj));
    }

    private void OnPentagramRemoved(EntityUid uid, PentagramComponent component, ComponentShutdown args)
    {
        if (!TryComp<SpriteComponent>(uid, out var sprite) || !sprite.LayerMapTryGet(PentagramKey.Key, out var layer))
            return;

        sprite.RemoveLayer(layer);
    }

    private void GetConstructIcon(Entity<ConstructComponent> ent, ref GetStatusIconsEvent args)
    {
        if (_prototype.TryIndex(ent.Comp.StatusIcon, out var iconPrototype))
            args.StatusIcons.Add(iconPrototype);
    }
    private void GetCultistIcon(Entity<BloodCultistComponent> ent, ref GetStatusIconsEvent args)
    {
        if (HasComp<BloodCultLeaderComponent>(ent))
            return;

        if (_prototype.TryIndex(ent.Comp.StatusIcon, out var iconPrototype))
            args.StatusIcons.Add(iconPrototype);
    }
    private void GetCultLeaderIcon(Entity<BloodCultLeaderComponent> ent, ref GetStatusIconsEvent args)
    {
        if (_prototype.TryIndex(ent.Comp.StatusIcon, out var iconPrototype))
            args.StatusIcons.Add(iconPrototype);
    }
}
