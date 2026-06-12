// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
// SPDX-FileCopyrightText: 2025 Goob-Station
//
// SPDX-License-Identifier: MIT

using Content.Client.Eui;
using Content.Client.Silicons.Laws.SiliconLawEditUi;
using Content.Shared.Eui;
using Content.Shared.Silicons.Laws;

namespace Content.Client._Funkystation.MalfAI;

/// <summary>
/// Client-side EUI for the Malf AI law editor.
/// Reuses the SiliconLawUi window.
/// </summary>
public sealed class MalfAiLawEui : BaseEui
{
    private readonly EntityManager _entityManager;
    private SiliconLawUi _siliconLawUi;
    private EntityUid _target;

    public MalfAiLawEui()
    {
        _entityManager = IoCManager.Resolve<EntityManager>();

        _siliconLawUi = new SiliconLawUi();
        _siliconLawUi.OnClose += () => SendMessage(new CloseEuiMessage());
        _siliconLawUi.Save.OnPressed += _ =>
            SendMessage(new SiliconLawsSaveMessage(_siliconLawUi.GetLaws(), _entityManager.GetNetEntity(_target)));
    }

    public override void Opened()
    {
        _siliconLawUi.OpenCentered();
    }

    public override void HandleState(EuiStateBase state)
    {
        if (state is not SiliconLawsEuiState s)
            return;

        _target = _entityManager.GetEntity(s.Target);
        _siliconLawUi.SetLaws(s.Laws);
    }

    public override void Closed()
    {
        _siliconLawUi.Dispose();
    }
}
