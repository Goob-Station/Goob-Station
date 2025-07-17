using Content.Pirate.Common._EinsteinEngines.Chat;
using Content.Server.Administration.Logs;
using Content.Server.Administration.Managers;
using Content.Server.Chat.Managers;
using Content.Server.Chat.Systems;
using Content.Shared.Abilities.Psionics;
using Content.Server.Abilities.Psionics;
using Content.Shared.Psionics.Passives;
using Content.Shared.Bed.Sleep;
using Content.Shared.Chat;
using Content.Shared.Database;
using Content.Shared.Drugs;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Physics;
using Content.Shared.Speech;
using Content.Shared.Speech.Muting;
using Content.Shared.CombatMode.Pacification;
using Content.Shared.CombatMode;
using Content.Shared.Nutrition.Components;
using Content.Shared.Psionics.Glimmer;
using Content.Shared.Humanoid;
using Content.Shared.Damage;
using Content.Server.Psionics.Glimmer;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;
using Robust.Shared.Network;
using Robust.Shared.Random;
using Robust.Shared.GameObjects.Components.Localization;
using System.Linq;
using System.Text;
using Robust.Shared.Player;
using Robust.Shared.Enums;
using Content.Pirate.Common.Chat;

namespace Content.Pirate.Server._EinsteinEngines.Chat;

