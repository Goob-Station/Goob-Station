using JetBrains.Annotations;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.Manager;
using Content.Shared.Actions;
using Content.Shared.Psionics;
using Content.Shared.Abilities.Psionics;
using Content.Shared.Popups;
using Content.Shared.Chat;
using Content.Shared.Psionics.Glimmer;
using Content.Shared.Random;
using Content.Server.Chat.Managers;
using Robust.Shared.Player;

namespace Content.Server.Abilities.Psionics;

[UsedImplicitly]
public sealed partial class AddPsionicActions : PsionicPowerFunction
{
    /// <summary>
    ///     The list of each Action that this power adds in the form of ActionId and ActionEntity
    /// </summary>
    [DataField]
    public List<EntProtoId> Actions = new();

    public override void OnAddPsionic(
        EntityUid uid,
        IComponentFactory factory,
        IEntityManager entityManager,
        ISerializationManager serializationManager,
        ISharedPlayerManager playerManager,
        ILocalizationManager loc,
        PsionicComponent psionicComponent,
        PsionicPowerPrototype proto)
    {
        var actions = entityManager.System<SharedActionsSystem>();
        foreach (var id in Actions)
        {
            EntityUid? actionId = null;
            if (actions.AddAction(uid, ref actionId, id))
            {
                actions.StartUseDelay(actionId);
                psionicComponent.Actions.Add(proto.ID, actionId);
            }
        }
    }
}

[UsedImplicitly]
public sealed partial class RemovePsionicActions : PsionicPowerFunction
{
    // As a novelty, this does not require any DataFields.
    // This removes all Actions directly associated with a specific power, which works with our current system of record-keeping
    // for psi-powers.
    public override void OnAddPsionic(
        EntityUid uid,
        IComponentFactory factory,
        IEntityManager entityManager,
        ISerializationManager serializationManager,
        ISharedPlayerManager playerManager,
        ILocalizationManager loc,
        PsionicComponent psionicComponent,
        PsionicPowerPrototype proto)
    {
        var actions = entityManager.System<SharedActionsSystem>();
        if (psionicComponent.Actions is null
            || !psionicComponent.Actions.ContainsKey(proto.ID))
            return;

        var copy = serializationManager.CreateCopy(psionicComponent.Actions, notNullableOverride: true);

        foreach (var (id, actionUid) in copy)
        {
            if (id != proto.ID)
                continue;

            actions.RemoveAction(uid, actionUid);
        }
    }
}

[UsedImplicitly]
public sealed partial class AddPsionicPowerComponents : PsionicPowerFunction
{
    /// <summary>
    ///     The list of what Components this power adds.
    /// </summary>
    [DataField]
    public ComponentRegistry Components = new();

    public override void OnAddPsionic(
        EntityUid uid,
        IComponentFactory factory,
        IEntityManager entityManager,
        ISerializationManager serializationManager,
        ISharedPlayerManager playerManager,
        ILocalizationManager loc,
        PsionicComponent psionicComponent,
        PsionicPowerPrototype proto)
    {
        foreach (var entry in Components.Values)
        {
            if (entityManager.HasComponent(uid, entry.Component.GetType()))
                continue;

            var comp = (Component) serializationManager.CreateCopy(entry.Component, notNullableOverride: true);
            comp.Owner = uid;
            entityManager.AddComponent(uid, comp);
        }
    }
}

[UsedImplicitly]
public sealed partial class RemovePsionicPowerComponents : PsionicPowerFunction
{
    /// <summary>
    ///     The list of what Components this power removes.
    /// </summary>
    [DataField]
    public ComponentRegistry Components = new();

    public override void OnAddPsionic(
        EntityUid uid,
        IComponentFactory factory,
        IEntityManager entityManager,
        ISerializationManager serializationManager,
        ISharedPlayerManager playerManager,
        ILocalizationManager loc,
        PsionicComponent psionicComponent,
        PsionicPowerPrototype proto)
    {
        foreach (var (name, _) in Components)
            entityManager.RemoveComponentDeferred(uid, factory.GetComponent(name).GetType());
    }
}

[UsedImplicitly]
public sealed partial class AddPsionicStatSources : PsionicPowerFunction
{
    /// <summary>
    ///     How much this power will increase or decrease a user's Amplification.
    /// </summary>
    [DataField]
    public float AmplificationModifier;

