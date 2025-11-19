// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

using Robust.Shared.Utility;

namespace Content.Client.Stylesheets.SheetletConfigs;

public interface ICheckboxConfig
{
    public ResPath CheckboxUncheckedPath { get; }
    public ResPath CheckboxCheckedPath { get; }
}

