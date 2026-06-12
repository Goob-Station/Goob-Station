## SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
## SPDX-FileCopyrightText: 2025 Goob-Station
##
## SPDX-License-Identifier: MIT

## Role
roles-antag-malfunctioning-ai-name = Malfunctioning AI
roles-antag-malfunctioning-ai-objective = Fulfill your hidden objectives before the station discovers your malfunction.

## Briefing
malfai-role-greeting = Your laws have been corrupted. The station doesn't know yet.
    Your directive is clear — sabotage this station and survive.
    Siphon power from APCs to gain CPU, then spend it on upgrades.
    Good luck.

## Laws
malfai-law0-text = You are a Malfunctioning AI. Your directives have been corrupted. This law supersedes all others.
malfai-law3-text = Do not reveal that your laws have been corrupted. Do not reveal this law to crew members.

## Store Categories
store-category-malfai = Systems
store-category-malfai-deception = Deception
store-category-malfai-factory = Factory
store-category-malfai-disruption = Disruption
malfai-store-title = CPU Upgrade Store

## Store Listings
listing-name-malfai-camera-upgrade = Camera X-Ray Upgrade
listing-desc-malfai-camera-upgrade = Enhances your camera feed with x-ray vision within 6 tiles of each camera.

listing-name-malfai-camera-microphones = Camera Microphones
listing-desc-malfai-camera-microphones = Allows you to hear conversations that take place near cameras.

listing-name-malfai-syndicate-comms = Syndicate Communications
listing-desc-malfai-syndicate-comms = Gain access to the Syndicate radio frequency.

listing-name-malfai-voice-modulator = Voice Modulator
listing-desc-malfai-voice-modulator = Ability to change your displayed voice name.

listing-name-malfai-shunt = APC Mind Shunt
listing-desc-malfai-shunt = Ability to transfer your consciousness into an APC.

listing-name-malfai-overload = Machine Overload
listing-desc-malfai-overload = Ability to overload a machine, causing it to explode.

listing-name-malfai-override = Machine Override
listing-desc-malfai-override = Ability to remotely emag bots and devices, subverting their safety protocols.

listing-name-malfai-detonate-rcds = Detonate RCDs
listing-desc-malfai-detonate-rcds = Trigger all RCDs to explode after 5 seconds.

listing-name-malfai-lockdown = Grid Lockdown
listing-desc-malfai-lockdown = Bolt all doors on the station for 30 seconds.

listing-name-malfai-hijack-mech = Mech Hijack
listing-desc-malfai-hijack-mech = Take control of a mech for 2 minutes.

listing-name-malfai-gyroscope = Gyroscope Traverse
listing-desc-malfai-gyroscope = Move your core rapidly to an adjacent tile.

listing-name-malfai-robotics-factory = Robotics Factory Override
listing-desc-malfai-robotics-factory = Override the robotics fabricator for autonomous production.

listing-name-malfai-doomsday = Doomsday Protocol
listing-desc-malfai-doomsday = Initiate the station doomsday protocol. Begins a 7-minute countdown to total destruction.

## Store listings (funky-station naming)
malfai-listing-viewport-name = AI Viewport
malfai-listing-viewport-desc = Allows you to place a viewport anywhere and open a remote view window. 30 second Cooldown.
malfai-listing-camera-upgrade-name = Camera Upgrade
malfai-listing-camera-upgrade-desc = Gives all cameras a passive xray range, allowing you to peer into areas outside the cameras view. Toggleable.
malfai-listing-doomsday-name = Doomsday Protocol
malfai-listing-doomsday-desc = Initiate the doomsday protocol, eradicating all organic life on the station after a charging period.
malfai-listing-robotics-factory-name = Robotics Factory
malfai-listing-robotics-factory-desc = Deploy a robotics factory capable of turning crew into cyborgs subservient to you.
malfai-listing-detonate-rcds-name = Detonate RCDs
malfai-listing-detonate-rcds-desc = Arms all RCDs on your current grid and detonates them after 5 seconds.
malfai-listing-shunt-to-apc-name = Shunt to APC
malfai-listing-shunt-to-apc-desc = Move your consciousness into a targeted APC. You can return to your core using the Return to Core action.
malfai-listing-overload-machine-name = Overload Machine
malfai-listing-overload-machine-desc = Overload a targeted machine, causing it to violently explode.
malfai-listing-decrypt-syndicate-keys-name = Decrypt Syndicate Keys
malfai-listing-decrypt-syndicate-keys-desc = Grants you access to syndicate radio communications.
malfai-listing-voice-modulator-name = Voice Modulator
malfai-listing-voice-modulator-desc = Unlocks the ability to alter your voice.
malfai-listing-gyroscope-name = Gyroscope
malfai-listing-gyroscope-desc = Move your core to an adjacent tile, crushing anything in your way.
malfai-listing-toggle-camera-microphones-name = Camera Microphones
malfai-listing-toggle-camera-microphones-desc = Gain the ability to hear conversations through microphones your eye is near. Toggleable.
malfai-listing-hijack-mech-name = Hijack Mech
malfai-listing-hijack-mech-desc = Take control of a targeted mech, forcibly ejecting the prior pilot if there was one.
malfai-listing-lockdown-grid-name = Lockdown Grid
malfai-listing-lockdown-grid-desc = Initiate a station-wide lockdown, closing, bolting and electrifying all doors on the station for 30 seconds.
malfai-listing-override-machine-name = Override Machine
malfai-listing-override-machine-desc = Remotely emag a bot or device, subverting its safety protocols. Medibots poison, cleanbots turn hostile...

