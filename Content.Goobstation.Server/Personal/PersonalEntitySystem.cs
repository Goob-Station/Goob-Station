// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 BombasterDS <deniskaporoshok@gmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Personal;
using Content.Server.Hands.Systems;
using Content.Server.Storage.EntitySystems;
using Content.Shared.GameTicking;
using Content.Shared.Inventory;
using Content.Shared.Players;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.Personal;

public sealed class PersonalEntitySystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly StorageSystem _storage = default!;
    [Dependency] private readonly HandsSystem _hands = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PlayerSpawnCompleteEvent>(OnPlayerSpawned);
    }

    private void OnPlayerSpawned(PlayerSpawnCompleteEvent args)
    {
        // You'll need to name prototype usernamePersonal to use this :godo:
        var protoString = args.Player.Name + "Personal";

        protoString = protoString.Replace("localhost@", null);

        if (!_prototype.TryIndex<PersonalEntityPrototype>(protoString, out var personalProto))
            return;

        var mind = args.Player.GetMind();

        if (mind == null)
            return;

        foreach (var item in personalProto.ItemList)
        {
            var spawned = SpawnNextToOrDrop(item, args.Mob);
            TryPutInBackpackOrHands(args.Mob, spawned);
        }
    }

    /// <summary>
    /// This entity should be in storage or storage API system, not here actually.
    /// Tries to put item in backpack or hands if no backpack.
    /// </summary>
    private bool TryPutInBackpackOrHands(EntityUid entity, EntityUid itemToPut)
    {
        // Hardcoded backpack :good:
        if (_inventory.TryGetSlotEntity(entity, "back", out var backpack)
            && _storage.Insert(backpack.Value, itemToPut, out _))
            return true;

        if (_hands.TryPickupAnyHand(entity, itemToPut, false))
            return true;

        return false;
    }
}
