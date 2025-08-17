using Content.Shared._CorvaxGoob.CCCVars;
using Content.Shared._CorvaxGoob.Skills;
using Content.Shared.Implants;
using Content.Shared.Tag;
using Robust.Shared.Configuration;

namespace Content.Server._CorvaxGoob.Skills;

public sealed class SkillsSystem : SharedSkillsSystem
{
    [Dependency] private readonly TagSystem _tag = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;

    [ValidatePrototypeId<TagPrototype>]
    public const string SkillsTag = "Skills";

    private bool _skillsEnabled = true;

    public override void Initialize()
    {
        base.Initialize();

        _skillsEnabled = _cfg.GetCVar(CCCVars.SkillsEnabled);
        Subs.CVar(_cfg, CCCVars.SkillsEnabled, value => _skillsEnabled = value);

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

    public bool IsSkillsEnabled()
    {
        return _skillsEnabled;
    }

    public bool HasSkill(EntityUid entity, Shared._CorvaxGoob.Skills.Skills skill, SkillsComponent? component = null)
    {
        if (!_skillsEnabled)
            return true;

        if (!Resolve(entity, ref component, false))
            return false;

        return component.Skills.Contains(skill);
    }
}
