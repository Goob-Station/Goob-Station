# SPDX-FileCopyrightText: 2021 E F R <602406+Efruit@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

# Examine Text
gas-volume-pump-system-examined =
    Насос настроен на [color={ $statusColor }]{ $rate }{ $rate ->
        [one] литр/сек
        [few] литра/сек
       *[other] литров/сек
    }[/color].
