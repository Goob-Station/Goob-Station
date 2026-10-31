// SPDX-License-Identifier: MIT

using System.Numerics;
using Content.Client.Gameplay;
using Content.Shared.Popups;
using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controllers;

namespace Content.Client.Popups;

/// <summary>
/// Handles screens-space popups. World popups are handled via PopupOverlay.
/// </summary>
public sealed partial class PopupUIController : UIController, IOnStateEntered<GameplayState>, IOnStateExited<GameplayState>
{
    [UISystemDependency] private readonly PopupSystem? _popup = default!;

    private PopupRootControl? _popupControl;

    public override void Initialize()
    {
        base.Initialize();
        InitializeRunechat(); // Goob
    }

    public void OnStateEntered(GameplayState state)
    {
        _popupControl = new PopupRootControl(_popup, this);

        UIManager.RootControl.AddChild(_popupControl);
    }

    public void OnStateExited(GameplayState state)
    {
        if (_popupControl == null)
            return;

        UIManager.RootControl.RemoveChild(_popupControl);
        _popupControl = null;
    }

    public void DrawPopup(PopupSystem.PopupLabel popup, DrawingHandleScreen handle, Vector2 position, float scale)
    {
        var lifetime = PopupSystem.GetPopupLifetime(popup);

        // Keep alpha at 1 until TotalTime passes half its lifetime, then gradually decrease to 0.
        var alpha = MathF.Min(1f, 1f - MathF.Max(0f, popup.TotalTime - lifetime / 2) * 2 / lifetime);

        var updatedPosition = position - new Vector2(0f, MathF.Min(8f, 12f * (popup.TotalTime * popup.TotalTime + popup.TotalTime)));
        // Goob start
        var (font, color, outlineThickness) = GetPopupStyle(popup.Type, _runechat, _runechatFontSize, _runechatOutlineScale);
        TextOutline? outline = outlineThickness > 0f ? new TextOutline(outlineThickness, Color.Black.WithAlpha(alpha)) : null;
        var offset = _runechat ? popup.RandomOffset : Vector2.Zero;

        var dimensions = handle.GetDimensions(font, popup.Text, scale);
        handle.DrawString(font, updatedPosition - dimensions / 2f + offset, popup.Text, scale, color.WithAlpha(alpha), outline);
        // Goob end
    }

    /// <summary>
    /// Handles drawing all screen popups.
    /// </summary>
    private sealed class PopupRootControl : Control
    {
        private readonly PopupSystem? _popup;
        private readonly PopupUIController _controller;

        public PopupRootControl(PopupSystem? system, PopupUIController controller)
        {
            _popup = system;
            _controller = controller;
        }

        protected override void Draw(DrawingHandleScreen handle)
        {
            base.Draw(handle);

            if (_popup == null)
                return;

            // Different window
            var windowId = UserInterfaceManager.RootControl.Window.Id;

            foreach (var popup in _popup.CursorLabels)
            {
                if (popup.InitialPos.Window != windowId)
                    continue;

                _controller.DrawPopup(popup, handle, popup.InitialPos.Position, UIScale);
            }
        }
    }
}