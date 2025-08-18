// SPDX-FileCopyrightText: 2023 0x6273 <0x40@keemail.me>
// SPDX-FileCopyrightText: 2023 20kdc <asdd2808@gmail.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 CerberusWolfie <wb.johnb.willis@gmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Mnemotechnican <69920617+Mnemotechnician@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 SX-7 <sn1.test.preria.2002@gmail.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Server.Chat.Systems;

using Content.Shared.Chat; // Einstein Engines - Languages
using Content.Shared.Chat.Prototypes;
using Content.Shared.Damage;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Traits.Assorted;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using Robust.Shared.Utility;
using System.Collections.Generic;

public sealed class EmoteOnDamageSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly ChatSystem _chatSystem = default!;
    [Dependency] private readonly MobThresholdSystem _mobThreshold = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<EmoteOnDamageComponent, DamageChangedEvent>(OnDamage);
    }

    private void OnDamage(EntityUid uid, EmoteOnDamageComponent emoteOnDamage, DamageChangedEvent args)
    {
        if (!args.DamageIncreased)
            return;

        // CorvaxGoob-AutoEmote-Start
        if (args.DamageDelta is null)
            return;

        if (TryComp<MobThresholdsComponent>(uid, out var mobThresholds) && (mobThresholds.CurrentThresholdState is Shared.Mobs.MobState.Dead || mobThresholds.CurrentThresholdState is Shared.Mobs.MobState.Critical))
            return;

        if (HasComp<PainNumbnessComponent>(uid))
            return;
        // CorvaxGoob-AutoEmote-End

        if (emoteOnDamage.LastEmoteTime + emoteOnDamage.EmoteCooldown > _gameTiming.CurTime)
            return;

        if (emoteOnDamage.EmotesThreshold.Count == 0) // CorvaxGoob-AutoEmote
            return;

        if (!_random.Prob(emoteOnDamage.EmoteChance))
            return;

        // CorvaxGoob-AutoEmote-Start
        float summaryDamage = 0;

        foreach (var damage in args.DamageDelta.DamageDict)
        {
            if (emoteOnDamage.AllowedDamageType.Contains(damage.Key))
                summaryDamage += damage.Value.Float();
        }

        if (summaryDamage < emoteOnDamage.PainThreshold)
            return;

        if (!TryComp<DamageableComponent>(uid, out var damageable))
            return;

        float closest = 0;
        var total = damageable.TotalDamage;

        foreach (var emote in emoteOnDamage.EmotesThreshold)
        {
            if (total >= emote.Key && emote.Key > closest)
                closest = emote.Key;
        }

        if (closest != 0)
        {
            var selectedEmote = _random.Pick(emoteOnDamage.EmotesThreshold[closest]);

            if (emoteOnDamage.WithChat)
            {
                _chatSystem.TryEmoteWithChat(uid, selectedEmote, emoteOnDamage.HiddenFromChatWindow ? ChatTransmitRange.HideChat : ChatTransmitRange.Normal);
            }
            else
            {
                _chatSystem.TryEmoteWithoutChat(uid, selectedEmote);
            }

            emoteOnDamage.LastEmoteTime = _gameTiming.CurTime;
        }
        // CorvaxGoob-AutoEmote-End
    }

    /// <summary>
    /// Try to add an emote to the entity, which will be performed at an interval.
    /// </summary>
    public bool AddEmote(EntityUid uid, float threshold, string emotePrototypeId, EmoteOnDamageComponent? emoteOnDamage = null) // CorvaxGoob-AutoEmote : added threshold arg
    {
        if (!Resolve(uid, ref emoteOnDamage, logMissing: false))
            return false;

        DebugTools.Assert(emoteOnDamage.LifeStage <= ComponentLifeStage.Running);
        DebugTools.Assert(_prototypeManager.HasIndex<EmotePrototype>(emotePrototypeId), "Prototype not found. Did you make a typo?");

        // CorvaxGoob-AutoEmote-Start
        if (!emoteOnDamage.EmotesThreshold.TryGetValue(threshold, out var emotes))
            return emoteOnDamage.EmotesThreshold.TryAdd(threshold, [emotePrototypeId]);
        else
            return emoteOnDamage.EmotesThreshold[threshold].Add(emotePrototypeId);
        // CorvaxGoob-AutoEmote-End
    }

    /// <summary>
    /// Stop preforming an emote. Note that by default this will queue empty components for removal.
    /// </summary>
    public bool RemoveEmote(EntityUid uid, float threshold, string emotePrototypeId, EmoteOnDamageComponent? emoteOnDamage = null, bool removeEmpty = true) // CorvaxGoob-AutoEmote : added threshold arg
    {
        if (!Resolve(uid, ref emoteOnDamage, logMissing: false))
            return false;

        DebugTools.Assert(_prototypeManager.HasIndex<EmotePrototype>(emotePrototypeId), "Prototype not found. Did you make a typo?");

        if (!emoteOnDamage.EmotesThreshold.TryGetValue(threshold, out var emotes))// CorvaxGoob-AutoEmote
            return false;

        if (!emoteOnDamage.EmotesThreshold[threshold].Remove(emotePrototypeId)) // CorvaxGoob-AutoEmote : Changes
            return false;

        if (removeEmpty) // CorvaxGoob-AutoEmote : Changes
            foreach (var emoteThreshold in emoteOnDamage.EmotesThreshold)
                if (emoteOnDamage.EmotesThreshold[emoteThreshold.Key].Count != 0)
                    return true;

        RemCompDeferred(uid, emoteOnDamage);

        return true;
    }
}
