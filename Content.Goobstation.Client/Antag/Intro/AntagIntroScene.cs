using System.Linq;
using System.Numerics;
using Content.Client.Chat.UI;
using Content.Client.Humanoid;
using Content.Client.Station;
using Content.Client.Viewport;
using Content.Client.Weapons.Melee;
using Content.Shared.Chat.Prototypes;
using Content.Shared.Doors.Components;
using Content.Shared.Fluids;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Prototypes;
using Content.Shared.Light;
using Content.Shared.Preferences;
using Content.Shared.Rotation;
using Content.Shared.Speech.Components;
using Content.Shared.Weapons.Melee;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Audio;
using Robust.Shared.EntitySerialization;
using Robust.Shared.EntitySerialization.Systems;
using Robust.Shared.Graphics;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Serialization.Markdown.Mapping;
using Robust.Shared.Serialization.Markdown.Sequence;
using Robust.Shared.Serialization.Markdown.Value;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Goobstation.Client.Antag.Intro;

/// <summary>
/// Animation played when a player gets an antagonist role.
/// </summary>
public abstract class AntagIntroScene : Control
{
    public Func<float?>? TrackPosition;
    public Action<SoundSpecifier>? OnSound;
    public Action<float>? OnFadeOut;

    [Dependency] protected readonly IEntityManager Entities = default!;
    [Dependency] protected readonly IMapManager MapMan = default!;
    [Dependency] protected readonly IPrototypeManager Proto = default!;
    [Dependency] protected readonly ITileDefinitionManager TileDefs = default!;
    [Dependency] protected readonly IRobustRandom Rand = default!;
    [Dependency] private readonly IComponentFactory _components = default!;

    protected readonly SharedMapSystem Maps;
    protected readonly MetaDataSystem Meta;
    protected readonly SharedTransformSystem Xforms;
    protected readonly SharedAppearanceSystem Appearance;
    protected readonly SharedPointLightSystem Lights;
    protected readonly SharedHandsSystem Hands;
    protected readonly SpriteSystem Sprites;
    protected readonly AnimationPlayerSystem Animations;
    protected readonly MeleeWeaponSystem Melee;
    protected readonly HumanoidAppearanceSystem Humanoids;
    protected readonly StationSpawningSystem Spawning;

    /// <summary>
    /// How long the piece runs.
    /// </summary>
    protected abstract float Length { get; }

    /// <summary>
    /// Lays the set out. Called once, before anything is timed.
    /// </summary>
    protected abstract void Set();

    /// <summary>
    /// Everything the piece does and when, as one table.
    /// </summary>
    protected abstract IEnumerable<Cue> Script();

    /// <summary>
    /// Where everyone is, and what the lights are doing, on this frame.
    /// </summary>
    protected abstract void Direct(float t);

    /// <summary>
    /// What the camera is holding on this frame.
    /// </summary>
    protected abstract Shot Frame(float t);

    /// <summary>
    /// A colour over the whole frame, for the frames that want one.
    /// </summary>
    protected virtual Color? Wash(float t) => null;

    /// <summary>
    /// A sound, and something to do, at a moment in the piece.
    /// </summary>
    protected readonly record struct Cue(float At,
        SoundSpecifier? Sound = null,
        Action? Then = null);

    /// <summary>
    /// The camera.
    /// </summary>
    protected readonly record struct Shot(
        float Tiles,
        float X,
        float Row,
        EntityUid Eye,
        Vector2 Shake = default);

    #region Set

    protected MapId MapId;
    protected Entity<MapGridComponent> Grid;
    private readonly ScalingViewport _viewport;
    private readonly LayoutContainer _bubbles = new();
    private readonly Eye _eye = new() { DrawFov = true, DrawLight = true };
    private EntityUid? _mapUid;
    private readonly List<EntityUid> _loose = new();
    private readonly List<EntityUid> _loaded = new();
    private readonly Dictionary<EntityUid, List<int>> _wounds = new();
    private readonly Dictionary<EntityUid, int> _hurt = new();
    private readonly Dictionary<EntityUid, ProtoId<EmoteSoundsPrototype>> _voices = new();
    private readonly Dictionary<EntityUid, (Control Bubble, float Until)> _said = new();
    private const float Heard = 3.5f;
    protected const float Tail = 0.55f;
    /// <summary>
    /// How long the fade out for the music takes.
    /// </summary>
    protected const float Hush = 6f;

