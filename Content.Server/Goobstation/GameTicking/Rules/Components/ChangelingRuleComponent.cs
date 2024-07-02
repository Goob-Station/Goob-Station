using Content.Shared.NPC.Prototypes;
using Content.Shared.Roles;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Server.GameTicking.Rules.Components;

[RegisterComponent, Access(typeof(ChangelingRuleSystem))]
public sealed partial class ChangelingRuleComponent : Component
{
    public readonly List<EntityUid> ChangelingMinds = new();

    public SoundSpecifier BriefingSound = new SoundPathSpecifier("/Audio/Goobstation/Ambience/Antag/changeling_start.ogg");

    public ProtoId<AntagPrototype> ChangelingPrototypeId = "Changeling";

    public ProtoId<NpcFactionPrototype> ChangelingFactionId = "Changeling";

    public ProtoId<NpcFactionPrototype> NanotrasenFactionId = "NanoTrasen";

    public List<ProtoId<EntityPrototype>> BaseChangelingActions = new()
    {
        "ActionEvolutionMenu",
        "ActionAbsorbDNA",
        "ActionStingExtractDNA",
        "ActionChangelingTransform",
        "ActionEnterStasis"
    };
}
