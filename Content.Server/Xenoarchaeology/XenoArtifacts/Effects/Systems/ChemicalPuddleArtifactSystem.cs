// SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 slarticodefast <161409025+slarticodefast@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Fluids.EntitySystems;
using Content.Server.Xenoarchaeology.XenoArtifacts.Effects.Components;
using Content.Server.Xenoarchaeology.XenoArtifacts.Events;
using Robust.Shared.Random;

namespace Content.Server.Xenoarchaeology.XenoArtifacts.Effects.Systems;

/// <summary>
/// This handles <see cref="ChemicalPuddleArtifactComponent"/>
/// </summary>
public sealed class ChemicalPuddleArtifactSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly ArtifactSystem _artifact = default!;
    [Dependency] private readonly PuddleSystem _puddle = default!;

    /// <summary>
    /// The key for the node data entry containing
    /// the chemicals that the puddle is made of.
    /// </summary>
    public const string NodeDataChemicalList = "nodeDataChemicalList";

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<ChemicalPuddleArtifactComponent, ArtifactActivatedEvent>(OnActivated);
    }

    private void OnActivated(EntityUid uid, ChemicalPuddleArtifactComponent component, ArtifactActivatedEvent args)
    {
        if (!TryComp<ArtifactComponent>(uid, out var artifact))
            return;

        if (!_artifact.TryGetNodeData(uid, NodeDataChemicalList, out List<string>? chemicalList, artifact))
        {
            chemicalList = new();
            for (var i = 0; i < component.ChemAmount; i++)
            {
                var chemProto = _random.Pick(component.PossibleChemicals);
                chemicalList.Add(chemProto);
            }

            _artifact.SetNodeData(uid, NodeDataChemicalList, chemicalList, artifact);
        }

        var amountPerChem = component.ChemicalSolution.MaxVolume / component.ChemAmount;
        foreach (var reagent in chemicalList)
        {
            component.ChemicalSolution.AddReagent(reagent, amountPerChem);
        }

        _puddle.TrySpillAt(uid, component.ChemicalSolution, out _);
    }
}