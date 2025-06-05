// SPDX-FileCopyrightText: 2025 August Eymann <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Xenobiology.Components;
using Content.Shared.ActionBlocker;
using Content.Shared.Damage;
using Content.Shared.DoAfter;
using Content.Shared.Emag.Systems;
using Content.Shared.Inventory;
using Content.Shared.Jittering;
using Content.Shared.Mobs.Systems;
using Content.Shared.Nutrition.EntitySystems;
using Content.Shared.Popups;
using Content.Shared.Stunnable;
using Content.Shared.Throwing;
using Robust.Server.GameObjects;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.Xenobiology.Systems;

/// <summary>
/// This handles the server-side of Xenobiology.
/// </summary>
public sealed partial class XenobiologySystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly HungerSystem _hunger = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly MetaDataSystem _metaData = default!;
    [Dependency] private readonly SharedContainerSystem _containerSystem = default!;
    [Dependency] private readonly InventorySystem _inventorySystem = default!;
    [Dependency] private readonly EmagSystem _emag = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly ActionBlockerSystem _actionBlocker = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedJitteringSystem _jitter = default!;
    [Dependency] private readonly ThrowingSystem _throw = default!;
    [Dependency] private readonly INetManager _net = default!;

    private ISawmill _sawmill = default!;

    public override void Initialize()
    {
        InitializeTaming();
        InitializeGrowth();
        InitializeBreeding();
        InitializeVacuum();
        InitializeActions();

        _sawmill = Logger.GetSawmill("Xenobiology");
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        UpdateBreeding();
        UpdateGrowth();
        UpdateHunger();
    }

    /// <summary>
    /// Returns the extract associated by the slimes breed.
    /// </summary>
    /// <param name="slime">The slime entity.</param>
    /// <returns>Grey if no breed can be found.</returns>
    public EntProtoId GetProducedExtract(Entity<SlimeComponent> slime)
    {
        return _prototypeManager.TryIndex(slime.Comp.Breed, out var breedPrototype)
            ? breedPrototype.ProducedExtract
            : slime.Comp.DefaultExtract;
    }
}
