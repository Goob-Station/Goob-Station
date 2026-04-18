ent-GasTrinaryBase = { ent-GasPipeSansLayers }
  .desc = { ent-GasPipeSansLayers.desc }

ent-GasTrinaryFlippedBase = { "" }
  .desc = { "" }
  .suffix = Flipped

ent-GasFilterBase = { ent-GasPipeSansLayers }
  .desc = { ent-GasPipeSansLayers.desc }

ent-GasFilter = gas filter
  .desc = Very useful for filtering gases.

ent-GasFilterFlipped = { ent-GasTrinaryFlippedBase }
  .desc = { ent-GasTrinaryFlippedBase.desc }

ent-GasMixer = gas mixer
  .desc = Very useful for mixing gases.

ent-GasMixerFlipped = gas mixer
  .desc = { ent-GasMixer.desc }
  .suffix = Flipped

ent-PressureControlledValve = pneumatic valve
  .desc = A bidirectional valve controlled by pressure. Opens if the output pipe is lower than the pressure of the control pipe by 101.325 kPa.
