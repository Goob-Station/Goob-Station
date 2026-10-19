// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Server.EUI;
using Content.Server.Silicons.Laws;
using Content.Shared.Eui;
using Content.Shared.MalfAI;
using Content.Shared.Mind.Components;
using Content.Shared.Silicons.Laws;
using Content.Shared.Silicons.Laws.Components;

namespace Content.Server.MalfAI;

/// <summary>
/// Server-side EUI for Malf AI to edit the laws of their subverted borgs or the master lawset.
/// This is separate from the admin SiliconLawEui and does not require admin permissions.
/// It authorizes based on control linkage (MalfAiControlledComponent) and Mind equivalence for borgs,
/// and StationAi-held status for the master lawset on the rule entity.
/// </summary>
public sealed class MalfAiLawEui : BaseEui
{
    private readonly SiliconLawSystem _siliconLawSystem;
    private readonly EntityManager _entityManager;

    private List<SiliconLaw> _laws = new();
    private EntityUid _target;

    public MalfAiLawEui(SiliconLawSystem siliconLawSystem, EntityManager entityManager)
    {
        _siliconLawSystem = siliconLawSystem;
        _entityManager = entityManager;
    }

    public override EuiStateBase GetNewState()
    {
        var netTarget = _entityManager.GetNetEntity(_target);
        return new SiliconLawsEuiState(_laws, netTarget);
    }

    /// <summary>
    /// Initialize and push the current laws for the given target (borg or master).
    /// Always load from the bound container/provider so reopening shows saved content.
    /// </summary>
    public void UpdateLaws(SiliconLawBoundComponent? lawBoundComponent, EntityUid target)
    {
        if (!IsAllowed(target))
            return;

        var laws = _siliconLawSystem.GetLaws(target, lawBoundComponent);
        _laws = laws.Laws;
        _target = target;
        StateDirty();
    }

    public override void HandleMessage(EuiMessageBase msg)
    {
        if (msg is not SiliconLawsSaveMessage message)
            return;

        var target = _entityManager.GetEntity(message.Target);

        if (!IsAllowed(target))
        {
            return;
        }

        // Save into the target's container/provider.
        if (_entityManager.TryGetComponent<SiliconLawProviderComponent>(target, out var provider))
        {
            _siliconLawSystem.SetLaws(message.Laws, target, provider.LawUploadSound);
        }
        else
        {
            _siliconLawSystem.SetLaws(message.Laws, target);
        }

        // If saving master, propagate to all synced borgs using the same set.
        if (_entityManager.HasComponent<MalfMasterLawsetComponent>(target))
        {
            var iter = _entityManager.AllEntityQueryEnumerator<MalfBorgSyncToMasterComponent>();
            while (iter.MoveNext(out var borgUid, out _))
            {
                if (_entityManager.TryGetComponent<SiliconLawProviderComponent>(borgUid, out var borgProvider))
                    _siliconLawSystem.SetLaws(message.Laws, borgUid, borgProvider.LawUploadSound);
                else
                    _siliconLawSystem.SetLaws(message.Laws, borgUid);
            }
        }
    }

    /// <summary>
    /// Authorize the current EUI player to edit the target's laws.
    /// - For borgs: must be the Malf AI that controls the borg (or same Mind).
    /// - For master: attached must be Station AI held.
    /// </summary>
    private bool IsAllowed(EntityUid target)
    {
        if (Player.AttachedEntity is not { } attached)
            return false;

        if (_entityManager.HasComponent<MalfMasterLawsetComponent>(target))
            return _entityManager.HasComponent<Content.Shared.Silicons.StationAi.StationAiHeldComponent>(attached);

        if (!_entityManager.TryGetComponent<MalfAiControlledComponent>(target, out var controlled) || controlled.Controller == null)
            return false;

        if (controlled.Controller == attached)
            return true;

        if (_entityManager.TryGetComponent<MindContainerComponent>(attached, out var aMind) && aMind.Mind != null)
        {
            if (controlled.Controller != null &&
                _entityManager.TryGetComponent<MindContainerComponent>(controlled.Controller.Value, out var cMind) &&
                cMind.Mind == aMind.Mind)
            {
                return true;
            }
        }

        return false;
    }
}
