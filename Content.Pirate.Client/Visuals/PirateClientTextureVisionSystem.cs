using System.Linq;
using Content.Pirate.Shared.Visuals;
using Content.Pirate.Shared.Visuals.Components;
using Content.Shared.Humanoid;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Robust.Client.GameObjects;
using Robust.Shared.Player;
using Robust.Shared.Maths;
using Robust.Shared.Utility;

namespace Content.Pirate.Client.Visuals;

// This only lies to the local client: nearby living mobs are redrawn as monsters, skeletons or food.
public sealed class PirateClientTextureVisionSystem : EntitySystem
{
    [Dependency] private readonly ISharedPlayerManager _player = default!;
    [Dependency] private readonly SpriteSystem _sprite = default!;

    private const string OverrideLayerPrefix = "pirate-texture-vision-";
    private const float UpdateInterval = 0.1f;

    private static readonly ResPath SkeletonRsi = new("Mobs/Species/Skeleton/parts.rsi");
    private static readonly Dictionary<HumanoidVisualLayers, string> SkeletonLayerStates = new()
    {
        { HumanoidVisualLayers.LLeg, "l_leg" },
        { HumanoidVisualLayers.RLeg, "r_leg" },
        { HumanoidVisualLayers.LFoot, "l_foot" },
        { HumanoidVisualLayers.RFoot, "r_foot" },
        { HumanoidVisualLayers.LArm, "l_arm" },
        { HumanoidVisualLayers.RArm, "r_arm" },
    };

    private static readonly OverrideLayerDefinition[] XenoVariants =
    [
        new(new SpriteSpecifier.Rsi(new ResPath("_White/Mobs/Aliens/Xenomorphs/drone.rsi"), "xenomorph")),
        new(new SpriteSpecifier.Rsi(new ResPath("_White/Mobs/Aliens/Xenomorphs/hunter.rsi"), "xenomorph")),
        new(new SpriteSpecifier.Rsi(new ResPath("_White/Mobs/Aliens/Xenomorphs/sentinel.rsi"), "xenomorph")),
        new(new SpriteSpecifier.Rsi(new ResPath("_White/Mobs/Aliens/Xenomorphs/praetorian.rsi"), "xenomorph")),
    ];

    private static readonly OverrideLayerDefinition[] FoodVariants =
    [
        new(new SpriteSpecifier.Rsi(new ResPath("Objects/Consumable/Food/burger.rsi"), "plain")),
        new(new SpriteSpecifier.Rsi(new ResPath("Objects/Consumable/Food/burger.rsi"), "cheese")),
        new(new SpriteSpecifier.Rsi(new ResPath("Objects/Consumable/Food/burger.rsi"), "chicken")),
        new(new SpriteSpecifier.Rsi(new ResPath("Objects/Consumable/Food/burger.rsi"), "human")),
        new(new SpriteSpecifier.Rsi(new ResPath("Objects/Consumable/Food/burger.rsi"), "clown")),
        new(new SpriteSpecifier.Rsi(new ResPath("Objects/Consumable/Food/snacks.rsi"), "chips")),
        new(new SpriteSpecifier.Rsi(new ResPath("Objects/Consumable/Food/snacks.rsi"), "popcorn")),
        new(new SpriteSpecifier.Rsi(new ResPath("Objects/Consumable/Food/snacks.rsi"), "raisins")),
        new(new SpriteSpecifier.Rsi(new ResPath("Objects/Consumable/Food/snacks.rsi"), "energybar")),
        new(new SpriteSpecifier.Rsi(new ResPath("Objects/Consumable/Food/snacks.rsi"), "boritos")),
    ];

