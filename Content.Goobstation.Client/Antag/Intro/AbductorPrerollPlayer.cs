using Content.Shared.Humanoid.Prototypes;
using Content.Shared.Light;
using Robust.Shared.Audio;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Client.Antag.Intro;

/// <summary>
/// The animated intro for abductors.
/// </summary>
public sealed class AbductorPrerollPlayer : AntagIntroScene
{
    #region Timing

    private const float Walk0 = 0.150f;
    private const float Walk1 = 0.560f;
    private const float Walk2 = 0.970f;
    private const float Arrive = 1.240f;
    private const float Halt = 1.620f;
    private const float Turn = 2.050f;
    private const float Cry = 2.330f;
    private const float Back = 2.420f;
    private const float Prod = 2.780f;
    private const float Drop = 2.960f;
    private const float Over = 3.500f;
    private const float Lift = 4.250f;
    private const float Take = 4.820f;
    private const float Empty = 5.300f;
    private const float Ship = 5.900f;
    private const float Work = 7.200f;
    private const float Sting = 8.400f;
    private const float Runtime = 9.400f;

    #endregion

    #region Sound

    private static readonly SoundSpecifier Step = Pick("FootstepFloor", -6f);
    private static readonly SoundSpecifier Heavy = Pick("FootstepHeavy", -3f);
    private static readonly SoundSpecifier In = Sound("/Audio/_Shitmed/Misc/alien_teleport.ogg", -2f);
    private static readonly SoundSpecifier Out = Sound("/Audio/_Shitmed/Misc/alien_teleport.ogg", 0f);
    private static readonly SoundSpecifier Shock = Sound("/Audio/Weapons/egloves.ogg", 0f);
    private static readonly SoundSpecifier Taser = Sound("/Audio/Weapons/Guns/Hits/taser_hit.ogg", -2f);
    private static readonly SoundSpecifier Thud = Sound("/Audio/Effects/Footsteps/largethud.ogg", -2f);
    private static readonly SoundSpecifier Scanner = Sound("/Audio/Machines/scanning.ogg", -4f);
    private static readonly SoundSpecifier Working = Sound("/Audio/Machines/scanbuzz.ogg", -5f);
    private static readonly SoundSpecifier Theirs = Sound("/Audio/_Shitmed/Misc/abductor.ogg", -3f);

    #endregion

    #region Set

    private const int Far = 9;
    private const string ShipFile = "/Maps/_Shitmed/Shuttles/ShuttleEvent/abductor_shuttle.yml";
    private static readonly EntProtoId Slab = "AbductorOperatingTable";
    private static readonly Color Unlit = Color.FromHex("#08080b");
    private static readonly Color Tube = Color.FromHex("#dbe6ef");
    private static readonly Color Alien = Color.FromHex("#5ffbd0");
    private static readonly Color AlienFlash = Color.FromHex("#0d2c24");
    private static readonly Color ShockFlash = Color.FromHex("#11141c");
    private static readonly ProtoId<SpeciesPrototype> Abductors = "Abductor";
    private EntityUid _crew;
    private EntityUid _agent;
    private EntityUid _scientist;
    private EntityUid _prod;
    private EntityUid _lamp;
    private EntityUid _table;
    private MapId _ship;
    private float _slabX;
    private float _slabRow;

    protected override float Length => Runtime;

    #endregion

    #region Building

    protected override void Set()
    {
        Ambient(Unlit);

        _lamp = Maintenance(Far, Tube, 7f).Lamp;

        var ship = Load(ShipFile, out _ship);

        Power(ship.Owner);

        _table = Find(ship.Owner, Slab);

        if (!Entities.EntityExists(_table))
            throw new InvalidOperationException($"No {Slab} on {ShipFile}.");

        (_slabX, _slabRow) = Where(_table);
        Glow(_table, Alien, 5f);

        _crew = Crewman(0f, 2.2f);
        _agent = Cast(Abductors, "AbductorAgentGear", 0f, Far + 4f, out _);
        _scientist = Cast(Abductors, "AbductorScientistGear", 1f, Far + 4f, out _);

        Glow(_agent, Alien, 4.5f);
        _prod = Arm(_agent, "Wonderprod");
    }

    #endregion

    #region Script

