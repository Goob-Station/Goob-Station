# Robotic Arm

signal-port-name-input-machine = Сигнальный порт: Входной порт
signal-port-description-input-machine = Слот для автоматизации машины, из которого можно брать предметы, вместо того чтобы брать их с пола.
signal-port-name-output-machine = Сигнальный порт: Выходной порт
signal-port-description-output-machine = Слот автоматизации машины, в который можно вставлять предметы, вместо того чтобы класть их на пол.
signal-port-name-item-moved = Сигнальный порт: Перемещенный предмет
signal-port-description-item-moved = Сигнальный порт, который получает импульс после перемещения предмета этим манипулятором.
signal-port-name-automation-slot-filter = Сигнальный порт: Слот фильтра
signal-port-description-automation-slot-filter = Слот для фильтра автоматической машины.

# Измельчитель реагентов

signal-port-name-automation-slot-beaker = Элемент: Слот для мензурки
signal-port-description-automation-slot-beaker = Слот автоматизации для мензурки машины для обработки жидкостей.
signal-port-name-automation-slot-input = Элемент: Входные элементы
signal-port-description-automation-slot-input = Слот автоматизации для хранения входных элементов машины.

# Flatpacker

signal-port-name-automation-slot-board = Элемент: Слот для платы
signal-port-description-automation-slot-board = Слот автоматизации для печатной платы плоского упаковщика.
signal-port-name-automation-slot-materials = Элемент: Хранилище материалов
signal-port-description-automation-slot-materials = Слот автоматизации для вставки материалов в хранилище машины.

# Disposal Unit

signal-port-name-flush = Смыв
signal-port-description-flush = Сигнальный порт для переключения механизма смыва утилизационного блока.
signal-port-name-eject = Выброс
signal-port-description-eject = Сигнальный порт для выброса содержимого утилизационного блока.
signal-port-name-ready = Готовность
signal-port-description-ready = Сигнальный порт, на который подается импульс после того, как утилизатор полностью нагнетает давление.

# Storage Bin

signal-port-name-automation-slot-storage = Предмет: Хранилище
signal-port-description-automation-slot-storage = Слот автоматизации для инвентаря буфера хранения.
signal-port-name-storage-inserted = Вставлено
signal-port-description-storage-inserted = Сигнальный порт, который получает импульсы после того, как предмет вставляется в буфер хранения.
signal-port-name-storage-removed = Удалено
signal-port-description-storage-removed = Сигнальный порт, который получает импульс после извлечения предмета из буфера.

# Fax Machine

signal-port-name-automation-slot-paper = Элемент: Бумага
signal-port-description-automation-slot-paper = Слот автоматизации для лотка для бумаги в факсе.
signal-port-name-fax-copy = Копирование - факс
signal-port-description-fax-copy = Сигнальный порт для копирования бумаги в факсе аппарата.

# Constructor / Interactor

signal-port-name-machine-start = Запуск
signal-port-description-machine-start = Сигнальный порт для однократного запуска машины.
signal-port-name-machine-autostart = Автозапуск
signal-port-description-machine-autostart = Сигнальный порт для управления запуском после завершения работы автоматически.
signal-port-name-machine-started = Запущено
signal-port-description-machine-started = Сигнальный порт, на который подается импульс после запуска машины.
signal-port-name-machine-completed = Завершено
signal-port-description-machine-completed = Сигнальный порт, который получает импульс после того, как машина завершает свою работу.
signal-port-name-machine-failed = Не удалось
signal-port-description-machine-failed = Сигнальный порт, который получает импульсы после того, как машина не смогла запуститься.

# Interactor

signal-port-name-automation-slot-tool = Предмет: инструмент
signal-port-description-automation-slot-tool = Слот автоматизации для инструмента, которым владеет интерактор.

# Autodoc

signal-port-name-automation-slot-autodoc-hand = Предмет: Рука Автодока
signal-port-description-automation-slot-autodoc-hand = Слот автоматизации для удерживаемого органа/части/и т.д. автодока из инструкций STORE ITEM / GRAB ITEM.

# Газовый баллон

signal-port-name-automation-slot-gas-tank = Предмет: Газовый баллон
signal-port-description-automation-slot-gas-tank = Слот автоматизации для газового баллона.

# Radiation Collector

signal-port-name-rad-empty = Пустой
signal-port-description-rad-empty = Сигнальный порт устанавливается на HIGH, если баллон отсутствует или давление в нем ниже 33%, LOW - в противном случае.
signal-port-name-rad-low = Низкий
signal-port-description-rad-low = Сигнальный порт устанавливается на HIGH, если давление в баке ниже 66 %, LOW - в противном случае.
signal-port-name-rad-full = Полный
signal-port-description-rad-full = Сигнальный порт устанавливается на HIGH, если давление в баке выше 66 %, LOW - в противном случае.
