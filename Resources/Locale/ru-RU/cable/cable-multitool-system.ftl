# SPDX-FileCopyrightText: 2021 20kdc <asdd2808@gmail.com>
# SPDX-FileCopyrightText: 2022 Kara <lunarautomaton6@gmail.com>
# SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

cable-multitool-system-internal-error-no-power-node = Ваш мультитул выдаёт сообщение: "ВНУТРЕННЯЯ ОШИБКА: НЕ КАБЕЛЬ ПИТАНИЯ".
cable-multitool-system-internal-error-missing-component = Ваш мультитул выдаёт сообщение: "ВНУТРЕННЯЯ ОШИБКА: КАБЕЛЬ АНОМАЛЕН".
cable-multitool-system-verb-name = Питание
cable-multitool-system-verb-tooltip = Используйте мультитул для просмотра статистики питания.

cable-multitool-system-statistics =
    Ваш мультитул показывает статистику:
    Источник тока: { POWERWATTS($supplyc) }
    От батарей: { POWERWATTS($supplyb) }
    Теоретическое снабжение: { POWERWATTS($supplym) }
    Идеальное потребление: { POWERWATTS($consumption) }
    Входной запас: { POWERJOULES($storagec) } / { POWERJOULES($storagem) } ({ TOSTRING($storager, "P1") })
    Выходной запас: { POWERJOULES($storageoc) } / { POWERJOULES($storageom) } ({ TOSTRING($storageor, "P1") })
