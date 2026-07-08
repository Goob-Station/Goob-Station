// SPDX-FileCopyrightText: 2024 Simon <63975668+Simyon264@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Configuration;

namespace Content.Shared.CCVar;

public sealed partial class CCVars
{
    public static readonly CVarDef<string> StatusMoMMIUrl =
        CVarDef.Create("status.mommiurl", "", CVar.SERVERONLY);

    public static readonly CVarDef<string> StatusMoMMIPassword =
        CVarDef.Create("status.mommipassword", "", CVar.SERVERONLY | CVar.CONFIDENTIAL);

}