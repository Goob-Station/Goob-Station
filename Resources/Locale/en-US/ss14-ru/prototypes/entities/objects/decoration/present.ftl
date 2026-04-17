ent-PresentBase = present
  .desc = A little box with incredible surprises inside.

ent-Present = { ent-PresentBase }
  .desc = { ent-PresentBase.desc }
  .suffix = Empty

ent-PresentRandomUnsafe = { ent-PresentBase }
  .desc = { ent-PresentBase.desc }
  .suffix = Filled, any item

ent-PresentRandomInsane = { ent-PresentRandomUnsafe }
  .desc = { ent-PresentRandomUnsafe.desc }
  .suffix = Filled, any entity

ent-PresentRandom = { ent-PresentBase }
  .desc = { ent-PresentBase.desc }
  .suffix = Filled Safe

ent-PresentRandomCoal = { ent-PresentBase }
  .desc = { ent-PresentBase.desc }
  .suffix = Filled Coal

ent-PresentRandomCash = { ent-PresentBase }
  .desc = { ent-PresentBase.desc }
  .suffix = Filled Cash

ent-PresentTrash = wrapping paper
  .desc = Carefully folded, taped, and tied with a bow. Then ceremoniously ripped apart and tossed on the floor.
