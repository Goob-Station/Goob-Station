// SPDX-FileCopyrightText: 2025 GabyChangelog <agentepanela2@gmail.com>
// SPDX-FileCopyrightText: 2025 Kyoth25f <kyoth25f@gmail.com>
// SPDX-FileCopyrightText: 2025 SX-7 <sn1.test.preria.2002@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using System.Numerics;
using Content.Client.Stylesheets;
using Content.Shared.StatusIcon;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Prototypes;

namespace Content.Client.VoiceMask;


public sealed partial class VoiceMaskNameChangeWindow
{
    [Dependency] private readonly IPrototypeManager _protoManager = default!;

    public Action<ProtoId<JobIconPrototype>>? OnJobIconChanged;

    private List<ProtoId<JobIconPrototype>> _jobIcons = new();
    private ProtoId<JobIconPrototype>? _currentJobIconId = new();
    private Dictionary<ProtoId<JobIconPrototype>, Button> _jobIconButtons = new();
    private ButtonGroup? _jobIconButtonGroup = new();

    private const int JobIconColumnCount = 10;


    public void ReloadJobIcons()
    {
        var icons = _protoManager.EnumeratePrototypes<JobIconPrototype>()
            .Where(icon => icon.AllowSelection)
            .ToList();

        icons.Sort((x, y) => string.Compare(x.LocalizedJobName, y.LocalizedJobName, StringComparison.CurrentCulture));

        _jobIcons = icons.Select(icon => (ProtoId<JobIconPrototype>) icon.ID).ToList();
    }

    public void AddJobIcons()
    {
        IconGrid.DisposeAllChildren();

        for (var i = 0; i < _jobIcons.Count; i++)
        {
            var jobIcon = _protoManager.Index(_jobIcons[i]);

            var styleBase = StyleBase.ButtonOpenBoth;
            var mod = i % JobIconColumnCount;

            if (mod == 0)
                styleBase = StyleBase.ButtonOpenRight;
            else if (mod == JobIconColumnCount - 1)
                styleBase = StyleBase.ButtonOpenLeft;

            var jobIconButton = new Button
            {
                Access = AccessLevel.Public,
                StyleClasses = { styleBase },
                MaxSize = new Vector2(42, 28),
                Group = _jobIconButtonGroup,
                Pressed = jobIcon.ID == _currentJobIconId,
                ToolTip = jobIcon.LocalizedJobName
            };

            var jobIconTexture = new TextureRect
            {
                Texture = _spriteSystem.Frame0(jobIcon.Icon),
                TextureScale = new Vector2(2.5f, 2.5f),
                Stretch = TextureRect.StretchMode.KeepCentered,
            };

            _jobIconButtons.Add(jobIcon.ID, jobIconButton);

            jobIconButton.AddChild(jobIconTexture);
            jobIconButton.OnPressed += _ =>
            {
                _currentJobIconId = jobIcon.ID;
                OnJobIconChanged?.Invoke(jobIcon.ID);
            };

            IconGrid.AddChild(jobIconButton);
        }
    }

    public void SetCurrentJobIcon(ProtoId<JobIconPrototype>? jobIconProtoId)
    {
        _currentJobIconId = jobIconProtoId;

        if (jobIconProtoId is not { } protoId)
        {
            // Setting it to null deselects the pressed button.
            if (_jobIconButtonGroup?.Pressed is { } pressedButton)
                pressedButton.Pressed = false;

            return;
        }

        if (!_jobIconButtons.TryGetValue(protoId, out var button))
            return;

        // ButtonGroup will take care of making the other button unpressed.
        button.Pressed = true;
    }
}
