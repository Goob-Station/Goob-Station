// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Administration.BanList;
using Robust.Client.UserInterface.Controls;

namespace Content.Client.Administration.UI.BanList;

public interface IBanListLine<T> where T : SharedServerBan
{
    T Ban { get; }
    Label Reason { get; }
    Label BanTime { get; }
    Label Expires { get; }
    Label BanningAdmin { get; }
}