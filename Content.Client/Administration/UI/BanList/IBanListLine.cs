// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

﻿using Content.Shared.Administration.BanList;
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
