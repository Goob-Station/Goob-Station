using Content.Server.Radio.Components;
using Content.Shared._Gabystation.MalfAi.Components;
using Content.Shared._Gabystation.MalfAi.Events;

namespace Content.Server._Gabystation.MalfAi;

public sealed partial class MalfAiSystem
{
    private void InitializeAbilities()
    {
        // Acho que esse evento pode ser mais genérico, pra qualquer entidade com IntrinsicRadioTransmitterComponent e ActiveRadioComponent.
        SubscribeLocalEvent<MalfunctioningAiComponent, RadioKeyUnlockedEvent>(OnRadioKeyUnlocked);
    }

    private void OnRadioKeyUnlocked(Entity<MalfunctioningAiComponent> malf, ref RadioKeyUnlockedEvent args)
    {
        if (TryComp<IntrinsicRadioTransmitterComponent>(malf.Owner, out var transmitter))
            transmitter.Channels.UnionWith(args.Channels);

        if (TryComp<ActiveRadioComponent>(malf.Owner, out var radio))
            radio.Channels.UnionWith(args.Channels);
    }
}
