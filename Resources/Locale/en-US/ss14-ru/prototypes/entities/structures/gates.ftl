ent-LogicEmptyCircuit = empty circuit
  .desc = Something seems to be missing.

ent-BaseLogicItem = { ent-LogicEmptyCircuit }
  .desc = { ent-LogicEmptyCircuit.desc }

ent-LogicGateOr = logic gate
  .desc = A logic gate with two inputs and one output. Technicians can change its mode of operation using a screwdriver.
  .suffix = Or

ent-LogicGateAnd = { ent-LogicGateOr }
  .desc = { ent-LogicGateOr.desc }
  .suffix = And

ent-LogicGateXor = { ent-LogicGateOr }
  .desc = { ent-LogicGateOr.desc }
  .suffix = Xor

ent-LogicGateNor = { ent-LogicGateOr }
  .desc = { ent-LogicGateOr.desc }
  .suffix = Nor

ent-LogicGateNand = { ent-LogicGateOr }
  .desc = { ent-LogicGateOr.desc }
  .suffix = Nand

ent-LogicGateXnor = { ent-LogicGateOr }
  .desc = { ent-LogicGateOr.desc }
  .suffix = Xnor

ent-EdgeDetector = edge detector
  .desc = Splits rising and falling edges into unique pulses and detects how edgy you are.

ent-PowerSensor = power sensor
  .desc = Generates signals in response to powernet changes. Can be cycled between cable voltages.

ent-MemoryCell = memory cell
  .desc = A D-Latch circuit that stores a signal which can be changed depending on input and enable ports.
