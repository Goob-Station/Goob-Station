# SPDX-FileCopyrightText: 2024 Kara <lunarautomaton6@gmail.com>
# SPDX-FileCopyrightText: 2024 Plykiya <58439124+Plykiya@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 ThatGuyUSA <thatguyusa123@gmail.com>
# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
# SPDX-FileCopyrightText: 2025 Ignaz "Ian" Kraft <ignaz.k@live.de>
# SPDX-FileCopyrightText: 2025 Killerqu00 <47712032+Killerqu00@users.noreply.github.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

contraband-examine-text-Minor = { $type ->
        *[item] [color=yellow]Цей предмет вважається незначною контрабандою.[/color]
        [reagent] [color=yellow]Цей реагент вважається незначною контрабандою.[/color]
    }
contraband-examine-text-Restricted = { $type ->
        *[item] [color=yellow]Цей предмет має відомчі обмеження.[/color]
        [reagent] [color=yellow]Цей реагент має відомчі обмеження.[/color]
    }
contraband-examine-text-Restricted-department = { $type ->
        *[item] [color=yellow]Цей предмет обмежений для {$departments} і може вважатися контрабандою.[/color]
        [reagent] [color=yellow]Цей реагент обмежений для {$departments} і може вважатися контрабандою.[/color]
    }
contraband-examine-text-Major = { $type ->
        *[item] [color=red]Цей предмет вважається серйозною контрабандою.[/color]
        [reagent] [color=red]Цей реагент вважається серйозною контрабандою.[/color]
    }
contraband-examine-text-GrandTheft = { $type ->
        *[item] [color=red]Цей предмет є дуже цінною ціллю для агентів Синдикату![/color]
        [reagent] [color=red]Цей реагент є дуже цінною ціллю для агентів Синдикату![/color]
    }
contraband-examine-text-Syndicate = { $type ->
        *[item] [color=crimson]Цей предмет є вкрай незаконною контрабандою Синдикату![/color]
        [reagent] [color=crimson]Цей реагент є вкрай незаконною контрабандою Синдикату![/color]
    }
contraband-examine-text-avoid-carrying-around = [color=red][italic]Вам, мабуть, не варто відкрито носити це без поважної причини.[/italic][/color]
contraband-examine-text-in-the-clear = [color=green][italic]Ви можете вільно носити це відкрито.[/italic][/color]
contraband-examinable-verb-text = Законність
contraband-examinable-verb-message = Перевірити законність цього предмета.
contraband-department-plural = {$department}
contraband-job-plural = {MAKEPLURAL($job)}
contraband-examine-text-Magical = { $type ->
        *[item] [color=#b337b3]Цей предмет є вкрай незаконною магічною контрабандою![/color]
        [reagent] [color=#b337b3]Цей реагент є вкрай незаконною магічною контрабандою![/color]
    }
contraband-examine-text-Clown = [color=yellow]Цей предмет належить клоуну, поверніть його, поки він не засумував.[/color]
contraband-examine-text-Highly-Illegal = { $type ->
        *[item] [color=crimson]Цей предмет є вкрай незаконною контрабандою![/color]
        [reagent] [color=crimson]Цей реагент є вкрай незаконною контрабандою![/color]
    }