    private static readonly HashSet<HumanoidVisualLayers> SkeletonHiddenBodyLayers =
    [
        HumanoidVisualLayers.Special,
        HumanoidVisualLayers.Tail,
        HumanoidVisualLayers.Wings,
        HumanoidVisualLayers.Hair,
        HumanoidVisualLayers.FacialHair,
        HumanoidVisualLayers.Face,
        HumanoidVisualLayers.Chest,
        HumanoidVisualLayers.UndergarmentBottom,
        HumanoidVisualLayers.UndergarmentTop,
        HumanoidVisualLayers.Groin,
        HumanoidVisualLayers.Head,
        HumanoidVisualLayers.Snout,
        HumanoidVisualLayers.HeadSide,
        HumanoidVisualLayers.HeadTop,
        HumanoidVisualLayers.TailBehind,
        HumanoidVisualLayers.TailOversuit,
        HumanoidVisualLayers.Eyes,
        HumanoidVisualLayers.RArm,
        HumanoidVisualLayers.LArm,
        HumanoidVisualLayers.RHand,
        HumanoidVisualLayers.LHand,
        HumanoidVisualLayers.RLeg,
        HumanoidVisualLayers.LLeg,
        HumanoidVisualLayers.RFoot,
        HumanoidVisualLayers.LFoot,
    ];

    private readonly Dictionary<EntityUid, OverrideState> _states = new();
    private float _accumulator;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PirateClientTextureVisionComponent, ComponentShutdown>(OnVisionShutdown);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        _accumulator += frameTime;
        if (_accumulator < UpdateInterval)
            return;

