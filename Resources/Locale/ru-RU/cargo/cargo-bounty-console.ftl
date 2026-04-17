# SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 Killerqu00 <47712032+Killerqu00@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 lzk <124214523+lzk228@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
# SPDX-FileCopyrightText: 2025 BarryNorfolk <barrynorfolkman@protonmail.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

bounty-console-menu-title = Консоль запросов
bounty-console-label-button-text = Распечатать этикетку
bounty-console-skip-button-text = Пропустить
bounty-console-time-label = Время: [color=orange]{ $time }[/color]
bounty-console-reward-label = Награда: [color=limegreen]${ $reward }[/color]
bounty-console-manifest-label = Манифест: [color=orange]{ $item }[/color]
bounty-console-manifest-entry =
    { $amount ->
        [1] { $item }
       *[other] { $item } x{ $amount }
    }
bounty-console-manifest-reward = Награда: ${ $reward }
bounty-console-description-label = [color=gray]{ $description }[/color]
bounty-console-id-label = ID#{ $id }

bounty-console-flavor-left = Запросы, полученные от местных недобросовестных торговцев.
bounty-console-flavor-right = v1.4

bounty-manifest-header = [font size=14][bold]Официальный манифест запроса[/bold] (ID#{ $id })[/font]
bounty-manifest-list-start = Манифест:

bounty-console-tab-available-label = Доступные
bounty-console-tab-history-label = История
bounty-console-history-empty-label = История запросов не найдена
bounty-console-history-notice-completed-label = [color=limegreen]Выполнено[/color]
bounty-console-history-notice-skipped-label = [color=red]Пропущено[/color] { $id }
