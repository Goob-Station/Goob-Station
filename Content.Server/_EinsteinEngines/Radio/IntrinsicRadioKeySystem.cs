// SPDX-FileCopyrightText: 2024 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Radio.Components;
using Content.Shared.Radio;
using Robust.Shared.Prototypes;

namespace Content.Server._EinsteinEngines.Radio;

public sealed class IntrinsicRadioKeySystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<IntrinsicRadioTransmitterComponent, EncryptionChannelsChangedEvent>(OnTransmitterChannelsChanged);
        SubscribeLocalEvent<ActiveRadioComponent, EncryptionChannelsChangedEvent>(OnReceiverChannelsChanged);
    }

    private void OnTransmitterChannelsChanged(EntityUid uid, IntrinsicRadioTransmitterComponent component, EncryptionChannelsChangedEvent args)
    {
        UpdateChannels(args.Component, ref component.Channels);
        Dirty(uid, component);
    }

    private void OnReceiverChannelsChanged(EntityUid uid, ActiveRadioComponent component, EncryptionChannelsChangedEvent args)
    {
        UpdateChannels(args.Component, ref component.Channels);
        Dirty(uid, component);
    }

    private void UpdateChannels(EncryptionKeyHolderComponent keyHolderComp, ref HashSet<ProtoId<RadioChannelPrototype>> channels)
    {
        channels.Clear();
        channels.UnionWith(keyHolderComp.Channels);
    }
}
