using Content.Goobstation.Shared.Wizard.Systems;
using Content.Goobstation.Shared.Wizard.Systems.Spells;
using Robust.Client.GameObjects;

namespace Content.Goobstation.Client.Wizard.Systems.Spells;

public sealed partial class SpellsSystem : SharedSpellsSystem
{
    [Dependency] private readonly TransformSystem _xform = default!;
    [Dependency] private readonly RaysSystem _rays = default!;
}