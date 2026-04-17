# SPDX-FileCopyrightText: 2024 deltanedas <39013340+deltanedas@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 deltanedas <@deltanedas:kde.org>
# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
# SPDX-FileCopyrightText: 2025 JohnOakman <sremy2012@hotmail.fr>
# SPDX-FileCopyrightText: 2025 Piras314 <p1r4s@proton.me>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

autodoc-program-step-surgery = провести операцию на { $part }: { $name }
autodoc-program-step-grab-item = взять предмет: '{ $name }'
autodoc-program-step-grab-any = взять любой: { $name }
autodoc-item-organ = Орган
autodoc-item-part = Часть тела
autodoc-program-step-store-item = положить предмет на хранение
autodoc-program-step-set-label = установить метку: '{ $label }'
autodoc-program-step-wait = ждать { $length } секунд

autodoc-program-completed = ПРОГРАММА ЗАВЕРШЕНА
autodoc-error = ОШИБКА: { $error }
autodoc-fatal-error = ФАТАЛЬНАЯ ОШИБКА: { $error }
autodoc-waiting = ОЖИДАНИЕ ПРОГРАММЫ

autodoc-error-missing-patient = ПАЦИЕНТ ОТСУТСТВУЕТ
autodoc-error-body-part = ЧАСТЬ ТЕЛА НЕ ОБНАРУЖЕНА
autodoc-error-surgery-impossible = ВЫБРАННАЯ ОПЕРАЦИЯ НЕВОЗМОЖНА
autodoc-error-reality-breaking = РАЗРЫВ РЕАЛЬНОСТИ
autodoc-error-step-invalid-None = ВЫБРАННЫЙ ШАГ НЕВОЗМОЖЕН
autodoc-error-step-invalid-MissingSkills = ОТСУТСТВУЕТ НАВЫК
autodoc-error-step-invalid-NeedsOperatingTable = ОТСУТСТВУЕТ ОПЕРАЦИОННЫЙ СТОЛ
autodoc-error-step-invalid-Armor = КОНЕЧНОСТЬ ЗАБЛОКИРОВАНА ОДЕЖДОЙ
autodoc-error-step-invalid-ToolInvalid = ВЫБРАННЫЙ ИНСТРУМЕНТ НЕ ПРИГОДЕН
autodoc-error-step-invalid-SurgeryInvalid = ПАЦИЕНТ НЕПРИГОДЕН ДЛЯ ОПЕРАЦИИ
autodoc-error-step-invalid-MissingPreviousSteps = ПРЕДЫДУЩИЕ ШАГИ НЕ ЗАВЕРШЕНЫ
autodoc-error-step-invalid-StepCompleted = ШАГ УЖЕ ВЫПОЛНЕН
autodoc-error-step-invalid-MissingTool = ОТСУТСТВУЕТ ПРАВИЛЬНЫЙ ИНСТРУМЕНТ
autodoc-error-step-invalid-DoAfterFailed = НЕЛЬЗЯ ВЫПОЛНИТЬ ДО
autodoc-error-item-unavailable = ПРЕДМЕТ НЕДОСТУПЕН
autodoc-error-surgery-failed = ОПЕРАЦИЯ НЕУДАЧНА
autodoc-error-hand-full = МАНИПУЛЯТОР ПОЛОН
autodoc-error-storage-full = ХРАНИЛИЩЕ ПРЕДМЕТОВ ЗАПОЛНЕНО
autodoc-error-patient-unsedated = ПАЦИЕНТ ТРЕБУЕТ СЕДАЦИИ

# These intentionally have chinese alongside them for aesthetic purposes.
# I barely speak chinese, but I double checked I wasn't saying a slur with google translate, so this should be ok. Especially since 99.9% of our players don't speak chinese anyway.

