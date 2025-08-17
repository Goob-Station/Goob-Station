namespace Content.Shared._CorvaxGoob.Skills;

public abstract class SharedSkillsSystem : EntitySystem
{
    public void GrantAllSkills(EntityUid entity, SkillsComponent? component = null)
    {
        component ??= EnsureComp<SkillsComponent>(entity);

        component.Skills.UnionWith(Enum.GetValues<Skills>());

        Dirty(entity, component);
    }
}
