// SPDX-FileCopyrightText: 2024 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 Fishbait <Fishbait@git.ml>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 fishbait <gnesse@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using Content.Goobstation.Shared.Blob.Components;
using Content.Shared.Interaction.Events;
using Content.Shared.Popups;
using Content.Shared.Radio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Blob;

public abstract class SharedBlobMobSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly SharedAudioSystem _audioSystem = default!;
    private EntityQuery<BlobTileComponent> _tileQuery;
    private EntityQuery<BlobMobComponent> _mobQuery;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BlobMobComponent, AttackAttemptEvent>(OnBlobAttackAttempt);
        SubscribeNetworkEvent<BlobMobGetPulseEvent>(OnPulse);
        _tileQuery = GetEntityQuery<BlobTileComponent>();
        _mobQuery = GetEntityQuery<BlobMobComponent>();

        SubscribeLocalEvent<BlobSpeakComponent,GetDefaultRadioChannelEvent>(OnGetDefaultRadioChannel);
    }

    private void OnGetDefaultRadioChannel(Entity<BlobSpeakComponent> ent, ref GetDefaultRadioChannelEvent args)
    {
        //args.Channel = ent.Comp.Channel;
    }


    [ValidatePrototypeId<EntityPrototype>]
    private const string HealEffect = "EffectHealPlusTripleYellow";

    private void OnPulse(BlobMobGetPulseEvent ev)
    {
        if(!TryGetEntity(ev.BlobEntity, out var blobEntity))
            return;

        SpawnAttachedTo(HealEffect, new EntityCoordinates(blobEntity.Value, Vector2.Zero));
    }

    private void OnBlobAttackAttempt(EntityUid uid, BlobMobComponent component, AttackAttemptEvent args)
    {
        if (args.Cancelled || !_tileQuery.HasComp(args.Target) && !_mobQuery.HasComp(args.Target))
            return;

        _popupSystem.PopupCursor(Loc.GetString("blob-mob-attack-blob"), PopupType.Large);
        args.Cancel();
    }
}