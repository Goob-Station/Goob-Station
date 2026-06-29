using Content.Pirate.Shared.Blinking;
using Content.Server.Chat.Systems;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Player;

namespace Content.Pirate.Server.Blinking;

/// <summary>
/// Sends blink emote effects to clients in PVS.
/// </summary>
public sealed class BlinkingSystem : EntitySystem
{
    public const string BlinkEmote = "Blink";
    public const string BlinkRapidEmote = "BlinkRapid";

    private static readonly SoundSpecifier BlinkSound = new SoundPathSpecifier("/Audio/_Pirate/Effects/Emotes/blink.ogg");

    [Dependency] private readonly SharedAudioSystem _audio = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<BlinkingComponent, EmoteEvent>(OnEmote);
    }

    private void OnEmote(Entity<BlinkingComponent> ent, ref EmoteEvent args)
    {
        if (!ent.Comp.Enabled)
            return;

        switch (args.Emote.ID)
        {
            case BlinkEmote:
                _audio.PlayPvs(BlinkSound, ent.Owner);
                Blink(ent.Owner);
                break;
            case BlinkRapidEmote:
                _audio.PlayPvs(BlinkSound, ent.Owner);
                Blink(ent.Owner, rapid: true);
                break;
        }
    }

    /// <summary>
    /// Makes an entity visibly blink for nearby clients.
    /// </summary>
    public void Blink(EntityUid uid, bool rapid = false)
    {
        RaiseNetworkEvent(new BlinkEffectEvent(GetNetEntity(uid), rapid), Filter.Pvs(uid, entityManager: EntityManager));
    }
}
