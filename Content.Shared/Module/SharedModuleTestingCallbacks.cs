// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

using Robust.Shared.ContentPack;

namespace Content.Shared.Module
{
    public abstract class SharedModuleTestingCallbacks : ModuleTestingCallbacks
    {
        public Action SharedBeforeIoC { get; set; } = default!;
    }
}
