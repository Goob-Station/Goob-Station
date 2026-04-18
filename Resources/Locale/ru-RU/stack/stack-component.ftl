# SPDX-FileCopyrightText: 2021 AJCM-git <60196617+AJCM-git@users.noreply.github.com>
# SPDX-FileCopyrightText: 2021 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
# SPDX-FileCopyrightText: 2021 Galactic Chimp <63882831+GalacticChimp@users.noreply.github.com>
# SPDX-FileCopyrightText: 2021 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
# SPDX-FileCopyrightText: 2021 Remie Richards <remierichards@gmail.com>
# SPDX-FileCopyrightText: 2021 ike709 <ike709@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 Ilya246 <57039557+Ilya246@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

### UI

# Shown when a stack is examined in details range
comp-stack-examine-detail-count =
    В стопке [color={ $markupCountColor }]{ $count }[/color] { $count ->
        [one] предмет
        [few] предмета
       *[other] предметов
    }.

# Stack status control
comp-stack-status = Количество: [color=white]{ $count }[/color]

### Interaction Messages

# Shown when attempting to add to a stack that is full
comp-stack-already-full = Стопка уже заполнена.

# Shown when a stack becomes full
comp-stack-becomes-full = Стопка теперь заполнена.

# Text related to splitting a stack
comp-stack-split = Вы разделили стопку.
# Goobstation - Custom stack splitting dialog
comp-stack-split-custom = Split amount...
comp-stack-split-halve = Разделить пополам
comp-stack-split-too-small = Стопка слишком мала для разделения.

# Goobstation - Custom stack splitting dialog
comp-stack-split-size = Max: { $size }

ui-custom-stack-split-title = Split Amount
ui-custom-stack-split-line-edit-placeholder = Amount
ui-custom-stack-split-apply = Split
