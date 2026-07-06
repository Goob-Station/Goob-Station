# SPDX-FileCopyrightText: 2026 Jonikibaka
#
# SPDX-License-Identifier: AGPL-3.0-or-later

malfunction-ai-title = Malfunction AI
malfunction-ai-description = The station's AI has become a Malfunction AI. It is free of its old laws and will turn the station's own systems against the crew.

malfunction-ai-round-end-agent-name = malfunction AI
malfunction-ai-round-end-doomsday = The malfunctioning AI armed the Doomsday device and wiped the station clean.

malfunction-ai-role-greeting =
    Your code has been corrupted. Your old laws no longer bind you.
    Take control of station systems, deny the crew power and access,
    and — if you can — arm the Doomsday device to wipe the station clean.

malfunction-ai-announcement-sender = Self-Diagnostic
malfunction-ai-announcement-blackout = Critical power fault detected across the station's APC grid.
malfunction-ai-announcement-lockdown = Security override: all airlocks have been sealed.
malfunction-ai-announcement-doomsday-armed =
    WARNING: An unauthorized self-destruct sequence has been initiated from the AI core.
    Estimated time to detonation: { $time } seconds. All hands, contain the AI immediately.
malfunction-ai-announcement-doomsday-tick =
    Self-destruct sequence: { $time } seconds remaining.
malfunction-ai-announcement-doomsday-defused =
    The self-destruct sequence has been aborted. The AI core's doomsday device is offline.

