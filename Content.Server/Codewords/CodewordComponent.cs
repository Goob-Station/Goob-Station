// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

﻿namespace Content.Server.Codewords;

/// <summary>
/// Container for generated codewords.
/// </summary>
[RegisterComponent, Access(typeof(CodewordSystem))]
public sealed partial class CodewordComponent : Component
{
    /// <summary>
    /// The codewords that were generated.
    /// </summary>
    [DataField]
    public string[] Codewords = [];
}
