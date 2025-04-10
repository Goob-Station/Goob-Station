// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Aineias1 <dmitri.s.kiselev@gmail.com>
// SPDX-FileCopyrightText: 2025 FaDeOkno <143940725+FaDeOkno@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 McBosserson <148172569+McBosserson@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Milon <plmilonpl@gmail.com>
// SPDX-FileCopyrightText: 2025 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Rouden <149893554+Roudenn@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Unlumination <144041835+Unlumy@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 coderabbitai[bot] <136622811+coderabbitai[bot]@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <linebarrelerenthusiast@gmail.com>
// SPDX-FileCopyrightText: 2025 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 whateverusername0 <whateveremail>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Shared._Shitmed.Targeting;
using Content.Shared.Damage;
using Content.Shared.Mobs.Components;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map.Components;
using Robust.Shared.Timing;

namespace Content.Shared._Lavaland.Damage;

/// <summary>
///     We have to use it's own system even for the damage field because WIZDEN SYSTEMS FUCKING SUUUUUUUUUUUCKKKKKKKKKKKKKKK
/// </summary>
public abstract class SharedDamageSquareSystem : EntitySystem
{
    [Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly DamageableSystem _dmg = default!;
    [Dependency] private readonly SharedAudioSystem _aud = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    private const float ImmunityFrames = 0.3f;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DamageSquareComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(Entity<DamageSquareComponent> ent, ref MapInitEvent args)
    {
        Timer.Spawn((int) ent.Comp.DamageDelay * 1000,
            () =>
            {
                if (!TerminatingOrDeleted(ent))
                    Damage(ent);
            });
    }

    private void Damage(Entity<DamageSquareComponent> field)
    {
        var xform = Transform(field);
        if (xform.GridUid == null)
            return;

        var grid = xform.GridUid.Value;
        var tile = _map.GetTileRef(grid, Comp<MapGridComponent>(grid), xform.Coordinates);

        var lookup = _lookup.GetLocalEntitiesIntersecting(tile, 0f, LookupFlags.Uncontained)
            .Where(HasComp<MobStateComponent>)
            .ToList();

        foreach (var entity in lookup)
        {
            if (!TryComp<DamageableComponent>(entity, out var dmg))
                continue;

            if (TryComp<DamageSquareImmunityComponent>(entity, out var immunity))
            {
                if (immunity.HasImmunityUntil > _timing.CurTime || immunity.IsImmune)
                    continue;

                RemComp(entity, immunity);
            }

            // Damage
            _dmg.TryChangeDamage(entity, field.Comp.Damage, damageable: dmg, targetPart: TargetBodyPart.Torso);
            // Sound
            if (field.Comp.Sound != null)
                _aud.PlayEntity(field.Comp.Sound, entity, entity, AudioParams.Default.WithVolume(-3f));
            // Immunity frames
            EnsureComp<DamageSquareImmunityComponent>(entity).HasImmunityUntil = _timing.CurTime + TimeSpan.FromSeconds(ImmunityFrames);
        }

        RemComp(field, field.Comp);
    }
}