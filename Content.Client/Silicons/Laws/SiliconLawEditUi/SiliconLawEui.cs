// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Aidenkrz <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Simon <63975668+Simyon264@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
using Content.Client.Eui;
using Content.Shared.Eui;
using Content.Shared.Silicons.Laws;

namespace Content.Client.Silicons.Laws.SiliconLawEditUi;

public sealed class SiliconLawEui : BaseEui
{
    private readonly EntityManager _entityManager;

    private SiliconLawUi _siliconLawUi;
    private EntityUid _target;

    public SiliconLawEui()
    {
        _entityManager = IoCManager.Resolve<EntityManager>();

        _siliconLawUi = new SiliconLawUi();
        _siliconLawUi.OnClose += () => SendMessage(new CloseEuiMessage());
        _siliconLawUi.Save.OnPressed += _ => SendMessage(new SiliconLawsSaveMessage(_siliconLawUi.GetLaws(), _entityManager.GetNetEntity(_target)));
    }

    public override void HandleState(EuiStateBase state)
    {
        if (state is not SiliconLawsEuiState s)
        {
            return;
        }

        _target = _entityManager.GetEntity(s.Target);
        _siliconLawUi.SetLaws(s.Laws);
    }

    public override void Opened()
    {
        _siliconLawUi.OpenCentered();
    }
}