using System.Linq;
using Content.Goobstation.Shared.Shizophrenia;
using Content.Shared.Humanoid;
using Content.Shared.StatusIcon;
using Content.Shared.StatusIcon.Components;
using Robust.Client.Audio;
using Robust.Client.GameObjects;
using Robust.Client.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Goobstation.Client.Shizophrenia;

public sealed class ShizophreniaSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly SpriteSystem _sprite = default!;

    private Dictionary<NetEntity, TimeSpan> _layers = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeNetworkEvent<SetHallucinationAppearanceMessage>(OnAppearanceMessage);
        SubscribeLocalEvent<SchizophreniaComponent, GetStatusIconsEvent>(OnGetStatusIcons);
    }

    private void OnAppearanceMessage(SetHallucinationAppearanceMessage args)
    {
        if (_player.LocalSession == null)
            return;

        // Get target entity
        var ents = EntityManager.AllEntities<HumanoidAppearanceComponent>().Where(x => x.Owner != _player.LocalEntity && !HasComp<HallucinationComponent>(x)).ToList();
        var selected = _random.Pick(ents);

        var proto = _random.Pick(args.Appearance.Prototypes);
        var item = Spawn(proto);

        if (!TryComp<SpriteComponent>(selected, out var sprite) || !TryComp<SpriteComponent>(item, out var itemSprite))
            return;

        var state = _random.Pick(args.Appearance.States);

        // Ensure that given prototype sprite contains our state
        var rsi = itemSprite.BaseRSI;
        if (rsi == null || !rsi.TryGetState(state, out _))
            return;

        // Build layer
        var layer = new PrototypeLayerData();
        layer.RsiPath = rsi.Path.ToString();
        layer.State = state;

        // Set layer and play sound
        _sprite.LayerMapReserve(selected.Owner, "hallucination");
        _sprite.LayerSetData(selected.Owner, "hallucination", layer);
        if (_layers.TryAdd(GetNetEntity(selected), _timing.CurTime + TimeSpan.FromSeconds(5)))
            _audio.PlayEntity(args.Appearance.Sound, _player.LocalSession, selected);

        QueueDel(item);
    }

    private void OnGetStatusIcons(Entity<SchizophreniaComponent> ent, ref GetStatusIconsEvent args)
    {
        if (TryComp<HallucinationComponent>(_player.LocalEntity, out var hallucination) && hallucination.Idx == ent.Comp.Idx)
            args.StatusIcons.Add(_prototypeManager.Index<FactionIconPrototype>("ShizophrenicIcon"));
    }

    public override void FrameUpdate(float frameTime)
    {
        base.FrameUpdate(frameTime);

        foreach (var item in _layers.ToDictionary())
        {
            if (item.Value > _timing.CurTime)
                continue;

            if (!TryGetEntity(item.Key, out var ent))
                continue;

            _sprite.RemoveLayer(ent.Value, "hallucination", false);
            _layers.Remove(item.Key);
        }
    }
}
