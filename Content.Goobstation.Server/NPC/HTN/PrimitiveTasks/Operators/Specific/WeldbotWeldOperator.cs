// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
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
using Content.Shared.Damage.Prototypes;
using Content.Shared.Emag.Components;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Content.Shared.Tag;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;

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

        var tagPrototype = _prototypeManager.Index<TagPrototype>(SiliconTag);

        if (!_entMan.TryGetComponent<TagComponent>(target, out var tagComponent) || !_tagSystem.HasTag(tagComponent, tagPrototype)
            || !_entMan.TryGetComponent<WeldbotComponent>(owner, out var botComp)
            || !_entMan.TryGetComponent<DamageableComponent>(target, out var damage)
            || !_interaction.InRangeUnobstructed(owner, target)
            || (damage.DamagePerGroup["Brute"].Value == 0 && !_entMan.HasComponent<EmaggedComponent>(owner)))
            return HTNOperatorStatus.Failed;

        if (botComp.IsEmagged)
        {
            if (!_prototypeManager.TryIndex<DamageGroupPrototype>("Burn", out var prototype))
                return HTNOperatorStatus.Failed;

            _damageableSystem.TryChangeDamage(target, new DamageSpecifier(prototype, 10), true, false, damage);
        }
        else
        {
            if (!_prototypeManager.TryIndex<DamageGroupPrototype>("Brute", out var prototype))
                return HTNOperatorStatus.Failed;

            _damageableSystem.TryChangeDamage(target, new DamageSpecifier(prototype, -50), true, false, damage);
        }

        _audio.PlayPvs(botComp.WeldSound, target);

        if(damage.DamagePerGroup["Brute"].Value == 0) //only say "all done if we're actually done!"
            _chat.TrySendInGameICMessage(owner, Loc.GetString("weldbot-finish-weld"), InGameICChatType.Speak, hideChat: true, hideLog: true);

        return HTNOperatorStatus.Finished;
    }
}
