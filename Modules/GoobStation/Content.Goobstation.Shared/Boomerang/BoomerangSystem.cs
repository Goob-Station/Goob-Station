// SPDX-FileCopyrightText: 2025 ActiveMammmoth <140334666+ActiveMammmoth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 ActiveMammmoth <kmcsmooth@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Ilya246 <ilyukarno@gmail.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 SX-7 <sn1.test.preria.2002@gmail.com>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 keronshb <54602815+keronshb@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Hands.EntitySystems;
using Content.Shared.Throwing;
using Robust.Shared.Map;

namespace Content.Goobstation.Shared.Boomerang;

public sealed class BoomerangSystem : EntitySystem
{
    [Dependency] private readonly ThrowingSystem _throwingSystem = default!;
    [Dependency] private readonly SharedHandsSystem _handsSystem = default!;

    private List<(EntityUid, EntityCoordinates, float, EntityUid?)> _toThrow = new();

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<BoomerangComponent, LandEvent>(OnLanded);
        SubscribeLocalEvent<BoomerangComponent, ThrownEvent>(OnThrown);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        foreach (var (uid, coords, speed, thrower) in _toThrow)
        {
            if (!TerminatingOrDeleted(uid) && (thrower == null || !TerminatingOrDeleted(thrower)))
                _throwingSystem.TryThrow(uid, coords, speed, user: thrower, recoil: false, playSound: false);
        }

        _toThrow.Clear();
    }

    private void OnThrown(Entity<BoomerangComponent> ent, ref ThrownEvent args)
    {
        if (ent.Comp.Thrower == null)
            SetThrower(ent, args.User);
    }

    private void OnLanded(Entity<BoomerangComponent> ent, ref LandEvent args)
    {
        if (ent.Comp.Thrower == null)
            return;

        var thrower = ent.Comp.Thrower.Value;

        if (TerminatingOrDeleted(thrower) || ent.Comp.CurrentHops >= ent.Comp.MaxHops)
        {
            SetThrower(ent, null);
            return;
        }

        var xform = Transform(ent);
        var throwerXform = Transform(thrower);

        if (!xform.Coordinates.TryDistance(EntityManager, throwerXform.Coordinates, out var distance))
        {
            SetThrower(ent, null);
            return;
        }

        if (distance < ent.Comp.PickupDistance)
        {
            // if we fail to pick up throw with no user so it can hit you
            if (!_handsSystem.TryPickup(thrower, ent))
                _toThrow.Add((ent, throwerXform.Coordinates, ent.Comp.ReturnSpeed, null));

            SetThrower(ent, null); // don't throw it anymore
            return;
        }

        // everything is fine and it's out-of-range, re-throw to thrower on next frame (or it breaks)
        _toThrow.Add((ent, throwerXform.Coordinates, ent.Comp.ReturnSpeed, thrower));
        ent.Comp.CurrentHops++;
    }

    /// <summary>
    /// Sets the entity a boomerang should return to and resets the hops counter
    /// </summary>
    public void SetThrower(Entity<BoomerangComponent> ent, EntityUid? newThrower)
    {
        ent.Comp.Thrower = newThrower;
        ent.Comp.CurrentHops = 0;
        Dirty(ent);
    }
}
