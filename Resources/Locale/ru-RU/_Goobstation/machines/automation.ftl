# Роботизированная рука

signal-port-name-input-machine = Предмет: Машина ввода
signal-port-description-input-machine = Слот автоматизации машины для извлечения предметов, вместо того чтобы брать их с пола.
signal-port-name-output-machine = Предмет: Машина вывода
signal-port-description-output-machine = Слот автоматизации машины для помещения предметов внутрь, вместо того чтобы ставить их на пол.
signal-port-name-item-moved = Предмет перемещён
signal-port-description-item-moved = Сигнальный порт, который активируется после перемещения предмета этой рукой.
signal-port-name-automation-slot-filter = Предмет: Слот фильтра
signal-port-description-automation-slot-filter = Слот автоматизации для фильтра машины автоматизации.

# Измельчитель реагентов

signal-port-name-automation-slot-beaker = Предмет: Слот стакана
signal-port-description-automation-slot-beaker = Слот автоматизации для стакана машины, работающей с жидкостями.
signal-port-name-automation-slot-input = Предмет: Входные предметы
signal-port-description-automation-slot-input = Слот автоматизации для хранения входных предметов машины.

# Флетпакер (Flatpacker)

signal-port-name-automation-slot-board = Предмет: Слот платы
signal-port-description-automation-slot-board = Слот автоматизации для платы флетпакера.
signal-port-name-automation-slot-materials = Предмет: Хранилище материалов
signal-port-description-automation-slot-materials = Слот автоматизации для помещения материалов в хранилище машины.

# Устройство для выброса мусора (Disposal Unit)

signal-port-name-flush = Смыв
signal-port-description-flush = Сигнальный порт для управления механизмом смыва в устройстве для выброса.
signal-port-name-eject = Извлечь
signal-port-description-eject = Сигнальный порт для выброса содержимого устройства.
signal-port-name-ready = Готово
signal-port-description-ready = Сигнальный порт, который активируется, когда устройство полностью набирает давление.

# Контейнер для хранения (Storage Bin)

signal-port-name-automation-slot-storage = Предмет: Хранилище
signal-port-description-automation-slot-storage = Слот автоматизации для инвентаря контейнера хранения.
signal-port-name-storage-inserted = Вставлено
signal-port-description-storage-inserted = Сигнальный порт, который активируется после помещения предмета в контейнер.
signal-port-name-storage-removed = Удалено
signal-port-description-storage-removed = Сигнальный порт, который активируется после удаления предмета из контейнера.

# Факсовая машина

signal-port-name-automation-slot-paper = Предмет: Бумага
signal-port-description-automation-slot-paper = Слот автоматизации для лотка с бумагой факсовой машины.
signal-port-name-fax-copy = Копировать факс
signal-port-description-fax-copy = Сигнальный порт для копирования бумаги факсовой машины.

# Конструктор / Интеректор

signal-port-name-machine-start = Запуск
signal-port-description-machine-start = Сигнальный порт для однократного запуска машины.
signal-port-name-machine-autostart = Автозапуск
signal-port-description-machine-autostart = Сигнальный порт для управления автоматическим запуском после завершения.
signal-port-name-machine-started = Запущено
signal-port-description-machine-started = Сигнальный порт, который активируется после запуска машины.
signal-port-name-machine-completed = Завершено
signal-port-description-machine-completed = Сигнальный порт, который активируется после завершения работы машины.
signal-port-name-machine-failed = Сбой
signal-port-description-machine-failed = Сигнальный порт, который активируется, если машина не смогла запуститься.

# Интеректор

signal-port-name-automation-slot-tool = Предмет: Инструмент
signal-port-description-automation-slot-tool = Слот автоматизации для удерживаемого инструмента интеректора.

# Автодок

signal-port-name-automation-slot-autodoc-hand = Предмет: Рука автодока
signal-port-description-automation-slot-autodoc-hand = Слот автоматизации для удерживаемого автодоком органа/части и т.п. из инструкций STORE ITEM / GRAB ITEM.
signal-port-name-automation-slot-gas-tank = Слот: Баллон
signal-port-description-automation-slot-gas-tank = Автоматизированный слот для газовой емкости.
signal-port-name-rad-empty = Пусто
signal-port-description-rad-empty = Выдает сигнал HIGH, если емкость отсутствует или заполнена менее чем на 33%, в остальных случаях - LOW.
signal-port-name-rad-low = Низкий
signal-port-description-rad-low = Выдает сигнал HIGH, если емкость заполнена менее чем на 66%, в остальных случаях - LOW.
signal-port-name-rad-full = Полный
signal-port-description-rad-full = Выдает сигнал HIGH, если емкость заполнена более чем на 66%, в остальных случаях - LOW.
