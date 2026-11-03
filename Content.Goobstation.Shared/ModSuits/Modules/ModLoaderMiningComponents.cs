using Content.Shared.Weather;
using Content.Shared.Whitelist;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.ModSuits.Modules;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ModAshCoatComponent : Component
{
    [DataField, AutoNetworkedField]
    public int Layers;

    [DataField, AutoNetworkedField]
    public int MaxLayers = 10;

    [DataField]
    public List<string> AccretionTilePrefixes = new();

    [DataField]
    public List<string> KeepTilePrefixes = new();

    [DataField]
    public List<ProtoId<WeatherPrototype>> AccretionWeather = new();

    [DataField]
    public TimeSpan WeatherInterval = TimeSpan.FromSeconds(2);

    [DataField]
    public Dictionary<string, float> LayerReductions = new();

    [DataField]
    public float LayerExplosionCoefficient = 0.03f;

    [DataField]
    public float FullSpeedModifier = 1.1f;

    [DataField]
    public SoundSpecifier FullSound = new SoundPathSpecifier("/Audio/Effects/sparks1.ogg");

    [ViewVariables]
    public Vector2i? LastTile;

    [ViewVariables]
    public EntityUid? LastGrid;

    [ViewVariables]
    public TimeSpan NextWeatherLayer;
}

[RegisterComponent, NetworkedComponent]
public sealed partial class ModMagnetModuleComponent : Component
{
    [DataField]
    public EntityWhitelist? Whitelist;

    [DataField]
    public float PickRange = 0.75f;

    [DataField]
    public float PullSpeed = 8f;

    [DataField]
    public float LaunchRange = 7f;

    [DataField]
    public float LaunchSpeed = 12f;

    [DataField]
    public SoundSpecifier Sound = new SoundPathSpecifier("/Audio/_Goobstation/Items/Modsuit/gravhit.ogg");
}

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ModMagnetizedComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntityUid? Puller;
}

[RegisterComponent, NetworkedComponent]
public sealed partial class ModCandelaModuleComponent : Component
{
    [DataField]
    public EntityWhitelist? Whitelist;

    [DataField]
    public float Spacing = 6f;

    [DataField]
    public SoundSpecifier Sound = new SoundPathSpecifier("/Audio/Effects/thunk.ogg");

    [ViewVariables]
    public MapCoordinates? LastBeacon;

    [ViewVariables]
    public bool WarnedEmpty;
}
