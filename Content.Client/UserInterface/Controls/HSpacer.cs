// SPDX-FileCopyrightText: 2022 Jesse Rougeau <jmaster9999@gmail.com>
// SPDX-FileCopyrightText: 2023 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Client.UserInterface;

namespace Content.Client.UserInterface.Controls;

public sealed class HSpacer : Control
{
    public float Spacing { get => MinHeight; set => MinHeight = value; }
    public HSpacer()
    {
        MinHeight = Spacing;
    }
    public HSpacer(float height = 5)
    {
        Spacing = height;
        MinHeight = height;
    }
}