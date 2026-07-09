// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Emoting;
using Content.Server.Chat.Systems;
using Content.Server.Power.EntitySystems;
using Content.Shared.Chat;
using Content.Shared.Chat.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.Emoting;

public sealed partial class AnimatedEmotesSystem : SharedAnimatedEmotesSystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AnimatedEmotesComponent, EmoteEvent>(OnEmote);
    }

    private void OnEmote(Entity<AnimatedEmotesComponent> ent, ref EmoteEvent args)
    {
        PlayEmoteAnimation(ent, args.Emote.ID);
    }

    public void PlayEmoteAnimation(Entity<AnimatedEmotesComponent> ent, ProtoId<EmotePrototype> prot)
    {
        ent.Comp.Emote = prot;
        Dirty(ent);

        if (prot == "Flip")
            ApplyFlipEffects(ent);
    }
}
