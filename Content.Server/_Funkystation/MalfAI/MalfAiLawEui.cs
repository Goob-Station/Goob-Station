// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
// SPDX-FileCopyrightText: 2025 Goob-Station
//
// SPDX-License-Identifier: MIT

using System.Linq;
using Content.Server.EUI;
using Content.Server.Silicons.Laws;
using Content.Shared.Eui;
using Content.Shared._Funkystation.MalfAI;
using Content.Shared.Silicons.Laws;
using Content.Shared.Silicons.Laws.Components;

namespace Content.Server._Funkystation.MalfAI;

/// <summary>
/// EUI for the Malf AI to edit borg laws or the master lawset.
/// </summary>
public sealed class MalfAiLawEui : BaseEui
{
    private readonly SiliconLawSystem _siliconLaw;
    private readonly EntityManager _entMan;

    private List<SiliconLaw> _laws = new();
    private EntityUid _target;
    private bool _isMasterLawset;

    public MalfAiLawEui(SiliconLawSystem siliconLaw, EntityManager entMan)
    {
        _siliconLaw = siliconLaw;
        _entMan = entMan;
    }

    public override EuiStateBase GetNewState()
    {
        return new SiliconLawsEuiState(_laws, _entMan.GetNetEntity(_target));
    }

    public void OpenForMasterLawset(EntityUid malfAi)
    {
        _target = malfAi;
        _isMasterLawset = true;

        if (_entMan.TryGetComponent<MalfMasterLawsetComponent>(malfAi, out var master))
        {
            _laws = master.Laws.Select((law, idx) => new SiliconLaw
            {
                LawString = law,
                Order = idx + 1,
            }).ToList();
        }
        else
        {
            _laws = new List<SiliconLaw>();
        }

        StateDirty();
    }

    public override void HandleMessage(EuiMessageBase msg)
    {
        if (msg is not SiliconLawsSaveMessage message)
            return;

        var target = _entMan.GetEntity(message.Target);

        if (_isMasterLawset)
        {
            if (_entMan.TryGetComponent<MalfMasterLawsetComponent>(target, out var master))
            {
                master.Laws = message.Laws.OrderBy(l => l.Order).Select(l => l.LawString).ToList();

                // Propagate the new master lawset to all synced borgs immediately.
                var iter = _entMan.AllEntityQueryEnumerator<MalfBorgSyncToMasterComponent>();
                while (iter.MoveNext(out var borgUid, out _))
                {
                    if (_entMan.TryGetComponent<SiliconLawProviderComponent>(borgUid, out var borgProvider))
                        _siliconLaw.SetLaws(message.Laws, borgUid, borgProvider.LawUploadSound);
                    else
                        _siliconLaw.SetLaws(message.Laws, borgUid);
                }
            }
        }
        else
        {
            if (_entMan.TryGetComponent<SiliconLawProviderComponent>(target, out var provider))
                _siliconLaw.SetLaws(message.Laws, target, provider.LawUploadSound);
        }

        _laws = message.Laws;
        StateDirty();
    }
}
