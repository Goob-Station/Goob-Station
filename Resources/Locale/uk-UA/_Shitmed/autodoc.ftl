# SPDX-FileCopyrightText: 2024 deltanedas <39013340+deltanedas@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 deltanedas <@deltanedas:kde.org>
# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
# SPDX-FileCopyrightText: 2025 JohnOakman <sremy2012@hotmail.fr>
# SPDX-FileCopyrightText: 2025 Piras314 <p1r4s@proton.me>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

autodoc-program-step-surgery = виконати операцію на {$part}: {$name}
autodoc-program-step-grab-item = взяти предмет: '{$name}'
autodoc-program-step-grab-any = взяти будь-який: {$name}
autodoc-item-organ = Орган
autodoc-item-part = Частина тіла
autodoc-program-step-store-item = зберегти предмет
autodoc-program-step-set-label = встановити мітку: '{$label}'
autodoc-program-step-wait = чекати {$length} секунд

autodoc-program-completed = ПРОГРАМУ ЗАВЕРШЕНО
autodoc-error = ПОМИЛКА: {$error}
autodoc-fatal-error = ФАТАЛЬНА ПОМИЛКА: {$error}
autodoc-waiting = ПРОГРАМА ОЧІКУЄ

autodoc-error-missing-patient = ПАЦІЄНТ ВІДСУТНІЙ
autodoc-error-body-part = ЧАСТИНУ ТІЛА НЕ ВИЯВЛЕНО
autodoc-error-surgery-impossible = ОБРАНА ОПЕРАЦІЯ НЕМОЖЛИВА
autodoc-error-item-unavailable = ПРЕДМЕТ НЕДОСТУПНИЙ
autodoc-error-surgery-failed = ОПЕРАЦІЯ НЕ ВДАЛАСЯ
autodoc-error-hand-full = МАНІПУЛЯТОР ПРЕДМЕТІВ ЗАЙНЯТИЙ
autodoc-error-storage-full = ЛІТКИ ДЛЯ ПРЕДМЕТІВ ЗАПОВНЕНІ
autodoc-error-patient-unsedated = ПАЦІЄНТ ПОТРЕБУЄ СЕДАЦІЇ

# Вони навмисно мають китайські символи поруч для естетики.
# Я ледве розмовляю китайською, але я перевірив, що не кажу образ, за допомогою гугл-перекладача, тож це має бути гаразд. Особливо враховуючи, що 99.9% наших гравців все одно не розмовляють китайською.

# Китайський текст приблизно перекладається як "Славна Народна Автоматична Хірургічна Машина"
autodoc-title = AUTODOC MK.XIV
# Китайський текст приблизно перекладається як "Новий План"
autodoc-create-program = НОВА ПРОГРАМА
# Китайський текст приблизно перекладається як "Назва Плану"
autodoc-program-title = НАЗВА ПРОГРАМИ
autodoc-program-title-placeholder = Програма {$number}
# Китайський текст приблизно перекладається як "Вийти"
autodoc-abort-program = СКАСУВАТИ ПРОГРАМУ

# Китайський текст приблизно перекладається як "Переглянути План"
autodoc-view-program-title = ПЕРЕГЛЯНУТИ ПРОГРАМУ
# Китайський текст приблизно перекладається як "Забезпечити Безпеку"
autodoc-safety-enabled = БЕЗПЕКА УВІМКНЕНА
# Китайський текст буквально перекладається як "Не Забезпечувати Безпеку" (це, ймовірно, має сенс у китайській граматиці, я думаю. Сподіваюся.)
autodoc-safety-disabled = БЕЗПЕКА ВИМКНЕНА
# Китайський текст приблизно перекладається як "Видалити План"
autodoc-remove-program = ВИДАЛИТИ ПРОГРАМУ
# Китайський текст приблизно перекладається як "Додати Крок"
autodoc-add-step = ДОДАТИ КРОК
# Китайський текст приблизно перекладається як "Видалити Крок"
autodoc-remove-step = ВИДАЛИТИ КРОК
# Китайський текст приблизно перекладається як "Запустити Славний Народний Проект" (як і з усіма цими довгими реченнями, я не дуже впевнений, що переклад має сенс)
autodoc-start-program = ЗАПУСТИТИ ПРОГРАМУ
# Китайський текст приблизно перекладається як "імпортувати програму")
autodoc-import-program = ІМПОРТ ПРОГРАМИ
# Китайський текст приблизно перекладається як "імпорт програми")
autodoc-export-program = ЕКСПОРТ ПРОГРАМИ

# Китайський текст приблизно перекладається як "Почати хірургічну операцію"
autodoc-add-step-surgery = ВИКОНАТИ ОПЕРАЦІЮ
# Китайський текст приблизно перекладається як "Взяти Предмет"
autodoc-add-step-grab-item = ВЗЯТИ ПРЕДМЕТ
autodoc-add-step-grab-item-prompt = Назва предмету
autodoc-add-step-grab-item-placeholder = серце гнома
# Китайський текст приблизно перекладається як "Взяти Орган"
autodoc-add-step-grab-organ = ВЗЯТИ ОРГАН
# Китайський текст приблизно перекладається як "Підняти частину тіла"
autodoc-add-step-grab-part = ВЗЯТИ ЧАСТИНУ ТІЛА
# Китайський текст приблизно перекладається як "Помістити предмет у місце для зберігання", це занадто довго, бо я не знаю, як написати це коротше!!
autodoc-add-step-store-item = ЗБЕРЕГТИ ПРЕДМЕТ
# Китайський текст приблизно перекладається як "Встановити Мітку"
autodoc-add-step-set-label = ВСТАНОВИТИ МІТКУ
autodoc-add-step-set-label-prompt = Мітка
# Китайський текст приблизно перекладається як "Чекати" це, ймовірно, найнечемніший спосіб сказати це, але я дурний і це все, що я знаю :)
autodoc-add-step-wait = ЧЕКАТИ
autodoc-add-step-wait-prompt = Кількість секунд для очікування

autodoc-body-part-Other = Інше
autodoc-body-part-Torso = Торс
autodoc-body-part-Chest = Груди
autodoc-body-part-Groin = Пах
autodoc-body-part-Head = Голова
autodoc-body-part-Arm = Рука
autodoc-body-part-Hand = Кисть
autodoc-body-part-Leg = Нога
autodoc-body-part-Foot = Стопа
autodoc-body-part-Tail = Хвіст

autodoc-body-symmetry-ignored = Будь-який
autodoc-body-symmetry-None = Немає
autodoc-body-symmetry-Left = Ліва
autodoc-body-symmetry-Right = Права

autodoc-submit = Підтвердити
