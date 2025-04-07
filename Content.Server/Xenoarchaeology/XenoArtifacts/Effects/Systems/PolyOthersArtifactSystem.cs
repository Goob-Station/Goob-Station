// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Pieter-Jan Briers <pieterjan.briers@gmail.com>
// SPDX-FileCopyrightText: 2023 brainfood1183 <113240905+brainfood1183@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 AJCM-git <60196617+AJCM-git@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Server.Polymorph.Systems;
using Content.Server.Xenoarchaeology.XenoArtifacts.Effects.Components;
using Content.Server.Xenoarchaeology.XenoArtifacts.Events;
using Content.Shared.Humanoid;
using Content.Shared.Mobs.Systems;
using Robust.Shared.Audio.Systems;

namespace Content.Server.Xenoarchaeology.XenoArtifacts.Effects.Systems;

public sealed class PolyOthersArtifactSystem : EntitySystem
{
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly MobStateSystem _mob = default!;
    [Dependency] private readonly PolymorphSystem _poly = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    /// <summary>
    /// On effect trigger polymorphs targets in range.
    /// </summary>
    public override void Initialize()
    {
        SubscribeLocalEvent<PolyOthersArtifactComponent, ArtifactActivatedEvent>(OnActivate);
    }

    /// <summary>
    /// Provided target is alive and is not a zombie, polymorphs the target.
    /// </summary>
    private void OnActivate(Entity<PolyOthersArtifactComponent> ent, ref ArtifactActivatedEvent args)
    {
        var xform = Transform(ent);
        var humanoids = new HashSet<Entity<HumanoidAppearanceComponent>>();
        _lookup.GetEntitiesInRange(xform.Coordinates, ent.Comp.Range, humanoids);

        foreach (var comp in humanoids)
        {
            var target = comp.Owner;
            if (_mob.IsAlive(target))
            {
                _poly.PolymorphEntity(target, ent.Comp.PolymorphPrototypeName);
                _audio.PlayPvs(ent.Comp.PolySound, ent);
            }
        }
    }
}