// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server._Hood.Phone;
using Content.Shared._Hood.Phone;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Speech.Components;
using Content.Shared.Telephone;
using Robust.Shared.GameObjects;

namespace Content.IntegrationTests.Tests._Hood.Phone;

[TestFixture]
public sealed class PhoneCommunicationTest
{
    [Test]
    public async Task SmsIsServerResolvedAndDeliveredExactlyOnce()
    {
        await using var pair = await PoolManager.GetServerClient();
        var server = pair.Server;
        var testMap = await pair.CreateTestMap();
        var entities = server.ResolveDependency<IEntityManager>();
        var itemSlots = entities.System<ItemSlotsSystem>();
        var phones = entities.System<PhoneSystem>();

        await server.WaitAssertion(() =>
        {
            (Entity<PhoneComponent> Phone, Entity<SimCardComponent> Sim) SpawnOnlinePhone()
            {
                var phoneUid = entities.SpawnEntity("HoodPhoneStreetline", testMap.GridCoords);
                var simUid = entities.SpawnEntity("HoodSimCard", testMap.GridCoords);
                Assert.That(itemSlots.TryInsert(phoneUid, PhoneComponent.SimSlotId, simUid, null), Is.True);

                return (
                    new Entity<PhoneComponent>(phoneUid, entities.GetComponent<PhoneComponent>(phoneUid)),
                    new Entity<SimCardComponent>(simUid, entities.GetComponent<SimCardComponent>(simUid)));
            }

            var sender = SpawnOnlinePhone();
            var recipient = SpawnOnlinePhone();
            var noSimUid = entities.SpawnEntity("HoodPhoneStreetline", testMap.GridCoords);
            var noSim = new Entity<PhoneComponent>(noSimUid, entities.GetComponent<PhoneComponent>(noSimUid));
            var offlineSimUid = entities.SpawnEntity("HoodSimCard", testMap.GridCoords);
            var offlineSim = new Entity<SimCardComponent>(
                offlineSimUid,
                entities.GetComponent<SimCardComponent>(offlineSimUid));

            var senderNumber = sender.Sim.Comp.Number!.Value;
            var recipientNumber = recipient.Sim.Comp.Number!.Value;
            var offlineNumber = offlineSim.Comp.Number!.Value;

            Assert.Multiple(() =>
            {
                Assert.That(
                    phones.SendSms(noSim, recipientNumber, "hello", Guid.NewGuid(), noSim.Owner),
                    Is.EqualTo(PhoneOperationError.NoSim));
                Assert.That(
                    phones.SendSms(sender.Phone, uint.MaxValue, "hello", Guid.NewGuid(), sender.Phone.Owner),
                    Is.EqualTo(PhoneOperationError.InvalidNumber));
                Assert.That(
                    phones.SendSms(sender.Phone, senderNumber, "hello", Guid.NewGuid(), sender.Phone.Owner),
                    Is.EqualTo(PhoneOperationError.SelfTarget));
                Assert.That(
                    phones.SendSms(sender.Phone, offlineNumber, "hello", Guid.NewGuid(), sender.Phone.Owner),
                    Is.EqualTo(PhoneOperationError.Offline));
                Assert.That(
                    phones.SendSms(sender.Phone, recipientNumber, "   ", Guid.NewGuid(), sender.Phone.Owner),
                    Is.EqualTo(PhoneOperationError.EmptyMessage));
            });

            var requestId = Guid.NewGuid();
            Assert.That(
                phones.SendSms(sender.Phone, recipientNumber, "first delivery", requestId, sender.Phone.Owner),
                Is.EqualTo(PhoneOperationError.None));
            Assert.That(
                phones.SendSms(sender.Phone, recipientNumber, "duplicate payload", requestId, sender.Phone.Owner),
                Is.EqualTo(PhoneOperationError.None));

            var outgoing = phones.GetConversation(sender.Sim, recipientNumber);
            var incoming = phones.GetConversation(recipient.Sim, senderNumber);

            Assert.Multiple(() =>
            {
                Assert.That(outgoing, Has.Count.EqualTo(1));
                Assert.That(incoming, Has.Count.EqualTo(1));
                Assert.That(outgoing[0].Id, Is.EqualTo(requestId));
                Assert.That(incoming[0].Id, Is.EqualTo(requestId));
                Assert.That(outgoing[0].Content, Is.EqualTo("first delivery"));
                Assert.That(incoming[0].Content, Is.EqualTo("first delivery"));
                Assert.That(outgoing[0].SenderNumber, Is.EqualTo(senderNumber));
                Assert.That(incoming[0].RecipientNumber, Is.EqualTo(recipientNumber));
            });
        });

        await pair.CleanReturnAsync();
    }

