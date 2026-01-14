using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.Input;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Client.Cult;

public sealed class BloodCultRuneScribeBUI(EntityUid owner, Enum uiKey) : BoundUserInterface(owner, uiKey)
{
    [Dependency] private EntityManager _ent = default!;
    [Dependency] private IClyde _clyde = default!;
    [Dependency] private IPrototypeManager _prot = default!;
    [Dependency] private IInputManager _input = default!;

    private readonly SpriteSystem? _sprite;
}
