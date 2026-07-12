// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Construction.Components;
using Content.Shared.Examine;
using Content.Shared.Lathe;
using Content.Shared.Materials;
using Content.Shared.Stacks;
using Robust.Shared.Prototypes;
using System.Collections;
using System.Linq;

namespace Content.Shared.Construction
{
    /// <summary>
    /// Deals with machine parts and machine boards.
    /// </summary>
    public sealed class MachinePartSystem : EntitySystem
    {
        [Dependency] private readonly IPrototypeManager _prototype = default!;
        [Dependency] private readonly SharedLatheSystem _lathe = default!;
        [Dependency] private readonly SharedConstructionSystem _construction = default!;

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<MachineBoardComponent, ExaminedEvent>(OnMachineBoardExamined);
        }

        private void OnMachineBoardExamined(EntityUid uid, MachineBoardComponent component, ExaminedEvent args)
        {
            if (!args.IsInDetailsRange)
                return;

            using (args.PushGroup(nameof(MachineBoardComponent)))
            {
                args.PushMarkup(Loc.GetString("machine-board-component-on-examine-label"));
                foreach (var (material, amount) in component.StackRequirements)
                {
                    var stack = _prototype.Index(material);
                    var name = _prototype.Index(stack.Spawn).Name;

                    args.PushMarkup(Loc.GetString("machine-board-component-required-element-entry-text",
                        ("amount", amount),
                        ("requiredElement", Loc.GetString(name))));
                }

                foreach (var (_, info) in component.ComponentRequirements)
                {
                    var examineName = _construction.GetExamineName(info);
                    args.PushMarkup(Loc.GetString("machine-board-component-required-element-entry-text",
                        ("amount", info.Amount),
                        ("requiredElement", examineName)));
                }

                foreach (var (_, info) in component.TagRequirements)
                {
                    var examineName = _construction.GetExamineName(info);
                    args.PushMarkup(Loc.GetString("machine-board-component-required-element-entry-text",
                        ("amount", info.Amount),
                        ("requiredElement", examineName)));
                }

                if (!CanGetMachineBoardCost((uid, component))) // Goobstation
                {
                    args.PushMarkup(Loc.GetString("machine-board-cannot-be-flatpacked"));
                }
            }
        }

        public bool TryGetMachineBoardMaterialCost(Entity<MachineBoardComponent> entity, out Dictionary<string, int> materials, int coefficient = 1)
        {
            var (_, comp) = entity;

            materials = new Dictionary<string, int>();

            foreach (var (stackId, amount) in comp.StackRequirements)
            {
                var stackProto = _prototype.Index(stackId);
                var defaultProto = _prototype.Index(stackProto.Spawn);

                if (defaultProto.TryGetComponent<PhysicalCompositionComponent>(out var physComp, EntityManager.ComponentFactory))
                {
                    foreach (var (mat, matAmount) in physComp.MaterialComposition)
                    {
                        materials.TryAdd(mat, 0);
                        materials[mat] += matAmount * amount * coefficient;
                    }
                }
                else if (_lathe.TryGetRecipesFromEntity(stackProto.Spawn, out var recipes))
                {
                    var partRecipe = recipes[0];
                    if (recipes.Count > 1)
                        partRecipe = recipes.MinBy(p => p.Materials.Values.Sum());

                    foreach (var (mat, matAmount) in partRecipe!.Materials)
                    {
                        materials.TryAdd(mat, 0);
                        materials[mat] += matAmount * amount * coefficient;
                    }
                }
                else
                {
                    // The item has no material cost, so we cannot get the full cost.
                    return false;
                }
            }

            var genericPartInfo = comp.ComponentRequirements.Values.Concat(comp.TagRequirements.Values);
            foreach (var info in genericPartInfo)
            {
                var amount = info.Amount;
                var defaultProtoId = info.DefaultPrototype;

                if (_lathe.TryGetRecipesFromEntity(defaultProtoId, out var recipes))
                {
                    var partRecipe = recipes[0];
                    if (recipes.Count > 1)
                        partRecipe = recipes.MinBy(p => p.Materials.Values.Sum());

                    foreach (var (mat, matAmount) in partRecipe!.Materials)
                    {
                        materials.TryAdd(mat, 0);
                        materials[mat] += matAmount * amount * coefficient;
                    }
                }
                else if (_prototype.Resolve(defaultProtoId, out var defaultProto) &&
                         defaultProto.TryGetComponent<PhysicalCompositionComponent>(out var physComp, EntityManager.ComponentFactory))
                {
                    foreach (var (mat, matAmount) in physComp.MaterialComposition)
                    {
                        materials.TryAdd(mat, 0);
                        materials[mat] += matAmount * amount * coefficient;
                    }
                }
                else
                {
                    // The item has no material cost, so we cannot get the full cost.
                    return false;
                }
            }

            // We were able to construct all elements of the recipe.
            return true;
        }

        /// <summary>
        /// Goobstation - Check whether if entity has recipe lathe or physical composition for all of it's material
        /// </summary>
        /// <param name="ent">The entity</param>
        /// <returns></returns>
        public bool CanGetMachineBoardCost(Entity<MachineBoardComponent> ent)
        {
            foreach (var (stackId, _) in ent.Comp.StackRequirements)
            {
                var stackProto = _prototype.Index(stackId);
                var defaultProto = _prototype.Index(stackProto.Spawn);

                if (defaultProto.TryGetComponent<PhysicalCompositionComponent>(out var physComp, EntityManager.ComponentFactory))
                {
                    continue;
                }
                else if (_lathe.CanGetRecipesFromEntity(stackProto.Spawn))
                {
                    continue;
                }
                else
                {
                    // The item has no material cost, so we cannot get the full cost.
                    return false;
                }
            }

            var genericPartInfo = ent.Comp.ComponentRequirements.Values.Concat(ent.Comp.TagRequirements.Values);
            foreach (var info in genericPartInfo)
            {
                var defaultProtoId = info.DefaultPrototype;

                if (_lathe.CanGetRecipesFromEntity(defaultProtoId))
                {
                    continue;
                }
                else if (_prototype.Resolve(defaultProtoId, out var defaultProto) &&
                         defaultProto.TryGetComponent<PhysicalCompositionComponent>(out var physComp, EntityManager.ComponentFactory))
                {
                    continue;
                }
                else
                {
                    // The item has no material cost, so we cannot get the full cost.
                    return false;
                }
            }

            return true;
        }
    }
}
