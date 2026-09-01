using Content.Goobstation.Common.Wizard.Events;
using Content.Goobstation.Shared.Wizard.Systems;
using Content.Goobstation.Shared.Wizard.Systems.Spells;
using Robust.Client.GameObjects;
using Robust.Client.Player;

namespace Content.Goobstation.Client.Wizard.Spells.Systems;

public sealed partial class SpellsSystem : SharedSpellsSystem
{
    [Dependency] private readonly TransformSystem _xform = default!;
    [Dependency] private readonly RaysSystem _rays = default!;
}