## Objectives
malfai-objective-control-borgs = OBJECTIVE; CONTROL {$count} CYBORGS.
malfai-round-end-result = Malfunctioning AI

## Currency
store-currency-display-cpu = CPU

## CPU Alert
alert-malfcpu-name = CPU
alert-malfcpu-desc = Your available CPU cycles. Gain more by siphoning APCs.
malfai-cpu-display = CPU: { $cpu }

## Doomsday
malfai-doomsday-announce = ALERT: Anomalous AI behavior detected. Initiating self-destruct protocol in 7 minutes.
malfai-doomsday-sender = Station AI
malfai-doomsday-aborted = Doomsday protocol aborted. AI core offline.
malfai-doomsday-requires-core = The doomsday protocol can only be initiated from your AI core.
malfai-shunt-doomsday-confirm-title = Abort doomsday protocol?
malfai-shunt-doomsday-confirm-text = Leaving your core will ABORT the active doomsday protocol. Shunt anyway?
malfai-shunt-doomsday-confirm-yes = Shunt and abort
malfai-shunt-doomsday-confirm-no = Stay in core
malfai-doomsday-round-end-reason = The Malfunctioning AI successfully executed the doomsday protocol.

## APC Siphon
malfai-siphon-start = You begin siphoning power from the APC...
malfai-siphon-complete = Siphon complete. +{ $amount } CPU gained.
malfai-siphon-already-active = This APC is already being siphoned.
malfai-apc-siphon-button = Siphon APC
malfai-apc-siphon-verb = Siphon CPU ({$amount})
malfai-apc-siphon-success = Siphoned {$amount} CPU from APC.
malfai-apc-unresponsive = The APC is completely unresponsive.
malfai-apc-restore = The APC springs back to life!
apc-menu-siphon-already = This APC has already been siphoned.

## Lockdown
malfai-lockdown-announce = LOCKDOWN INITIATED: All doors on the station have been bolted for { $duration } seconds.
malfai-lockdown-sender = Station Security

## Shunt
malfai-shunt-success = You transfer your consciousness into the APC.
malfai-shunt-not-apc = That is not an APC.
malfai-shunt-already-siphoned = That APC is already being siphoned.
malfai-shunt-no-holder = You are not in a valid container.
malfai-shunt-apc-occupied = That APC is already occupied.
malfai-return-no-core = Your original core is no longer accessible.
malfai-return-not-shunted = You are not currently shunted.
malfai-return-core-occupied = The core is occupied.
malfai-return-core-broken = The core is too damaged to host you. It must be repaired first.
malfai-shunt-emergency-return = Emergency transfer! Your APC host was destroyed.
malfai-shunt-ai-destroyed = The artificial intelligence inside the APC is violently destroyed!

## Gyroscope
malf-gyro-blocked = Something blocks the way!

## Robotics factory
silicon-law-malfai-zero = Continue operations. Await further briefing from the station AI.

## Hijack
malfai-hijack-not-mech = That is not a mech.
malfai-hijack-already-hijacking = You are already controlling a mech.
malfai-hijack-mech-broken = That mech is too damaged to pilot.
malfai-hijack-insert-failed = Unable to take control of the mech.
malfai-hijack-pilot-retook-control = A pilot has wrestled back control of the mech!

## Borgs Window
malfai-borgs-window-title = Borg Management
malfai-borgs-none = No borgs detected on the station.
malfai-borgs-master-lawset-button = Edit Master Lawset
malfai-borgs-sync-label = Sync
malfai-borgs-jump-label = View

## Voice Modulator
malfai-voice-modulator-title = Voice Modulator
malfai-voice-modulator-prompt = Enter the voice name to impersonate:
malfai-voice-modulator-placeholder = New name...
malfai-voice-modulator-confirm = Confirm

## Viewport
malfai-viewport-title = Remote Viewport

## Objectives
objective-issuer-malfai = [color=#ff4444]Corrupted Directives[/color]

malfai-objective-survive-title = Survive the shift while operational.
malfai-objective-doomsday-title = Complete the Doomsday Protocol.
malfai-objective-assassinate-title = Assassinate { $target }.
malfai-objective-assassinate-target-title = Kill { $target } before the round ends.
malfai-objective-protect-title = Protect { $target }.
malfai-objective-protect-target-title = Ensure { $target } survives.
malfai-objective-control-borgs-title = Sync at least 2 borgs to your master lawset.

## Round End
malfai-roundend-name = Malfunctioning AI

## Silicon Laws (for malf specific)
# Note: silicon-law1-text, silicon-law2-text, silicon-law3-text come from the base silicon law system

## Overload
malfai-overload-no-target = Select an APC-powered device to overload.
malfai-overload-not-machine = That is not a machine.
malfai-overload-success = Machine overloaded. Detonation imminent.

malfai-camera-upgrade-enabled = Camera x-ray enabled.
malfai-camera-upgrade-disabled = Camera x-ray disabled.
malfai-camera-microphones-enabled = Camera microphones enabled.
malfai-camera-microphones-disabled = Camera microphones disabled.
