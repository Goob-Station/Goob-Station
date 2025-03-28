using Content.Shared.EntityEffects;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.Weapons.DelayedKnockdown;

public sealed partial class RemoveComponentEffect : EntityEffect
{
    [DataField]
    public string? Locale;

    [DataField(required: true)]
    public string Component = string.Empty; // riders yelling at me

    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
    {
        return Locale == null ? null : Loc.GetString(Locale);
    }

    public override void Effect(EntityEffectBaseArgs args)
    {
        args.EntityManager.RemoveComponentDeferred(args.TargetEntity,
            args.EntityManager.ComponentFactory.GetRegistration(Component).Type);
    }
}