    /// <summary>
    ///     How much this power will increase or decrease a user's Dampening.
    /// </summary>
    [DataField]
    public float DampeningModifier;

    public override void OnAddPsionic(
        EntityUid uid,
        IComponentFactory factory,
        IEntityManager entityManager,
        ISerializationManager serializationManager,
        ISharedPlayerManager playerManager,
        ILocalizationManager loc,
        PsionicComponent psionicComponent,
        PsionicPowerPrototype proto)
    {
        if (AmplificationModifier != 0)
            psionicComponent.AmplificationSources.Add(proto.Name, AmplificationModifier);

        if (DampeningModifier != 0)
            psionicComponent.DampeningSources.Add(proto.Name, DampeningModifier);
    }
}

[UsedImplicitly]
public sealed partial class RemovePsionicStatSources : PsionicPowerFunction
{
    public override void OnAddPsionic(
        EntityUid uid,
        IComponentFactory factory,
        IEntityManager entityManager,
        ISerializationManager serializationManager,
        ISharedPlayerManager playerManager,
        ILocalizationManager loc,
        PsionicComponent psionicComponent,
        PsionicPowerPrototype proto)
    {
        psionicComponent.AmplificationSources.Remove(proto.Name);
        psionicComponent.DampeningSources.Remove(proto.Name);
    }
}

[UsedImplicitly]
public sealed partial class PsionicFeedbackPopup : PsionicPowerFunction
{
    /// <summary>
    ///     What message will be sent to the player as a Popup.
    ///     If left blank, it will default to the Const "generic-power-initialization-feedback"
    /// </summary>
    [DataField]
    public string InitializationPopup = "generic-power-initialization-feedback";

    [DataField]
    public PopupType InitPopupType = PopupType.MediumCaution;

    public override void OnAddPsionic(
        EntityUid uid,
        IComponentFactory factory,
        IEntityManager entityManager,
        ISerializationManager serializationManager,
        ISharedPlayerManager playerManager,
        ILocalizationManager loc,
        PsionicComponent psionicComponent,
        PsionicPowerPrototype proto)
    {
        var popups = entityManager.System<SharedPopupSystem>();
        if (playerManager.TryGetSessionByEntity(uid, out var session)
            || session is null
            || !loc.TryGetString(InitializationPopup, out var popupString))
            return;

        popups.PopupEntity(popupString, uid, uid, InitPopupType);
    }
}

[UsedImplicitly]
public sealed partial class PsionicFeedbackSelfChat : PsionicPowerFunction
{
    /// <summary>
    ///     What message will be sent to the player as a Chat message.
    ///     If left blank, it will default to the Const "generic-power-initialization-feedback"
    /// </summary>
    [DataField]
    public string FeedbackMessage = "generic-power-initialization-feedback";

    /// <summary>
    ///     What color will the initialization feedback display in the chat window with.
    /// </summary>
    [DataField]
    public string InitializationFeedbackColor = "#8A00C2";

    /// <summary>
    ///     What font size will the initialization message use in chat.
    /// </summary>
    [DataField]
    public int InitializationFeedbackFontSize = 12;


    /// <summary>
    ///     Which chat channel will the initialization message use.
    /// </summary>
    [DataField]
    public ChatChannel InitializationFeedbackChannel = ChatChannel.Emotes;

    public override void OnAddPsionic(
        EntityUid uid,
        IComponentFactory factory,
        IEntityManager entityManager,
        ISerializationManager serializationManager,
        ISharedPlayerManager playerManager,
        ILocalizationManager loc,
        PsionicComponent psionicComponent,
        PsionicPowerPrototype proto)
    {
        var chatManager = IoCManager.Resolve<IChatManager>();
        if (!playerManager.TryGetSessionByEntity(uid, out var session)
            || session is null
            || !loc.TryGetString(FeedbackMessage, out var feedback))
            return;

        var feedbackMessage = $"[font size={InitializationFeedbackFontSize}][color={InitializationFeedbackColor}]{feedback}[/color][/font]";
        chatManager.ChatMessageToOne(
            InitializationFeedbackChannel,
            feedbackMessage,
            feedbackMessage,
            EntityUid.Invalid,
            false,
            session.Channel);
    }
}

