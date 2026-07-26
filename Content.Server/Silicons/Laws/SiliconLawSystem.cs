// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Server._DV.Objectives.Events; // DeltaV
using Content.Server.Administration;
using Content.Server.Chat.Managers;
using Content.Server.Station.Systems;
using Content.Shared.Administration;
using Content.Shared.Chat;
using Content.Shared.Emag.Systems;
using Content.Shared.GameTicking;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Content.Shared.Radio.Components;
using Content.Shared.Roles;
using Content.Shared.Roles.Components;
using Content.Shared.Silicons.Laws;
using Content.Shared.Silicons.Laws.Components;
using Content.Shared.Database; // goob logging
using Content.Server.Administration.Logs; // goob logging
using Robust.Server.GameObjects;
using Robust.Shared.Audio;
using Robust.Shared.Containers;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Toolshed;

// Goobstation usings
using Content.Goobstation.Common.Silicons.Components;
using Content.Goobstation.Maths.FixedPoint;
using Content.Goobstation.Shared.CustomLawboard;
using Robust.Shared.Random;
using Content.Shared.Random;
using Content.Shared.Random.Helpers;
using Content.Shared.Research.Components;
using Content.Server.Radio.EntitySystems;
using Content.Server.Research.Systems;

// Corvax-Next-AiRemoteControl
using Content.Shared.Silicons.StationAi;
using Content.Shared.Tag;
using Content.Shared._CorvaxNext.Silicons.Borgs.Components;
namespace Content.Server.Silicons.Laws;

public sealed class SiliconLawSystem : SharedSiliconLawSystem
{
    [Dependency] private readonly IChatManager _chatManager = default!;
    [Dependency] private readonly IAdminLogManager _adminLogger = default!; // goob logging
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly SharedRoleSystem _roles = default!;
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly UserInterfaceSystem _userInterface = default!;
    [Dependency] private readonly EmagSystem _emag = default!;

    // Goobstation
    [Dependency] private readonly IRobustRandom _robustRandom = default!;
    [Dependency] private readonly IonStormSystem _ionStorm = default!;
    [Dependency] private readonly ResearchSystem _research = default!;
    [Dependency] private readonly RadioSystem _radio = default!;
    [Dependency] private readonly TagSystem _tagSystem = default!; // Corvax-Next-AiRemoteControl



    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SiliconLawBoundComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<SiliconLawBoundComponent, MindAddedMessage>(OnMindAdded);
        SubscribeLocalEvent<SiliconLawBoundComponent, ToggleLawsScreenEvent>(OnToggleLawsScreen);
        SubscribeLocalEvent<SiliconLawBoundComponent, BoundUIOpenedEvent>(OnBoundUIOpened);
        SubscribeLocalEvent<SiliconLawBoundComponent, PlayerSpawnCompleteEvent>(OnPlayerSpawnComplete);

