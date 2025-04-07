// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Abbey Armbruster <abbeyjarmb@gmail.com>
// SPDX-FileCopyrightText: 2023 Pieter-Jan Briers <pieterjan.briers@gmail.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 GreaseMonk <1354802+GreaseMonk@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Shared.Audio;

public abstract class SharedAmbientSoundSystem : EntitySystem
{
    private EntityQuery<AmbientSoundComponent> _query;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<AmbientSoundComponent, ComponentGetState>(GetCompState);
        SubscribeLocalEvent<AmbientSoundComponent, ComponentHandleState>(HandleCompState);

        _query = GetEntityQuery<AmbientSoundComponent>();
    }

    public virtual void SetAmbience(EntityUid uid, bool value, AmbientSoundComponent? ambience = null)
    {
        if (!_query.Resolve(uid, ref ambience, false) || ambience.Enabled == value)
            return;

        ambience.Enabled = value;
        QueueUpdate(uid, ambience);
        Dirty(uid, ambience);
    }

    public virtual void SetRange(EntityUid uid, float value, AmbientSoundComponent? ambience = null)
    {
        if (!_query.Resolve(uid, ref ambience, false) || MathHelper.CloseToPercent(ambience.Range, value))
            return;

        ambience.Range = value;
        QueueUpdate(uid, ambience);
        Dirty(uid, ambience);
    }

    protected virtual void QueueUpdate(EntityUid uid, AmbientSoundComponent ambience)
    {
        // client side tree
    }

    public virtual void SetVolume(EntityUid uid, float value, AmbientSoundComponent? ambience = null)
    {
        if (!_query.Resolve(uid, ref ambience, false) || MathHelper.CloseToPercent(ambience.Volume, value))
            return;

        ambience.Volume = value;
        Dirty(uid, ambience);
    }

    public virtual void SetSound(EntityUid uid, SoundSpecifier sound, AmbientSoundComponent? ambience = null)
    {
        if (!_query.Resolve(uid, ref ambience, false) || ambience.Sound == sound)
            return;

        ambience.Sound = sound;
        QueueUpdate(uid, ambience);
        Dirty(uid, ambience);
    }

    private void HandleCompState(EntityUid uid, AmbientSoundComponent component, ref ComponentHandleState args)
    {
        if (args.Current is not AmbientSoundComponentState state) return;
        SetAmbience(uid, state.Enabled, component);
        SetRange(uid, state.Range, component);
        SetVolume(uid, state.Volume, component);
        SetSound(uid, state.Sound, component);
    }

    private void GetCompState(EntityUid uid, AmbientSoundComponent component, ref ComponentGetState args)
    {
        args.State = new AmbientSoundComponentState
        {
            Enabled = component.Enabled,
            Range = component.Range,
            Volume = component.Volume,
            Sound = component.Sound,
        };
    }
}