public sealed partial class TelepathicChatSystem : EntitySystem, ITelepathicChatSystem
{
    [Dependency] private readonly IAdminManager _adminManager = default!;
    [Dependency] private readonly IChatManager _chatManager = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IAdminLogManager _adminLogger = default!;
    [Dependency] private readonly GlimmerSystem _glimmerSystem = default!;
    [Dependency] private readonly ChatSystem _chatSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        InitializePsychognomy();
    }

    public void InitializeSystems()
    {
        IoCManager.Register<ITelepathicChatSystem, TelepathicChatSystem>();
    }

    // --- Psychognomy helpers ---
    private void InitializePsychognomy()
    {
        SubscribeLocalEvent<HumanoidAppearanceComponent, GetPsychognomicDescriptorEvent>(DescribeHumanoid);
        SubscribeLocalEvent<GrammarComponent, GetPsychognomicDescriptorEvent>(DescribeGrammar);
        SubscribeLocalEvent<DamageableComponent, GetPsychognomicDescriptorEvent>(DescribeDamage);
        SubscribeLocalEvent<MobStateComponent, GetPsychognomicDescriptorEvent>(DescribeMobState);
        SubscribeLocalEvent<HungerComponent, GetPsychognomicDescriptorEvent>(DescribeHunger);
        SubscribeLocalEvent<PhysicsComponent, GetPsychognomicDescriptorEvent>(DescribePhysics);
        SubscribeLocalEvent<FixturesComponent, GetPsychognomicDescriptorEvent>(DescribeFixtures);
        SubscribeLocalEvent<GlimmerSourceComponent, GetPsychognomicDescriptorEvent>(DescribeGlimmerSource);
        SubscribeLocalEvent<PsionicComponent, GetPsychognomicDescriptorEvent>(DescribePsion);
        SubscribeLocalEvent<InnatePsionicPowersComponent, GetPsychognomicDescriptorEvent>(DescribeInnatePsionics);
    }

    public string SourceToDescriptor(EntityUid source)
    {
        var ev = new GetPsychognomicDescriptorEvent();
        RaiseLocalEvent(source, ev);
        ev.Descriptors.Add(Loc.GetString("p-descriptor-ignorant"));
        return _random.Pick(ev.Descriptors);
    }

    private bool IsEligibleForTelepathy(EntityUid entity)
    {
        return HasComp<TelepathyComponent>(entity)
            && !HasComp<PsionicsDisabledComponent>(entity)
            && !HasComp<PsionicInsulationComponent>(entity)
            && !HasComp<SleepingComponent>(entity)
            && (!TryComp<MobStateComponent>(entity, out var mobstate) || mobstate.CurrentState == MobState.Alive);
    }

    private (IEnumerable<INetChannel> normal, IEnumerable<INetChannel> psychog) GetPsionicChatClients()
    {
        var psions = Filter.Empty()
            .AddWhereAttachedEntity(IsEligibleForTelepathy)
            .Recipients;
        var normalSessions = psions.Where(p => !HasComp<PsychognomistComponent>(p.AttachedEntity)).Select(p => p.Channel);
        var psychogSessions = psions.Where(p => HasComp<PsychognomistComponent>(p.AttachedEntity)).Select(p => p.Channel);
        return (normalSessions, psychogSessions);
    }

    private IEnumerable<INetChannel> GetAdminClients()
    {
        return _adminManager.ActiveAdmins.Select(p => p.Channel);
    }

    private List<INetChannel> GetDreamers(IEnumerable<INetChannel> removeList)
    {
        var filteredList = new List<INetChannel>();
        var filtered = Filter.Empty()
            .AddWhereAttachedEntity(entity =>
                HasComp<PsionicComponent>(entity) && !HasComp<TelepathyComponent>(entity)
                || HasComp<SleepingComponent>(entity)
                || HasComp<SeeingRainbowsComponent>(entity) && !HasComp<PsionicsDisabledComponent>(entity) && !HasComp<PsionicInsulationComponent>(entity))
            .Recipients
            .Select(p => p.Channel);
        if (filtered.ToList() != null)
            filteredList = filtered.ToList();
        foreach (var entity in removeList)
            filteredList.Remove(entity);
        return filteredList;
    }

    private string ObfuscateMessageReadability(string message, float chance)
    {
        var modifiedMessage = new StringBuilder(message);
        for (var i = 0; i < message.Length; i++)
        {
            if (char.IsWhiteSpace(modifiedMessage[i]))
                continue;
            if (_random.Prob(1 - chance))
                modifiedMessage[i] = '~';
        }
        return modifiedMessage.ToString();
    }

    // --- Psychognomy event and describers ---
    private void DescribeHumanoid(EntityUid uid, HumanoidAppearanceComponent component, GetPsychognomicDescriptorEvent ev)
    {
        if (component.Sex != Sex.Unsexed)
            ev.Descriptors.Add(component.Sex == Sex.Male ? Loc.GetString("p-descriptor-male") : Loc.GetString("p-descriptor-female"));
        ev.Descriptors.Add(component.Age >= 100 ? Loc.GetString("p-descriptor-old") : Loc.GetString("p-descriptor-young"));
    }
    private void DescribeGrammar(EntityUid uid, GrammarComponent component, GetPsychognomicDescriptorEvent ev)
    {
        if (component.Gender == Gender.Male || component.Gender == Gender.Female)
            ev.Descriptors.Add(component.Gender == Gender.Male ? Loc.GetString("p-descriptor-masculine") : Loc.GetString("p-descriptor-feminine"));
    }
    private void DescribeDamage(EntityUid uid, DamageableComponent component, GetPsychognomicDescriptorEvent ev)
    {
        if (component.DamageContainerID == "CorporealSpirit")
        {
            ev.Descriptors.Add(Loc.GetString("p-descriptor-liminal"));
            if (!HasComp<HumanoidAppearanceComponent>(uid))
                ev.Descriptors.Add(Loc.GetString("p-descriptor-old"));
            return;
        }
        ev.Descriptors.Add(Loc.GetString("p-descriptor-hylic"));
    }
    private void DescribeMobState(EntityUid uid, MobStateComponent component, GetPsychognomicDescriptorEvent ev)
    {
        if (component.CurrentState != MobState.Critical)
            return;
        ev.Descriptors.Add(Loc.GetString("p-descriptor-liminal"));
    }
    private void DescribeHunger(EntityUid uid, HungerComponent component, GetPsychognomicDescriptorEvent ev)
    {
        if (component.CurrentThreshold > HungerThreshold.Peckish)
            return;
        ev.Descriptors.Add(Loc.GetString("p-descriptor-hungry"));
    }
    private void DescribeFixtures(EntityUid uid, FixturesComponent component, GetPsychognomicDescriptorEvent ev)
    {
        foreach (var fixture in component.Fixtures.Values)
            if (fixture.CollisionMask == (int) CollisionGroup.GhostImpassable)
            {
                ev.Descriptors.Add(Loc.GetString("p-descriptor-pneumatic"));
                return;
            }
    }
    private void DescribePhysics(EntityUid uid, PhysicsComponent component, GetPsychognomicDescriptorEvent ev)
    {
        if (component.FixturesMass < 45)
            ev.Descriptors.Add(Loc.GetString("p-descriptor-light"));
        else if (component.FixturesMass > 70)
            ev.Descriptors.Add(Loc.GetString("p-descriptor-heavy"));
    }
    private void DescribeGlimmerSource(EntityUid uid, GlimmerSourceComponent component, GetPsychognomicDescriptorEvent ev)
    {
        if (component.GlimmerPerSecond >= 0)
            ev.Descriptors.Add(Loc.GetString("p-descriptor-emanative"));
        else
        {
            ev.Descriptors.Add(Loc.GetString("p-descriptor-vampiric"));
            ev.Descriptors.Add(Loc.GetString("p-descriptor-hungry"));
        }
    }
    private void DescribePsion(EntityUid uid, PsionicComponent component, GetPsychognomicDescriptorEvent ev)
    {
        if (component.PsychognomicDescriptors.Count > 0)
        {
            foreach (var descriptor in component.PsychognomicDescriptors)
                ev.Descriptors.Add(Loc.GetString(descriptor));
        }
        if (!HasComp<SpeechComponent>(uid) || HasComp<MutedComponent>(uid))
            ev.Descriptors.Add(Loc.GetString("p-descriptor-dumb"));
        if (!HasComp<CombatModeComponent>(uid) || HasComp<PacifiedComponent>(uid))
            ev.Descriptors.Add(Loc.GetString("p-descriptor-passive"));
        foreach (var power in component.ActivePowers)
        {
            if (power.ID != "PyrokinesisPower" && power.ID != "NoosphericZapPower")
                continue;
            ev.Descriptors.Add(Loc.GetString("p-descriptor-kinetic"));
            return;
        }
    }
    private void DescribeInnatePsionics(EntityUid uid, InnatePsionicPowersComponent component, GetPsychognomicDescriptorEvent ev)
    {
        ev.Descriptors.Add(Loc.GetString("p-descriptor-gnostic"));
    }
    private sealed class GetPsychognomicDescriptorEvent : EntityEventArgs
    {
        public List<string> Descriptors = new();
    }

    public void SendTelepathicChat(EntityUid source, string message, string senderName, bool hideChat = false)
    {
        if (!IsEligibleForTelepathy(source))
            return;

        var clients = GetPsionicChatClients();
        var admins = GetAdminClients();
        string messageWrap;
        string adminMessageWrap;

        messageWrap = Loc.GetString("chat-manager-send-telepathic-chat-wrap-message",
            ("telepathicChannelName", Loc.GetString("chat-manager-telepathic-channel-name")), ("message", message));

        adminMessageWrap = Loc.GetString("chat-manager-send-telepathic-chat-wrap-message-admin",
            ("source", senderName), ("message", message));

        _adminLogger.Add(LogType.Chat, LogImpact.Low, $"Telepathic chat from {senderName}: {message}");

        _chatManager.ChatMessageToMany(ChatChannel.Telepathic, message, messageWrap, source, hideChat, true, clients.normal.ToList(), Color.PaleVioletRed);

        _chatManager.ChatMessageToMany(ChatChannel.Telepathic, message, adminMessageWrap, source, hideChat, true, admins, Color.PaleVioletRed);

        if (clients.psychog.Count() > 0)
        {
            var descriptor = SourceToDescriptor(source);
            string psychogMessageWrap;

            psychogMessageWrap = Loc.GetString("chat-manager-send-telepathic-chat-wrap-message-psychognomy",
                ("source", descriptor.ToUpper()), ("message", message));

            _chatManager.ChatMessageToMany(ChatChannel.Telepathic, message, psychogMessageWrap, source, hideChat, true, clients.psychog.ToList(), Color.PaleVioletRed);
        }

        if (_random.Prob(0.1f))
            _glimmerSystem.DeltaGlimmerInput(1);

        if (_random.Prob(Math.Min(0.33f + (float) _glimmerSystem.GlimmerOutput / 1500, 1)))
        {
            float obfuscation = 0.25f + (float) _glimmerSystem.GlimmerOutput / 2000;
            var obfuscated = ObfuscateMessageReadability(message, obfuscation);
            _chatManager.ChatMessageToMany(ChatChannel.Telepathic, obfuscated, messageWrap, source, hideChat, false, GetDreamers(clients.normal.Concat(clients.psychog)), Color.PaleVioletRed);
        }

        foreach (var repeater in EntityQuery<TelepathicRepeaterComponent>())
            _chatSystem.TrySendInGameICMessage(repeater.Owner, message, InGameICChatType.Speak, false);
    }

    // ...rest of the class (private helpers, etc.)...
}
