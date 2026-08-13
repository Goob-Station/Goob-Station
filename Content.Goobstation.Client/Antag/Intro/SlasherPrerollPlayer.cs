using System.Numerics;
using Content.Shared.Doors.Components;
using Content.Shared.Humanoid;
using Content.Shared.Light;
using Robust.Shared.Audio;

/// <summary>
/// Slashers intro.
/// </summary>
namespace Content.Goobstation.Client.Antag.Intro;

public sealed class SlasherPrerollPlayer : AntagIntroScene
{
    #region Timing

    private const float Tick1 = 0.265f;
    private const float Tick2 = 0.420f;
    private const float Beat0 = 0.600f;
    private const float BeatStep = 0.3357f;
    private const float Slam0 = Beat0;
    private const float Slam1 = Beat0 + BeatStep;
    private const float Slam2 = Beat0 + BeatStep * 2f;
    private const float Look2 = Beat0 + BeatStep * 4f;
    private const float Swipe0 = Beat0 + BeatStep * 5f;
    private const float Swipe1 = Beat0 + BeatStep * 6f;
    private const float Swipe2 = Beat0 + BeatStep * 7f;
    private const float Turn1 = 1.430f;
    private const float Back1 = 2.090f;
    private const float Cross = 2.400f;
    private const float Beep0 = 3.030f;
    private const float Beep1 = 3.100f;
    private const float Beep2 = 3.215f;
    private const float Pry0 = 3.305f;
    private const float Pry = 3.540f;
    private const float Back2 = 3.720f;
    private const float Turn2 = 3.820f;
    private const float Blow = 4.135f;
    private const float Emerg = 4.245f;
    private const float Seen = 4.480f;
    private const float Step1 = 5.690f;
    private const float Step2 = 5.915f;
    private const float Hit = 6.135f;
    private const float Cut0 = 6.260f;
    private const float Cut1 = 6.405f;
    private const float Cut2 = 6.535f;
    private const float Kill = 6.845f;
    private const float Turn3 = 7.320f;
    private const float Duration = 7.900f;
    private static readonly float[] Slams = { Slam0, Slam1, Slam2 };
    private static readonly float[] Reaches = { Swipe0, Swipe1, Swipe2, Beep0, Beep1, Beep2 };
    private static readonly float[] Strokes = { Hit + 0.06f, Cut0, Cut1, Cut2, Kill };
    private static readonly (float At, float Amp, float Decay)[] Knocks =
    {
        (Slam0, 0.05f, 10f),
        (Slam1, 0.05f, 10f),
        (Slam2, 0.05f, 10f),
        (Pry, 0.08f, 10f),
        (Hit, 0.10f, 6f),
        (Cut0, 0.07f, 6f),
        (Cut1, 0.07f, 6f),
        (Cut2, 0.07f, 6f),
        (Kill, 0.13f, 5f),
    };

    #endregion

    #region Sound

    private static readonly SoundSpecifier Body = Sound("/Audio/Effects/Footsteps/largethud.ogg", -1f);
    private static readonly SoundSpecifier Deny = Sound("/Audio/Machines/airlock_deny.ogg", -4f);
    private static readonly SoundSpecifier Slammed = Sound("/Audio/Machines/airlock_close.ogg", 1f);
    private static readonly SoundSpecifier Slice = Sound("/Audio/Weapons/bladeslice.ogg", 2f);
    private static readonly SoundSpecifier Footstep = Pick("FootstepFloor", -4f);
    private static readonly SoundSpecifier Heavy = Pick("FootstepHeavy", 2f);
    private static readonly SoundSpecifier Gib1 = Sound("/Audio/Effects/gib1.ogg", -1f);
    private static readonly SoundSpecifier Gib2 = Sound("/Audio/Effects/gib2.ogg", -1f);
    private static readonly SoundSpecifier Gib3 = Sound("/Audio/Effects/gib3.ogg", -1f);

    #endregion

    #region Set

    private const int FarEnd = 9;
    private const float AssistRow = 1.0f;
    private static readonly Color Unlit = Color.FromHex("#0a0a0d");
    private static readonly Color Tube = Color.FromHex("#e8dcc2");
    private static readonly Color EmergRed = Color.FromHex("#ff2a1c");
    private const float LampRange = 7.5f;
    private const float LampEnergy = 1.3f;
    private EntityUid _door;
    private EntityUid _lamp;
    private EntityUid _emergency;
    private EntityUid _assistant;
    private EntityUid _slasher;
    private EntityUid _machete;

    protected override float Length => Duration;

    #endregion

    #region Building

    protected override void Set()
    {
        Ambient(Unlit);

        var run = Maintenance(FarEnd, Tube, LampRange);
        _door = run.Door;
        _lamp = run.Lamp;
        _emergency = Lamp("PoweredSmallLight", Lane, 3, Direction.West, EmergRed, 14f);
        _assistant = Crewman(0f, AssistRow);
        _slasher = Cast(SharedHumanoidAppearanceSystem.DefaultSpecies, "SlasherGear", 0f, FarEnd - 1.4f, out _);
        Wounds(_assistant);
        _machete = Arm(_slasher, "SlasherMachete");
    }

