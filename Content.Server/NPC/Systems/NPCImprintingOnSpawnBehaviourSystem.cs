// SPDX-FileCopyrightText: 2024 Ed <96445749+TheShuEd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 Plykiya <58439124+Plykiya@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 faint <46868845+ficcialfaint@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2024 plykiya <plykiya@protonmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using Content.Shared.NPC.Components;
using Content.Shared.NPC.Systems;
using Content.Shared.Whitelist;
using Robust.Shared.Map;
using Robust.Shared.Random;
using NPCImprintingOnSpawnBehaviourComponent = Content.Server.NPC.Components.NPCImprintingOnSpawnBehaviourComponent;

namespace Content.Server.NPC.Systems;

public sealed partial class NPCImprintingOnSpawnBehaviourSystem : SharedNPCImprintingOnSpawnBehaviourSystem
{
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly NPCSystem _npc = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelistSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<NPCImprintingOnSpawnBehaviourComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(Entity<NPCImprintingOnSpawnBehaviourComponent> imprinting, ref MapInitEvent args)
    {
        HashSet<EntityUid> friends = new();
        _lookup.GetEntitiesInRange(imprinting, imprinting.Comp.SpawnFriendsSearchRadius, friends);

        foreach (var friend in friends)
        {
            if (_whitelistSystem.IsWhitelistPassOrNull(imprinting.Comp.Whitelist, friend))
            {
                AddImprintingTarget(imprinting, friend, imprinting.Comp);
            }
        }

        if (imprinting.Comp.Follow && imprinting.Comp.Friends.Count > 0)
        {
            var mommy = _random.Pick(imprinting.Comp.Friends);
            _npc.SetBlackboard(imprinting, NPCBlackboard.FollowTarget, new EntityCoordinates(mommy, Vector2.Zero));
        }
    }

    public void AddImprintingTarget(EntityUid entity, EntityUid friend, NPCImprintingOnSpawnBehaviourComponent component)
    {
        component.Friends.Add(friend);
        var exception = EnsureComp<FactionExceptionComponent>(entity);
        exception.Ignored.Add(friend);
    }
}