ent-AiHeld = { "" }
  .desc = Components added / removed from an entity that gets inserted into an AI core.

ent-AiHeldIntellicard = { "" }
  .desc = Components added / removed from an entity that gets inserted into an Intellicard.

ent-AiHolder = { "" }
  .desc = Handles AI interactions across holocards + AI cores

ent-Intellicard = intellicard
  .desc = A storage device for AIs.
  .suffix = Empty

ent-PlayerStationAiEmpty = AI Core
  .desc = The latest in Artificial Intelligences.
  .suffix = Empty

ent-PlayerStationAi = { ent-PlayerStationAiEmpty }
  .desc = { ent-PlayerStationAiEmpty.desc }
  .suffix = Job spawn

ent-StationAiBrain = { ent-PositronicBrain }
  .desc = { ent-PositronicBrain.desc }

ent-StationAiHolo = AI eye
  .desc = The AI's viewer.

ent-StationAiHoloLocal = AI hologram
  .desc = A holographic representation of an AI.

ent-PlayerBorgBattery = { ent-BaseBorgChassisNotIonStormable }
  .desc = { ent-BaseBorgChassisNotIonStormable.desc }
  .suffix = Battery

ent-PlayerBorgSyndicateAssaultBattery = { ent-BorgChassisSyndicateAssault }
  .desc = { ent-BorgChassisSyndicateAssault.desc }
  .suffix = Battery, Module, Operative

ent-PlayerBorgSyndicateAssaultGhostRole = { ent-PlayerBorgSyndicateAssaultBattery }
  .desc = { ent-PlayerBorgSyndicateAssaultBattery.desc }
  .suffix = Ghost role

ent-PlayerBorgSyndicateSaboteurBattery = { ent-BorgChassisSyndicateSaboteur }
  .desc = { ent-BorgChassisSyndicateSaboteur.desc }
  .suffix = Battery, Module, Operative

ent-PlayerBorgSyndicateSaboteurGhostRole = { ent-PlayerBorgSyndicateSaboteurBattery }
  .desc = { ent-PlayerBorgSyndicateSaboteurBattery.desc }
  .suffix = Ghost role

ent-PlayerBorgSyndicateInvasionGhostRoleSpawner = syndicate invasion borg spawner
  .desc = { "" }

ent-PlayerBorgDerelict = { ent-BorgChassisDerelict }
  .desc = { ent-BorgChassisDerelict.desc }
  .suffix = Battery, Module

ent-PlayerBorgDerelictGhostRole = { ent-PlayerBorgDerelict }
  .desc = { ent-PlayerBorgDerelict.desc }
  .suffix = Ghost role
