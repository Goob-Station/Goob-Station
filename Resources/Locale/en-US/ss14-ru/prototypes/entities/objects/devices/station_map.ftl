ent-BaseHandheldStationMap = station map
  .desc = Displays a readout of the current station.

ent-HandheldStationMap = { "" }
  .desc = { "" }
  .suffix = Handheld

ent-HandheldStationMapEmpty = { ent-HandheldStationMap }
  .desc = { ent-HandheldStationMap.desc }
  .suffix = Handheld, Empty

ent-HandheldStationMapUnpowered = { ent-BaseHandheldStationMap }
  .desc = { ent-BaseHandheldStationMap.desc }
  .suffix = Handheld, Always Powered
