using Content.Shared._Corvax.Skills;
using Content.Shared.Implants;
using Content.Shared.Tag;

namespace Content.Server._Corvax.Skills;

public sealed class SkillsSystem : SharedSkillsSystem
{
    [Dependency] private readonly TagSystem _tag = default!;

    [ValidatePrototypeId<TagPrototype>]
    public const string SkillsTag = "Skills";

    public override void Initialize()
    {
        SubscribeLocalEvent<ImplantImplantedEvent>(OnImplantImplanted);
    }

    private void OnImplantImplanted(ref ImplantImplantedEvent e)
    {
        if (e.Implanted is null)
            return;

        if (!_tag.HasTag(e.Implant, SkillsTag))
            return;

        GrantAllSkills(e.Implanted.Value);
    }
}
