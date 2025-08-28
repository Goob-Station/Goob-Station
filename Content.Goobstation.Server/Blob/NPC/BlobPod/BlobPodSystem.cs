// SPDX-FileCopyrightText: 2024 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 Fishbait <Fishbait@git.ml>
// SPDX-FileCopyrightText: 2024 fishbait <gnesse@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 BombasterDS2 <shvalovdenis.workmail@gmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Ilya246 <57039557+Ilya246@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Ilya246 <ilyukarno@gmail.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Rinary <72972221+Rinary1@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Blob.Components;
using Content.Goobstation.Shared.Blob.NPC.BlobPod;
using Content.Server.DoAfter;
using Content.Server.Explosion.EntitySystems;
using Content.Server.NPC.HTN;
using Content.Server.NPC.Systems;
using Content.Server.Popups;
using Content.Shared.ActionBlocker;
using Content.Shared.CombatMode;
using Content.Shared.Damage;
using Content.Shared.Destructible;
using Content.Shared.DoAfter;
using Content.Shared.Humanoid;
using Content.Shared.Interaction.Components;
using Content.Shared.Inventory;
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Systems;
using Content.Shared.Rejuvenate;
using Content.Shared._Starlight.CollectiveMind;
using Robust.Server.Audio;
using Robust.Shared.Containers;
using Robust.Shared.Player;

namespace Content.Goobstation.Server.Blob.NPC.BlobPod;

public sealed class BlobPodSystem : SharedBlobPodSystem
{
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly ExplosionSystem _explosionSystem = default!;
    [Dependency] private readonly DamageableSystem _damageableSystem = default!;
    [Dependency] private readonly NPCSystem _npc = default!;
    [Dependency] private readonly SharedMoverController _mover = default!;

    public bool Zombify(Entity<BlobPodComponent> ent, EntityUid target)
    {
        _inventory.TryGetSlotEntity(target, "head", out var headItem);
        if (HasComp<BlobMobComponent>(headItem))
            return false;

        _inventory.TryUnequip(target, "head", true, true);
        var equipped = _inventory.TryEquip(target, ent, "head", true, true);

        if (!equipped)
            return false;

        _popups.PopupEntity(Loc.GetString("blob-mob-zombify-second-end", ("pod", ent.Owner)),
            target,
            target,
            Content.Shared.Popups.PopupType.LargeCaution);
        _popups.PopupEntity(
            Loc.GetString("blob-mob-zombify-third-end", ("pod", ent.Owner), ("target", target)),
            target,
            Filter.PvsExcept(target),
            true,
            Content.Shared.Popups.PopupType.LargeCaution);

        RemComp<CombatModeComponent>(ent);
        RemComp<HTNComponent>(ent);
        RemComp<UnremoveableComponent>(ent);

        _audioSystem.PlayPvs(ent.Comp.ZombifyFinishSoundPath, ent);

        var rejEv = new RejuvenateEvent();
        RaiseLocalEvent(target, rejEv);

        ent.Comp.ZombifiedEntityUid = target;

        var zombieBlob = EnsureComp<ZombieBlobComponent>(target);
        EnsureComp<CollectiveMindComponent>(target).Channels.Add(ent.Comp.CollectiveMind);
        zombieBlob.CollectiveMindAdded = ent.Comp.CollectiveMind;
        zombieBlob.BlobPodUid = ent;
        if (HasComp<ActorComponent>(ent))
        {
            _npc.SleepNPC(target);
            _mover.SetRelay(ent, target);
        }

        return true;
    }
}
