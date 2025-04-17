using Content.Shared._Sunrise.Interrogator;
using Content.Shared.Verbs;
using Robust.Client.GameObjects;
using DrawDepth = Content.Shared.DrawDepth.DrawDepth;

namespace Content.Client._Sunrise.Interrogator;

public sealed class InterrogatorSystem: SharedInterrogatorSystem
{
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<InterrogatorComponent, ComponentInit>(OnComponentInit);
        SubscribeLocalEvent<InterrogatorComponent, GetVerbsEvent<AlternativeVerb>>(AddAlternativeVerbs);

        SubscribeLocalEvent<InterrogatorComponent, AppearanceChangeEvent>(OnAppearanceChange);
    }

    private void OnAppearanceChange(EntityUid uid, InterrogatorComponent component, ref AppearanceChangeEvent args)
    {
        if (args.Sprite == null)
        {
            return;
        }

        if (!_appearance.TryGetData<bool>(uid, InterrogatorComponent.InterrogatorVisuals.ContainsEntity, out var isOpen, args.Component)
            || !_appearance.TryGetData<bool>(uid, InterrogatorComponent.InterrogatorVisuals.IsOn, out var isOn, args.Component))
        {
            return;
        }

        if (isOpen)
        {
            args.Sprite.LayerSetState(InterrogatorVisualLayers.Base, "open");
            args.Sprite.LayerSetVisible(InterrogatorVisualLayers.Extract, false);
            args.Sprite.DrawDepth = (int) DrawDepth.Objects;
        }
        else
        {
            args.Sprite.DrawDepth = (int) DrawDepth.Mobs;
            args.Sprite.LayerSetState(InterrogatorVisualLayers.Extract, isOn ? "extraction-on" : "extraction-off");
            args.Sprite.LayerSetVisible(InterrogatorVisualLayers.Extract, true);
        }
    }
}

public enum InterrogatorVisualLayers : byte
{
    Base,
    Extract,
}
