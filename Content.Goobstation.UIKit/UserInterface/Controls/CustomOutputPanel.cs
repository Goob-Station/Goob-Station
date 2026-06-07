// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 gluesniffler <linebarrelerenthusiast@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.RichText;
using Robust.Shared.Utility;
using System.Numerics;

namespace Content.Goobstation.UIKit.UserInterface.Controls;

[Virtual]
public sealed class CustomOutputPanel : Control
{
    [Dependency] private readonly MarkupTagManager _tagManager = default!;
    [Dependency] private readonly IEntityManager _entManager = default!;

    public const string StylePropertyStyleBox = "stylebox";

    private readonly CustomRingBufferList<CustomRichTextEntry> _entries = new();
    private bool _isAtBottom = true;

    private int _totalContentHeight;
    private bool _firstLine = true;
    private StyleBox? _styleBoxOverride;
    private VScrollBar _scrollBar;

    public bool ScrollFollowing { get; set; } = true;

    private bool _invalidOnVisible;

    public CustomOutputPanel()
    {
        IoCManager.InjectDependencies(this);
        MouseFilter = Control.MouseFilterMode.Pass;
        RectClipContent = true;

        _scrollBar = new VScrollBar
        {
            Name = "_v_scroll",
            HorizontalAlignment = Control.HAlignment.Right
        };
        AddChild(_scrollBar);
        _scrollBar.OnValueChanged += _ => _isAtBottom = _scrollBar.IsAtEnd;
    }

    public int EntryCount => _entries.Count;

    public void UpdateLastMessage(FormattedMessage message)
    {
        var newEnt = new CustomRichTextEntry(message, this, _tagManager, _entManager, null);
        newEnt.Update(_tagManager, _getFont(), _getContentBox().Width, UIScale);
        _entries[_entries.Count - 1] = newEnt;
    }

    public StyleBox? StyleBoxOverride
    {
        get => _styleBoxOverride;
        set
        {
            _styleBoxOverride = value;
            InvalidateMeasure();
            _invalidateEntries();
        }
    }

    public void Clear()
    {
        _firstLine = true;
        foreach (var entry in _entries)
        {
            entry.RemoveControls();
        }

        _entries.Clear();
        _totalContentHeight = 0;
        _scrollBar.MaxValue = Math.Max(_scrollBar.Page, _totalContentHeight);
        _scrollBar.Value = 0;
    }

    public void RemoveEntry(Index index)
    {
        var entry = _entries[index];
        entry.RemoveControls();
        _entries.RemoveAt(index.GetOffset(_entries.Count));

        var font = _getFont();
        _totalContentHeight -= entry.Height + font.GetLineSeparation(UIScale);
        if (_entries.Count == 0)
        {
            Clear();
        }

        _scrollBar.MaxValue = Math.Max(_scrollBar.Page, _totalContentHeight);
    }

    public void AddText(string text)
    {
        var msg = new FormattedMessage();
        msg.AddText(text);
        AddMessage(msg);
    }

    public void AddMessage(FormattedMessage message)
    {
        var entry = new CustomRichTextEntry(message, this, _tagManager, _entManager, null);

        entry.Update(_tagManager, _getFont(), _getContentBox().Width, UIScale);

        _entries.Add(entry);
        var font = _getFont();
        _totalContentHeight += entry.Height;
        if (_firstLine)
        {
            _firstLine = false;
        }
        else
        {
            _totalContentHeight += font.GetLineSeparation(UIScale);
        }

        _scrollBar.MaxValue = Math.Max(_scrollBar.Page, _totalContentHeight);
        if (_isAtBottom && ScrollFollowing)
        {
            _scrollBar.MoveToEnd();
        }
    }

    public void ScrollToBottom()
    {
        _scrollBar.MoveToEnd();
        _isAtBottom = true;
    }

