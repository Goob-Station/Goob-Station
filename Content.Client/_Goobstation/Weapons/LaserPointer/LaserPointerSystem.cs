using System.Numerics;
using Content.Client.Gameplay;
using Content.Client.Hands.Systems;
using Content.Shared._Goobstation.Weapons.SmartGun;
using Content.Shared.CombatMode;
using Content.Shared.Hands.Components;
using Content.Shared.Wieldable.Components;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.Input;
using Robust.Client.Player;
using Robust.Client.State;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;

namespace Content.Client._Goobstation.Weapons.LaserPointer;

public sealed class LaserPointerSystem : SharedLaserPointerSystem
{
    [Dependency] private readonly IOverlayManager _overlay = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IEyeManager _eye = default!;
    [Dependency] private readonly IInputManager _input = default!;
    [Dependency] private readonly IStateManager _state = default!;
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly HandsSystem _hands = default!;

    public override void Initialize()
    {
        base.Initialize();

        _overlay.AddOverlay(new LaserPointerOverlay(EntityManager, _prototype));
    }

    public override void Shutdown()
    {
        base.Shutdown();

        _overlay.RemoveOverlay<LaserPointerOverlay>();
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (!Timing.IsFirstTimePredicted)
            return;

        if (!TryComp(_player.LocalEntity, out HandsComponent? hands) ||
            !TryComp(_player.LocalEntity.Value, out TransformComponent? xform))
            return;

        var player = _player.LocalEntity.Value;

        EntityUid? hovered = null;
        MapCoordinates? mousePos = null;
        if (_state.CurrentState is GameplayStateBase screen && TryComp(player, out CombatModeComponent? combat) &&
            combat.IsInCombatMode)
        {
            mousePos = _eye.PixelToMap(_input.MouseScreenPosition);

            if (mousePos.Value.MapId == MapId.Nullspace)
                mousePos = null;
            else
                hovered = screen.GetDamageableClickedEntity(mousePos.Value);
        }

        Vector2? dir = mousePos == null ? null : mousePos.Value.Position - _transform.GetWorldPosition(xform);

        foreach (var held in _hands.EnumerateHeld(player, hands))
        {
            if (!HasComp<LaserPointerComponent>(held))
                continue;

            if (hovered == null || !TryComp(held, out WieldableComponent? wieldable) || !wieldable.Wielded)
            {
                RaisePredictiveEvent(new LaserPointerEntityHoveredEvent(null, dir, GetNetEntity(held)));
                continue;
            }

            RaisePredictiveEvent(
                new LaserPointerEntityHoveredEvent(GetNetEntity(hovered.Value), dir, GetNetEntity(held)));
        }
    }
}
