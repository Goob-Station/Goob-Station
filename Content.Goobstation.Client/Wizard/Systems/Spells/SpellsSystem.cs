using Content.Goobstation.Shared.Wizard.Systems.Spells;
using Content.Shared._Goobstation.Wizard.SupermatterHalberd;
using Robust.Client.GameObjects;
using Robust.Shared.Timing;

namespace Content.Goobstation.Client.Wizard.Spells.Systems;

public sealed partial class SpellsSystem : SharedSpellsSystem
{
    [Dependency] private readonly RaysSystem _rays = default!;
    [Dependency] private readonly TransformSystem _xform = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
}