    protected override void Draw(DrawingHandleScreen handle)
    {
        base.Draw(handle);

        var style = _getStyleBox();
        var font = _getFont();
        var lineSeparation = font.GetLineSeparation(UIScale);
        style?.Draw(handle, PixelSizeBox, UIScale);
        var contentBox = _getContentBox();

        var entryOffset = -_scrollBar.Value;

        // A stack for format tags.
        // This stack contains the format tag to RETURN TO when popped off.
        // So when a new color tag gets hit this stack gets the previous color pushed on.
        var context = new MarkupDrawingContext(2);

        foreach (ref var entry in _entries)
        {
            if (entryOffset + entry.Height < 0)
            {
                // Controls within the entry are the children of this control, which means they are drawn separately
                // after this Draw call, so we have to mark them as invisible to prevent them from being drawn.
                //
                // An alternative option is to ensure that the control position updating logic in entry.Draw is always
                // run, and then setting RectClipContent = true to use scissor box testing to handle the controls
                // visibility
                entry.HideControls();
                entryOffset += entry.Height + lineSeparation;
                continue;
            }

            if (entryOffset > contentBox.Height)
            {
                entry.HideControls();
                continue;
            }

            entry.Draw(_tagManager, handle, font, contentBox, entryOffset, _scrollBar.PixelSize, context, UIScale);

            entryOffset += entry.Height + lineSeparation;
        }
    }

    protected override void MouseWheel(GUIMouseWheelEventArgs args)
    {
        base.MouseWheel(args);

        if (MathHelper.CloseToPercent(0, args.Delta.Y))
        {
            return;
        }

        _scrollBar.ValueTarget -= _getScrollSpeed() * args.Delta.Y;
    }

    protected override void Resized()
    {
        base.Resized();

        var styleBoxSize = _getStyleBox()?.MinimumSize.Y ?? 0;

        _scrollBar.Page = UIScale * (Height - styleBoxSize);
        _invalidateEntries();
    }

    protected override Vector2 MeasureOverride(Vector2 availableSize)
    {
        return _getStyleBox()?.MinimumSize ?? Vector2.Zero;
    }

    public void _invalidateEntries()
    {
        _totalContentHeight = 0;
        var font = _getFont();
        var sizeX = _getContentBox().Width;
        foreach (ref var entry in _entries)
        {
            entry.Update(_tagManager, font, sizeX, UIScale);
            _totalContentHeight += entry.Height + font.GetLineSeparation(UIScale);
        }

        _scrollBar.MaxValue = Math.Max(_scrollBar.Page, _totalContentHeight);
        if (_isAtBottom && ScrollFollowing)
        {
            _scrollBar.MoveToEnd();
        }
    }

    [System.Diagnostics.Contracts.Pure]
    private Font _getFont()
    {
        if (TryGetStyleProperty<Font>("font", out var font))
        {
            return font;
        }

        return UserInterfaceManager.ThemeDefaults.DefaultFont;
    }

    [System.Diagnostics.Contracts.Pure]
    private StyleBox? _getStyleBox()
    {
        if (StyleBoxOverride != null)
        {
            return StyleBoxOverride;
        }

        TryGetStyleProperty<StyleBox>(StylePropertyStyleBox, out var box);
        return box;
    }

    [System.Diagnostics.Contracts.Pure]
    private float _getScrollSpeed()
    {
        // The scroll speed depends on the UI scale because the scroll bar is working with physical pixels.
        return GetScrollSpeed(_getFont(), UIScale);
    }

    [System.Diagnostics.Contracts.Pure]
    private UIBox2 _getContentBox()
    {
        var style = _getStyleBox();
        var box = style?.GetContentBox(PixelSizeBox, UIScale) ?? PixelSizeBox;
        box.Right = Math.Max(box.Left, box.Right - _scrollBar.DesiredPixelSize.X);
        return box;
    }

    protected override void UIScaleChanged()
    {
        // If this control isn't visible, don't invalidate entries immediately.
        // This saves invalidating the debug console if it's hidden,
        // which is a huge boon as auto-scaling changes UI scale a lot in that scenario.
        if (!VisibleInTree)
            _invalidOnVisible = true;
        else
            _invalidateEntries();

        base.UIScaleChanged();
    }

    internal static float GetScrollSpeed(Font font, float scale)
    {
        return font.GetLineHeight(scale) * 2;
    }

    protected override void EnteredTree()
    {
        base.EnteredTree();
        // Due to any number of reasons the entries may be invalidated if added when not visible in the tree.
        // e.g. the control has not had its UI scale set and the messages were added, but the
        // existing ones were valid when the UI scale was set.
        _invalidateEntries();
    }

    protected override void VisibilityChanged(bool newVisible)
    {
        if (newVisible && _invalidOnVisible)
        {
            _invalidateEntries();
            _invalidOnVisible = false;
        }
    }
}
