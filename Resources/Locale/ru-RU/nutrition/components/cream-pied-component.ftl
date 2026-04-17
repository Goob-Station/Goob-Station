# SPDX-FileCopyrightText: 2021 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
# SPDX-FileCopyrightText: 2021 Galactic Chimp <63882831+GalacticChimp@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

cream-pied-component-on-hit-by-message =
    { $thrower } { GENDER($thrower) ->
        [male] КРЕМировал
        [female] КРЕМировала
        [epicene] КРЕМировали
       *[neuter] КРЕМировало
    } вас!
cream-pied-component-on-hit-by-message-others =
    { $thrower } { GENDER($thrower) ->
        [male] КРЕМировал
        [female] КРЕМировала
        [epicene] КРЕМировали
       *[neuter] КРЕМировало
    } { $owner }!