        _accumulator = 0f;
        RefreshOverrides();
    }

    private void OnVisionShutdown(Entity<PirateClientTextureVisionComponent> ent, ref ComponentShutdown args)
    {
        if (_player.LocalEntity == ent.Owner)
            ClearAllOverrides();
    }

    private void RefreshOverrides()
    {
        if (_player.LocalEntity is not { Valid: true } local ||
            !TryComp<PirateClientTextureVisionComponent>(local, out var vision) ||
            !TryComp<TransformComponent>(local, out var localXform))
        {
            ClearAllOverrides();
            return;
        }

        var active = new HashSet<EntityUid>();
        var query = EntityQueryEnumerator<SpriteComponent, TransformComponent, MobStateComponent>();
        while (query.MoveNext(out var uid, out var sprite, out var xform, out var mobState))
        {
            if (uid == local || mobState.CurrentState != MobState.Alive)
                continue;

            if (!xform.Coordinates.TryDistance(EntityManager, localXform.Coordinates, out var distance) || distance > vision.Range)
                continue;

            active.Add(uid);
            EnsureOverride((uid, sprite), vision.Mode);
        }

        var restoreBuffer = new EntityUid[Math.Max(_states.Count, 1)];
        var restoreCount = 0;

        foreach (var uid in _states.Keys)
        {
            if (!active.Contains(uid))
                restoreBuffer[restoreCount++] = uid;
        }

        for (var i = 0; i < restoreCount; i++)
        {
            RestoreOverride(restoreBuffer[i]);
        }
    }

    private void EnsureOverride(Entity<SpriteComponent> ent, PirateClientTextureVisionMode mode)
    {
        if (!_states.TryGetValue(ent.Owner, out var state) || state.Mode != mode)
        {
            RestoreOverride(ent.Owner);
            state = new OverrideState(mode, CaptureOriginalVisibility(ent.Comp), mode == PirateClientTextureVisionMode.Skeleton ? CaptureSkeletonOriginalLayers((ent.Owner, ent.Comp)) : null);
            _states[ent.Owner] = state;
        }

        ApplyOverride((ent.Owner, ent.Comp), state);
    }

    private void ApplyOverride(Entity<SpriteComponent> ent, OverrideState state)
    {
        var preserved = GetPreservedOriginalLayers((ent.Owner, ent.Comp), state.Mode);
        var count = ent.Comp.AllLayers.Count();

        for (var i = 0; i < count; i++)
        {
            var visible = preserved.Contains((byte) i) && i < state.LayerVisibility.Length && state.LayerVisibility[i];
            _sprite.LayerSetVisible((ent.Owner, ent.Comp), (byte) i, visible);
        }

        if (state.Mode == PirateClientTextureVisionMode.Skeleton)
        {
            ApplySkeletonOverride((ent.Owner, ent.Comp), state);
            return;
        }

        var overrides = GetOverrideLayers(ent.Owner, state.Mode);
        for (var i = 0; i < overrides.Count; i++)
        {
            var key = GetOverrideLayerKey(i);
            if (!_sprite.LayerMapTryGet((ent.Owner, ent.Comp), key, out _, false))
            {
                _sprite.LayerMapReserve((ent.Owner, ent.Comp), key);
            }

            _sprite.LayerSetSprite((ent.Owner, ent.Comp), key, overrides[i].Specifier);
            _sprite.LayerSetVisible((ent.Owner, ent.Comp), key, true);
        }

        for (var i = overrides.Count; i < state.AddedLayerCount; i++)
        {
            var key = GetOverrideLayerKey(i);
            if (ent.Comp.LayerMapTryGet(key, out _))
                ent.Comp.RemoveLayer(key);
        }

        state.AddedLayerCount = overrides.Count;
    }

    private void ApplySkeletonOverride(Entity<SpriteComponent> ent, OverrideState state)
    {
        var skeletonLayers = GetSkeletonLayerStates(ent.Owner);

        foreach (var (layer, stateId) in skeletonLayers)
        {
            if (!_sprite.LayerMapTryGet((ent.Owner, ent.Comp), layer, out var index, false))
                continue;

            _sprite.LayerSetSprite((ent.Owner, ent.Comp), index, new SpriteSpecifier.Rsi(SkeletonRsi, stateId));
            _sprite.LayerSetColor((ent.Owner, ent.Comp), index, Color.White);
            _sprite.LayerSetVisible((ent.Owner, ent.Comp), index, true);
        }
    }

    private void RestoreOverride(EntityUid uid)
    {
        if (!_states.Remove(uid, out var state) || !TryComp<SpriteComponent>(uid, out var sprite))
            return;

        if (state.Mode == PirateClientTextureVisionMode.Skeleton && state.OriginalSkeletonLayers != null)
        {
            foreach (var (layer, original) in state.OriginalSkeletonLayers)
            {
                if (!_sprite.LayerMapTryGet((uid, sprite), layer, out var index, false))
                    continue;

                if (original.Specifier != null)
                    _sprite.LayerSetSprite((uid, sprite), index, original.Specifier);

                _sprite.LayerSetColor((uid, sprite), index, original.Color);
                _sprite.LayerSetVisible((uid, sprite), index, original.Visible);
            }
        }
        else
        {
            for (var i = 0; i < state.AddedLayerCount; i++)
            {
                var key = GetOverrideLayerKey(i);
                if (sprite.LayerMapTryGet(key, out _))
                    sprite.RemoveLayer(key);
            }
        }

        var restoreCount = Math.Min(state.LayerVisibility.Length, sprite.AllLayers.Count());
        for (var i = 0; i < restoreCount; i++)
        {
            _sprite.LayerSetVisible((uid, sprite), (byte) i, state.LayerVisibility[i]);
        }
    }

    private void ClearAllOverrides()
    {
        foreach (var uid in _states.Keys.ToArray())
        {
            RestoreOverride(uid);
        }
    }

    private static bool[] CaptureOriginalVisibility(SpriteComponent sprite)
    {
        var count = sprite.AllLayers.Count();
        var visibility = new bool[count];
        for (var i = 0; i < count; i++)
        {
            visibility[i] = sprite[i].Visible;
        }

        return visibility;
    }

    private HashSet<byte> GetPreservedOriginalLayers(Entity<SpriteComponent> ent, PirateClientTextureVisionMode mode)
    {
        var preserved = new HashSet<byte>();
        if (mode != PirateClientTextureVisionMode.Skeleton)
            return preserved;

        var hiddenIndices = new HashSet<byte>();
        foreach (var layer in SkeletonHiddenBodyLayers)
        {
            if (_sprite.LayerMapTryGet((ent.Owner, ent.Comp), layer, out var index, false))
                hiddenIndices.Add((byte) index);
        }

        // Keep clothing and held-item layers visible while only swapping the body itself to skeleton parts.
        var count = ent.Comp.AllLayers.Count();
        for (byte i = 0; i < count; i++)
        {
            if (!hiddenIndices.Contains(i))
                preserved.Add(i);
        }

        return preserved;
    }

    private List<OverrideLayerDefinition> GetOverrideLayers(EntityUid uid, PirateClientTextureVisionMode mode)
    {
        return mode switch
        {
            PirateClientTextureVisionMode.Xeno => new List<OverrideLayerDefinition> { PickStableVariant(uid, XenoVariants) },
            PirateClientTextureVisionMode.Food => new List<OverrideLayerDefinition> { PickStableVariant(uid, FoodVariants) },
            PirateClientTextureVisionMode.Skeleton => BuildSkeletonLayers(uid),
            _ => BuildSkeletonLayers(uid),
        };
    }

    private Dictionary<HumanoidVisualLayers, OriginalLayerState> CaptureSkeletonOriginalLayers(Entity<SpriteComponent> ent)
    {
        var captured = new Dictionary<HumanoidVisualLayers, OriginalLayerState>();

        foreach (var layer in GetSkeletonLayerStates(ent.Owner).Keys)
        {
            if (!_sprite.LayerMapTryGet((ent.Owner, ent.Comp), layer, out var index, false))
                continue;

            var spriteLayer = ent.Comp[index];
            SpriteSpecifier? specifier = null;
            if (spriteLayer.Rsi != null && spriteLayer.RsiState.IsValid)
                specifier = new SpriteSpecifier.Rsi(spriteLayer.Rsi.Path, spriteLayer.RsiState.Name!);

            captured[layer] = new OriginalLayerState(specifier, spriteLayer.Color, spriteLayer.Visible);
        }

        return captured;
    }

    private Dictionary<HumanoidVisualLayers, string> GetSkeletonLayerStates(EntityUid uid)
    {
        var suffix = "m";
        if (TryComp<HumanoidAppearanceComponent>(uid, out var humanoid))
            suffix = humanoid.Sex == Sex.Female ? "f" : "m";
        else if (((uint) uid.GetHashCode() & 1) != 0)
            suffix = "f";

        var layers = new Dictionary<HumanoidVisualLayers, string>(SkeletonLayerStates)
        {
            [HumanoidVisualLayers.Chest] = $"chest_{suffix}",
            [HumanoidVisualLayers.Groin] = $"groin_{suffix}",
            [HumanoidVisualLayers.Head] = $"head_{suffix}",
        };

        return layers;
    }

    private static OverrideLayerDefinition PickStableVariant(EntityUid uid, IReadOnlyList<OverrideLayerDefinition> variants)
    {
        var index = (int) ((uint) uid.GetHashCode() % variants.Count);
        return variants[index];
    }

    private static List<OverrideLayerDefinition> BuildSkeletonLayers(EntityUid uid)
    {
        // Build the skeleton from parts so hands can stay visible for held items.
        var suffix = ((uint) uid.GetHashCode() & 1) == 0 ? "m" : "f";

        return new List<OverrideLayerDefinition>
        {
            new(new SpriteSpecifier.Rsi(SkeletonRsi, $"chest_{suffix}")),
            new(new SpriteSpecifier.Rsi(SkeletonRsi, $"groin_{suffix}")),
            new(new SpriteSpecifier.Rsi(SkeletonRsi, "l_leg")),
            new(new SpriteSpecifier.Rsi(SkeletonRsi, "r_leg")),
            new(new SpriteSpecifier.Rsi(SkeletonRsi, "l_foot")),
            new(new SpriteSpecifier.Rsi(SkeletonRsi, "r_foot")),
            new(new SpriteSpecifier.Rsi(SkeletonRsi, "l_arm")),
            new(new SpriteSpecifier.Rsi(SkeletonRsi, "r_arm")),
            new(new SpriteSpecifier.Rsi(SkeletonRsi, $"head_{suffix}")),
        };
    }

    private static string GetOverrideLayerKey(int index)
    {
        return $"{OverrideLayerPrefix}{index}";
    }

    private sealed class OverrideState
    {
        public PirateClientTextureVisionMode Mode;
        public readonly bool[] LayerVisibility;
        public readonly Dictionary<HumanoidVisualLayers, OriginalLayerState>? OriginalSkeletonLayers;
        public int AddedLayerCount;

        public OverrideState(PirateClientTextureVisionMode mode, bool[] layerVisibility, Dictionary<HumanoidVisualLayers, OriginalLayerState>? originalSkeletonLayers)
        {
            Mode = mode;
            LayerVisibility = layerVisibility;
            OriginalSkeletonLayers = originalSkeletonLayers;
        }
    }

    private readonly record struct OverrideLayerDefinition(SpriteSpecifier Specifier);
    private readonly record struct OriginalLayerState(SpriteSpecifier? Specifier, Color Color, bool Visible);
}