[UsedImplicitly]
public sealed partial class AddPsionicAssayFeedback : PsionicPowerFunction
{
    /// <summary>
    ///     What message will this power generate when scanned by an Assay user.
    ///     These are also used for the Psi-Potentiometer.
    /// </summary>
    [DataField]
    public string AssayFeedback = "";

    public override void OnAddPsionic(
        EntityUid uid,
        IComponentFactory factory,
        IEntityManager entityManager,
        ISerializationManager serializationManager,
        ISharedPlayerManager playerManager,
        ILocalizationManager loc,
        PsionicComponent psionicComponent,
        PsionicPowerPrototype proto)
    {
        if (AssayFeedback is "")
            return;

        psionicComponent.AssayFeedback.Add(AssayFeedback);
    }
}

[UsedImplicitly]
public sealed partial class RemoveAssayFeedback : PsionicPowerFunction
{
    [DataField]
    public string AssayFeedback = "";

    public override void OnAddPsionic(
        EntityUid uid,
        IComponentFactory factory,
        IEntityManager entityManager,
        ISerializationManager serializationManager,
        ISharedPlayerManager playerManager,
        ILocalizationManager loc,
        PsionicComponent psionicComponent,
        PsionicPowerPrototype proto)
    {
        if (AssayFeedback is ""
            || !psionicComponent.AssayFeedback.Contains(AssayFeedback))
            return;

        psionicComponent.AssayFeedback.Remove(AssayFeedback);
    }
}

[UsedImplicitly]
public sealed partial class AddPsionicPsychognomicDescriptors : PsionicPowerFunction
{
    [DataField]
    public string PsychognomicDescriptor = "";

    public override void OnAddPsionic(
        EntityUid uid,
        IComponentFactory factory,
        IEntityManager entityManager,
        ISerializationManager serializationManager,
        ISharedPlayerManager playerManager,
        ILocalizationManager loc,
        PsionicComponent psionicComponent,
        PsionicPowerPrototype proto)
    {
        // It is entirely intended that this doesn't include a Contains check.
        // The descriptors list allows duplicates, and will only ever pick one anyway.
        if (PsychognomicDescriptor is "")
            return;

        psionicComponent.PsychognomicDescriptors.Add(PsychognomicDescriptor);
    }
}

[UsedImplicitly]
public sealed partial class RemovePsionicPsychognomicDescriptors : PsionicPowerFunction
{
    [DataField]
    public string PsychognomicDescriptor = "";

    public override void OnAddPsionic(
        EntityUid uid,
        IComponentFactory factory,
        IEntityManager entityManager,
        ISerializationManager serializationManager,
        ISharedPlayerManager playerManager,
        ILocalizationManager loc,
        PsionicComponent psionicComponent,
        PsionicPowerPrototype proto)
    {
        if (PsychognomicDescriptor is ""
            || !psionicComponent.PsychognomicDescriptors.Contains(PsychognomicDescriptor))
            return;

        psionicComponent.PsychognomicDescriptors.Remove(PsychognomicDescriptor);
    }
}

[UsedImplicitly]
public sealed partial class PsionicModifyPowerSlots : PsionicPowerFunction
{
    [DataField]
    public int PowerSlotsModifier;
    public override void OnAddPsionic(
        EntityUid uid,
        IComponentFactory factory,
        IEntityManager entityManager,
        ISerializationManager serializationManager,
        ISharedPlayerManager playerManager,
        ILocalizationManager loc,
        PsionicComponent psionicComponent,
        PsionicPowerPrototype proto)
    {
        psionicComponent.PowerSlots += PowerSlotsModifier;
    }
}

[UsedImplicitly]
public sealed partial class PsionicModifyFamiliarLimit : PsionicPowerFunction
{
    [DataField]
    public int FamiliarLimitModifier;

    public override void OnAddPsionic(
        EntityUid uid,
        IComponentFactory factory,
        IEntityManager entityManager,
        ISerializationManager serializationManager,
        ISharedPlayerManager playerManager,
        ILocalizationManager loc,
        PsionicComponent psionicComponent,
        PsionicPowerPrototype proto)
    {
        psionicComponent.FamiliarLimit += FamiliarLimitModifier;
    }
}

