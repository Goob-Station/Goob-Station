# SPDX-FileCopyrightText: 2022 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

multi-handed-item-pick-up-fail = {$number -> 
    [one] Il vous faut une main libre de plus pour prendre { THE($item) }.
    *[other] Il vous faut { $number } main libres de plus pour prendre { THE($item) }.
}
