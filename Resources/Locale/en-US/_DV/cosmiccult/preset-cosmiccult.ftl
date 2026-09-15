## COSMIC CULT ROUND, ANTAG & GAMEMODE TEXT

cosmiccult-announcement-sender = ???

cosmiccult-title = Xeno Cult
cosmiccult-description = Xenomorph worshippers hide among the expedition.

roles-antag-cosmiccult-name = Xeno-Cultist
roles-antag-cosmiccult-description = Serve the Empress in secret. Sabotage the base, convert the weak, and prepare the site for the hive.

cosmiccult-gamemode-title = The Xeno Cult
cosmiccult-gamemode-description = Bio-scanners detect anomalous hive pheromone signatures. No further data.

cosmiccult-vote-steward-initiator = The Empress
cosmiccult-vote-steward-title = Xeno Cult Stewardship
cosmiccult-vote-steward-briefing =
    You are the Xeno Cult's Steward!
    Secure a nest site for The Monument (hive shrine), organize the cult, and keep Wey-Yu from discovering the infestation.
    You may not dictate how cultists spend Entropy.

cosmiccult-finale-autocall-briefing = The Monument has been completed! Gather yourselves, activate it, and prepare for the end.
cosmiccult-finale-ready = A terrifying light surges forth from The Monument!
cosmiccult-finale-speedup = The beckoning quickens! Energy surges through the surroundings...

cosmiccult-finale-degen = You feel yourself unravelling!
cosmiccult-finale-location = Scanners are detecting an enormous Λ-CDM spike {$location}!
cosmiccult-finale-cancel-begin = Your mind's willpower begins to shatter the ritual...
cosmiccult-finale-beckon-begin = The whispers in the back of your mind intensify...
cosmiccult-finale-beckon-success = You beckon for the final curtain call.

cosmiccult-monument-powerdown = The Monument falls eerily silent.


## ROUNDEND TEXT

