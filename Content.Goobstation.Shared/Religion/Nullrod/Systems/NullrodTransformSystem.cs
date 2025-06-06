// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Religion.Nullrod.Components;
using Content.Shared.Coordinates.Helpers;
using Content.Shared.Interaction;
using Content.Shared.Storage;
using Content.Shared.Tag;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;

namespace Content.Goobstation.Shared.Religion.Nullrod;

public sealed class NullrodTransformSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly INetManager _netManager = default!;
    [Dependency] private readonly TagSystem _tagSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<AltarSourceComponent, InteractUsingEvent>(OnInteractUsing);
        SubscribeLocalEvent<TryTransformNullrodEvent>(OnTryTransform);
    }

    private void OnInteractUsing(EntityUid uid, AltarSourceComponent component, InteractUsingEvent args)
    {
        if (args.Handled
        || _netManager.IsClient
        || HasComp<StorageComponent>(args.Target) // If it's a storage component like a bag, we ignore usage so it can be stored.
        || !_tagSystem.HasTag(args.Used, "Nullrod")) // Checks used entity for the tag we need.
        return;

        // Check if the new null rod would be unique
        var ev = new TryTransformNullrodEvent(component.RodProto);
        RaiseLocalEvent(ev);
        if (ev.Cancelled)
            return;

        // *flaaavor*
        Spawn(component.EffectProto, Transform(uid).Coordinates);
        _audio.PlayPvs(component.SoundPath, uid, AudioParams.Default.WithVolume(-4f));

        // Spawn proto associated with the altar.
        Spawn(component.RodProto, args.ClickLocation.SnapToGrid(EntityManager));

        // Remove the nullrod
        QueueDel(args.Used);
        args.Handled = true;
    }

    private void OnTryTransform(TryTransformNullrodEvent args)
    {
        var query = EntityQueryEnumerator<NullrodComponent>();
        while (query.MoveNext(out var uid, out _))
        {
            var proto = Prototype(uid);
            if (proto == null || proto.ID != args.Proto)
                continue;
            args.Cancel();
            return;
        }
    }
}
