// SPDX-FileCopyrightText: 2025 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Jezithyr <Jezithyr.@gmail.com>
// SPDX-FileCopyrightText: 2022 Jacob Tong <10494922+ShadowCommander@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Fishfish458 <47410468+Fishfish458@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 ShadowCommander <10494922+ShadowCommander@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2023 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Errant <35878406+Errant-4@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
using System.Numerics;
using Content.Client.UserInterface.Systems.Storage;
using Content.Client.UserInterface.Systems.Storage.Controls;
using Content.Shared.Storage;
using JetBrains.Annotations;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;

namespace Content.Client.Storage;

[UsedImplicitly]
public sealed class StorageBoundUserInterface : BoundUserInterface
{
    private StorageWindow? _window;

    public Vector2? Position => _window?.Position;

    public StorageBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();

        _window = IoCManager.Resolve<IUserInterfaceManager>()
            .GetUIController<StorageUIController>()
            .CreateStorageWindow(this);

        if (EntMan.TryGetComponent(Owner, out StorageComponent? storage))
        {
            _window.UpdateContainer((Owner, storage));
        }

        _window.OnClose += Close;
        _window.FlagDirty();
    }

    public void Refresh()
    {
        _window?.FlagDirty();
    }

    public void Reclaim()
    {
        if (_window == null)
            return;

        _window.OnClose -= Close;
        _window.Orphan();
        _window = null;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        Reclaim();
    }

    public void CloseWindow(Vector2 position)
    {
        if (_window == null)
            return;

        // Update its position before potentially saving.
        // Listen it makes sense okay.
        LayoutContainer.SetPosition(_window, position);
        _window?.Close();
    }

    public void Hide()
    {
        if (_window == null)
            return;

        _window.Visible = false;
    }

    public void Show()
    {
        if (_window == null)
            return;

        _window.Visible = true;
    }

    public void Show(Vector2 position)
    {
        if (_window == null)
            return;

        Show();
        LayoutContainer.SetPosition(_window, position);
    }
}
