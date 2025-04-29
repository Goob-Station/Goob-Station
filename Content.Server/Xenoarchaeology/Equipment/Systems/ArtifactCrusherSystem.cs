// SPDX-FileCopyrightText: 2024 12rabbits <53499656+12rabbits@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Alzore <140123969+Blackern5000@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 ArtisticRoomba <145879011+ArtisticRoomba@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Brandon Hu <103440971+Brandon-Huu@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Dimastra <65184747+Dimastra@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Dimastra <dimastra@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Ed <96445749+TheShuEd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Emisse <99158783+Emisse@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Eoin Mcloughlin <helloworld@eoinrul.es>
// SPDX-FileCopyrightText: 2024 IProduceWidgets <107586145+IProduceWidgets@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 JIPDawg <51352440+JIPDawg@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 JIPDawg <JIPDawg93@gmail.com>
// SPDX-FileCopyrightText: 2024 Jezithyr <jezithyr@gmail.com>
// SPDX-FileCopyrightText: 2024 Mervill <mervills.email@gmail.com>
// SPDX-FileCopyrightText: 2024 Moomoobeef <62638182+Moomoobeef@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 PJBot <pieterjan.briers+bot@gmail.com>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers@gmail.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 Plykiya <58439124+Plykiya@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 PopGamer46 <yt1popgamer@gmail.com>
// SPDX-FileCopyrightText: 2024 PursuitInAshes <pursuitinashes@gmail.com>
// SPDX-FileCopyrightText: 2024 QueerNB <176353696+QueerNB@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Saphire Lattice <lattice@saphi.re>
// SPDX-FileCopyrightText: 2024 ShadowCommander <10494922+ShadowCommander@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Simon <63975668+Simyon264@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 SpeltIncorrectyl <66873282+SpeltIncorrectyl@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Spessmann <156740760+Spessmann@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Thomas <87614336+Aeshus@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Winkarst <74284083+Winkarst-cpu@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2024 eoineoineoin <github@eoinrul.es>
// SPDX-FileCopyrightText: 2024 github-actions[bot] <41898282+github-actions[bot]@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 lzk <124214523+lzk228@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 slarticodefast <161409025+slarticodefast@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 stellar-novas <stellar_novas@riseup.net>
// SPDX-FileCopyrightText: 2024 themias <89101928+themias@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Body.Systems;
using Content.Server.Popups;
using Content.Server.Power.EntitySystems;
using Content.Server.Stack;
using Content.Server.Storage.Components;
using Content.Server.Xenoarchaeology.XenoArtifacts;
using Content.Shared.Body.Components;
using Content.Shared.Damage;
using Content.Shared.Power;
using Content.Shared.Verbs;
using Content.Shared.Whitelist;
using Content.Shared.Xenoarchaeology.Equipment;
using Robust.Shared.Collections;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server.Xenoarchaeology.Equipment.Systems;

/// <inheritdoc/>
public sealed class ArtifactCrusherSystem : SharedArtifactCrusherSystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly ArtifactSystem _artifact = default!;
    [Dependency] private readonly BodySystem _body = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly StackSystem _stack = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelistSystem = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ArtifactCrusherComponent, GetVerbsEvent<AlternativeVerb>>(OnGetVerbs);
        SubscribeLocalEvent<ArtifactCrusherComponent, PowerChangedEvent>(OnPowerChanged);
    }

    private void OnGetVerbs(Entity<ArtifactCrusherComponent> ent, ref GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract || args.Hands == null || ent.Comp.Crushing)
            return;

        if (!TryComp<EntityStorageComponent>(ent, out var entityStorageComp) ||
            entityStorageComp.Contents.ContainedEntities.Count == 0)
            return;

        if (!this.IsPowered(ent, EntityManager))
            return;

        var verb = new AlternativeVerb
        {
            Text = Loc.GetString("artifact-crusher-verb-start-crushing"),
            Priority = 2,
            Act = () => StartCrushing((ent, ent.Comp, entityStorageComp))
        };
        args.Verbs.Add(verb);
    }

    private void OnPowerChanged(Entity<ArtifactCrusherComponent> ent, ref PowerChangedEvent args)
    {
        if (!args.Powered)
            StopCrushing(ent);
    }

    public void StartCrushing(Entity<ArtifactCrusherComponent, EntityStorageComponent> ent)
    {
        var (uid, crusher, _) = ent;

        if (crusher.Crushing)
            return;

        if (crusher.AutoLock)
            _popup.PopupEntity(Loc.GetString("artifact-crusher-autolocks-enable"), uid);

        crusher.Crushing = true;
        crusher.NextSecond = _timing.CurTime + TimeSpan.FromSeconds(1);
        crusher.CrushEndTime = _timing.CurTime + crusher.CrushDuration;
        crusher.CrushingSoundEntity = AudioSystem.PlayPvs(crusher.CrushingSound, ent);
        Appearance.SetData(ent, ArtifactCrusherVisuals.Crushing, true);
        Dirty(ent, ent.Comp1);
    }

    public void FinishCrushing(Entity<ArtifactCrusherComponent, EntityStorageComponent> ent)
    {
        var (_, crusher, storage) = ent;
        StopCrushing((ent, ent.Comp1), false);
        AudioSystem.PlayPvs(crusher.CrushingCompleteSound, ent);
        crusher.CrushingSoundEntity = null;
        Dirty(ent, ent.Comp1);

        var contents = new ValueList<EntityUid>(storage.Contents.ContainedEntities);
        var coords = Transform(ent).Coordinates;
        foreach (var contained in contents)
        {
            if (_whitelistSystem.IsWhitelistPass(crusher.CrushingWhitelist, contained))
            {
                var amount = _random.Next(crusher.MinFragments, crusher.MaxFragments);
                var stacks = _stack.SpawnMultiple(crusher.FragmentStackProtoId, amount, coords);
                foreach (var stack in stacks)
                {
                    ContainerSystem.Insert((stack, null, null, null), crusher.OutputContainer);
                }
                _artifact.ForceActivateArtifact(contained);
            }

            if (!TryComp<BodyComponent>(contained, out var body))
                Del(contained);

            var gibs = _body.GibBody(contained, body: body, gibOrgans: true);
            foreach (var gib in gibs)
            {
                ContainerSystem.Insert((gib, null, null, null), crusher.OutputContainer);
            }
        }
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<ArtifactCrusherComponent, EntityStorageComponent>();
        while (query.MoveNext(out var uid, out var crusher, out var storage))
        {
            if (!crusher.Crushing)
                continue;

            if (crusher.NextSecond < _timing.CurTime)
            {
                var contents = new ValueList<EntityUid>(storage.Contents.ContainedEntities);
                foreach (var contained in contents)
                {
                    _damageable.TryChangeDamage(contained, crusher.CrushingDamage);
                }
                crusher.NextSecond += TimeSpan.FromSeconds(1);
                Dirty(uid, crusher);
            }

            if (crusher.CrushEndTime < _timing.CurTime)
            {
                FinishCrushing((uid, crusher, storage));
            }
        }
    }
}