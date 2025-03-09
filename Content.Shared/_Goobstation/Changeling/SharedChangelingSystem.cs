using Content.Shared._White.Overlays;
using Content.Shared.Eye.Blinding.Components;

namespace Content.Shared.Changeling;

public abstract class SharedChangelingSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ChangelingComponent, SwitchableOverlayToggledEvent>(OnVisionToggle);
    }

    private void OnVisionToggle(Entity<ChangelingComponent> ent, ref SwitchableOverlayToggledEvent args)
    {
        if (args.User != ent.Owner)
            return;

        if (TryComp(ent, out EyeProtectionComponent? eyeProtection))
            eyeProtection.ProtectionTime = args.Activated ? TimeSpan.Zero : TimeSpan.FromSeconds(10);

        UpdateFlashImmunity(ent, !args.Activated);
    }

    protected virtual void UpdateFlashImmunity(EntityUid uid, bool active) { }
}
