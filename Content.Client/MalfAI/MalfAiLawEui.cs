// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Client.Eui;
using Content.Client.Silicons.Laws.SiliconLawEditUi;
using Content.Shared.Eui;
using Content.Shared.Silicons.Laws;

namespace Content.Client.MalfAI;

/// <summary>
/// Client-side EUI for Malf AI law editing. This mirrors the admin client EUI
/// but is paired with the server MalfAiLawEui. It reuses the existing
/// SiliconLawUi window and messages.
/// </summary>
public sealed class MalfAiLawEui : BaseEui
{
    private readonly EntityManager _entityManager;

    private SiliconLawUi _siliconLawUi;
    private NetEntity _target;

    public MalfAiLawEui()
    {
        _entityManager = IoCManager.Resolve<EntityManager>();

        _siliconLawUi = new SiliconLawUi();
        _siliconLawUi.OnClose += () => SendMessage(new CloseEuiMessage());
        _siliconLawUi.Save.OnPressed += _ => {
            SendMessage(new SiliconLawsSaveMessage(_siliconLawUi.GetLaws(), _target));
        };
    }

    public override void HandleState(EuiStateBase state)
    {
        if (state is not SiliconLawsEuiState s)
        {
            return;
        }

        _target = s.Target;
        _siliconLawUi.SetLaws(s.Laws);
    }

    public override void Opened()
    {
        _siliconLawUi.OpenCentered();
    }
}