    [Test]
    public async Task CallsUseTelephoneLifecycleAndSurviveTeardown()
    {
        await using var pair = await PoolManager.GetServerClient();
        var server = pair.Server;
        var testMap = await pair.CreateTestMap();
        var entities = server.ResolveDependency<IEntityManager>();
        var itemSlots = entities.System<ItemSlotsSystem>();
        var phones = entities.System<PhoneSystem>();

        await server.WaitAssertion(() =>
        {
            (Entity<PhoneComponent> Phone, Entity<SimCardComponent> Sim) SpawnOnlinePhone()
            {
                var phoneUid = entities.SpawnEntity("HoodPhoneStreetline", testMap.GridCoords);
                var simUid = entities.SpawnEntity("HoodSimCard", testMap.GridCoords);
                Assert.That(itemSlots.TryInsert(phoneUid, PhoneComponent.SimSlotId, simUid, null), Is.True);

                return (
                    new Entity<PhoneComponent>(phoneUid, entities.GetComponent<PhoneComponent>(phoneUid)),
                    new Entity<SimCardComponent>(simUid, entities.GetComponent<SimCardComponent>(simUid)));
            }

            static TelephoneComponent Telephone(IEntityManager entities, Entity<PhoneComponent> phone)
                => entities.GetComponent<TelephoneComponent>(phone.Owner);

            var caller = SpawnOnlinePhone();
            var receiver = SpawnOnlinePhone();
            var noSimUid = entities.SpawnEntity("HoodPhoneStreetline", testMap.GridCoords);
            var noSim = new Entity<PhoneComponent>(noSimUid, entities.GetComponent<PhoneComponent>(noSimUid));
            var offlineSimUid = entities.SpawnEntity("HoodSimCard", testMap.GridCoords);
            var offlineSim = entities.GetComponent<SimCardComponent>(offlineSimUid);
            var callerNumber = caller.Sim.Comp.Number!.Value;
            var receiverNumber = receiver.Sim.Comp.Number!.Value;

            Assert.Multiple(() =>
            {
                Assert.That(
                    phones.Dial(noSim, receiverNumber, noSim.Owner),
                    Is.EqualTo(PhoneOperationError.NoSim));
                Assert.That(
                    phones.Dial(caller.Phone, uint.MaxValue, caller.Phone.Owner),
                    Is.EqualTo(PhoneOperationError.InvalidNumber));
                Assert.That(
                    phones.Dial(caller.Phone, callerNumber, caller.Phone.Owner),
                    Is.EqualTo(PhoneOperationError.SelfTarget));
                Assert.That(
                    phones.Dial(caller.Phone, offlineSim.Number!.Value, caller.Phone.Owner),
                    Is.EqualTo(PhoneOperationError.Offline));
            });

            Assert.That(
                phones.Dial(caller.Phone, receiverNumber, caller.Phone.Owner),
                Is.EqualTo(PhoneOperationError.None));
            Assert.Multiple(() =>
            {
                Assert.That(Telephone(entities, caller.Phone).CurrentState, Is.EqualTo(TelephoneState.Calling));
                Assert.That(Telephone(entities, receiver.Phone).CurrentState, Is.EqualTo(TelephoneState.Ringing));
            });

            Assert.That(phones.AcceptCall(receiver.Phone, receiver.Phone.Owner), Is.EqualTo(PhoneOperationError.None));
            Assert.Multiple(() =>
            {
                Assert.That(Telephone(entities, caller.Phone).CurrentState, Is.EqualTo(TelephoneState.InCall));
                Assert.That(Telephone(entities, receiver.Phone).CurrentState, Is.EqualTo(TelephoneState.InCall));
                Assert.That(entities.HasComponent<ActiveListenerComponent>(caller.Phone.Owner), Is.True);
                Assert.That(entities.HasComponent<ActiveListenerComponent>(receiver.Phone.Owner), Is.True);
            });

            Assert.That(phones.Hangup(caller.Phone), Is.EqualTo(PhoneOperationError.None));
            Assert.Multiple(() =>
            {
                Assert.That(Telephone(entities, caller.Phone).CurrentState, Is.EqualTo(TelephoneState.EndingCall));
                Assert.That(Telephone(entities, receiver.Phone).CurrentState, Is.EqualTo(TelephoneState.EndingCall));
                Assert.That(Telephone(entities, caller.Phone).LinkedTelephones, Is.Empty);
                Assert.That(Telephone(entities, receiver.Phone).LinkedTelephones, Is.Empty);
                Assert.That(entities.HasComponent<ActiveListenerComponent>(caller.Phone.Owner), Is.False);
                Assert.That(entities.HasComponent<ActiveListenerComponent>(receiver.Phone.Owner), Is.False);
            });

            var rejectedCaller = SpawnOnlinePhone();
            var rejectedReceiver = SpawnOnlinePhone();
            Assert.That(
                phones.Dial(
                    rejectedCaller.Phone,
                    rejectedReceiver.Sim.Comp.Number!.Value,
                    rejectedCaller.Phone.Owner),
                Is.EqualTo(PhoneOperationError.None));
            Assert.That(phones.RejectCall(rejectedReceiver.Phone), Is.EqualTo(PhoneOperationError.None));
            Assert.Multiple(() =>
            {
                Assert.That(Telephone(entities, rejectedCaller.Phone).LinkedTelephones, Is.Empty);
                Assert.That(Telephone(entities, rejectedReceiver.Phone).LinkedTelephones, Is.Empty);
                Assert.That(Telephone(entities, rejectedReceiver.Phone).CurrentState, Is.EqualTo(TelephoneState.EndingCall));
                Assert.That(
                    phones.GetCallDisposition(rejectedCaller.Phone.Owner),
                    Is.EqualTo(PhoneCallDisposition.Rejected));
                Assert.That(
                    phones.GetCallDisposition(rejectedReceiver.Phone.Owner),
                    Is.EqualTo(PhoneCallDisposition.Ended));
            });

            var removedCaller = SpawnOnlinePhone();
            var removedReceiver = SpawnOnlinePhone();
            Assert.That(
                phones.Dial(
                    removedCaller.Phone,
                    removedReceiver.Sim.Comp.Number!.Value,
                    removedCaller.Phone.Owner),
                Is.EqualTo(PhoneOperationError.None));
            Assert.That(
                phones.AcceptCall(removedReceiver.Phone, removedReceiver.Phone.Owner),
                Is.EqualTo(PhoneOperationError.None));
            Assert.That(
                itemSlots.TryEject(
                    removedCaller.Phone.Owner,
                    PhoneComponent.SimSlotId,
                    null,
                    out var ejected,
                    doAfter: false),
                Is.True);
            Assert.Multiple(() =>
            {
                Assert.That(ejected, Is.EqualTo(removedCaller.Sim.Owner));
                Assert.That(Telephone(entities, removedCaller.Phone).LinkedTelephones, Is.Empty);
                Assert.That(Telephone(entities, removedReceiver.Phone).LinkedTelephones, Is.Empty);
                Assert.That(Telephone(entities, removedCaller.Phone).CurrentState, Is.Not.EqualTo(TelephoneState.InCall));
                Assert.That(Telephone(entities, removedReceiver.Phone).CurrentState, Is.Not.EqualTo(TelephoneState.InCall));
            });

            var deletedCaller = SpawnOnlinePhone();
            var deletedReceiver = SpawnOnlinePhone();
            Assert.That(
                phones.Dial(
                    deletedCaller.Phone,
                    deletedReceiver.Sim.Comp.Number!.Value,
                    deletedCaller.Phone.Owner),
                Is.EqualTo(PhoneOperationError.None));
            Assert.That(
                phones.AcceptCall(deletedReceiver.Phone, deletedReceiver.Phone.Owner),
                Is.EqualTo(PhoneOperationError.None));

            entities.DeleteEntity(deletedCaller.Phone.Owner);

            Assert.Multiple(() =>
            {
                Assert.That(Telephone(entities, deletedReceiver.Phone).LinkedTelephones, Is.Empty);
                Assert.That(
                    Telephone(entities, deletedReceiver.Phone).CurrentState,
                    Is.Not.EqualTo(TelephoneState.InCall));
                Assert.That(entities.HasComponent<ActiveListenerComponent>(deletedReceiver.Phone.Owner), Is.False);
            });
        });

        await pair.CleanReturnAsync();
    }
}
