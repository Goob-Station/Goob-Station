// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 whateverusername0 <whateveremail>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Emoting;
using Content.Server.Chat.Systems;
using Content.Server.Medical;
using Content.Shared.Chat.Prototypes;
using Content.Shared.Emoting;
using Content.Shared.StatusEffectNew;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.Emoting;

public sealed partial class AnimatedEmotesSystem : SharedAnimatedEmotesSystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly StatusEffectsSystem _status = default!;
    [Dependency] private readonly VomitSystem _vomit = default!;


    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AnimatedEmotesComponent, EmoteEvent>(OnEmote);
        SubscribeLocalEvent<AnimatedEmotesComponent, BeforeEmoteEvent>(OnBeforeEmote);
    }

    private void OnBeforeEmote(Entity<AnimatedEmotesComponent> ent, ref BeforeEmoteEvent args)
    {
        var emote = _proto.Index<EmotePrototype>(args.Emote);
        if (emote.Event is not AnimationEmoteEvent { CausesVomit: true })
            return;

        if (_status.HasStatusEffect(ent, ent.Comp.BlockVomitEmoteStatus))
            args.Cancel();
    }

    private void OnEmote(EntityUid uid, AnimatedEmotesComponent component, ref EmoteEvent args)
    {
        PlayEmoteAnimation(uid, component, args.Emote.ID);

        var emote = _proto.Index<EmotePrototype>(args.Emote);
        if (emote.Event is not AnimationEmoteEvent { CausesVomit: true })
            return;

        if (_status.HasStatusEffect(uid, component.BlockVomitEmoteStatus))
            return;

        if (!_status.TryUpdateStatusEffectDuration(uid,
                component.VomitStatus,
                out var effect,
                component.VomitStatusTime))
            return;

        var counter = EnsureComp<CounterStatusEffectComponent>(effect.Value);
        counter.Count++;
        if (counter.Count < component.EmotesToVomit)
            return;

        _vomit.Vomit(uid);
        _status.TryAddStatusEffect(uid, component.BlockVomitEmoteStatus, out _, component.BlockVomitStatusTime);
    }

    public void PlayEmoteAnimation(EntityUid uid, AnimatedEmotesComponent component, ProtoId<EmotePrototype> prot)
    {
        component.Emote = prot;
        Dirty(uid, component);
    }
}
