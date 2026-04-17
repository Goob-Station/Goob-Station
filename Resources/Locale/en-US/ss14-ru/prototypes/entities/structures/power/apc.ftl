ent-BaseAPC = APC
  .desc = A control terminal for the area's electrical systems.

ent-APCFrame = APC frame
  .desc = A control terminal for the area's electrical systems, lacking the electronics.

ent-APCConstructed = { ent-BaseAPC }
  .desc = { ent-BaseAPC.desc }
  .suffix = Open

ent-APCBasic = { ent-BaseAPC }
  .desc = { ent-BaseAPC.desc }
  .suffix = Basic, 50kJ

ent-APCHighCapacity = { ent-BaseAPC }
  .desc = { ent-BaseAPC.desc }
  .suffix = High Capacity, 100kJ

ent-APCSuperCapacity = { ent-BaseAPC }
  .desc = { ent-BaseAPC.desc }
  .suffix = Super Capacity, 150kJ

ent-APCHyperCapacity = { ent-BaseAPC }
  .desc = { ent-BaseAPC.desc }
  .suffix = Hyper Capacity, 200kJ
