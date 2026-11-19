# SPDX-License-Identifier: AGPL-3.0-or-later

fermenting-barrel-added = { $count ->
    [one] You drop the crop into the barrel.
   *[other] You tip {$count} crops into the barrel.
}
fermenting-barrel-full = There isn't room in the barrel for that.
fermenting-barrel-opened = You lift the lid off the barrel.
fermenting-barrel-closed = You seal the lid on the barrel. Time to wait.
fermenting-barrel-empty-verb = Tip out crops
fermenting-barrel-emptied = You tip the waiting crops out of the barrel.
fermenting-barrel-examine-open = The lid is off. Crops can go in.
fermenting-barrel-examine-closed = The lid is on, and whatever's inside is fermenting.
fermenting-barrel-examine-waiting = { $count ->
    [one] One crop is waiting to ferment.
   *[other] {$count} crops are waiting to ferment.
}
