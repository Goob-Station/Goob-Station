// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Client.CharacterInfo;
using Content.Client.Message;
using Content.Shared.Points;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;

namespace Content.Client.Points;

/// <inheritdoc/>
public sealed class PointSystem : SharedPointSystem
{
    [Dependency] private readonly CharacterInfoSystem _characterInfo = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PointManagerComponent, AfterAutoHandleStateEvent>(OnHandleState);
        SubscribeLocalEvent<CharacterInfoSystem.GetCharacterInfoControlsEvent>(OnGetCharacterInfoControls);
    }

    private void OnHandleState(EntityUid uid, PointManagerComponent component, ref AfterAutoHandleStateEvent args)
    {
        _characterInfo.RequestCharacterInfo();
    }

    private void OnGetCharacterInfoControls(ref CharacterInfoSystem.GetCharacterInfoControlsEvent ev)
    {
        foreach (var point in EntityQuery<PointManagerComponent>())
        {
            var box = new BoxContainer
            {
                Margin = new Thickness(5),
                Orientation = BoxContainer.LayoutOrientation.Vertical
            };

            var title = new RichTextLabel
            {
                HorizontalAlignment = Control.HAlignment.Center
            };
            title.SetMarkup(Loc.GetString("point-scoreboard-header"));

            var text = new RichTextLabel();
            text.SetMessage(point.Scoreboard);

            box.AddChild(title);
            box.AddChild(text);
            ev.Controls.Add(box);
        }
    }
}