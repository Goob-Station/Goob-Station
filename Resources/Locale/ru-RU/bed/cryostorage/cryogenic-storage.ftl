# SPDX-FileCopyrightText: 2024 Tayrtahn <tayrtahn@gmail.com>
# SPDX-FileCopyrightText: 2024 lunarcomets <140772713+lunarcomets@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 lzk <124214523+lzk228@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later


### Announcement

earlyleave-cryo-job-unknown = Должность неизвестна
# {$entity} available for GENDER function purposes
earlyleave-cryo-announcement =
    { $character } ({ $job }) { GENDER($entity) ->
        [male] был перемещён
        [female] была перемещена
        [epicene] были перемещены
       *[neuter] было перемещено
    } в криогенное хранилище!
earlyleave-cryo-sender = Станция

cryostorage-paused-map-name = Карта хранения тел криосна
