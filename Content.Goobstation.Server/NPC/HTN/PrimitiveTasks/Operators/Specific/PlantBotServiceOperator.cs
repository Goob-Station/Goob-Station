// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
// SPDX-FileCopyrightText: 2025 Timfa <timfalken@hotmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Silicon.Bots;
using Content.Server.Botany.Components;
using Content.Server.Botany.Systems;
using Content.Server.Chat.Systems;
using Content.Server.NPC;
using Content.Server.NPC.HTN;
using Content.Server.NPC.HTN.PrimitiveTasks;
using Content.Shared.Chat;
using Content.Shared.Damage;
using Content.Shared.Emag.Components;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Content.Shared.Tag;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.NPC.HTN.PrimitiveTasks.Operators.Specific;

public sealed partial class PlantbotServiceOperator : HTNOperator
{
    [Dependency] private readonly IEntityManager _entMan = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

    private ChatSystem _chat = default!;
    private SharedAudioSystem _audio = default!;
    private SharedInteractionSystem _interaction = default!;
    private SharedPopupSystem _popup = default!;
    private PlantHolderSystem _plantHolderSystem = default!;
    private DamageableSystem _damageableSystem = default!;
    private TagSystem _tagSystem = default!;

    public const float RequiredWaterLevelToService = 80f;
    public const float RequiredWeedsAmountToWeed = 1f;
    public const float WaterTransferAmount = 10f;
    public const float WeedsRemovedAmount = 1f;
    public const string SiliconTag = "SiliconMob";

    /// <summary>
    /// Target entity to inject.
    /// </summary>
    [DataField(required: true)]
    public string TargetKey = string.Empty;

    public override void Initialize(IEntitySystemManager sysManager)
    {
        base.Initialize(sysManager);

        _chat = sysManager.GetEntitySystem<ChatSystem>();
        _audio = sysManager.GetEntitySystem<SharedAudioSystem>();
        _interaction = sysManager.GetEntitySystem<SharedInteractionSystem>();
        _popup = sysManager.GetEntitySystem<SharedPopupSystem>();
        _plantHolderSystem = sysManager.GetEntitySystem<PlantHolderSystem>();
    }

    public override void TaskShutdown(NPCBlackboard blackboard, HTNOperatorStatus status)
    {
        base.TaskShutdown(blackboard, status);
        blackboard.Remove<EntityUid>(TargetKey);
    }

    public override HTNOperatorStatus Update(NPCBlackboard blackboard, float frameTime)
    {
        var owner = blackboard.GetValue<EntityUid>(NPCBlackboard.Owner);

        if (!blackboard.TryGetValue<EntityUid>(TargetKey, out var target, _entMan) || _entMan.Deleted(target))
            return HTNOperatorStatus.Failed;

        if (!_entMan.TryGetComponent<PlantbotComponent>(owner, out var botComp)
            || !_entMan.TryGetComponent<PlantHolderComponent>(target, out var plantHolderComponent)
            || !_interaction.InRangeUnobstructed(owner, target)
            || (plantHolderComponent is { WaterLevel: >= RequiredWaterLevelToService, WeedLevel: <= RequiredWeedsAmountToWeed } && (!_entMan.HasComponent<EmaggedComponent>(owner) || plantHolderComponent.Dead || plantHolderComponent.WaterLevel <= 0f)))
            return HTNOperatorStatus.Failed;

        if (botComp.IsEmagged)
        {
            _plantHolderSystem.AdjustWater(target, -WaterTransferAmount);
            _audio.PlayPvs(botComp.RemoveWaterSound, target);
        }
        else
        {
            if (plantHolderComponent.WaterLevel <= RequiredWaterLevelToService)
            {
                _plantHolderSystem.AdjustWater(target, 10);
                _audio.PlayPvs(botComp.WaterSound, target);
                _chat.TrySendInGameICMessage(owner, Loc.GetString("plantbot-add-water"), InGameICChatType.Speak, hideChat: true, hideLog: true);
            }
            else if (plantHolderComponent.WeedLevel >= RequiredWeedsAmountToWeed)
            {
                plantHolderComponent.WeedLevel -= WeedsRemovedAmount;
                _audio.PlayPvs(botComp.WeedSound, target);
                _chat.TrySendInGameICMessage(owner, Loc.GetString("plantbot-remove-weeds"), InGameICChatType.Speak, hideChat: true, hideLog: true);
            }
            else
                return HTNOperatorStatus.Failed;
        }

        return HTNOperatorStatus.Finished;
    }
}
