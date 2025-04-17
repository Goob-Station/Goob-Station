// SPDX-FileCopyrightText: 2023 Julian Giebel <juliangiebel@live.de>
// SPDX-FileCopyrightText: 2023 Pieter-Jan Briers <pieterjan.briers@gmail.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.DeviceLinking.Components.Overload;
using Content.Server.DeviceLinking.Events;
using Robust.Server.Audio;
using Robust.Shared.Audio;

namespace Content.Server.DeviceLinking.Systems;

public sealed class DeviceLinkOverloadSystem : EntitySystem
{
    [Dependency] private readonly AudioSystem _audioSystem = default!;
    public override void Initialize()
    {
        SubscribeLocalEvent<SoundOnOverloadComponent, DeviceLinkOverloadedEvent>(OnOverloadSound);
        SubscribeLocalEvent<SpawnOnOverloadComponent, DeviceLinkOverloadedEvent>(OnOverloadSpawn);
    }

    private void OnOverloadSound(EntityUid uid, SoundOnOverloadComponent component, ref DeviceLinkOverloadedEvent args)
    {

        _audioSystem.PlayPvs(component.OverloadSound, uid, AudioParams.Default.WithVolume(component.VolumeModifier));
    }


    private void OnOverloadSpawn(EntityUid uid, SpawnOnOverloadComponent component, ref DeviceLinkOverloadedEvent args)
    {
        Spawn(component.Prototype, Transform(uid).Coordinates);
    }
}