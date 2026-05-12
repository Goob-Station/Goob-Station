

using Content.Goobstation.Shared.Doodon.Components;
using Content.Goobstation.Shared.Held;
using Content.Server.DoAfter;
using Content.Server.Stack;
using Content.Shared.DoAfter;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Inventory.VirtualItem;
using Content.Shared.Materials;
using Content.Shared.Popups;
using Content.Shared.Stacks;

namespace Content.Goobstation.Server.Doodon;

public sealed class DoodonSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly DoAfterSystem _doAfter = default!;
    [Dependency] private readonly StackSystem _stack = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    public override void Initialize()
    {
    }
}
