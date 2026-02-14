// SPDX-FileCopyrightText: 2025 August Eymann <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.EntityEffects;
using Content.Server._Shitmed.StatusEffects;
using Content.Server.Polymorph.Components;
using Content.Server.Polymorph.Systems;
using Content.Shared.Humanoid;
using Content.Shared.Polymorph;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.Xenobiology.Systems;

// Any Polymorphing etc needing to run serverside
public class XenobiologyTransformingSystem : EntitySystem
{
    public override void Initialize()
    {
        SubscribeLocalEvent<HumanoidAppearanceComponent, SpeciesChange>(OnSpeciesChange);
        SubscribeLocalEvent<HumanoidAppearanceComponent, ScrambleNearbyEffect>(OnScrambleNearby);
    }

    private void OnSpeciesChange(
        EntityUid uid,
        HumanoidAppearanceComponent appearance,
        ref SpeciesChange ev)
    {
        var polymorphSystem = EntityManager.System<PolymorphSystem>();

        var protMan = IoCManager.Resolve<IPrototypeManager>();
        if (!protMan.TryIndex(ev.NewSpecies, out var species))
            return;

        var config = new PolymorphConfiguration
        {
            Entity = species.Prototype,
            TransferDamage = true,
            Forced = true,
            Inventory = PolymorphInventoryChange.Transfer,
            RevertOnCrit = false,
            RevertOnDeath = false,
            TransferName = true,
        };

        var @new = polymorphSystem.PolymorphEntity(uid, config);
        if (@new.HasValue)
        {
            EntityManager.RemoveComponentDeferred<PolymorphedEntityComponent>(@new.Value);
        }
    }

    private void OnScrambleNearby(EntityUid uid, HumanoidAppearanceComponent appearance, ScrambleNearbyEffect args)
    {
        var lookupSys = EntityManager.System<EntityLookupSystem>();
        var scramSys = EntityManager.System<ScrambleDnaEffectSystem>();

        foreach (var entity in lookupSys.GetEntitiesInRange(uid, args.Radius))
        {
            if (EntityManager.HasComponent<HumanoidAppearanceComponent>(entity))
                scramSys.Scramble(entity);
        }
    }
}
