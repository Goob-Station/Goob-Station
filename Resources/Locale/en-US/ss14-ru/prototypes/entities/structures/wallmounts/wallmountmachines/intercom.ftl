ent-BaseIntercom = intercom
  .desc = An intercom. For when the station just needs to know something.

ent-IntercomAssembly = intercom assembly
  .desc = An intercom. It doesn't seem very helpful right now.

ent-IntercomConstructed = { ent-BaseIntercom }
  .desc = { ent-BaseIntercom.desc }
  .suffix = Empty, Panel Open

ent-Intercom = { ent-IntercomConstructed }
  .desc = { ent-IntercomConstructed.desc }

ent-BaseIntercomSecure = { ent-IntercomConstructed }
  .desc = { ent-IntercomConstructed.desc }

ent-IntercomCommon = { ent-IntercomConstructed }
  .desc = { ent-IntercomConstructed.desc }
  .suffix = Common

ent-IntercomCommand = { ent-IntercomConstructed }
  .desc = An intercom. It's been reinforced with metal.
  .suffix = Command

ent-IntercomEngineering = { ent-IntercomConstructed }
  .desc = { ent-IntercomConstructed.desc }
  .suffix = Engineering

ent-IntercomMedical = { ent-IntercomConstructed }
  .desc = { ent-IntercomConstructed.desc }
  .suffix = Medical

ent-IntercomScience = { ent-IntercomConstructed }
  .desc = { ent-IntercomConstructed.desc }
  .suffix = Science

ent-IntercomSecurity = { ent-IntercomConstructed }
  .desc = An intercom. It's been reinforced with metal from security helmets, making it a bitch-and-a-half to open.
  .suffix = Security

ent-IntercomService = { ent-IntercomConstructed }
  .desc = { ent-IntercomConstructed.desc }
  .suffix = Service

ent-IntercomSupply = { ent-IntercomConstructed }
  .desc = { ent-IntercomConstructed.desc }
  .suffix = Supply

ent-IntercomAll = { ent-IntercomConstructed }
  .desc = { ent-IntercomConstructed.desc }
  .suffix = All

ent-IntercomFreelance = { ent-IntercomConstructed }
  .desc = { ent-IntercomConstructed.desc }
  .suffix = Freelance
