using Content.Shared._White.FootPrint;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Shared.Random;

namespace Content.Client._White.FootPrint;

public sealed class FootPrintsVisualizerSystem : VisualizerSystem<FootPrintComponent>
{
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<FootPrintComponent, ComponentInit>(OnInitialized);
        SubscribeLocalEvent<FootPrintComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnInitialized(EntityUid uid, FootPrintComponent comp, ComponentInit args)
    {
        if (!TryComp<SpriteComponent>(uid, out var sprite))
            return;

        sprite.LayerMapReserveBlank(FootPrintVisualLayers.Print);
        UpdateAppearance(uid, comp, sprite);
    }

    private void OnShutdown(EntityUid uid, FootPrintComponent comp, ComponentShutdown args)
    {
        if (TryComp<SpriteComponent>(uid, out var sprite) &&
            sprite.LayerMapTryGet(FootPrintVisualLayers.Print, out var layer))
        {
            sprite.RemoveLayer(layer);
        }
    }

    private void UpdateAppearance(EntityUid uid, FootPrintComponent component, SpriteComponent sprite)
    {
        if (!sprite.LayerMapTryGet(FootPrintVisualLayers.Print, out var layer)
            || !TryComp<FootPrintsComponent>(component.PrintOwner, out var printsComponent)
            || !TryComp<AppearanceComponent>(uid, out var appearance))
            return;

        if (!_appearance.TryGetData<FootPrintVisuals>(uid, FootPrintVisualState.State, out var printVisuals, appearance))
            return;

        sprite.LayerSetState(layer, new RSI.StateId(printVisuals switch
        {
            FootPrintVisuals.BareFootPrint => printsComponent.RightStep ? printsComponent.RightBarePrint : printsComponent.LeftBarePrint,
            FootPrintVisuals.ShoesPrint => printsComponent.ShoesPrint,
            FootPrintVisuals.SuitPrint => printsComponent.SuitPrint,
            FootPrintVisuals.Dragging => _random.Pick(printsComponent.DraggingPrint),
            _ => throw new ArgumentOutOfRangeException($"Unknown {printVisuals} parameter.")
        }), printsComponent.RsiPath);

        if (_appearance.TryGetData<Color>(uid, FootPrintVisualState.Color, out var printColor, appearance))
            sprite.LayerSetColor(layer, printColor);
    }

    protected override void OnAppearanceChange (EntityUid uid, FootPrintComponent component, ref AppearanceChangeEvent args)
    {
        if (args.Sprite is not { } sprite)
            return;

        UpdateAppearance(uid, component, sprite);
    }
}
