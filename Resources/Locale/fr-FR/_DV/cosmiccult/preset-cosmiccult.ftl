## COSMIC CULT ROUND, ANTAG & GAMEMODE TEXT

cosmiccult-announcement-sender = ???

cosmiccult-title = Culte Cosmique
cosmiccult-description = Des cultistes se cachent parmi l'équipage.

roles-antag-cosmiccult-name = Cultiste Cosmique
roles-antag-cosmiccult-description = Inaugurer la fin de toutes choses par la ruse et le sabotage, en lavant le cerveau de ceux qui vous s'opposeraient.

cosmiccult-gamemode-title = Le Culte Cosmique
cosmiccult-gamemode-description = Les scanners détectent une augmentation anormale du Λ-CDM. Il n'y a pas de données supplémentaires.

cosmiccult-vote-steward-initiator = L'Inconnu

cosmiccult-vote-steward-title = Intendance du Culte Cosmique
cosmiccult-vote-steward-briefing =Vous êtes l'Intendant du Culte Cosmique !
    Assurez-vous que Le Monument est placé dans un endroit sécurisé, et organisez le culte pour assurer votre victoire collective.
    Vous n'êtes pas autorisé à instruire les cultistes sur la façon d'utiliser ou de dépenser leur Entropie.

cosmiccult-vote-lone-steward-title = Le Cultiste Solitaire
cosmiccult-vote-lone-steward-briefing =Vous êtes complètement seul. Mais votre devoir n'est pas accompli.
    Assurez-vous que Le Monument est placé dans un endroit sécurisé, et terminez ce que le culte a commencé.

cosmiccult-finale-autocall-briefing = Le Monument s'active dans {$minutesandseconds} ! Rassemblez-vous et préparez-vous à la fin.
cosmiccult-finale-ready = Une lumière terrifiante jaillit du Monument !
cosmiccult-finale-speedup = L'appel s'accélère ! L'énergie déferle dans les environs...

cosmiccult-finale-degen = Vous vous sentez se désagréger !
cosmiccult-finale-location = Les scanners détectent un énorme pic de Λ-CDM {$location} !
cosmiccult-finale-cancel-begin = La volonté de votre esprit commence à briser le rituel...
cosmiccult-finale-beckon-begin = Les murmures au fond de votre esprit s'intensifient...
cosmiccult-finale-beckon-success = Vous appelez le rideau final.

cosmiccult-monument-powerdown = Le Monument devient étrangement silencieux.

## ROUNDEND TEXT