    private Cue[] _script = Array.Empty<Cue>();
    private float _time;
    private int _cue;
    private bool _staged;
    private bool _failed;
    private bool _fading;
    private float _offset;

    public float Now => _failed
        ? Length
        : _fading
            ? _time + _offset
            : TrackPosition?.Invoke() ?? _time;

    public bool Finished => Now >= Length;

    protected AntagIntroScene()
    {
        IoCManager.InjectDependencies(this);

        Maps = Entities.System<SharedMapSystem>();
        Meta = Entities.System<MetaDataSystem>();
        Xforms = Entities.System<SharedTransformSystem>();
        Appearance = Entities.System<SharedAppearanceSystem>();
        Lights = Entities.System<SharedPointLightSystem>();
        Hands = Entities.System<SharedHandsSystem>();
        Sprites = Entities.System<SpriteSystem>();
        Animations = Entities.System<AnimationPlayerSystem>();
        Melee = Entities.System<MeleeWeaponSystem>();
        Humanoids = Entities.System<HumanoidAppearanceSystem>();
        Spawning = Entities.System<StationSpawningSystem>();
        MouseFilter = MouseFilterMode.Stop;
        RectClipContent = true;

        _viewport = new ScalingViewport
        {
            AlwaysRender = true,
            Eye = _eye,
            ViewportSize = new Vector2i(1280, 720),
            StretchMode = ScalingViewportStretchMode.Bilinear,
            RenderScaleMode = ScalingViewportRenderScaleMode.Fixed,
            MouseFilter = MouseFilterMode.Ignore,
        };

        AddChild(_viewport);
        AddChild(_bubbles);
        AddChild(new Curtain { Scene = this });
    }

    /// <summary>
    /// Builds the set.
    /// </summary>
    protected override void EnteredTree()
    {
        base.EnteredTree();

        if (_staged || _failed)
            return;

        try
        {
            _mapUid = Maps.CreateMap(out MapId);
            Grid = MapMan.CreateGridEntity(MapId);

            Set();

            _script = Script().OrderBy(cue => cue.At).ToArray();
            _staged = true;
        }
        catch (Exception e)
        {
            Logger.GetSawmill("antag.intro").Error($"Could not stage {GetType().Name}: {e}");
            _failed = true;
        }
    }

    /// <summary>
    /// The set only ever existed on this client, so it goes when the control does.
    /// </summary>
    protected override void ExitedTree()
    {
        base.ExitedTree();

        if (_mapUid is not { } map)
            return;

        _mapUid = null;

        foreach (var loose in _loose)
            Entities.DeleteEntity(loose);

        foreach (var loaded in _loaded)
            Entities.DeleteEntity(loaded);

        Entities.DeleteEntity(map);
    }

    protected override void FrameUpdate(FrameEventArgs args)
    {
        base.FrameUpdate(args);

        _time += args.DeltaSeconds;

        var t = Now;

        if (_staged && !_fading && t > Length - Tail)
        {
            _offset = t - _time;
            _fading = true;
            OnFadeOut?.Invoke(Hush);
        }

        while (_cue < _script.Length && _script[_cue].At <= t)
        {
            var cue = _script[_cue++];

            if (cue.Sound != null)
                OnSound?.Invoke(cue.Sound);

            cue.Then?.Invoke();
        }

        if (!_staged)
            return;

        Direct(t);
        Camera(Frame(t));
        Bubbles();
    }

    #endregion

    #region Staging

    /// <summary>
    /// The middle of a tile.
    /// </summary>
    protected static Vector2 Middle(float x, float row) => new(x + 0.5f, 0.5f - row);

    protected EntityUid Place(string proto, float x, float row)
        => Entities.SpawnEntity(proto, new MapCoordinates(Middle(x, row), MapId));

    /// <summary>Deck, wall to wall and end to end.</summary>
    protected void Deck(string tile, int fromX, int toX, int fromRow, int toRow)
    {
        var flat = new Tile(TileDefs[tile].TileId);

        for (var row = fromRow; row <= toRow; row++)
            for (var x = fromX; x <= toX; x++)
                Maps.SetTile(Grid, new Vector2i(x, -row), flat);
    }

    protected void Ambient(Color colour) => Maps.SetAmbientLight(MapId, colour);

