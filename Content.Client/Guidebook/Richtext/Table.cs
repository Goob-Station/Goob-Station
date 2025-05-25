// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Diagnostics.CodeAnalysis;
using Content.Client.UserInterface.Controls;
using JetBrains.Annotations;
using Robust.Client.UserInterface;

namespace Content.Client.Guidebook.Richtext;

[UsedImplicitly]
public sealed class Table : TableContainer, IDocumentTag
{
    public bool TryParseTag(Dictionary<string, string> args, [NotNullWhen(true)] out Control? control)
    {
        HorizontalExpand = true;
        control = this;

        if (!args.TryGetValue("Columns", out var columns) || !int.TryParse(columns, out var columnsCount))
        {
            Logger.Error("Guidebook tag \"Table\" does not specify required property \"Columns.\"");
            control = null;
            return false;
        }

        Columns = columnsCount;

        return true;
    }
}