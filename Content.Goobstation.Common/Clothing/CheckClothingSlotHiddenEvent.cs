// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Common.Clothing;

[ByRefEvent]
public record struct CheckClothingSlotHiddenEvent(string Slot, bool Visible = true);