        SubscribeLocalEvent<SiliconLawProviderComponent, GetSiliconLawsEvent>(OnDirectedGetLaws);
        SubscribeLocalEvent<SiliconLawProviderComponent, IonStormLawsEvent>(OnIonStormLaws);
        SubscribeLocalEvent<SiliconLawProviderComponent, MindAddedMessage>(OnLawProviderMindAdded);
        SubscribeLocalEvent<SiliconLawProviderComponent, MindRemovedMessage>(OnLawProviderMindRemoved);
        SubscribeLocalEvent<SiliconLawProviderComponent, SiliconEmaggedEvent>(OnEmagLawsAdded);
    }

    private void OnMapInit(EntityUid uid, SiliconLawBoundComponent component, MapInitEvent args)
    {
        GetLaws(uid, component);
    }

    private void OnMindAdded(EntityUid uid, SiliconLawBoundComponent component, MindAddedMessage args)
    {
        if (!TryComp<ActorComponent>(uid, out var actor))
            return;

        // Corvax-Next-AiRemoteControl-Start
        if (HasComp<AiRemoteControllerComponent>(uid)
            || _tagSystem.HasTag(uid, "StationAi")) // skip a law's notification for remotable and AI
            return;
        // Corvax-Next-AiRemoteControl-End

        // Goobstation admin laws logging start
        var laws = GetLaws(uid, component);
        _adminLogger.Add(LogType.SiliconLaws, LogImpact.Low, $"{ToPrettyString(uid):entity} joined the round with laws:\n{laws.LoggingString()}");
        // Goobstation end

        var msg = Loc.GetString("laws-notify");
        var wrappedMessage = Loc.GetString("chat-manager-server-wrap-message", ("message", msg));
        _chatManager.ChatMessageToOne(ChatChannel.Server, msg, wrappedMessage, default, false, actor.PlayerSession.Channel, colorOverride: Color.FromHex("#5ed7aa"));

        if (!TryComp<SiliconLawProviderComponent>(uid, out var lawcomp))
            return;

        if (!lawcomp.Subverted)
            return;

        var modifedLawMsg = Loc.GetString("laws-notify-subverted");
        var modifiedLawWrappedMessage = Loc.GetString("chat-manager-server-wrap-message", ("message", modifedLawMsg));
        _chatManager.ChatMessageToOne(ChatChannel.Server, modifedLawMsg, modifiedLawWrappedMessage, default, false, actor.PlayerSession.Channel, colorOverride: Color.Red);
    }

    private void OnLawProviderMindAdded(Entity<SiliconLawProviderComponent> ent, ref MindAddedMessage args)
    {
        if (!ent.Comp.Subverted)
            return;
        EnsureSubvertedSiliconRole(args.Mind);
    }

    private void OnLawProviderMindRemoved(Entity<SiliconLawProviderComponent> ent, ref MindRemovedMessage args)
    {
        if (!ent.Comp.Subverted)
            return;
        RemoveSubvertedSiliconRole(args.Mind);

    }


    private void OnToggleLawsScreen(EntityUid uid, SiliconLawBoundComponent component, ToggleLawsScreenEvent args)
    {
        if (args.Handled || !TryComp<ActorComponent>(uid, out var actor))
            return;
        args.Handled = true;

        _userInterface.TryToggleUi(uid, SiliconLawsUiKey.Key, actor.PlayerSession);
    }

    private void OnBoundUIOpened(EntityUid uid, SiliconLawBoundComponent component, BoundUIOpenedEvent args)
    {
        TryComp(uid, out IntrinsicRadioTransmitterComponent? intrinsicRadio);
        var radioChannels = intrinsicRadio?.Channels;

        var state = new SiliconLawBuiState(GetLaws(uid).Laws, radioChannels);
        _userInterface.SetUiState(args.Entity, SiliconLawsUiKey.Key, state);
    }

    private void OnPlayerSpawnComplete(EntityUid uid, SiliconLawBoundComponent component, PlayerSpawnCompleteEvent args)
    {
        component.LastLawProvider = args.Station;
    }

    private void OnDirectedGetLaws(EntityUid uid, SiliconLawProviderComponent component, ref GetSiliconLawsEvent args)
    {
        if (args.Handled)
            return;

        if (component.Lawset == null)
            component.Lawset = GetLawset(component.Laws);

        args.Laws = component.Lawset;

        args.Handled = true;
    }

    private void OnIonStormLaws(EntityUid uid, SiliconLawProviderComponent component, ref IonStormLawsEvent args)
    {
        // Emagged borgs are immune to ion storm
        if (!_emag.CheckFlag(uid, EmagType.Interaction))
        {
            component.Lawset = args.Lawset;

            // gotta tell player to check their laws
            NotifyLawsChanged(uid, component.LawUploadSound);

            // Show the silicon has been subverted.
            component.Subverted = true;

            // new laws may allow antagonist behaviour so make it clear for admins
            if(_mind.TryGetMind(uid, out var mindId, out _))
                EnsureSubvertedSiliconRole(mindId);

        }
    }

    private void OnEmagLawsAdded(EntityUid uid, SiliconLawProviderComponent component, ref SiliconEmaggedEvent args)
    {
        if (component.Lawset == null)
            component.Lawset = GetLawset(component.Laws);

        // Corvax-Next-AiRemoteControl-Start
        if (HasComp<AiRemoteControllerComponent>(uid)) // You can't emag controllable entities
            return;
        // Corvax-Next-AiRemoteControl-End

        // Show the silicon has been subverted.
        component.Subverted = true;

        // Add the first emag law before the others
        var name = CompOrNull<EmagSiliconLawComponent>(uid)?.OwnerName ?? Name(args.user); // DeltaV: Reuse emagger name if possible
        component.Lawset?.Laws.Insert(0, new SiliconLaw
        {
            LawString = Loc.GetString("law-emag-custom", ("name", name), ("title", Loc.GetString(component.Lawset.ObeysTo))), // DeltaV: pass name from variable
            Order = -1 // Goobstation - AI/borg law changes - borgs obeying AI
        });

        //Add the secrecy law after the others
        component.Lawset?.Laws.Add(new SiliconLaw
        {
            LawString = Loc.GetString("law-emag-secrecy", ("faction", Loc.GetString(component.Lawset.ObeysTo))),
            Order = component.Lawset.Laws.Max(law => law.Order) + 1
        });

        _adminLogger.Add(LogType.SiliconLaws, LogImpact.High, $"{ToPrettyString(uid):entity} laws changed due to emag by {ToPrettyString(args.user):user} to:{component.Lawset!.LoggingString()}"); // goob
    }

    protected override void EnsureSubvertedSiliconRole(EntityUid mindId)
    {
        base.EnsureSubvertedSiliconRole(mindId);

        if (!_roles.MindHasRole<SubvertedSiliconRoleComponent>(mindId))
            _roles.MindAddRole(mindId, "MindRoleSubvertedSilicon", silent: true);
    }

    protected override void RemoveSubvertedSiliconRole(EntityUid mindId)
    {
        base.RemoveSubvertedSiliconRole(mindId);

        if (_roles.MindHasRole<SubvertedSiliconRoleComponent>(mindId))
            _roles.MindRemoveRole<SubvertedSiliconRoleComponent>(mindId);
    }

    public SiliconLawset GetLaws(EntityUid uid, SiliconLawBoundComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return new SiliconLawset();

        var ev = new GetSiliconLawsEvent(uid);

        RaiseLocalEvent(uid, ref ev);
        if (ev.Handled)
        {
            component.LastLawProvider = uid;
            return ev.Laws;
        }

        var xform = Transform(uid);

        if (_station.GetOwningStation(uid, xform) is { } station)
        {
            RaiseLocalEvent(station, ref ev);
            if (ev.Handled)
            {
                component.LastLawProvider = station;
                return ev.Laws;
            }
        }

        if (xform.GridUid is { } grid)
        {
            RaiseLocalEvent(grid, ref ev);
            if (ev.Handled)
            {
                component.LastLawProvider = grid;
                return ev.Laws;
            }
        }

        if (component.LastLawProvider == null ||
            Deleted(component.LastLawProvider) ||
            Terminating(component.LastLawProvider.Value))
        {
            component.LastLawProvider = null;
        }
        else
        {
            RaiseLocalEvent(component.LastLawProvider.Value, ref ev);
            if (ev.Handled)
            {
                return ev.Laws;
            }
        }

        RaiseLocalEvent(ref ev);
        return ev.Laws;
    }

    public override void NotifyLawsChanged(EntityUid uid, SoundSpecifier? cue = null)
    {
        base.NotifyLawsChanged(uid, cue);

        if (!TryComp<ActorComponent>(uid, out var actor))
            return;

        var msg = Loc.GetString("laws-update-notify");
        var wrappedMessage = Loc.GetString("chat-manager-server-wrap-message", ("message", msg));
        _chatManager.ChatMessageToOne(ChatChannel.Server, msg, wrappedMessage, default, false, actor.PlayerSession.Channel, colorOverride: Color.Red);

        if (cue != null && _mind.TryGetMind(uid, out var mindId, out _))
            _roles.MindPlaySound(mindId, cue);
    }

    /// <summary>
    /// Extract all the laws from a lawset's prototype ids.
    /// </summary>
    public SiliconLawset GetLawset(ProtoId<SiliconLawsetPrototype> lawset)
    {
        var proto = _prototype.Index(lawset);
        var laws = new SiliconLawset()
        {
            Laws = new List<SiliconLaw>(proto.Laws.Count)
        };
        foreach (var law in proto.Laws)
        {
            laws.Laws.Add(_prototype.Index<SiliconLawPrototype>(law).ShallowClone());
        }
        laws.ObeysTo = proto.ObeysTo;

        return laws;
    }

    /// <summary>
    /// Set the laws of a silicon entity while notifying the player.
    /// </summary>
    public void SetLaws(List<SiliconLaw> newLaws, EntityUid target, SoundSpecifier? cue = null)
    {
        if (!TryComp<SiliconLawProviderComponent>(target, out var component))
            return;

        if (component.Lawset == null)
            component.Lawset = new SiliconLawset();

        component.Lawset.Laws = newLaws;
        NotifyLawsChanged(target, cue);

        _adminLogger.Add(LogType.SiliconLaws, LogImpact.Medium, $"{ToPrettyString(target):entity} laws changed to:{component.Lawset.LoggingString()}"); // goob
    }

    protected override void OnUpdaterInsert(Entity<SiliconLawUpdaterComponent> ent, ref EntInsertedIntoContainerMessage args)
    {
        // TODO: Prediction dump this
        if (!TryComp<SiliconLawProviderComponent>(args.Entity, out var provider))
            return;

        // var lawset = provider.Lawset ?? GetLawset(provider.Laws); // Goob edit below

        // Goob edit start
        if (HasComp<ActiveExperimentalLawProviderComponent>(ent))
        {
            var message = Loc.GetString("experimental-law-provider-fail");
            _radio.SendRadioMessage(ent, message, AnnouncementChannel, ent, escapeMarkup: false);
            RemComp<ActiveExperimentalLawProviderComponent>(ent);
        }

        if (TryComp(args.Entity, out ExperimentalLawProviderComponent? experimentalLaws))
        {
            ApplyExperimentalLaws(ent, (args.Entity, experimentalLaws, provider));
            return;
        }

        // This part is for custom lawboards as there's no lawset prototype for them.

        List<SiliconLaw>? lawset;

        if (TryComp(args.Entity, out CustomLawboardComponent? customLawboard))
        {
            lawset = customLawboard.Laws;
        }
        else
        {
            lawset = GetLawset(provider.Laws).Laws;
        }

        // Goob edit end

        var query = EntityManager.CompRegistryQueryEnumerator(ent.Comp.Components);

        while (query.MoveNext(out var update))
        {
            SetLaws(lawset, update, provider.LawUploadSound);

            // Corvax-Next-AiRemoteControl-Start
            if (TryComp<StationAiHeldComponent>(update, out var stationAiHeldComp)
                && stationAiHeldComp.CurrentConnectedEntity != null
                && HasComp<SiliconLawProviderComponent>(stationAiHeldComp.CurrentConnectedEntity))
            {
                SetLaws(lawset, stationAiHeldComp.CurrentConnectedEntity.Value, provider.LawUploadSound);
            }
            // Corvax-Next-AiRemoteControl-End
            RaiseLocalEvent(new AILawUpdatedEvent(update, provider.Laws)); // DeltaV
        }

        ent.Comp.LastLawset = provider.Laws;
    }

    // Corvax-Next-AiRemoteControl-Start
    public void SetLawsSilent(List<SiliconLaw> newLaws, EntityUid target, SoundSpecifier? cue = null)
    {
        if (!TryComp<SiliconLawProviderComponent>(target, out var component))
            return;

        if (component.Lawset == null)
            component.Lawset = new SiliconLawset();

        component.Lawset.Laws = newLaws;

        _adminLogger.Add(LogType.SiliconLaws, LogImpact.Medium, $"{ToPrettyString(target):entity} laws changed silently to:{component.Lawset.LoggingString()}"); // goob
    }
    // Corvax-Next-AiRemoteControl-End

    // Goob edit start
    private void ApplyExperimentalLaws(Entity<SiliconLawUpdaterComponent> ent, Entity<ExperimentalLawProviderComponent, SiliconLawProviderComponent> experiment)
    {
        var laws = GetRandomLaws(experiment.Comp1.RandomLawsets);
        var query = EntityManager.CompRegistryQueryEnumerator(ent.Comp.Components);

        while (query.MoveNext(out var update))
            SetLaws(laws.Laws, update, experiment.Comp2.LawUploadSound);

        var activeProv = EnsureComp<ActiveExperimentalLawProviderComponent>(ent);
        activeProv.Timer = experiment.Comp1.RewardTime;
        activeProv.RewardPoints = experiment.Comp1.RewardPoints;
        activeProv.OldSiliconLawsetId = ent.Comp.LastLawset;

        var message = Loc.GetString("experimental-law-provider-start", ("timeLeft", (int) experiment.Comp1.RewardTime));
        _radio.SendRadioMessage(ent, message, AnnouncementChannel, ent, escapeMarkup: false);

        QueueDel(experiment); // Don't need this experimental board anymore
    }

    private const string AnnouncementChannel = "Science";

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var activeExperimental = EntityQueryEnumerator<ActiveExperimentalLawProviderComponent>();
        while (activeExperimental.MoveNext(out var uid, out var provider))
        {
            provider.Timer -= frameTime;
            if (provider.Timer >= 0)
                continue;

            // Reward time!!!
            if (!TryComp(uid, out ResearchClientComponent? researchClient) ||
                !researchClient.ConnectedToServer ||
                researchClient.Server == null)
                continue;

            if (!TryComp(uid, out SiliconLawUpdaterComponent? updater))
                continue;

            // Replace laws back
            var lawset = GetLawset(provider.OldSiliconLawsetId).Laws;
            var query = EntityManager.CompRegistryQueryEnumerator(updater.Components);

            while (query.MoveNext(out var update))
                SetLaws(lawset, update, provider.LawRewardSound);

            RemCompDeferred(uid, provider);
            _research.ModifyServerPoints(researchClient.Server.Value, provider.RewardPoints);
            var message = Loc.GetString("experimental-law-provider-success", ("amount", provider.RewardPoints));
            _radio.SendRadioMessage(uid, message, AnnouncementChannel, uid, escapeMarkup: false);
        }
    }

    /// <summary>
    /// Goob edit: generates random ion storm lawset without an actual silicon.
    /// </summary>
    private SiliconLawset GetRandomLaws(ProtoId<WeightedRandomPrototype> availableSetsId)
    {
        // try to swap it out with a random lawset
        var lawsets = _prototype.Index(availableSetsId);
        var lawset = lawsets.Pick(_robustRandom);
        var laws = GetLawset(lawset);

        // clone it so not modifying stations lawset
        laws = laws.Clone();

        // shuffle them all
        // hopefully work with existing glitched laws if there are multiple ion storms
        var baseOrder = FixedPoint2.New(1);
        foreach (var law in laws.Laws)
            if (law.Order < baseOrder)
                baseOrder = law.Order;

        _robustRandom.Shuffle(laws.Laws);

        // change order based on shuffled position
        for (var i = 0; i < laws.Laws.Count; i++)
            laws.Laws[i].Order = baseOrder + i;

        // remove a random law
        laws.Laws.RemoveAt(_robustRandom.Next(laws.Laws.Count));

        // generate a new law...
        var newLaw = _ionStorm.GenerateLaw();

        // see if the law we add will replace a random existing law or be a new glitched order one
        if (laws.Laws.Count > 0)
        {
            var i = _robustRandom.Next(laws.Laws.Count);
            laws.Laws[i] = new SiliconLaw()
            {
                LawString = newLaw,
                Order = laws.Laws[i].Order
            };
        }
        else
        {
            laws.Laws.Insert(0,
                new SiliconLaw
                {
                    LawString = newLaw,
                    Order = -1,
                    LawIdentifierOverride = Loc.GetString("ion-storm-law-scrambled-number", ("length", _robustRandom.Next(5, 10)))
                });
        }

        // sets all unobfuscated laws' indentifier in order from highest to lowest priority
        // This could technically override the Obfuscation from the code above, but it seems unlikely enough to basically never happen
        var orderDeduction = -1;

        for (var i = 0; i < laws.Laws.Count; i++)
        {
            var notNullIdentifier = laws.Laws[i].LawIdentifierOverride ?? (i - orderDeduction).ToString();

            if (notNullIdentifier.Any(char.IsSymbol))
                orderDeduction += 1;
            else
                laws.Laws[i].LawIdentifierOverride = (i - orderDeduction).ToString();
        }

        return laws;
    }
    // Goob edit end
}

[ToolshedCommand, AdminCommand(AdminFlags.Admin)]
public sealed class LawsCommand : ToolshedCommand
{
    private SiliconLawSystem? _law;

    [CommandImplementation("list")]
    public IEnumerable<EntityUid> List()
    {
        var query = EntityManager.EntityQueryEnumerator<SiliconLawBoundComponent>();
        while (query.MoveNext(out var uid, out _))
        {
            yield return uid;
        }
    }

    [CommandImplementation("get")]
    public IEnumerable<string> Get([PipedArgument] EntityUid lawbound)
    {
        _law ??= GetSys<SiliconLawSystem>();

        foreach (var law in _law.GetLaws(lawbound).Laws)
        {
            yield return $"law {law.LawIdentifierOverride ?? law.Order.ToString()}: {Loc.GetString(law.LawString)}";
        }
    }
}
