using System.Numerics;
using Content.Goobstation.Maths.FixedPoint;
using Content.Pirate.Shared.Stains.Components;
using Content.Pirate.Shared.Wetness.Components;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Inventory;
using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Client.UserInterface;
using Robust.Shared.Enums;

namespace Content.Pirate.Client.Wetness;

/// <summary>
/// Debug overlay for worn wetness and stain reagents.
/// </summary>
public sealed class WetnessDebugOverlay : Overlay
{
    private readonly IEntityManager _entMan;
    private readonly IEyeManager _eye;
    private readonly IUserInterfaceManager _ui;

    private readonly EntityLookupSystem _lookup;
    private readonly InventorySystem _inventory;
    private readonly SharedSolutionContainerSystem _solution;

    private readonly Font _font;
    private readonly Font _fontHeader;

    public override OverlaySpace Space => OverlaySpace.ScreenSpace;

    public WetnessDebugOverlay(IEntityManager entMan, IEyeManager eye, IResourceCache resource, IUserInterfaceManager ui)
    {
        _entMan = entMan;
        _eye = eye;
        _ui = ui;

        _lookup = _entMan.System<EntityLookupSystem>();
        _inventory = _entMan.System<InventorySystem>();
        _solution = _entMan.System<SharedSolutionContainerSystem>();

        _font = new VectorFont(resource.GetResource<FontResource>("/EngineFonts/NotoSans/NotoSansMono-Regular.ttf"), 9);
        _fontHeader = new VectorFont(resource.GetResource<FontResource>("/Fonts/NotoSans/NotoSans-Bold.ttf"), 10);
        ZIndex = 200;
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        var handle = args.ScreenHandle;
        var uiScale = _ui.RootControl.UIScale;
        var lineHeight = 11f * uiScale;

        foreach (var uid in _lookup.GetEntitiesIntersecting(args.MapId, args.WorldBounds))
        {
            if (!_entMan.TryGetComponent<InventoryComponent>(uid, out var inv))
                continue;

            var lines = BuildLines(uid, inv);
            if (lines.Count == 0)
                continue;

            var screen = _eye.WorldToScreen(_lookup.GetWorldAABB(uid).Center).Rounded();
            // Keep the block beside the character.
            var start = new Vector2(screen.X - 280f * uiScale, screen.Y - lineHeight * lines.Count / 2f);

            var offset = Vector2.Zero;
            foreach (var (text, color, header) in lines)
            {
                handle.DrawString(header ? _fontHeader : _font, start + offset, text, uiScale, color);
                offset.Y += lineHeight;
            }
        }
    }

    private List<(string text, Color color, bool header)> BuildLines(EntityUid uid, InventoryComponent inv)
    {
        var lines = new List<(string, Color, bool)>();

        // Bare-body feet/hands state lives on the mob.
        AppendItem(lines, uid, "(body)");

        var enumerator = _inventory.GetSlotEnumerator((uid, inv));
        while (enumerator.NextItem(out var item, out _))
            AppendItem(lines, item, GetName(item));

        if (lines.Count > 0)
            lines.Insert(0, (GetName(uid), Color.White, true));

        return lines;
    }

    private void AppendItem(List<(string, Color, bool)> lines, EntityUid item, string label)
    {
        // Wetness only tracks clean water.
        WettableComponent? wet = null;
        if (_entMan.TryGetComponent<WettableComponent>(item, out var w) && w.Wetness > FixedPoint2.Zero)
            wet = w;

        // Stains come from the item's stain solution.
        StainableComponent? stainComp = null;
        Solution? stainSol = null;
        if (_entMan.TryGetComponent<StainableComponent>(item, out var stain)
            && _solution.TryGetSolution(item, stain.SolutionName, out _, out var sol)
            && sol.Volume > FixedPoint2.Zero)
        {
            stainComp = stain;
            stainSol = sol;
        }

        if (wet == null && stainSol == null)
            return;

        lines.Add((label, Color.LightGray, false));

        if (wet != null)
            lines.Add(($"   wet {wet.Wetness}/{wet.MaxWetness}  (Water)", Color.Cyan, false));

        if (stainSol != null)
        {
            lines.Add(($"   stain {stainSol.Volume}/{stainComp!.MaxStainVolume}", Color.Orange, false));
            foreach (var content in stainSol.Contents)
                lines.Add(($"     {content.Reagent.Prototype} {content.Quantity}", Color.Yellow, false));
        }
    }

    private string GetName(EntityUid uid)
    {
        return _entMan.TryGetComponent<MetaDataComponent>(uid, out var meta) ? meta.EntityName : uid.ToString();
    }
}
