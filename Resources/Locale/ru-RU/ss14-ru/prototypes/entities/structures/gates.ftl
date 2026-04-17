ent-LogicEmptyCircuit = пустая плата
    .desc = Кажется, чего-то не хватает.

ent-BaseLogicItem = { ent-LogicEmptyCircuit }
  .desc = { ent-LogicEmptyCircuit.desc }

ent-LogicGateOr = логический элемент
    .desc = Логический элемент с двумя портами на вход и одним на выход. Можно изменить логическую операцию с помощью отвёртки.
    .suffix = Or, ИЛИ

ent-LogicGateAnd = { ent-LogicGateOr }
    .desc = { ent-LogicGateOr.desc }
    .suffix = And, И

ent-LogicGateXor = { ent-LogicGateOr }
    .desc = { ent-LogicGateOr.desc }
    .suffix = Xor, Исключающее ИЛИ

ent-LogicGateNor = { ent-LogicGateOr }
    .desc = { ent-LogicGateOr.desc }
    .suffix = Nor, ИЛИ-НЕ

ent-LogicGateNand = { ent-LogicGateOr }
    .desc = { ent-LogicGateOr.desc }
    .suffix = Nand, И-НЕ

ent-LogicGateXnor = { ent-LogicGateOr }
    .desc = { ent-LogicGateOr.desc }
    .suffix = Xnor, Исключающее ИЛИ-НЕ

ent-EdgeDetector = детектор сигнала
    .desc = Определяет уровень сигнала и разделяет его. Устройство игнорирует импульсные сигналы.

ent-PowerSensor = датчик питания
    .desc = Генерирует сигналы в ответ на изменение напряжения в сети. Может циклически переключаться между напряжениями кабеля.

ent-MemoryCell = ячейка памяти
    .desc = Схема D-триггер защелки, хранящая сигнал, который может быть изменён в зависимости от входного и разрешающего портов.
