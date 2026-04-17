ent-StationAnchorBase = station anchor
  .desc = Prevents stations from moving.
  .suffix = On

ent-StationAnchorIndestructible = { ent-StationAnchorBase }
  .desc = { ent-StationAnchorBase.desc }
  .suffix = Indestructible, Unpowered

ent-StationAnchor = { ent-StationAnchorBase }
  .desc = { ent-StationAnchorBase.desc }

ent-StationAnchorOff = { ent-StationAnchorBase }
  .desc = { ent-StationAnchorBase.desc }
  .suffix = Off