[UsedImplicitly]
public sealed partial class PsionicModifyRemovable : PsionicPowerFunction
{
    [DataField]
    public bool Removable;

    public override void OnAddPsionic(
        EntityUid uid,
        IComponentFactory factory,
        IEntityManager entityManager,
        ISerializationManager serializationManager,
        ISharedPlayerManager playerManager,
        ILocalizationManager loc,
        PsionicComponent psionicComponent,
        PsionicPowerPrototype proto)
    {
        psionicComponent.Removable = Removable;
    }
}

[UsedImplicitly]
public sealed partial class PsionicModifyGlimmer : PsionicPowerFunction
{
    [DataField]
    public float GlimmerModifier;

    public override void OnAddPsionic(
        EntityUid uid,
        IComponentFactory factory,
        IEntityManager entityManager,
        ISerializationManager serializationManager,
        ISharedPlayerManager playerManager,
        ILocalizationManager loc,
        PsionicComponent psionicComponent,
        PsionicPowerPrototype proto)
    {
        var glimmerSystem = entityManager.System<GlimmerSystem>();
        glimmerSystem.DeltaGlimmerInput(GlimmerModifier);
    }
}

[UsedImplicitly]
public sealed partial class PsionicChangePowerPool : PsionicPowerFunction
{
    [DataField]
    public ProtoId<WeightedRandomPrototype> PowerPool = "RandomPsionicPowerPool";

    public override void OnAddPsionic(
        EntityUid uid,
        IComponentFactory factory,
        IEntityManager entityManager,
        ISerializationManager serializationManager,
        ISharedPlayerManager playerManager,
        ILocalizationManager loc,
        PsionicComponent psionicComponent,
        PsionicPowerPrototype proto)
    {
        psionicComponent.PowerPool = PowerPool;
    }
}

[UsedImplicitly]
public sealed partial class PsionicAddAvailablePowers : PsionicPowerFunction
{
    /// <summary>
    ///     I can't validate these using this method. So this is a string.
    /// </summary>
    [DataField]
    public string PowerPrototype = "";

    [DataField]
    public float Weight = 1f;

    public override void OnAddPsionic(
        EntityUid uid,
        IComponentFactory factory,
        IEntityManager entityManager,
        ISerializationManager serializationManager,
        ISharedPlayerManager playerManager,
        ILocalizationManager loc,
        PsionicComponent psionicComponent,
        PsionicPowerPrototype proto)
    {
        var protoMan = IoCManager.Resolve<IPrototypeManager>();
        if (!protoMan.HasIndex<PsionicPowerPrototype>(PowerPrototype)
            || psionicComponent.AvailablePowers.ContainsKey(PowerPrototype))
            return;

        psionicComponent.AvailablePowers.Add(PowerPrototype, Weight);
    }
}

[UsedImplicitly]
public sealed partial class PsionicRemoveAvailablePowers : PsionicPowerFunction
{
    /// <summary>
    ///     I can't validate these using this method. So this is a string.
    /// </summary>
    [DataField]
    public string PowerPrototype = "";

    public override void OnAddPsionic(
        EntityUid uid,
        IComponentFactory factory,
        IEntityManager entityManager,
        ISerializationManager serializationManager,
        ISharedPlayerManager playerManager,
        ILocalizationManager loc,
        PsionicComponent psionicComponent,
        PsionicPowerPrototype proto)
    {
        psionicComponent.AvailablePowers.Remove(PowerPrototype);
    }
}

[UsedImplicitly]
public sealed partial class PsionicModifyRollChances : PsionicPowerFunction
{
    [DataField]
    public float BaselinePowerCostModifier;

    [DataField]
    public float BaselineChanceModifier;

    public override void OnAddPsionic(
        EntityUid uid,
        IComponentFactory factory,
        IEntityManager entityManager,
        ISerializationManager serializationManager,
        ISharedPlayerManager playerManager,
        ILocalizationManager loc,
        PsionicComponent psionicComponent,
        PsionicPowerPrototype proto)
    {
        psionicComponent.BaselinePowerCost += BaselinePowerCostModifier;
        psionicComponent.Chance += BaselineChanceModifier;
    }
}
