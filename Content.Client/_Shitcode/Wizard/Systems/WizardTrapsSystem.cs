using Content.Shared._Goobstation.Wizard.Traps;
using Robust.Client.GameObjects;

namespace Content.Client._Shitcode.Wizard.Systems;

public sealed class WizardTrapsSystem : SharedWizardTrapsSystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<WizardTrapComponent, AppearanceChangeEvent>(OnAppearanceChange);
    }

    private void OnAppearanceChange(Entity<WizardTrapComponent> ent, ref AppearanceChangeEvent args)
    {
        if (!args.AppearanceData.TryGetValue(TrapVisuals.Alpha, out var alpha))
            return;

        if (args.Sprite is not { } sprite)
            return;

        sprite.Color = sprite.Color.WithAlpha((float) alpha);
    }
}
