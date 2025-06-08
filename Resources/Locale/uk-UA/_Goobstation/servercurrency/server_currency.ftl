# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
# SPDX-FileCopyrightText: 2025 SX-7 <92227810+SX-7@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

server-currency-name-singular = Губ Коін
server-currency-name-plural = Губ Коіни

## Команди

server-currency-gift-command = подарувати
server-currency-gift-command-description = Дарує частину вашого балансу іншому гравцеві.
server-currency-gift-command-help = Використання: gift <гравець> <значення>
server-currency-gift-command-error-1 = Ви не можете дарувати собі!
server-currency-gift-command-error-2 = Ви не можете дозволити собі подарувати це! Ваш баланс {$balance}.
server-currency-gift-command-giver = Ви подарували {$player} {$amount}.
server-currency-gift-command-reciever = {$player} подарував вам {$amount}.

server-currency-balance-command = баланс
server-currency-balance-command-description = Повертає ваш баланс.
server-currency-balance-command-help = Використання: balance
server-currency-balance-command-return = У вас є {$balance}.

server-currency-add-command = balance:add
server-currency-add-command-description = Додає валюту до балансу гравця.
server-currency-add-command-help = Використання: balance:add <гравець> <значення>

server-currency-remove-command = balance:rem
server-currency-remove-command-description = Видаляє валюту з балансу гравця.
server-currency-remove-command-help = Використання: balance:rem <гравець> <значення>

server-currency-set-command = balance:set
server-currency-set-command-description = Встановлює баланс гравця.
server-currency-set-command-help = Використання: balance:set <гравець> <значення>

server-currency-get-command = balance:get
server-currency-get-command-description = Отримує баланс гравця.
server-currency-get-command-help = Використання: balance:get <гравець>

server-currency-command-completion-1 = Ім'я користувача
server-currency-command-completion-2 = Значення
server-currency-command-error-1 = Не вдалося знайти гравця з таким іменем.
server-currency-command-error-2 = Значення має бути цілим числом.
server-currency-command-return = {$player} має {$balance}.

# Оновлення 65%

gs-balanceui-title = Магазин
gs-balanceui-confirm = Підтвердити

gs-balanceui-gift-label = Переказ:
gs-balanceui-gift-player = Гравець
gs-balanceui-gift-player-tooltip = Введіть ім'я гравця, якому ви хочете надіслати гроші
gs-balanceui-gift-value = Значення
gs-balanceui-gift-value-tooltip = Сума грошей для переказу

gs-balanceui-shop-label = Магазин токенів
gs-balanceui-shop-empty = Немає в наявності!
gs-balanceui-shop-buy = Купити
gs-balanceui-shop-footer = ⚠ Напишіть адмінам (ahelp), щоб використати ваш токен. Тільки 1 використання на день.

gs-balanceui-shop-token-label = Токени
gs-balanceui-shop-tittle-label = Титули

gs-balanceui-shop-buy-token-antag = Купити токен антагоніста - {$price} Губ Коінів
gs-balanceui-shop-buy-token-admin-abuse = Купити токен адмінського свавілля - {$price} Губ Коінів
gs-balanceui-shop-buy-token-hat = Купити токен капелюха - {$price} Губ Коінів

gs-balanceui-shop-token-antag = Токен антагоніста високого рівня
gs-balanceui-shop-token-admin-abuse = Токен адмінського свавілля
gs-balanceui-shop-token-hat = Токен капелюха

gs-balanceui-shop-buy-token-antag-desc = Дозволяє вам стати будь-яким антагоністом. (За винятком чарівників)
gs-balanceui-shop-buy-token-admin-abuse-desc = Дозволяє вам попросити адміна зловживати своїми повноваженнями проти вас. Адмінам рекомендується відриватися на повну.
gs-balanceui-shop-buy-token-hat-desc = Адмін дасть вам випадковий капелюх.

gs-balanceui-admin-add-label = Додати (або відняти) гроші:
gs-balanceui-admin-add-player = Ім'я гравця
gs-balanceui-admin-add-value = Значення

gs-balanceui-remark-token-antag = Купив токен антагоніста.
gs-balanceui-remark-token-admin-abuse = Купив токен адмінського свавілля.
gs-balanceui-remark-token-hat = Купив токен капелюха.
gs-balanceui-shop-click-confirm = Натисніть ще раз, щоб підтвердити
gs-balanceui-shop-purchased = Придбано {$item}
