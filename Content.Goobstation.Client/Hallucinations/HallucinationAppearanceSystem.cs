using Content.Goobstation.Shared.Hallucinations;
using Content.Shared.Humanoid;
using Robust.Client.GameObjects;
using Robust.Client.Player;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Goobstation.Client.Hallucinations;

/// <summary>
/// Client half of the hallucinations.
/// </summary>
public sealed class HallucinationAppearanceSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SpriteSystem _sprite = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    private const string LayerKey = "hallucination-appearance";
    private const string MorphPrefix = "hallucination-morph-";
    private const float SearchRange = 10f;
    private static readonly TimeSpan Lifetime = TimeSpan.FromSeconds(15);

    private readonly Dictionary<EntityUid, Fake> _fakes = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeNetworkEvent<SetHallucinationAppearanceMessage>(OnAppearanceMessage);
        SubscribeNetworkEvent<SetHallucinationMorphMessage>(OnMorphMessage);
    }

    private void OnAppearanceMessage(SetHallucinationAppearanceMessage args)
    {
        if (_player.LocalEntity is not { } localPlayer
            || args.Prototypes.Count == 0
            || args.States.Count == 0
            || !TryPickTarget(localPlayer, out var target))
            return;

        if (!_proto.TryIndex<EntityPrototype>(_random.Pick(args.Prototypes), out var proto)
            || !proto.TryGetComponent<SpriteComponent>(out var itemSprite, EntityManager.ComponentFactory)
            || itemSprite.BaseRSI is not { } rsi)
            return;

        var state = _random.Pick(args.States);
        if (!rsi.TryGetState(state, out _))
            return;

        _sprite.LayerMapReserve(target, LayerKey);
        _sprite.LayerSetData(target, LayerKey, new PrototypeLayerData { RsiPath = rsi.Path.ToString(), State = state });
        _fakes[target] = new Fake { End = _timing.CurTime + Lifetime, Added = { LayerKey } };

        if (_player.LocalSession != null)
            _audio.PlayEntity(args.Sound, _player.LocalSession, target);
    }

    private void OnMorphMessage(SetHallucinationMorphMessage args)
    {
        if (_player.LocalEntity is not { } localPlayer
            || args.Mobs.Count == 0
            || !_proto.TryIndex<EntityPrototype>(_random.Pick(args.Mobs), out var proto)
            || !proto.TryGetComponent<SpriteComponent>(out var mobSprite, EntityManager.ComponentFactory)
            || !TryPickTarget(localPlayer, out var target)
            || !TryComp<SpriteComponent>(target, out var sprite))
            return;

        var hidden = new List<int>();
        var index = 0;
        foreach (var layer in sprite.AllLayers)
        {
            if (layer.Visible)
                hidden.Add(index);
            index++;
        }

        var fake = new Fake { End = _timing.CurTime + Lifetime, Hidden = hidden };
        var next = 0;
        foreach (var layer in mobSprite.AllLayers)
        {
            if (layer.ActualRsi is not { } rsi || layer.RsiState.Name is not { } state)
                continue;

            var key = MorphPrefix + next++;
            _sprite.LayerMapReserve((target, sprite), key);
            _sprite.LayerSetData((target, sprite), key, new PrototypeLayerData { RsiPath = rsi.Path.ToString(), State = state });
            fake.Added.Add(key);
        }

        if (fake.Added.Count == 0)
            return;

        foreach (var i in hidden)
            _sprite.LayerSetVisible((target, sprite), i, false);

        _fakes[target] = fake;
    }

    private bool TryPickTarget(EntityUid localPlayer, out EntityUid target)
    {
        target = default;

        var playerCoords = _transform.GetMapCoordinates(localPlayer);
        var candidates = new List<EntityUid>();

        var query = EntityQueryEnumerator<HumanoidAppearanceComponent, SpriteComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out _, out _, out var xform))
        {
            if (uid == localPlayer || _fakes.ContainsKey(uid))
                continue;

            var coords = _transform.GetMapCoordinates(uid, xform);
            if (coords.MapId != playerCoords.MapId
                || (coords.Position - playerCoords.Position).LengthSquared() > SearchRange * SearchRange)
                continue;

            candidates.Add(uid);
        }

        if (candidates.Count == 0)
            return false;

        target = _random.Pick(candidates);
        return true;
    }

    public override void FrameUpdate(float frameTime)
    {
        base.FrameUpdate(frameTime);

        if (_fakes.Count == 0)
            return;

        var expired = new List<EntityUid>();
        foreach (var (uid, fake) in _fakes)
            if (fake.End <= _timing.CurTime)
                expired.Add(uid);

        foreach (var uid in expired)
        {
            var fake = _fakes[uid];
            _fakes.Remove(uid);
            Restore(uid, fake);
        }
    }

    private void Restore(EntityUid uid, Fake fake)
    {
        if (TerminatingOrDeleted(uid) || !TryComp<SpriteComponent>(uid, out var sprite))
            return;

        foreach (var key in fake.Added)
            _sprite.RemoveLayer((uid, sprite), key, false);

        foreach (var index in fake.Hidden)
            _sprite.LayerSetVisible((uid, sprite), index, true);
    }

    /// <summary>
    /// Layers added (to remove) and hidden (to show again).
    /// </summary>
    private sealed class Fake
    {
        public TimeSpan End;
        public List<string> Added = new();
        public List<int> Hidden = new();
    }
}