    protected const int Lane = 1;
    protected const int Course = 2;

    protected readonly record struct Run(EntityUid Door, EntityUid Lamp);

    /// <summary>
    /// Generic maintenance hallway set.
    /// </summary>
    protected Run Maintenance(int far, Color tube, float range)
    {
        Deck("FloorTechMaint2", -Course, Course, -3, 0);
        Deck("Plating", -Course, Course, 1, far);

        for (var row = 1; row <= far; row++)
        {
            Place("WallSolid", -Course, row);
            Place("WallSolid", Course, row);
            Place("CableApcExtension", 0, row);
            Place("CableMV", 0, row);
            Place("Catwalk", 0, row);
        }

        for (var x = -Course; x <= Course; x++)
        {
            if (x != 0)
                Place("WallSolid", x, 0);

            Place("WallSolid", x, far + 1);
        }

        var door = Place("AirlockMaintLocked", 0, 0);

        Appearance.SetData(door, DoorVisuals.State, DoorState.Closed);
        Appearance.SetData(door, DoorVisuals.BoltLights, true);

        Clutter("ClosetMaintenance", Lane, 2);
        Clutter("AirCanister", -Lane, 4);
        Clutter("ClosetMaintenance", Lane, 6);
        Clutter("AirCanister", Lane, 8);

        void Clutter(string proto, float x, float row)
        {
            if (row <= far)
                Place(proto, x, row);
        }

        Spill(3f, MathF.Min(5f, far - 1f), Lane, 3);

        return new Run(door, Lamp("PoweredSmallLight", -Lane, 1f, Direction.East, tube, range));
    }

    protected EntityUid Cast(ProtoId<SpeciesPrototype> species,
        string gear,
        float x,
        float row,
        out HumanoidCharacterProfile profile)
    {
        var kind = Proto.Index(species);
        var mob = Place(kind.DollPrototype, x, row);

        profile = HumanoidCharacterProfile.RandomWithSpecies(species);

        Humanoids.LoadProfile(mob, profile);
        Meta.SetEntityName(mob, profile.Name);
        Spawning.EquipStartingGear(mob, gear, raiseEvent: false);
        Face(mob, Direction.North);

        if (Proto.Index<EntityPrototype>(kind.Prototype).TryGetComponent<VocalComponent>(out var vocal, _components)
            && vocal.Sounds is { } voices
            && voices.TryGetValue(profile.Sex, out var voice))
            _voices[mob] = voice;

        return mob;
    }

    protected EntityUid Crewman(float x, float row)
        => Cast(SharedHumanoidAppearanceSystem.DefaultSpecies, "AntagIntroCrewGear", x, row, out _);

    protected EntityUid Arm(EntityUid mob, string proto)
    {
        var held = Entities.SpawnEntity(proto, MapCoordinates.Nullspace);

        _loose.Add(held);
        Hands.TryPickupAnyHand(mob, held, checkActionBlocker: false, animate: false);

        return held;
    }

    protected void Wounds(EntityUid mob)
    {
        var layers = new List<int>();

        foreach (var part in WoundParts)
        {
            var layer = Sprites.AddLayer((mob, null),
                new SpriteSpecifier.Rsi(new ResPath(WoundRsi), $"{part}_Brute_{WoundSteps[0]}"));

            Sprites.LayerSetVisible((mob, null), layer, false);
            layers.Add(layer);
        }

        _wounds[mob] = layers;
        _hurt[mob] = -1;
    }

    protected void Hurt(EntityUid mob)
    {
        if (!_wounds.TryGetValue(mob, out var layers)
            || !Entities.EntityExists(mob)
            || _hurt[mob] >= WoundSteps.Length - 1)
            return;

        var step = ++_hurt[mob];

        for (var i = 0; i < layers.Count; i++)
        {
            Sprites.LayerSetVisible((mob, null), layers[i], true);
            Sprites.LayerSetRsiState((mob, null), layers[i], $"{WoundParts[i]}_Brute_{WoundSteps[step]}");
        }
    }

    protected void Bleed(EntityUid mob)
    {
        if (!Entities.EntityExists(mob))
            return;

        foreach (var part in WoundParts)
        {
            Sprites.AddLayer((mob, null),
                new SpriteSpecifier.Rsi(new ResPath(BleedRsi), $"{part}_Severe"));
        }
    }

