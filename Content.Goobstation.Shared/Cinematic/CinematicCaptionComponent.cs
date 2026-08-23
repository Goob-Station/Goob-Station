using Robust.Shared.Audio;
using Robust.Shared.Utility;

namespace Content.Goobstation.Shared.Cinematic;

/// <summary>
/// A caption typed onto the owning players screen.
/// </summary>
[RegisterComponent]
public sealed partial class CinematicCaptionComponent : Component
{
    /// <summary>
    /// Largest line height in pixels.
    /// If it goes above 256 the engine will murder you.
    /// Going below 8 makes everything illegible.
    /// Purposely not datafields so no one messes with them.
    /// </summary>
    public const int MaxGlyphSheetExtent = 248;
    public const int MinGlyphRasterSize = 8;

    [DataField(required: true)]
    public LocId Text;

    [DataField]
    public float WriteTime = 1f;

    [DataField]
    public string Cursor = "_";

    [DataField]
    public SoundSpecifier? TextSound = new SoundPathSpecifier("/Audio/_Goobstation/Cinematic/ui_write.ogg");

    /// <summary>
    /// Subject name, if any, shown above the actual text of the caption.
    /// </summary>
    [DataField]
    public bool ShowSubject;

    [DataField]
    public ResPath FontPath = new("/Fonts/RobotoMono/RobotoMono-Bold.ttf");

    [DataField]
    public int FontSize = 24;

    [DataField]
    public float LineSpacing = 1.3f;

    /// <summary>
    /// Extra space between letters.
    /// </summary>
    [DataField]
    public float Tracking;

    /// <summary>
    /// How far the text goes before it starts the next line.
    /// As a fraction of the viewport width.
    /// </summary>
    [DataField]
    public float MaxWidthFraction = 0.62f;

    /// <summary>
    /// Where the middle of the caption sits.
    /// as a fraction of the viewport height.
    /// </summary>
    [DataField]
    public float VerticalPosition = 0.5f;

    [DataField]
    public Color TextColor = Color.FromHex("#05070c");

    [DataField]
    public float SubjectScale = 0.6f;

    [DataField]
    public float SubjectGap = 0.5f;

    [DataField]
    public float SubjectTracking = 0.16f;

    #region Aura flame stuff used for heretics text

    [DataField]
    public bool AuraEnabled = true;

    [DataField]
    public string AuraShader = "HereticCaption";

    [DataField]
    public string BlurShader = "HereticBlur";

    [DataField]
    public Color HotColor = Color.White;

    [DataField]
    public Color MidColor = Color.FromHex("#9fc4ff");

    [DataField]
    public Color DeepColor = Color.FromHex("#2c4a75");

    /// <summary>
    /// How hard the scene behind the caption is pushed down.
    /// 0 disables it.
    /// </summary>
    [DataField]
    public float ScrimAmount = 0.85f;

    /// <summary>
    /// Reach of the aura.
    /// As a multiplier of the font size.
    /// </summary>
    [DataField]
    public float AuraScale = 1f;

    [DataField]
    public int BlurLevelCount = 6;

    [DataField]
    public float BlurSpread = 1.4f;

    [DataField]
    public float BlurPassSigma = 2f;

    [DataField]
    public float BloomReachFraction = 0.16f;

    [DataField]
    public float PressureReachFraction = 0.60f;

    #endregion

    [DataField]
    public float Shake;

    [DataField]
    public float LetterWave;

    [DataField]
    public float WaveSpeed = 1f;

    [DataField]
    public float SlamScale;

    [DataField]
    public float SlamDecay = 5f;

    [DataField]
    public float Kick;

    [DataField]
    public float KickTime;

    [DataField]
    public float KickDecay = 9f;

    [DataField]
    public float Throb;

    /// <summary>
    /// Speed of the texts animation.
    /// </summary>
    [DataField]
    public float StepRate;

    [DataField]
    public float IgniteTime = 0.14f;

    // Everything below is filled in as the caption runs.

    [DataField]
    public string Target = string.Empty;

    [DataField]
    public string Subject = string.Empty;

    [DataField]
    public float Progress;

    [DataField]
    public float Age;

    [DataField]
    public EntityUid? Stream;
}
