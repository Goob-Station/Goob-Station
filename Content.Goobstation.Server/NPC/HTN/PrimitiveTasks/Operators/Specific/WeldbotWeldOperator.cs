// SPDX-FileCopyrightText: 2025 BombasterDS <deniskaporoshok@gmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Rouden <149893554+Roudenn@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
// SPDX-FileCopyrightText: 2025 Timfa <timfalken@hotmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Silicon.Bots;
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
using System.Linq;
using Content.Shared.Repairable;

namespace Content.Goobstation.Server.NPC.HTN.PrimitiveTasks.Operators.Specific;

public sealed partial class WeldbotWeldOperator : HTNOperator
{
    [Dependency] private readonly IEntityManager _entMan = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    private ChatSystem _chat = default!;
    private WeldbotSystem _weldbot = default!;
    private SharedAudioSystem _audio = default!;
    private SharedInteractionSystem _interaction = default!;
    private SharedPopupSystem _popup = default!;
    private DamageableSystem _damageableSystem = default!;
    private TagSystem _tagSystem = default!;

    /// <summary>
    /// Target entity to inject.
    /// </summary>
    [DataField(required: true)]
    public string TargetKey = string.Empty;

    public override void Initialize(IEntitySystemManager sysManager)
    {
        base.Initialize(sysManager);
        _chat = sysManager.GetEntitySystem<ChatSystem>();
        _weldbot = sysManager.GetEntitySystem<WeldbotSystem>();
        _audio = sysManager.GetEntitySystem<SharedAudioSystem>();
        _interaction = sysManager.GetEntitySystem<SharedInteractionSystem>();
        _popup = sysManager.GetEntitySystem<SharedPopupSystem>();
        _damageableSystem = sysManager.GetEntitySystem<DamageableSystem>();
        _tagSystem = sysManager.GetEntitySystem<TagSystem>();
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

        if (!_entMan.TryGetComponent<RepairableComponent>(target, out var repairComp)
            || !_entMan.TryGetComponent<WeldbotComponent>(owner, out var botComp)
            || !_entMan.TryGetComponent<DamageableComponent>(target, out var damage)
            || !_interaction.InRangeUnobstructed(owner, target)
            || damage.Damage.DamageDict.Keys.Intersect(botComp.DamageAmount.DamageDict.Keys).All(key => damage.Damage.DamageDict[key] == 0)
            && !_entMan.HasComponent<EmaggedComponent>(owner))
            return HTNOperatorStatus.Failed;

        if (botComp.IsEmagged)
        {
            _damageableSystem.TryChangeDamage(target, -botComp.DamageAmount, true, false, damage);
        }
        else
        {
            _damageableSystem.TryChangeDamage(target, botComp.DamageAmount, true, false, damage);
        }

        _audio.PlayPvs(botComp.WeldSound, target);

        if (damage.Damage.DamageDict.Keys.Intersect(botComp.DamageAmount.DamageDict.Keys).All(key => damage.Damage.DamageDict[key] == 0)) //only say "all done if we're actually done!"
            _chat.TrySendInGameICMessage(owner, Loc.GetString("weldbot-finish-weld"), InGameICChatType.Speak, hideChat: true, hideLog: true);

        return HTNOperatorStatus.Finished;
    }
}
