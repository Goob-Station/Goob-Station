// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
// SPDX-FileCopyrightText: 2025 Goob-Station
//
// SPDX-License-Identifier: MIT

using Content.Server.Radio.Components;
using Content.Shared._Funkystation.MalfAI;

namespace Content.Server._Funkystation.MalfAI;

/// <summary>
/// Grants the Malf AI access to the Syndicate radio channel when it purchases the upgrade.
/// </summary>
public sealed class MalfAiSyndicateCommsSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MalfAiMarkerComponent, MalfAiSyndicateKeysUnlockedEvent>(OnSyndicateKeysUnlocked);
    }

    private void OnSyndicateKeysUnlocked(Entity<MalfAiMarkerComponent> ent, ref MalfAiSyndicateKeysUnlockedEvent args)
    {
        // The AI uses intrinsic radio: grant transmit and receive on the Syndicate channel.
        // Both components are server-only.
        var transmitter = EnsureComp<IntrinsicRadioTransmitterComponent>(ent.Owner);
        transmitter.Channels.Add("Syndicate");

        var activeRadio = EnsureComp<ActiveRadioComponent>(ent.Owner);
        activeRadio.Channels.Add("Syndicate");
    }
}