    #endregion

    #region Script

    protected override IEnumerable<Cue> Script() => new[]
    {
        new Cue(Slam0, Body, Refuse), new Cue(Slam0, Deny),
        new Cue(Slam1, Body, Refuse), new Cue(Slam1, Deny),
        new Cue(Slam2, Body, Refuse), new Cue(Slam2, Deny),
        new Cue(Swipe0, Deny, Refuse),
        new Cue(Swipe1, Deny, Refuse),
        new Cue(Swipe2, Deny, Refuse),
        new Cue(Beep0, Deny, Refuse),
        new Cue(Beep1, Deny, Refuse),
        new Cue(Beep2, Deny, Refuse),
        new Cue(Pry, Slammed, Give),
        new Cue(Pry + 0.16f, Then: Shut),
        new Cue(Back2, Footstep),
        new Cue(Back2 + 0.19f, Footstep),
        new Cue(Blow, Then: Dark),
        new Cue(Emerg, Then: Red),
        new Cue(Step1, Heavy),
        new Cue(Step2, Heavy),
        new Cue(Hit, Heavy, Stroke), new Cue(Hit, Slice),
        new Cue(Hit + 0.04f, Then: () => Emote(_assistant, ScreamEmote)),
        new Cue(Cut0, Slice, Stroke),
        new Cue(Cut1, Slice, Stroke),
        new Cue(Cut2, Slice, Stroke),
        new Cue(Kill, Slice, Finish),
        new Cue(6.980f, Gib1, Butcher),
        new Cue(7.145f, Gib2, Butcher),
        new Cue(7.310f, Gib3, Butcher),
    };

    /// <summary>
    /// Plays the door access denied animation.
    /// </summary>
    private void Refuse()
    {
        if (Entities.TryGetComponent<DoorComponent>(_door, out var door))
            Animate(_door, door.DenyingAnimation, DoorComponent.AnimationKey);
    }

    /// <summary>
    /// Starts opening then shuts the door.
    /// </summary>
    private void Give()
    {
        if (Entities.TryGetComponent<DoorComponent>(_door, out var door))
            Animate(_door, door.OpeningAnimation, DoorComponent.AnimationKey);

        Place("EffectSparks", 0f, 0.4f);
    }

    private void Shut()
    {
        Animations.Stop(_door, DoorComponent.AnimationKey);
        Appearance.SetData(_door, DoorVisuals.State, DoorState.Closed);
    }

    /// <summary>
    /// Light stuff.
    /// </summary>
    private void Dark() => Burn(_lamp, 0f, PoweredLightState.Broken);

    private void Red()
    {
        Burn(_emergency, 0.1f, PoweredLightState.On);
        Appearance.SetData(_door, DoorVisuals.EmergencyLights, true);
    }

    /// <summary>
    /// Plays the melee strike animation.
    /// </summary>
    private void Stroke()
    {
        Strike(_slasher, _machete, _assistant);
        Hurt(_assistant);
    }

    /// <summary>
    /// Knocks down the target.
    /// </summary>
    private void Finish()
    {
        Stroke();
        Bleed(_assistant);
        Fall(_assistant);
        Spill(1.4f, 2.2f, Lane, 4);
    }

    /// <summary>
    /// Keeps spilling blood after the target is dead.
    /// </summary>
    private void Butcher()
    {
        Hurt(_assistant);
        Spill(1.2f, 2.6f, Lane, 2);
    }

    #endregion

    #region Direction

    protected override void Direct(float t)
    {
        Fitting(t);
        People(t);
    }

    private void Fitting(float t)
    {
        if (t < Blow)
            Burn(_lamp, LampEnergy * LampGain(t));

        if (t < Emerg)
            return;

        var red = 0.9f * Ramp(t, Emerg, 0.45f) * (0.76f + 0.24f * MathF.Sin((t - Emerg) * 2.4f));
        red *= MathHelper.Lerp(1f, 1.8f, Ease.OutCubic(t, Turn3, 0.35f));

        Burn(_emergency, red, PoweredLightState.On);
    }

    /// <summary>
    /// The lamp flickers.
    /// </summary>
    private static float LampGain(float t)
    {
        if (t < Tick1 || t >= Blow)
            return 0f;

        if (t < Tick2 + 0.085f)
            return Within(t, Tick1, 0.085f) || Within(t, Tick2, 0.085f) ? 1f : 0.05f;

        if (Within(t, Cross, 0.09f))
            return 1.7f;

        var gain = MathHelper.Lerp(0.9f, 0.5f, Ramp(t, Pry0, Blow - Pry0));

        if (t > Blow - 0.22f && Hash01((int) (t * 70f)) < 0.45f)
            gain *= 0.25f;

        return gain;
    }

    /// <summary>
    /// Where the two of them are and which way they are facing.
    /// </summary>
    private void People(float t)
    {
        var (x, row, face) = Assistant(t);

        Stand(_assistant, x, row);

        if (face is { } facing)
            Face(_assistant, facing);

        var (sx, sRow, sFace) = Slasher(t);

        Stand(_slasher, sx, sRow);
        Face(_slasher, sFace);
    }