    protected void Fall(EntityUid mob)
    {
        if (!Entities.EntityExists(mob))
            return;

        Entities.EnsureComponent<AppearanceComponent>(mob);
        Entities.EnsureComponent<RotationVisualsComponent>(mob);
        Appearance.SetData(mob, RotationVisuals.RotationState, RotationState.Horizontal);
    }

    protected void Spill(float fromRow, float toRow, float wide, int count)
    {
        for (var i = 0; i < count; i++)
        {
            var puddle = Place("PuddleBloodSmall",
                Rand.NextFloat(-wide, wide), Rand.NextFloat(fromRow, toRow));

            Appearance.SetData(puddle, PuddleVisuals.CurrentVolume,
                Rand.NextFloat(0.1f, SharedPuddleSystem.LowThreshold * 0.9f));
            Appearance.SetData(puddle, PuddleVisuals.SolutionColor, Blood);
        }
    }

    protected void Face(EntityUid mob, Direction facing)
    {
        if (Entities.EntityExists(mob))
            Xforms.SetWorldRotation(mob, facing.ToAngle());
    }

    protected void Stand(EntityUid mob, float x, float row)
    {
        if (Entities.EntityExists(mob))
            Xforms.SetWorldPosition(mob, Middle(x, row));
    }

    /// <summary>
    /// Somebody standing on a tile of another map, for a scene whose sets are not all on the same one.
    /// A world position alone cannot carry anybody off the map they are already on.
    /// </summary>
    protected void Stand(EntityUid mob, MapId map, float x, float row)
    {
        if (Entities.EntityExists(mob))
            Xforms.SetMapCoordinates(mob, new MapCoordinates(Middle(x, row), map));
    }

    protected void Strike(EntityUid attacker, EntityUid weapon, EntityUid target, bool wide = true)
    {
        if (!Entities.EntityExists(target)
            || !Entities.TryGetComponent<MeleeWeaponComponent>(weapon, out var melee))
            return;

        var reach = Xforms.GetWorldPosition(target) - Xforms.GetWorldPosition(attacker);
        var visual = melee.Range - 0.2f;

        if (reach.Length() > visual)
            reach = reach.Normalized() * visual;

        Melee.DoLunge(attacker,
            weapon,
            melee.Angle,
            reach,
            wide ? melee.WideAnimation : melee.Animation,
            wide ? melee.WideAnimationRotation : melee.AnimationRotation,
            melee.FlipAnimation);
    }

    protected void Animate(EntityUid uid, object? animation, string key)
    {
        if (animation is not Robust.Client.Animations.Animation anim)
            return;

        Animations.Stop(uid, key);
        Animations.Play(uid, anim, key);
    }

    protected void Emote(EntityUid mob, ProtoId<EmotePrototype> id)
    {
        if (!Entities.EntityExists(mob) || _said.ContainsKey(mob))
            return;

        var emote = Proto.Index(id);

        if (_voices.TryGetValue(mob, out var voice)
            && Proto.Index(voice).Sounds is { } sounds
            && sounds.TryGetValue(id, out var says))
            OnSound?.Invoke(says);

        var label = new RichTextLabel { MaxWidth = SpeechBubble.SpeechMaxWidth };

        label.SetMessage(FormattedMessage.FromMarkupPermissive(
            Loc.GetString("chat-manager-entity-me-wrap-message",
                ("entityName", Entities.GetComponent<MetaDataComponent>(mob).EntityName),
                ("entity", mob),
                ("message", emote.ChatMessages.Count > 0 ? Loc.GetString(emote.ChatMessages[0]) : string.Empty))));

        var bubble = new PanelContainer
        {
            StyleClasses = { "speechBox", "emoteBox" },
            Children = { label },
        };

        _bubbles.AddChild(bubble);
        bubble.Measure(Vector2Helpers.Infinity);
        _said[mob] = (bubble, Now + Heard);
    }

    private void Bubbles()
    {
        if (_said.Count == 0)
            return;

        var now = Now;

        foreach (var (mob, (bubble, until)) in _said)
        {
            if (now >= until || !Entities.EntityExists(mob))
            {
                bubble.Orphan();
                _said.Remove(mob);
                continue;
            }

            var over = Xforms.GetWorldPosition(mob) + new Vector2(0f, 0.55f);
            var at = _viewport.WorldToScreen(over) / UIScale;

            LayoutContainer.SetPosition(bubble,
                at - new Vector2(bubble.DesiredSize.X * 0.5f, bubble.DesiredSize.Y));
        }
    }

