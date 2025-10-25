// SPDX-FileCopyrightText: 2025 Doctor-Cpu <77215380+Doctor-Cpu@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GabyChangelog <agentepanela2@gmail.com>
// SPDX-FileCopyrightText: 2025 Will-Oliver-Br <164823659+Will-Oliver-Br@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Client.Clothing;
using Content.Client.Items.Systems;
using Content.Shared.Clothing;
using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Hands;
using Content.Shared.Stains;
using Content.Shared.Forensics;
using Robust.Client.GameObjects;
using Robust.Shared.Reflection;
using Robust.Shared.Prototypes;

namespace Content.Client.Stains;

public sealed partial class StainableSystem : SharedStainableSystem
{
    [Dependency] private readonly IReflectionManager _reflection = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

    private string _layerPrefix = string.Empty;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StainableComponent, AppearanceChangeEvent>(OnAppearanceChanged);
        SubscribeLocalEvent<StainableComponent, GetEquipmentVisualsEvent>(OnClothingVisuals, after: [typeof(ClientClothingSystem)]);
        SubscribeLocalEvent<StainableComponent, GetInhandVisualsEvent>(OnItemVisuals, after: [typeof(ItemSystem)]);

        _layerPrefix = _reflection.GetEnumReference(StainVisualLayers.Layer);
    }

    private void OnAppearanceChanged(Entity<StainableComponent> ent, ref AppearanceChangeEvent args)
    {
        if (args.Sprite is not {} sprite)
            return;

        foreach (var layer in ent.Comp.RevealedIconVisuals)
            sprite.RemoveLayer(layer);

        ent.Comp.RevealedIconVisuals.Clear();

        foreach (var (_, layer) in UpdateVisuals(ent, ent.Comp.IconVisuals))
        {
            var layerId = sprite.AddLayer(layer);
            ent.Comp.RevealedIconVisuals.Add(layerId);
        }
    }

    private void OnClothingVisuals(Entity<StainableComponent> ent, ref GetEquipmentVisualsEvent args)
    {
        if (!ent.Comp.ClothingVisuals.TryGetValue(args.Slot, out var layers))
            return;

        foreach (var (key, layer) in UpdateVisuals(ent, layers, args.Slot))
            args.Layers.Add((key, layer));
    }

    private void OnItemVisuals(Entity<StainableComponent> ent, ref GetInhandVisualsEvent args)
    {
        if (!ent.Comp.ItemVisuals.TryGetValue(args.Location, out var layers))
            return;

        foreach (var (key, layer) in UpdateVisuals(ent, layers, args.Location))
            args.Layers.Add((key, layer));
    }

    private IEnumerable<(string, PrototypeLayerData)> UpdateVisuals(Entity<StainableComponent> ent, List<PrototypeLayerData> layers, object? identifier = null)
    {
        if (!Solution.TryGetSolution(ent.Owner, ent.Comp.SolutionId, out var solution))
            yield break;

        if (solution.Value.Comp.Solution.Volume <= FixedPoint2.Zero)
            yield break;

        var color = solution.Value.Comp.Solution.GetColor(_prototypeManager);

        var prefix = identifier == null
            ? $"{_layerPrefix}"
            : $"{identifier}-{_layerPrefix}";

        for (var i = 0; i < layers.Count; i++)
        {
            var layer = layers[i];
            var key = $"{prefix}-{i}";

            layer.Color = color;

            yield return (key, layer);
        }
    }
}