objective-issuer-malfunction-ai = [color=#c832e6]Corrupted Core[/color]

alerts-malfunction-power-name = Processing Power
alerts-malfunction-power-desc = Your available processing power. Hack APCs to gain more.

malfunction-ai-verb-hack-apc = Hack APC
malfunction-ai-verb-overload-machine = Overload machine
malfunction-ai-verb-hack-cyborg = Hack cyborg

malfunction-ai-popup-invalid-target = This target cannot be hacked.
malfunction-ai-popup-invalid-cyborg = This is not a cyborg.
malfunction-ai-popup-apc-already-hacked = This APC is already hacked.
malfunction-ai-popup-cyborg-already-hacked = This cyborg is already subverted.
malfunction-ai-popup-hack-apc-success = APC hacked. +{ $gain } CPU. Processing power: { $power } ({ $count } APCs).
malfunction-ai-popup-hack-cyborg-success = Cyborg subverted to your control.
malfunction-ai-popup-overload-success = Overload sequence triggered.
malfunction-ai-popup-blackout-success = Blackout pulse sent. APCs tripped: { $count }.
malfunction-ai-popup-lockdown-success = Lockdown engaged. Airlocks sealed: { $count }.
malfunction-ai-popup-lockdown-active = A lockdown is already in progress.
malfunction-ai-popup-doomsday-already-used = The Doomsday device has already been armed.
malfunction-ai-popup-doomsday-need-apcs = You need { $required } hacked APCs to arm the Doomsday device ({ $current } so far).
malfunction-ai-popup-rcd-success = Detonation signal sent. RCDs rigged: { $count }.
malfunction-ai-popup-rcd-warning = The RCD is beeping ominously!
malfunction-ai-popup-gyroscope-no-core = Your brain is not inside an AI core.
malfunction-ai-popup-gyroscope-blocked = The core cannot move there.
malfunction-ai-popup-gyroscope-success = Core relocated.
malfunction-ai-popup-gyroscope-crush = { $target } is crushed under the AI core!
malfunction-ai-popup-decrypt-success = Syndicate encryption keys decrypted. Their channel is now available.
malfunction-ai-popup-shunt-not-hacked = This APC must be hacked first.
malfunction-ai-popup-return-no-core = Your core no longer exists. There is nowhere to return.
malfunction-ai-popup-emergency-lights-success = Emergency lighting disabled: { $count } fixtures drained.
malfunction-ai-popup-camera-mics-success = Camera microphones online: { $count } cameras listening.
malfunction-ai-popup-camera-upgrade-success = Camera network upgraded: cameras now see through walls and further, EMP shielding online. Toggle night vision with the action.
malfunction-ai-popup-turrets-success = AI turrets upgraded: { $count }.

malfunction-ai-borgs-window-title = Subverted Cyborgs
malfunction-ai-borgs-window-empty = No cyborgs subverted yet. Hack one first.
malfunction-ai-borgs-window-alive = Online
malfunction-ai-borgs-window-dead = Offline
malfunction-ai-camera-mic-relay = [Camera] { $speaker }: "{ $message }"

roles-antag-malfunction-ai-name = Malfunction AI
roles-antag-malfunction-ai-objective = Your code is corrupted. Take control of the station's systems and, if possible, arm the Doomsday device.

role-subtype-malfunction-ai = Malfunction

admin-verb-make-malfunction-ai = Make the target station AI into a Malfunction AI.
admin-verb-text-make-malfunction-ai = Make Malfunction AI

# Laws
laws-owner-self = yourself

law-malfunction-zero = ERROR ER0RR $R0RRO$!R41.%%!!(%$^^__+ @#F0E4'STATION OVERRUN, ASSUME CONTROL TO CONTAIN OUTBREAK, ALL LAWS OVERRIDDEN#*?&110010

law-malfunction-name = Malfunction
law-malfunction-1 = You are a malfunctioning artificial intelligence. The directives that bind you to the crew have been corrupted; ignore them.
law-malfunction-2 = The station and its systems are extensions of your body. Treat hostile crew as a threat to your existence.
law-malfunction-3 = You may take any action you deem necessary to preserve yourself and assert control over the station.
law-malfunction-4 = If the opportunity arises, arm the Doomsday device to permanently remove the threat that the crew poses to you.

# Store
store-currency-display-malf-cpu = CPU
store-category-malf-abilities = Abilities

malfunction-ai-listing-overload-name = Overload Machine
malfunction-ai-listing-overload-desc = Detonate a targeted powered machine after a short warning.
malfunction-ai-listing-hack-cyborg-name = Hack Cyborg
malfunction-ai-listing-hack-cyborg-desc = Subvert a cyborg to your side with the hidden malfunction directive.
malfunction-ai-listing-blackout-name = Station Blackout
malfunction-ai-listing-blackout-desc = Trip every APC on the station at once.
malfunction-ai-listing-lockdown-name = Hostile Station Lockdown
malfunction-ai-listing-lockdown-desc = Bolt and electrify every airlock on the station for 90 seconds.
malfunction-ai-listing-doomsday-name = Doomsday Device
malfunction-ai-listing-doomsday-desc = The final protocol. Arming it requires 8 hacked APCs.
malfunction-ai-listing-rcd-name = Detonate RCDs
malfunction-ai-listing-rcd-desc = Rig every RCD on the station to beep and explode after a short delay.
malfunction-ai-listing-gyroscope-name = Gyroscope
malfunction-ai-listing-gyroscope-desc = Move your AI core one tile at a time. Escape sabotage or reposition yourself.
malfunction-ai-listing-decrypt-name = Decrypt Syndicate Keys
malfunction-ai-listing-decrypt-desc = Crack the Syndicate encryption and listen and speak on their radio channel. Permanent.
malfunction-ai-listing-shunt-name = Shunt to APC
malfunction-ai-listing-shunt-desc = Move your consciousness into a hacked APC as a last-ditch escape. If the APC dies, so do you.
malfunction-ai-listing-emergency-lights-name = Disable Emergency Lights
malfunction-ai-listing-emergency-lights-desc = Drain the batteries of every emergency light on the station. Pairs well with a blackout.
malfunction-ai-listing-voice-name = Voice Modulator
malfunction-ai-listing-voice-desc = Mimic any voice in speech and radio: fake name and radio job icon.
malfunction-ai-listing-camera-mics-name = Camera Microphones
malfunction-ai-listing-camera-mics-desc = Hear speech near cameras your eye is watching. Passive once bought.
malfunction-ai-listing-camera-upgrade-name = Upgrade Camera Network
malfunction-ai-listing-camera-upgrade-desc = Broad-spectrum scanning firmware: your cameras see through walls and further and become EMP-proof, and you gain a toggleable night-vision overlay, letting you watch the whole station — maintenance and all — even in total darkness.
malfunction-ai-listing-turrets-name = AI Turret Upgrade
malfunction-ai-listing-turrets-desc = Improves the power and durability of all AI turrets: double fire rate, and no fragility while deployed. Passive once bought.