    protected override IEnumerable<Cue> Script() => new[]
    {
        new Cue(Walk0, Step),
        new Cue(Walk1, Step),
        new Cue(Walk2, Step),
        new Cue(Arrive, In),
        new Cue(Arrive + 0.28f, Heavy),
        new Cue(Cry, Then: () => Emote(_crew, ScreamEmote)),
        new Cue(Prod, Shock, () => Strike(_agent, _prod, _crew, wide: false)),
        new Cue(Prod + 0.05f, Taser),
        new Cue(Drop, Thud, () => Fall(_crew)),
        new Cue(Over, Heavy),
        new Cue(Lift, Heavy),
        new Cue(Take, Out, Vanish),
        new Cue(Ship, Scanner),
        new Cue(Work, Working),
        new Cue(Sting, Theirs),
    };

    private void Vanish()
    {
        Stand(_crew, _ship, _slabX, _slabRow);
        Stand(_agent, _ship, _slabX - 1.1f, _slabRow);
        Stand(_scientist, _ship, _slabX + 0.9f, _slabRow - 0.6f);

        Face(_agent, Direction.East);
        Face(_scientist, Direction.South);
    }

    #endregion

    #region Direction

    protected override void Direct(float t)
    {
        Fittings(t);
        People(t);
    }

    private void Fittings(float t)
    {
        var tube = 0.85f + 0.1f * MathF.Sin(t * 27f);

        if (MathF.Sin(t * 2.3f) > 0.972f)
            tube *= Hash01((int) (t * 50f)) > 0.5f ? 0.85f : 0.45f;

        Burn(_lamp, t < Take ? tube : 0f, t < Take ? PoweredLightState.On : PoweredLightState.Broken);

        var carried = 0.7f * Ramp(t, Arrive, 0.5f) * (0.82f + 0.18f * MathF.Sin((t - Arrive) * 5f));

        if (t >= Take)
            carried = 0.35f;

        Shine(_agent, carried);

        var slab = 0.9f * Ramp(t, Ship, 0.5f);
        slab *= MathHelper.Lerp(1f, 1.6f, Ease.OutCubic(t, Work, 0.6f));

        Shine(_table, slab);
    }

    private void People(float t)
    {
        if (t < Take)
        {
            var row = MathHelper.Lerp(2.2f, 3.4f, Ease.OutCubic(t, Walk0, Halt - Walk0));
            row += 0.34f * Ease.OutCubic(t, Back, 0.3f);
            row += 0.22f * Ease.OutQuint(t, Prod, 0.18f);

            Stand(_crew, 0f, row);

            if (t < Drop)
                Face(_crew, t < Turn ? Direction.South : Direction.North);
        }

        if (t < Arrive || t >= Take)
            return;

        Stand(_agent, -0.55f,
            MathHelper.Lerp(1.5f, 2.55f, Ease.OutCubic(t, Prod - 0.22f, 0.4f)));
        Stand(_scientist, 0.3f,
            MathHelper.Lerp(1.2f, 2.9f, Ease.OutCubic(t, Over, 0.7f)));

        Face(_agent, Direction.South);
        Face(_scientist, Direction.South);
    }

    #endregion

    #region Camera

    protected override Shot Frame(float t)
    {
        var tiles = 8.6f;
        var x = 0f;
        var row = 3.2f;
        var eye = _crew;

        tiles -= 1.1f * Ease.OutCubic(t, Arrive, 0.8f);

        if (t >= Prod)
        {
            tiles = 5.4f;
            row = 3.7f;
        }

        if (t >= Over)
        {
            tiles = MathHelper.Lerp(5.4f, 4.2f, Ease.InCubic(t, Over, Take - Over));
            row = 3.9f;
        }

        if (t >= Take)
        {
            tiles = 7.8f;
            row = 3.6f;
            eye = _lamp;
        }

        if (t >= Ship)
        {
            tiles = MathHelper.Lerp(7.5f, 5f, Ease.OutCubic(t, Ship, Length - Ship));
            x = _slabX;
            row = _slabRow - 0.3f;
            eye = _crew;
        }

        var shake = Shove(t, Arrive, 0.05f, 7f)
                    + Shove(t, Prod, 0.11f, 7f)
                    + Shove(t, Drop, 0.05f, 9f)
                    + Shove(t, Take, 0.09f, 6f);

        return new Shot(tiles, x, row, eye, shake);
    }

    protected override Color? Wash(float t)
    {
        if (Within(t, Arrive, 0.05f))
            return AlienFlash.WithAlpha(0.5f);

        if (Within(t, Prod, 0.035f))
            return ShockFlash.WithAlpha(0.85f);

        if (Within(t, Take, 0.09f))
            return AlienFlash.WithAlpha(0.8f);

        if (Within(t, Empty, 0.05f))
            return Color.Black;

        if (Within(t, Ship, 0.06f))
            return AlienFlash.WithAlpha(0.35f);

        return null;
    }

    #endregion
}