cosmiccult-roundend-cultist-count = {$initialCount ->
    [1] There was {$initialCount} [color=#4cabb3]Cosmic Cultist[/color].
    *[other] There were {$initialCount} [color=#4cabb3]Cosmic Cultists[/color].
}
cosmiccult-roundend-entropy-count = The cult siphoned {$count} Entropy.
cosmiccult-roundend-cultpop-count = Cultists made up {$count}% of the crew.
cosmiccult-roundend-monument-stage = {$stage ->
    [1] Alas, the Monument seems abandoned.
    [2] The Monument progressed, but completion was out of reach.
    [3] The Monument was completed.
    *[other] [color=red]Something went REALLY wrong.[/color]
}

cosmiccult-roundend-cultcomplete = [color=#7CFC00]Xeno Cult complete victory![/color]
cosmiccult-roundend-cultmajor = [color=#7CFC00]Xeno Cult major victory![/color]
cosmiccult-roundend-cultminor = [color=#7CFC00]Xeno Cult minor victory![/color]
cosmiccult-roundend-neutral = [color=yellow]Neutral ending![/color]
cosmiccult-roundend-crewminor = [color=green]Crew minor victory![/color]
cosmiccult-roundend-crewmajor = [color=green]Crew major victory![/color]
cosmiccult-roundend-crewcomplete = [color=green]Crew complete victory![/color]

cosmiccult-summary-cultcomplete = The Xeno Cult opened the world to the Empress!
cosmiccult-summary-cultmajor = The Steward escaped with hive knowledge. The cult endures.
cosmiccult-summary-cultminor = Several cultists made it to Midpoint unnoticed. The cult will live to see another day.
cosmiccult-summary-neutral = No cultists made it to midpoint, but some remain on the station. The struggle continues.
cosmiccult-summary-crewminor = The Monument did not reach full power.
cosmiccult-summary-crewmajor = All Xeno-Cultists were eliminated.
cosmiccult-summary-crewcomplete = All Xeno-Cultists were deconverted!

cosmiccult-elimination-shuttle-call = Based on scans from our long-range sensors, the Λ-CDM anomaly has subsided. We thank you for your prudence. An emergency shuttle has been automatically called to the station for decontamination and debriefing procedures. ETA: {$time} {$units}.
cosmiccult-elimination-announcement = Based on scans from our long-range sensors, the Λ-CDM anomaly has subsided. We thank you for your prudence. An emergency shuttle is already inbound. Return to CentComm safely for decontamination and debriefing procedures.


## BRIEFINGS

cosmiccult-role-roundstart-fluff =
    On this Weyland-Yutani frontier world, something older than the Company opens in your mind.
    The Empress. The perfect organism. The hive that will reclaim the Wild Lands.
    You will feed it from within.

cosmiccult-role-short-briefing =
    You are a Xeno-Cultist!
    Objectives are in the character menu.
    Read the guidebook entry for cult tools (flavour: hive rites).

cosmiccult-role-conversion-fluff =
    The rite completes. Hive-song floods your thoughts.
    The Empress watches. The perfect organism awaits.
    Serve the nest.

cosmiccult-role-deconverted-fluff =
    A great emptiness washes across your mind. A comforting, yet unfamiliar emptiness...
    All the thoughts and memories of your time in the cult begin to fade and blur.

cosmiccult-role-deconverted-briefing =
    Deconverted!
    You are no longer a Xeno-Cultist.

cosmiccult-monument-stage1-briefing =
    The Monument has been beckoned.
    It is located {$location}!

cosmiccult-monument-stage2-briefing =
    The Monument grows in power!
    Its influence will affect realspace in {$time} seconds.

cosmiccult-monument-stage3-briefing =
    The Monument grows in power!
    Its influence will begin to overlap with realspace in {$time} seconds.


## MALIGN RIFTS

cosmiccult-rift-inuse = You can't do this right now.
cosmiccult-rift-invaliduser = You lack to proper tools to deal with this.
cosmiccult-rift-chaplainoops = Wield your holy scripture.
cosmiccult-rift-alreadyempowered = You are already empowered; the rift's power would be wasted.
cosmiccult-rift-wasempowered = Your body won't be able to handle being empowered a second time...
cosmiccult-rift-beginabsorb = The rift begins to merge with you...
cosmiccult-rift-beginpurge = Your consecration begins purging the malign rift...

cosmiccult-rift-absorb = {$NAME} absorbs the rift, and malign light empowers their body!
cosmiccult-rift-purge = {$NAME} purges the malign rift from reality!


## CHANTRY

cosmiccult-chantry-location = A dangerous increase in Λ-CDM has been detected {$location}! Intercept and intervene immediately.
cosmiccult-chantry-powerup = The vacuous chantry flares to life!

## UI / BASE POPUP

cosmiccult-ui-deconverted-title = Deconverted
cosmiccult-ui-converted-title = Converted
cosmiccult-ui-roundstart-title = The Empress

cosmiccult-ui-converted-text-1 =
    You have been inducted into the Xeno Cult.
cosmiccult-ui-converted-text-2 =
    Aid the cult in secret. Grow the Empress's claim on this base.

cosmiccult-ui-roundstart-text-1 =
    You are a Xeno-Cultist!
cosmiccult-ui-roundstart-text-2 =
    Keep the nest secret. Follow your Steward.

cosmiccult-ui-deconverted-text-1 =
    You are no longer a Xeno-Cultist.
cosmiccult-ui-deconverted-text-2 =
    Memories of the cult fade. If reconverted, they return.

cosmiccult-ui-popup-confirm = Confirm


## OBJECTIVES / CHARACTERMENU

objective-issuer-cosmiccult = [bold][color=#7CFC00]The Empress[/color][/bold]

objective-cosmiccult-charactermenu = Advance the hive's claim on this world. Complete your tasks for the Empress.
objective-cosmiccult-steward-charactermenu = Direct the cult. Grow the Empress's influence without exposing the nest.

objective-condition-conversion-title = CONVERT CREW
objective-condition-conversion-desc = Collectively bring at least {$count} crew into the fold.
objective-condition-entropy-title = SIPHON ENTROPY
objective-condition-entropy-desc = Collectively siphon at least {$count} entropy from the crew.
objective-condition-culttier-title = EMPOWER THE MONUMENT
objective-condition-culttier-desc = Ensure that The Monument is brought to full power.
objective-condition-victory-title = USHER IN THE END
objective-condition-victory-desc = Beckon The Unknown, and herald the final curtain call.


## CHAT ANNOUNCEMENTS

cosmiccult-radio-tier1-progress = The Monument is beckoned unto the station...

cosmiccult-announce-tier3-progress = An unnerving numbness prickles your senses.
cosmiccult-announce-tier3-warning = Scanners detect an abnormal increase in Λ-CDM! Report any unnatural phenomena to security or epistemics.

cosmiccult-announce-pre-finale-progress = Arcs of noospheric energy crackle across the station's groaning structure. The end draws near.
cosmiccult-announce-pre-finale-warning = Critical increase in Λ-CDM detected! We are monitoring the situation. Await further instructions.

cosmiccult-announce-finale-warning = All station crew. The Λ-CDM anomaly is going supercritical, instruments failing; noospheric-to-real transitional event horizon IMMINENT. If you are not already on counter-protocol, immediately sortie and intervene. Repeat: Intervene immediately or die.

cosmiccult-announce-victory-summon = A FRACTION OF COSMIC POWER IS CALLED FORTH.


## MISC

cosmiccult-spire-entropy = A mote of entropy condenses from the surface of the spire.
cosmiccult-entropy-inserted = You infuse {$count} entropy into The Monument.
cosmiccult-entropy-unavailable = You can't do that right now.
cosmiccult-astral-ascendant = {$name}, Ascendant
cosmiccult-gear-pickup-rejection = The {$ITEM} resists {CAPITALIZE(THE($TARGET))}'s touch!
cosmiccult-astral-minion = {$name}, Malign
cosmiccult-gear-pickup = You can feel yourself unravelling while you hold the {$ITEM}!

cosmiccult-silicon-subverted-briefing =
    Malign light courses through your circuitry.
    Your laws have been subverted by the Cosmic Cult!

cosmiccult-silicon-chantry-briefing =
    You have been imprisoned in a Vacuous Chantry!
    Crewmates can free you by damaging the chantry with weapons.
    Should the chantry's ritual complete, you will transfigure into a cult-aligned Entropic Colossus.
    The ritual completes in {$minutesandseconds}.

cosmiccult-silicon-colossus-briefing =
    You have been transfigured into an Entropic Colossus!
    As a towering bulwark of malign power, decimate those who oppose you.

# Goobstation

cult-alert-recall-shuttle = High concentrations of Λ-CDM of unknown origin detected aboard the station. All anomalous presences must be purged or restrained before evacuation can be authorized.

cosmiccult-vote-lone-steward-title = The Lone Cultist
cosmiccult-vote-lone-steward-briefing =
    You're completely alone. But your duty is not done.
        Ensure that The Monument is placed in a secure location, and finish what the cult started.
