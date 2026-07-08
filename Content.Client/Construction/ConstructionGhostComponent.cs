// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Construction.Prototypes;

namespace Content.Client.Construction
{
    [RegisterComponent]
    public sealed partial class ConstructionGhostComponent : Component
    {
        public int GhostId { get; set; }
        [ViewVariables] public ConstructionPrototype? Prototype { get; set; }
    }
}