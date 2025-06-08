# SPDX-FileCopyrightText: 2024 IProduceWidgets <107586145+IProduceWidgets@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

# FTLdiskburner
cmd-ftldisk-desc = Створює диск з FTL-координатами для перельоту на карту, на якій знаходиться вказаний EntityID
cmd-ftldisk-help = ftldisk [EntityID]

cmd-ftldisk-no-transform = Сутність {$destination} не має компонента Transform!
cmd-ftldisk-no-map = Сутність {$destination} не має карти!
cmd-ftldisk-no-map-comp = Сутність {$destination} якимось чином знаходиться на карті {$map} без компонента карти.
cmd-ftldisk-map-not-init = Сутність {$destination} знаходиться на карті {$map}, яка не ініціалізована! Перевірте, чи безпечно її ініціалізувати, а потім ініціалізуйте карту, інакше гравці застрягнуть на місці!
cmd-ftldisk-map-paused = Сутність {$desintation} знаходиться на карті {$map}, яка призупинена! Будь ласка, спочатку зніміть паузу з карти, інакше гравці застрягнуть на місці.
cmd-ftldisk-planet = Сутність {$desintation} знаходиться на карті планети {$map} і вимагатиме точки FTL. Вона може вже існувати.
cmd-ftldisk-already-dest-not-enabled = Сутність {$destination} знаходиться на карті {$map}, яка вже має FTLDestinationComponent, але він не увімкнений! Встановіть це вручну для безпеки.
cmd-ftldisk-requires-ftl-point = Сутність {$destination} знаходиться на карті {$map}, яка вимагає точки FTL для переміщення! Вона може вже існувати.

cmd-ftldisk-hint = Мережевий ID карти