    #endregion

    #region Lights

    protected EntityUid Lamp(string proto, float x, float row, Direction facing, Color colour, float range)
    {
        var lamp = Place(proto, x, row);

        Face(lamp, facing);
        Lights.SetColor(lamp, colour);
        Lights.SetRadius(lamp, range);
        Lights.SetEnabled(lamp, false);
        Bulb(lamp, PoweredLightState.Empty);

        return lamp;
    }

    protected void Glow(EntityUid uid, Color colour, float range)
    {
        Lights.EnsureLight(uid);
        Lights.SetColor(uid, colour);
        Lights.SetRadius(uid, range);
        Lights.SetEnabled(uid, false);
    }

    protected void Burn(EntityUid lamp, float energy, PoweredLightState? bulb = null)
    {
        Shine(lamp, energy);
        Bulb(lamp, bulb ?? (energy > 0.02f ? PoweredLightState.On : PoweredLightState.Empty));
    }

    protected void Shine(EntityUid uid, float energy)
    {
        Lights.SetEnabled(uid, energy > 0.02f);
        Lights.SetEnergy(uid, energy);
    }

    protected Entity<MapGridComponent> Load(string path, out MapId map)
    {
        var loader = Entities.System<MapLoaderSystem>();

        // Because these sets are loaded client side only we remove any components the client doesn't know.
        // (They usually don't need to do anything besides exist as sprites anyways)
        if (loader.TryReadFile(new ResPath(path), out var data))
            IgnoreUnknown(data);

        var opts = DeserializationOptions.Default with { InitializeMaps = true };

        if (!loader.TryLoadMap(new ResPath(path), out var loaded, out var grids, opts)
            || grids.Count != 1)
        {
            throw new InvalidOperationException($"Could not load one grid out of {path}.");
        }

        _loaded.Add(loaded.Value.Owner);
        map = loaded.Value.Comp.MapId;

        return grids.First();
    }

    /// <summary>
    /// Every component in a map file that this side has never heard of.
    /// </summary>
    private void IgnoreUnknown(MappingDataNode data)
    {
        if (!data.TryGet("entities", out SequenceDataNode? groups))
            return;

        var unknown = new HashSet<string>();

        foreach (var group in groups.OfType<MappingDataNode>())
        {
            if (!group.TryGet("entities", out SequenceDataNode? entities))
                continue;

            foreach (var ent in entities.OfType<MappingDataNode>())
            {
                if (!ent.TryGet("components", out SequenceDataNode? comps))
                    continue;

                foreach (var comp in comps.OfType<MappingDataNode>())
                {
                    if (comp.TryGet("type", out ValueDataNode? kind)
                        && !_components.TryGetRegistration(kind.Value, out _)
                        && !_components.IsIgnored(kind.Value))
                        unknown.Add(kind.Value);
                }
            }
        }

        if (unknown.Count > 0)
            _components.RegisterIgnore(unknown.ToArray());
    }

    /// <summary>
    /// Switches on everything on a loaded grid that has a light in it.
    /// </summary>
    protected void Power(EntityUid grid)
    {
        var children = Entities.GetComponent<TransformComponent>(grid).ChildEnumerator;

        while (children.MoveNext(out var child))
        {
            if (!Lights.TryGetLight(child, out _))
                continue;

            Lights.SetEnabled(child, true);

            if (Entities.HasComponent<AppearanceComponent>(child))
                Bulb(child, PoweredLightState.On);
        }
    }

    protected EntityUid Find(EntityUid grid, EntProtoId proto)
    {
        var children = Entities.GetComponent<TransformComponent>(grid).ChildEnumerator;

        while (children.MoveNext(out var child))
            if (Entities.GetComponent<MetaDataComponent>(child).EntityPrototype?.ID == proto.Id)
                return child;

        return EntityUid.Invalid;
    }

    /// <summary>
    /// Which tile something is standing on.</summary>
    protected (float X, float Row) Where(EntityUid uid)
    {
        var at = Xforms.GetWorldPosition(uid);

        return (at.X - 0.5f, 0.5f - at.Y);
    }

    private void Bulb(EntityUid lamp, PoweredLightState state)
        => Appearance.SetData(lamp, PoweredLightVisuals.BulbState, state);

