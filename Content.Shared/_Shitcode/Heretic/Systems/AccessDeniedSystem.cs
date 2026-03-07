using Content.Goobstation.Common.Heretic;
using Content.Shared._Shitcode.Heretic.Components.StatusEffects;
using Content.Shared.StatusEffectNew;
using Content.Shared.StatusEffectNew.Components;

namespace Content.Shared._Shitcode.Heretic.Systems;

public sealed class AccessDeniedSystem : EntitySystem
{
    [Dependency] private readonly StatusEffectsSystem _status = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StatusEffectContainerComponent, BeforeAccessReaderCheckEvent>(OnBeforeCheck);
    }

    private void OnBeforeCheck(Entity<StatusEffectContainerComponent> ent, ref BeforeAccessReaderCheckEvent args)
    {
        if (_status.HasEffectComp<AccessDeniedStatusEffectComponent>(ent))
            args.Cancelled = true;
    }
}
