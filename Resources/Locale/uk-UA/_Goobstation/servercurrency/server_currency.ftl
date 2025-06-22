# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
# SPDX-FileCopyrightText: 2025 SX-7 <92227810+SX-7@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

server-currency-name-singular = Губкоїн
server-currency-name-plural = Губкоїни

## Команди

server-currency-gift-command = подарувати
server-currency-gift-command-description = Дарує частину вашого балансу іншому гравцеві.
server-currency-gift-command-help = Використання: подарувати <гравець> <кількість>
server-currency-gift-command-error-1 = Ви не можете дарувати собі!
server-currency-gift-command-error-2 = Ви не можете собі цього дозволити! Ваш баланс: {$balance}.
server-currency-gift-command-giver = Ви дали {$player} {$amount}.
server-currency-gift-command-reciever = {$player} дав вам {$amount}.

server-currency-balance-command = баланс
server-currency-balance-command-description = Показує ваш баланс.
server-currency-balance-command-help = Використання: баланс
server-currency-balance-command-return = Ваш баланс: {$balance}.

server-currency-add-command = баланс:додати
server-currency-add-command-description = Додає валюту на баланс гравця.
server-currency-add-command-help = Використання: баланс:додати <гравець> <кількість>

server-currency-remove-command = баланс:відняти
server-currency-remove-command-description = Віднімає валюту з балансу гравця.
server-currency-remove-command-help = Використання: баланс:відняти <гравець> <кількість>

server-currency-set-command = баланс:встановити
server-currency-set-command-description = Встановлює баланс гравця.
server-currency-set-command-help = Використання: баланс:встановити <гравець> <кількість>

server-currency-get-command = баланс:отримати
server-currency-get-command-description = Отримує баланс гравця.
server-currency-get-command-help = Використання: баланс:отримати <гравець>

server-currency-command-completion-1 = Ім'я користувача
server-currency-command-completion-2 = Значення
server-currency-command-error-1 = Не вдалося знайти гравця з таким іменем.
server-currency-command-error-2 = Значення має бути цілим числом.
server-currency-command-return = {$player} має {$balance}.

# Оновлення 65%

gs-balanceui-title = Крамниця
gs-balanceui-confirm = Підтвердити

gs-balanceui-gift-label = Переказ:
gs-balanceui-gift-player = Гравець
gs-balanceui-gift-player-tooltip = Введіть ім'я гравця, якому ви хочете надіслати гроші
gs-balanceui-gift-value = Сума
gs-balanceui-gift-value-tooltip = Сума грошей для переказу

gs-balanceui-shop-label = Крамниця жетонів
gs-balanceui-shop-empty = Немає в наявності!
gs-balanceui-shop-buy = Купити
gs-balanceui-shop-footer = ⚠ Зверніться до адмінів (Ahelp), щоб використати ваш жетон. Лише 1 використання на день.

gs-balanceui-shop-token-label = Жетони
gs-balanceui-shop-tittle-label = Титули

gs-balanceui-shop-buy-token-antag = Купити жетон антагоніста - {$price} губкоїнів
gs-balanceui-shop-buy-token-admin-abuse = Купити жетон зловживання адміна - {$price} губкоїнів
gs-balanceui-shop-buy-token-hat = Купити жетон капелюха - {$price} губкоїнів

gs-balanceui-shop-token-antag = Жетон антагоніста високого рівня
gs-balanceui-shop-token-admin-abuse = Жетон зловживання адміна
gs-balanceui-shop-token-hat = Жетон капелюха

gs-balanceui-shop-buy-token-antag-desc = Дозволяє стати будь-яким антагоністом (окрім чарівників)
gs-balanceui-shop-buy-token-admin-abuse-desc = Дозволяє вам попросити адміна зловживати своїми повноваженнями проти вас. Адмінам рекомендується не стримуватися.
gs-balanceui-shop-buy-token-hat-desc = Адмін видасть вам випадковий капелюх.

gs-balanceui-admin-add-label = Додати (або відняти) гроші:
gs-balanceui-admin-add-player = Ім'я гравця
gs-balanceui-admin-add-value = Сума

gs-balanceui-remark-token-antag = Куплено жетон антагоніста.
gs-balanceui-remark-token-admin-abuse = Куплено жетон зловживання адміна.
gs-balanceui-remark-token-hat = Куплено жетон капелюха.
gs-balanceui-shop-click-confirm = Натисніть ще раз для підтвердження
gs-balanceui-shop-purchased = Придбано {$item}
