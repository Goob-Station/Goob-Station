// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Emoting;
using Content.Server.Chat.Systems;
using Content.Server.Power.EntitySystems;
using Content.Shared.Chat;
using Content.Shared.Chat.Prototypes;
using Robust.Shared.Configuration;
using Robust.Shared.Prototypes;
using Content.Goobstation.Common.CCVar;
using Content.Shared.CCVar;

namespace Content.Goobstation.Server.Emoting;

public sealed partial class AnimatedEmotesSystem : SharedAnimatedEmotesSystem
{
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    private bool _flipDodgeEnabled = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AnimatedEmotesComponent, EmoteEvent>(OnEmote);
        Subs.CVar(_cfg, GoobCVars.FlipDodgeEnabled, value => _flipDodgeEnabled = value, true);
    }

    private void OnEmote(Entity<AnimatedEmotesComponent> ent, ref EmoteEvent args)
    {
        PlayEmoteAnimation(ent, args.Emote.ID);
    }

    public void PlayEmoteAnimation(Entity<AnimatedEmotesComponent> ent, ProtoId<EmotePrototype> prot)
    {
        ent.Comp.Emote = prot;
        Dirty(ent);

        if ((prot == "Flip") && _flipDodgeEnabled)
            ApplyFlipEffects(ent);
    }
}
