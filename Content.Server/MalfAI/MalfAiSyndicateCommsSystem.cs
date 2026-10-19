using Content.Shared.Radio.Components;
using Content.Shared.MalfAI;
using Robust.Shared.GameObjects;
using Robust.Shared.Serialization;

namespace Content.Server.MalfAI;

/// <summary>
/// Event raised when the AI purchases syndicate keys
/// </summary>
[Serializable, DataDefinition]
public sealed partial class MalfAiSyndicateKeysUnlockedEvent : EntityEventArgs
{
}

/// <summary>
/// System that handles granting syndicate radio communications to malfunction AI
/// </summary>
public sealed class MalfAiSyndicateCommsSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<MalfAiMarkerComponent, MalfAiSyndicateKeysUnlockedEvent>(OnSyndicateKeysUnlocked);
    }

    private void OnSyndicateKeysUnlocked(EntityUid uid, MalfAiMarkerComponent component, MalfAiSyndicateKeysUnlockedEvent args)
    {
        // Add or get the IntrinsicRadioTransmitterComponent for sending syndicate messages
        var transmitterComp = EnsureComp<IntrinsicRadioTransmitterComponent>(uid);
        transmitterComp.Channels.Add("Syndicate");

        // Add or get the ActiveRadioComponent for receiving syndicate messages
        var activeRadioComp = EnsureComp<ActiveRadioComponent>(uid);
        activeRadioComp.Channels.Add("Syndicate");

        // IntrinsicRadioTransmitterComponent and ActiveRadioComponent are server-only and don't need network synchronization
    }
}
