using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.Array;
using Robust.Shared.Utility;

namespace Content.Goobstation.Shared.Cinematic;


[Prototype]
public sealed partial class CinematicCaptionStylePrototype : IPrototype, IInheritingPrototype
{
    [IdDataField]
    public string ID { get; set; } = default!;

    [ParentDataField(typeof(AbstractPrototypeIdArraySerializer<CinematicCaptionStylePrototype>))]
    public string[]? Parents { get; set; }

    [NeverPushInheritance]
    [AbstractDataField]
    public bool Abstract { get; set; }

    #region Typography

    [DataField]
    public ResPath FontPath = new("/Fonts/RobotoMono/RobotoMono-Bold.ttf");

    [DataField]
    public ResPath? SubjectFontPath;

    [DataField]
    public int FontSize = 24;

    [DataField]
    public float LineSpacing = 1.3f;

    /// <summary>
    /// Extra space between letters as a fraction of the font size.
    /// </summary>
    [DataField]
    public float Tracking;

    [DataField]
    public float MaxWidthFraction = 0.62f;

    /// <summary>
    /// Where the middle of the caption sits as a fraction of the viewport height.
    /// </summary>
    [DataField]
    public float VerticalPosition = 0.5f;

    [DataField]
    public Color TextColor = Color.FromHex("#05070c");

    [DataField]
    public string Cursor = "_";

    [DataField]
    public float SubjectScale = 0.6f;

    [DataField]
    public float SubjectGap = 0.5f;

    [DataField]
    public float SubjectTracking = 0.16f;

    #endregion

    #region Writing

    [DataField]
    public SoundSpecifier? TextSound = new SoundPathSpecifier("/Audio/_Goobstation/Cinematic/ui_write.ogg");

    [DataField]
    public float TextSoundFadeTime;

    [DataField]
    public float PunctuationPause = 4f;

    [DataField]
    public float IgniteTime = 0.14f;

    [DataField]
    public float FadeOutTime = 0.25f;

    #endregion

    #region Aura

    [DataField]
    public bool AuraEnabled = true;

    [DataField]
    public string AuraShader = "CinematicCaptionAura";

    [DataField]
    public string BlurShader = "CinematicCaptionBlur";

    [DataField]
    public Color HotColor = Color.White;

    [DataField]
    public Color MidColor = Color.FromHex("#9fc4ff");

    [DataField]
    public Color DeepColor = Color.FromHex("#2c4a75");

    [DataField]
    public float ScrimAmount = 0.85f;

    [DataField]
    public float AuraScale = 1f;

    [DataField]
    public float BlurSpread = 1.4f;

    [DataField]
    public float BlurPassSigma = 2f;

    [DataField]
    public float BloomReachFraction = 0.16f;

    [DataField]
    public float PressureReachFraction = 0.60f;

    #endregion

    #region Motion

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

    [DataField]
    public float StepRate;

    #endregion
}
