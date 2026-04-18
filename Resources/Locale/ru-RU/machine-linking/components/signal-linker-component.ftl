# SPDX-FileCopyrightText: 2021 Paul Ritter <ritter.paul1@googlemail.com>
# SPDX-FileCopyrightText: 2022 Jack Fox <35575261+DubiousDoggo@users.noreply.github.com>
# SPDX-FileCopyrightText: 2022 Kara <lunarautomaton6@gmail.com>
# SPDX-FileCopyrightText: 2022 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

signal-linker-component-saved = Успешно связано с устройством { $machine }!
signal-linker-component-linked-port = Успешно связан { $machine1 }:{ $port1 } c { $machine2 }:{ $port2 }!
signal-linker-component-unlinked-port = Успешно отвязан { $machine1 }:{ $port1 } от { $machine2 }:{ $port2 }!
signal-linker-component-connection-refused = { $machine } отказывается связываться!
signal-linker-component-max-connections-receiver = Достигнут максимум соединений для приёмника!
signal-linker-component-max-connections-transmitter = Достигнут максимум соединений для передатчика!

signal-linker-component-type-mismatch = Тип порта не совпадает с типом сохранённого порта!

signal-linker-component-out-of-range = Превышена дальность соединения!

# Verbs
signal-linking-verb-text-link-default = Связать стандартные порты
signal-linking-verb-success = Успешно подключены все стандартные соединения { $machine }.
signal-linking-verb-fail = Не удалось подключить все стандартные соединения { $machine }.
signal-linking-verb-disabled-no-transmitter = Сначала вам необходимо взаимодействовать с передатчиком, затем соедините со стандартным портом.
signal-linking-verb-disabled-no-receiver = Сначала вам необходимо взаимодействовать с приёмником, затем соедините со стандартным портом.
