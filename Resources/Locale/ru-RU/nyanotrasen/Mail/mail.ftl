# SPDX-FileCopyrightText: 2024 BombasterDS <115770678+BombasterDS@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

mail-recipient-mismatch = Имя или должность получателя не совпадает.
mail-invalid-access = Имя и должность получателя совпадают, но доступ не соответствует ожидаемому.
mail-locked = Противовзломная защёлка не снята. Приложите ID получателя.
mail-desc-far = Почтовый конверт. С такого расстояния невозможно разглядеть, кому она адресована.
mail-desc-close = Почтовый конверт, адресованный { CAPITALIZE($name) }, { $job }.
mail-desc-fragile = Имеет [color=red]красную хрупкую наклейку[/color].
mail-desc-priority = На противовзломной системе активна [color=yellow]лента жёлтого приоритета[/color]. Лучше доставить это вовремя!
mail-desc-priority-inactive = На противовзломной системе [color=#886600]лента жёлтого приоритета[/color] неактивна.
mail-unlocked = Противовзломная система разблокирована.
mail-unlocked-by-emag = Противовзломная система *БЗЗТ*.
mail-unlocked-reward = Противовзломная система разблокирована. { $bounty } кредитов было добавлено на счёт снабжения.
mail-penalty-lock = ПРОТИВОВЗЛОМНАЯ СИСТЕМА СЛОМАНА. СЧЁТ СНАБЖЕНИЯ УМЕНЬШЕН НА { $credits } КРЕДИТОВ.
mail-penalty-fragile = ЦЕЛОСТНОСТЬ НАРУШЕНА. СЧЁТ СНАБЖЕНИЯ УМЕНЬШЕН НА { $credits } КРЕДИТОВ.
mail-penalty-expired = СРОК ДОСТАВКИ ПРОСРОЧЕН. СЧЁТ СНАБЖЕНИЯ УМЕНЬШЕН НА { $credits } КРЕДИТОВ.
mail-item-name-unaddressed = неадресованная почта
mail-item-name-addressed = почта ({ $recipient })
command-mailto-description = Поставить почту в очередь на доставку объекту. Пример использования: `mailto 1234 5678 false false`. Содержимое контейнера будет перемещено в реальную почту.

### Frontier: add is-large description

command-mailto-help = Использование: { $command } <entityUid получателя> <entityUid контейнер> [хрупкое: true или false] [приоритетное: true или false] [коробка: true или false, опционально]
command-mailto-no-mailreceiver = Получатель не имеет компонент { $requiredComponent }.
command-mailto-no-blankmail = Прототип  { $blankMail } не существует. Что-то пошло не так. Свяжитесь с программистом.
command-mailto-bogus-mail = { $blankMail } не имеет { $requiredMailComponent }. Что-то пошло не так. Свяжитесь с программистом.
command-mailto-invalid-container = У объекта нет контейнера { $requiredContainer }.
command-mailto-unable-to-receive = Не удалось настроить получателя для почты. Возможно, отсутствует ID.
command-mailto-no-teleporter-found = Не удалось сопоставить получателя ни с одним почтовым телепортером станции. Получатель может находиться вне станции.
command-mailto-success = Успешно! Почтовая посылка будет отправлена при следующей телепортации через { $timeToTeleport } сек.
command-mailnow = Принудительно заставить все почтовые телепортеры доставить новую партию почты как можно скорее. Это не обойдёт лимит недоставленной почты.
command-mailnow-help = Использование: { $command }
command-mailnow-success = Успешно! Все почтовые телепортеры скоро доставят новую партию почты.
