// SPDX-FileCopyrightText: 2020 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2021 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 E F R <602406+Efruit@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Paul Ritter <ritter.paul1@googlemail.com>
// SPDX-FileCopyrightText: 2023 faint <46868845+ficcialfaint@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 Julian Giebel <juliangiebel@live.de>
// SPDX-FileCopyrightText: 2024 Winkarst <74284083+Winkarst-cpu@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Client.UserInterface.Controls;
using Robust.Shared.Utility;

namespace Content.Client.Message;

public static class RichTextLabelExt
{


     /// <summary>
     /// Sets the labels markup.
     /// </summary>
     /// <remarks>
     /// Invalid markup will cause exceptions to be thrown. Don't use this for user input!
     /// </remarks>
    public static RichTextLabel SetMarkup(this RichTextLabel label, string markup)
    {
        label.SetMessage(FormattedMessage.FromMarkupOrThrow(markup));
        return label;
    }

     /// <summary>
     /// Sets the labels markup.<br/>
     /// Uses <c>FormatedMessage.FromMarkupPermissive</c> which treats invalid markup as text.
     /// </summary>
    public static RichTextLabel SetMarkupPermissive(this RichTextLabel label, string markup)
    {
        label.SetMessage(FormattedMessage.FromMarkupPermissive(markup));
        return label;
    }
}