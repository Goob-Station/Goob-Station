using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Audio;
using Content.Shared.Humanoid;
namespace Content.Shared.ERP;

[Prototype("interaction")]
public sealed partial class ERPPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField(required: true)]
    public string Name = default!;

    [DataField]
    public SpriteSpecifier Icon = new SpriteSpecifier.Texture(new("/Textures/Casha/ERPicon/fon.png"));

    [DataField("sound")]
    public List<SoundSpecifier> Sounds = new();

    [DataField] public HashSet<string> Emotes = new();

    [DataField]
    public Sex UserSex = Sex.Unsexed;

    [DataField]
    public Sex TargetSex = Sex.Unsexed;

    [DataField]
    public bool UserWithoutCloth = false;

    [DataField]
    public bool TargetWithoutCloth = false;

    [DataField] public bool Erp = false;

    [DataField] public int LovePercent = 0; 
}