    private static (float X, float Row, Direction? Face) Assistant(float t)
    {
        var x = 0f;
        var row = AssistRow;
        Direction? face = Direction.North;

        foreach (var slam in Slams)
            row -= 0.13f * Jolt(t, slam, 9f, 0.3f);

        if (t >= Turn1 && t < Back1)
        {
            face = Direction.South;
            row += 0.06f * Jolt(t, Look2, 11f, 0.25f);
        }

        foreach (var at in Reaches)
        {
            row -= 0.075f * (t < at
                ? Ramp(t, at - 0.1f, 0.1f)
                : Jolt(t, at, 10f, 0.26f));
        }

        if (t >= Pry0 && t < Pry)
        {
            row -= 0.16f * Ramp(t, Pry0, Pry - Pry0);
            x += 0.02f * MathF.Sin(t * 41f);
        }

        row += 0.24f * Jolt(t, Pry, 8f, 0.3f);

        if (t >= Back2)
            row = MathHelper.Lerp(AssistRow, AssistRow + 0.42f, Ease.OutCubic(t, Back2, 0.34f));

        if (t >= Turn2)
            face = Direction.South;

        if (t >= Hit)
        {
            var p = Ease.OutCubic(t, Hit, 0.35f);
            x = 0.38f * p;
            row = MathHelper.Lerp(row, 0.92f, p);
        }

        if (t >= Kill)
        {
            var p = Ease.OutCubic(t, Kill, 0.45f);
            x = MathHelper.Lerp(0.38f, 0.52f, p);
            row = MathHelper.Lerp(0.92f, 1.24f, p);
            face = null;
        }

        return (x, row, face);
    }

    /// <summary>
    /// Where the slasher is.
    /// </summary>
    private static (float X, float Row, Direction Face) Slasher(float t)
    {
        var face = t >= Turn3 ? Direction.South : Direction.North;

        if (Within(t, Cross, 0.34f))
            return (MathHelper.Lerp(-1.6f, 1.6f, (t - Cross) / 0.34f), FarEnd - 0.6f, face);

        var x = 0f;
        var row = t < Seen ? FarEnd - 0.6f : FarEnd - 1.4f;

        row -= 0.62f * Ease.OutCubic(t, Step1, 0.26f);
        row -= 0.72f * Ease.OutCubic(t, Step2, 0.24f);

        if (t >= Hit)
        {
            var p = Ease.OutQuint(t, Hit, 0.1f);
            x = MathHelper.Lerp(0f, -0.46f, p);
            row = MathHelper.Lerp(FarEnd - 2.74f, 1.85f, p);
        }

        if (t >= Kill)
        {
            var p = Ease.OutCubic(t, Kill, 0.5f);
            x = MathHelper.Lerp(-0.46f, -0.34f, p);
            row = MathHelper.Lerp(1.85f, 1.72f, p);
        }

        foreach (var stroke in Strokes)
        {
            if (!Within(t, stroke, 0.22f))
                continue;

            var k = MathF.Pow(MathF.Sin(MathF.PI * (t - stroke) / 0.22f), 0.6f);
            x += 0.3f * k;
            row -= 0.05f * k;
        }

        return (x, row, face);
    }

    #endregion

    #region Camera

    /// <summary>
    /// The frame...
    /// </summary>
    protected override Shot Frame(float t)
    {
        var tiles = 9.8f;
        var row = 3.8f;

        tiles -= 0.9f * Ease.InCubic(t, Pry, Hit - Pry);
        tiles -= 0.5f * Ease.OutCubic(t, Step1, 0.22f);
        tiles -= 0.6f * Ease.OutCubic(t, Step2, 0.22f);

        if (t >= Hit)
        {
            var stage = t >= Cut2 ? 3 : t >= Cut1 ? 2 : t >= Cut0 ? 1 : 0;
            tiles = 5.2f - stage * 0.7f;
            row = 1.6f;
        }

        if (t >= Kill)
        {
            tiles = MathHelper.Lerp(2.6f, 3.6f, Ease.OutCubic(t, Kill, 0.6f));
            row = 1.7f;
        }

        if (t >= Turn3)
            tiles = MathHelper.Lerp(3.6f, 2.6f, Ease.OutCubic(t, Turn3, Duration - Turn3));

        var shake = Vector2.Zero;

        foreach (var (at, amp, decay) in Knocks)
            shake += Shove(t, at, amp, decay);

        return new Shot(tiles, 0f, row, _assistant, shake);
    }

    /// <summary>
    /// The cut to white.
    /// </summary>
    protected override Color? Wash(float t)
    {
        if (Within(t, Hit, 0.03f))
            return new Color(1f, 0.93f, 0.9f, 0.92f);

        if (Within(t, Cut0, 0.02f) || Within(t, Cut1, 0.02f) || Within(t, Cut2, 0.02f))
            return EmergRed.WithAlpha(0.42f);

        if (Within(t, Kill, 0.028f))
            return new Color(1f, 0.96f, 0.94f, 0.8f);

        return null;
    }

    #endregion
}
