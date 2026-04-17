# SPDX-FileCopyrightText: 2022 Kara <lunarautomaton6@gmail.com>
# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

immovable-rod-collided-rod-not-good = Ох чёрт, это не к добру.
immovable-rod-penetrated-mob = { CAPITALIZE($rod) } начисто разносит { $mob }!

immovable-rod-consumed-none = { CAPITALIZE($rod) } не поглотил ни одной души.
immovable-rod-consumed-souls =
    { CAPITALIZE($rod) } поглотил { $amount } { $amount ->
        [one] душу
        [few] души
       *[other] душ
    }.
