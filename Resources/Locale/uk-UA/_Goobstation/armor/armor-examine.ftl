# SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
# SPDX-FileCopyrightText: 2024 username <113782077+whateverusername0@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
# SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

armor-examine-stamina = - [color=cyan]Витривалість[/color] пошкодження зменшено на [color=lightblue]{$num}%[/color].

armor-examine-cancel-delayed-knockdown = - [color=green]Повністю скасовує[/color] відкладене оглушення від електрокийка.

armor-examine-modify-delayed-knockdown-delay = - { $deltasign ->
          [1] [color=green]Збільшує[/color]
          *[-1] [color=red]Зменшує[/color]
      } затримку відкладеного оглушення від електрокийка на [color=lightblue]{NATURALFIXED($amount, 2)} { $amount ->
          [1] секунду
          *[other] секунд
      }[/color].

armor-examine-modify-delayed-knockdown-time = - { $deltasign ->
          [1] [color=red]Збільшує[/color]
          *[-1] [color=green]Зменшує[/color]
      } час відкладеного оглушення від електрокийка на [color=lightblue]{NATURALFIXED($amount, 2)} { $amount ->
          [1] секунду
          *[other] секунд
      }[/color].
