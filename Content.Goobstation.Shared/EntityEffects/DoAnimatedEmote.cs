// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.EntityEffects;
using Content.Shared.Chemistry.Reaction;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;
using Content.Goobstation.Shared.Emoting;
using Content.Shared.Chat.Prototypes;

namespace Content.Goobstation.Shared.EntityEffects;

public sealed partial class DoAnimatedEmoteEntityEffectSystem : EntityEffectSystem<AnimatedEmotesComponent, DoAnimatedEmote>
{
    [Dependency] private SharedAnimatedEmotesSystem _animatedEmotes = default!;

    protected override void Effect(Entity<AnimatedEmotesComponent> ent, ref EntityEffectEvent<DoAnimatedEmote> args)
    {
        _animatedEmotes.SetEmote(ent.AsNullable(), args.Effect.Emote);
    }
}

public sealed partial class DoAnimatedEmote : EntityEffectBase<DoAnimatedEmote>
{
    [DataField(required: true)]
    public ProtoId<EmotePrototype> Emote;

    public override string? EntityEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => "Placeholder";
}