    #endregion

    #region Camera

    private void Camera(Shot shot)
    {
        var h = (float) PixelSize.Y;

        if (h <= 0f)
            return;

        if (_viewport.ViewportSize != PixelSize)
            _viewport.ViewportSize = PixelSize;

        var on = MapId;
        var from = Middle(shot.X, shot.Row);

        if (Entities.TryGetComponent<TransformComponent>(shot.Eye, out var xform))
        {
            on = xform.MapID;
            from = Xforms.GetWorldPosition(shot.Eye);
        }

        _eye.Position = new MapCoordinates(from, on);
        _eye.Offset = Middle(shot.X, shot.Row) - from + shot.Shake;
        _eye.Zoom = Vector2.One * (shot.Tiles * EyeManager.PixelsPerMeter / h);
    }

    /// <summary>
    /// Impact frames.
    /// </summary>
    protected static Vector2 Shove(float t, float at, float amp, float decay)
    {
        var s = t - at;

        if (s is < 0f or >= 0.5f)
            return Vector2.Zero;

        var a = MathF.Exp(-decay * s) * amp;

        return new Vector2(MathF.Sin(s * 103f) * a, MathF.Cos(s * 77f) * a);
    }

    #endregion

    #region Sound

    protected static SoundSpecifier Sound(string path, float gain)
        => new SoundPathSpecifier(path, AudioParams.Default.WithVolume(gain));

    protected static SoundSpecifier Pick(string collection, float gain)
        => new SoundCollectionSpecifier(collection, AudioParams.Default.WithVolume(gain));

    #endregion

    #region Pieces

    private const string WoundRsi = "/Textures/_Shitmed/Mobs/Effects/brute_damage.rsi";
    private const string BleedRsi = "/Textures/_Shitmed/Mobs/Effects/bleeding_damage.rsi";

    private static readonly string[] WoundParts =
        { "Chest", "Head", "LArm", "RArm", "LLeg", "RLeg" };

    private static readonly int[] WoundSteps = { 10, 20, 40, 50, 70, 100 };

    protected static readonly Color Blood = Color.FromHex("#8e0d12");

    protected static readonly ProtoId<EmotePrototype> ScreamEmote = "Scream";

    protected static float Clamp01(float v) => Math.Clamp(v, 0f, 1f);

    protected static float Ramp(float t, float from, float over) => Clamp01((t - from) / over);

    protected static bool Within(float t, float at, float span) => t >= at && t < at + span;

    protected static float Jolt(float t, float at, float decay, float window)
    {
        var s = t - at;

        return s >= 0f && s < window ? MathF.Exp(-decay * s) : 0f;
    }

    /// <summary>
    /// Technically we coulddd use robust random but framerate would mess with it.
    /// </summary>
    protected static float Hash01(int n)
    {
        n = (n << 13) ^ n;
        var m = (n * (n * n * 15731 + 789221) + 1376312589) & 0x7fffffff;
        return m / 2147483647f;
    }

    protected static class Ease
    {
        public static float InCubic(float p) => p * p * p;
        public static float OutCubic(float p) => 1f - MathF.Pow(1f - p, 3f);
        public static float OutQuint(float p) => 1f - MathF.Pow(1f - p, 5f);
        public static float InCubic(float t, float at, float over) => InCubic(Ramp(t, at, over));
        public static float OutCubic(float t, float at, float over) => OutCubic(Ramp(t, at, over));
        public static float OutQuint(float t, float at, float over) => OutQuint(Ramp(t, at, over));
    }

    private sealed class Curtain : Control
    {
        public AntagIntroScene Scene = default!;

        protected override void Draw(DrawingHandleScreen handle)
        {
            base.Draw(handle);

            var t = Scene.Now;
            var full = new UIBox2(Vector2.Zero, PixelSize);

            if (Scene.Wash(t) is { } wash)
                handle.DrawRect(full, wash);

            var fade = 0f;

            if (t < 0.18f)
                fade = 1f - Clamp01(t / 0.18f);

            if (t > Scene.Length - Tail)
                fade = MathF.Max(fade, Clamp01((t - (Scene.Length - Tail)) / Tail));

            if (fade > 0.004f)
                handle.DrawRect(full, Color.Black.WithAlpha(fade));
        }
    }

    #endregion
}