# Chinese text translates approximately to "People's Glorious Automatic Surgery Machine"
autodoc-title = АвтоДок 人民辉煌自动手术机 MK.XIV
# Chinese text translates approximately to "New Plan"
autodoc-create-program = НОВАЯ ПРОГРАММА 新计划
# Chinese text translates approximately to "Plan Title"
autodoc-program-title = НАЗВАНИЕ ПРОГРАММЫ 计划标题
autodoc-program-title-placeholder = Программа { $number }
# Chinese text translates approximately to "Quit"
autodoc-abort-program = ОТМЕНИТЬ ПРОГРАММУ 退出

# Chinese text translates approximately to "View Plan"
autodoc-view-program-title = ПРОСМОТР ПРОГРАММЫ 查看计划
# Chinese text translates approximately to "Ensure Safety"
autodoc-safety-enabled = БЕЗОПАСНОСТЬ ВКЛ. 确保安全
# Chinese text translates literally to "No Ensure Safety" (it probably makes sense in Chinese grammar, I think. I hope.)
autodoc-safety-disabled = БЕЗОПАСНОСТИ ВЫКЛ. 不确保安全
# Chinese text translates approximately to "Delete Plan"
autodoc-remove-program = УДАЛИТЬ ПРОГРАММУ 删除计划
# Chinese text translates approximately to "Add a Step"
autodoc-add-step = ДОБАВИТЬ ШАГ 添加一步
# Chinese text translates approximately to "Remove a Step"
autodoc-remove-step = УДАЛИТЬ ШАГ 删除一步
# Chinese text translates approximately to "Launch the Glorious People's Project" (as with all these long sentences, im not super sure on the translation making sense)
autodoc-start-program = ЗАПУСТИТЬ ПРОГРАММУ 发起光荣人民计划
# Chinese text translates approximately to "import program")
autodoc-import-program = ИМПОРТ ПРОГРАММЫ 进口计划
# Chinese text translates approximately to "import program")
autodoc-export-program = ЭКСПОРТ ПРОГРАММЫ 出口计划

# Chinese text translates approximately to "Start Surgical Operation"
autodoc-add-step-surgery = НАЧАТЬ ОПЕРАЦИЮ 开始手术
# Chinese text translates approximately to "Take Item"
autodoc-add-step-grab-item = ВЗЯТЬ ПРЕДМЕТ 拿走物品
autodoc-add-step-grab-item-prompt = Название предмета
autodoc-add-step-grab-item-placeholder = сердце дворфа
# Chinese text translates approximately to "Take Organ"
autodoc-add-step-grab-organ = ВЗЯТЬ ОРГАН 拿管风琴
# Chinese text translates approximately to "Pick up body part"
autodoc-add-step-grab-part = ВЗЯТЬ ЧАСТЬ ТЕЛА 拾起身体部位
# Chinese text translates approximately to "Place item in storage space", this is overly long because I don't know how to write it shorter!!
autodoc-add-step-store-item = ПОМЕСТИТЬ В ХРАНИЛИЩЕ 将物品放入存储空间
# Chinese text translates approximately to "Set Label"
autodoc-add-step-set-label = УСТАНОВИТЬ МЕТКУ 设置标签
autodoc-add-step-set-label-prompt = Метка
# Chinese text translates approximately to "Wait" this is probably the most impolite way you can say it, but im stupid and this is all i know :)
autodoc-add-step-wait = ЖДАТЬ 等
autodoc-add-step-wait-prompt = Секунд ожидания

autodoc-body-part-Other = Другое
autodoc-body-part-Torso = Торс
autodoc-body-part-Chest = Грудь
autodoc-body-part-Groin = Пах
autodoc-body-part-Head = Голова
autodoc-body-part-Arm = Рука
autodoc-body-part-Hand = Кисть
autodoc-body-part-Leg = Нога
autodoc-body-part-Foot = Стопа
autodoc-body-part-Tail = Хвост

autodoc-body-symmetry-ignored = Любая
autodoc-body-symmetry-None = Нет
autodoc-body-symmetry-Left = Левая
autodoc-body-symmetry-Right = Правая

autodoc-submit = Отправить