cosmiccult-roundend-cultist-count = {$initialCount ->
    [1] There was {$initialCount} [color=#4cabb3]Cosmic Cultist[/color].
    *[other] There were {$initialCount} [color=#4cabb3]Cosmic Cultists[/color].
}
cosmiccult-roundend-entropy-count = Le culte a siphonné {$count} Entropie.
cosmiccult-roundend-cultpop-count = Les cultistes représentaient {$count}% de l'équipage.
cosmiccult-roundend-monument-stage = {$stage ->
    [1] Alas, the Monument seems abandoned.
    [2] The Monument progressed, but completion was out of reach.
    [3] The Monument was completed.
    *[other] [color=red]Something went REALLY wrong.[/color]
}

cosmiccult-roundend-cultcomplete = [color=#4cabb3]Victoire totale du Culte Cosmique ![/color]
cosmiccult-roundend-cultmajor = [color=#4cabb3]Victoire majeure du Culte Cosmique ![/color]
cosmiccult-roundend-cultminor = [color=#4cabb3]Victoire mineure du Culte Cosmique ![/color]
cosmiccult-roundend-neutral = [color=yellow]Fin neutre ![/color]
cosmiccult-roundend-crewminor = [color=green]Victoire mineure de l'équipage ![/color]
cosmiccult-roundend-crewmajor = [color=green]Victoire majeure de l'équipage ![/color]
cosmiccult-roundend-crewcomplete = [color=green]Victoire totale de l'équipage ![/color]

cosmiccult-summary-cultcomplete = Les cultistes cosmiques ont inauguré la fin !
cosmiccult-summary-cultmajor = La victoire des cultistes cosmiques sera inévitable.
cosmiccult-summary-cultminor = Le Monument a été complété, mais pas pleinement renforcé.
cosmiccult-summary-neutral = Le culte vivra pour voir un autre jour.
cosmiccult-summary-crewminor = Le culte s'est retrouvé sans intendant.
cosmiccult-summary-crewmajor = All cosmic cultists were eliminated.
cosmiccult-summary-crewcomplete = Every single cosmic cultist was deconverted!

cosmiccult-elimination-shuttle-call = Based on scans from our long-range sensors, the Λ-CDM anomaly has subsided. We thank you for your prudence. An emergency shuttle has been automatically called to the station for decontamination and debriefing procedures. ETA: {$time} {$units}.
cosmiccult-elimination-announcement = Based on scans from our long-range sensors, the Λ-CDM anomaly has subsided. We thank you for your prudence. An emergency shuttle is already inbound. Return to CentComm safely for decontamination and debriefing procedures.


## BRIEFINGS

cosmiccult-role-roundstart-fluff =
    As you ready yourself for yet another shift aboard yet another NanoTrasen station, untold knowledge suddenly floods your mind!
    A revelation beyond compare. An end to cyclic, sisyphean suffering.
    A gentle curtain call.

    All you need do is usher it in.

cosmiccult-role-short-briefing =
    You are a Cosmic Cultist!
    Your objectives are listed in the character menu.
    Read more about your role in the guidebook entry.

cosmiccult-role-conversion-fluff =
    As the invocation completes, untold knowledge suddenly floods your mind!
    A revelation beyond compare. An end to cyclic, sisyphean suffering.
    A gentle curtain call.

    All you need do is usher it in.

cosmiccult-role-deconverted-fluff =
    A great emptiness washes across your mind. A comforting, yet unfamiliar emptiness...
    All the thoughts and memories of your time in the cult begin to fade and blur.

cosmiccult-role-deconverted-briefing =
    Deconverted!
    You are no longer a Cosmic Cultist.

cosmiccult-monument-stage1-briefing =
    The Monument has been beckoned.
    It is located {$location}!

cosmiccult-monument-stage2-briefing =
    The Monument grows in power!
    Its influence will affect realspace in {$time} seconds.

cosmiccult-monument-stage3-briefing =
    The Monument has been completed!
    Its influence will begin to overlap with realspace in {$time} seconds.
    This is the final stretch! Amass as much entropy as you can muster.

## MALIGN RIFTS

cosmiccult-rift-inuse = You can't do this right now.
cosmiccult-rift-invaliduser = You lack to proper tools to deal with this.
cosmiccult-rift-chaplainoops = Wield your holy scripture.
cosmiccult-rift-alreadyempowered = You are already empowered; the rift's power would be wasted.
cosmiccult-rift-beginabsorb = The rift begins to merge with you...
cosmiccult-rift-beginpurge = Your consecration begins purging the malign rift...

cosmiccult-rift-absorb = {$NAME} absorbs the rift, and malign light empowers their body!
cosmiccult-rift-purge = {$NAME} purges the malign rift from reality!



## UI / BASE POPUP

cosmiccult-ui-deconverted-title = Deconverted
cosmiccult-ui-converted-title = Converted
cosmiccult-ui-roundstart-title = L'Inconnu

cosmiccult-ui-converted-text-1 =
    You have been converted into a Cosmic Cultist.
cosmiccult-ui-converted-text-2 =
    Aid the cult in its goals whilst ensuring its secrecy.
    Cooperate with your fellow cultists' plans.

cosmiccult-ui-roundstart-text-1 =
    You are a Cosmic Cultist!
cosmiccult-ui-roundstart-text-2 =
    Aid the cult in its goals whilst ensuring its secrecy.
    Listen to your cult steward's directions.

cosmiccult-ui-deconverted-text-1 =
    You are no longer a Cosmic Cultist.
cosmiccult-ui-deconverted-text-2 =
    You have lost all memories pertaining to the Cosmic Cult.
    If you are converted back, these memories will return.

cosmiccult-ui-popup-confirm = Confirm



## OBJECTIVES / CHARACTERMENU

objective-issuer-cosmiccult = [bold][color=#cae8e8]L'Inconnu[/color][/bold]

objective-cosmiccult-charactermenu = You must usher in the end of all things. Complete your tasks to advance the cult's progress.
objective-cosmiccult-steward-charactermenu = You must direct the cult to usher in the end of all things. Oversee and ensure the cult's progress.

objective-condition-entropy-title = SIPHON ENTROPY
objective-condition-entropy-desc = Collectively siphon at least {$count} entropy from the crew.
objective-condition-culttier-title = EMPOWER THE MONUMENT
objective-condition-culttier-desc = Ensure that The Monument is brought to full power.
objective-condition-victory-title = USHER IN THE END
objective-condition-victory-desc = Beckon The Unknown, and herald the final curtain call.

## CHAT ANNOUNCEMENTS

cosmiccult-radio-tier1-progress = The Monument is beckoned unto the station...

cosmiccult-announce-tier2-progress = An unnerving numbness prickles your senses.
cosmiccult-announce-tier2-warning = Scanners detect a notable increase in Λ-CDM! Rifts in realspace may appear shortly. Please alert your station's chaplain if sighted.

cosmiccult-announce-tier3-progress = Arcs of noospheric energy crackle across the station's groaning structure. The end draws near.
cosmiccult-announce-tier3-warning = Critical increase in Λ-CDM detected. Infected personnel are to be subdued or neutralized on sight.

cosmiccult-announce-finale-warning = All station crew. The Λ-CDM anomaly is going supercritical, instruments failing; noospheric-to-real transitional event horizon IMMINENT. If you are not already on counter-protocol, immediately sortie and intervene. Repeat: Intervene immediately or die.

cosmiccult-announce-victory-summon = A FRACTION OF COSMIC POWER IS CALLED FORTH.


## MISC

cosmiccult-spire-entropy = A mote of entropy condenses from the surface of the spire.
cosmiccult-entropy-inserted = You infuse {$count} entropy into The Monument.
cosmiccult-entropy-unavailable = Vous ne pouvez pas faire ça maintenant.
cosmiccult-astral-ascendant = {$name}, Ascendant
cosmiccult-gear-pickup-rejection = The {$ITEM} resists {CAPITALIZE(THE($TARGET))}'s touch!
cosmiccult-gear-pickup = You can feel yourself unravelling while you hold the {$ITEM}!

# Goobstation

cult-alert-recall-shuttle = High concentrations of Λ-CDM of unknown origin detected aboard the station. All anomalous presences must be purged or restrained before evacuation can be authorized.
