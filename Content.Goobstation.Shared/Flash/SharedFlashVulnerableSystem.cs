using Content.Goobstation.Common.Flash;

namespace Content.Goobstation.Shared.Flash;

public sealed class SharedFlashVulnerableSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<FlashVulnerableComponent, CheckFlashVulnerable>(OnFlashVulnerableCheck);
    }

    public void OnFlashVulnerableCheck(Entity<FlashVulnerableComponent> ent, ref CheckFlashVulnerable args)
    {
        args.Vulnerable = true;
    }
}
