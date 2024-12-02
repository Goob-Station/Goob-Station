using Content.Server.Flash.Components;
using Content.Server.Radio.Components;
using Content.Shared._Goobstation.Changeling;
using Content.Shared._Goobstation.Changeling.Components;

namespace Content.Server._Goobstation.Changeling.EntitySystems;

public sealed partial class ChangelingAbilitiesSystem
{
    public void SubscribeUtilityAbilities()
    {
        SubscribeLocalEvent<ChangelingComponent, BuyAugmentedEyesEvent>(OnAugmentedEyesight);
        SubscribeLocalEvent<ChangelingComponent, BuyHivemindAccessEvent>(OnHivemindAccess);
    }

    public void OnAugmentedEyesight(Entity<ChangelingComponent> changeling, ref BuyAugmentedEyesEvent args)
    {
        EnsureComp<FlashImmunityComponent>(changeling);
        PopupSystem.PopupEntity(Loc.GetString("changeling-augmented-eyesight-buy"), changeling, changeling);
    }

    public void OnHivemindAccess(Entity<ChangelingComponent> changeling, ref BuyHivemindAccessEvent args)
    {
        EnsureComp<HivemindComponent>(changeling);
        var reciever = EnsureComp<IntrinsicRadioReceiverComponent>(changeling);
        var transmitter = EnsureComp<IntrinsicRadioTransmitterComponent>(changeling);
        var radio = EnsureComp<ActiveRadioComponent>(changeling);
        radio.Channels = new() { "Hivemind" };
        transmitter.Channels = new() { "Hivemind" };

        PopupSystem.PopupEntity(Loc.GetString("changeling-hivemind-start"), changeling, changeling);
    }
}
