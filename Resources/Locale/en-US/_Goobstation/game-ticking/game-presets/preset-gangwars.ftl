roles-antag-gang-leader-name = Gang Leader
roles-antag-gang-leader-objective = Prove you still have what it takes. Claim territory, rack up points, and crush every rival gang standing in your way.

roles-antag-gang-name = Gang Member
roles-antag-gang-objective = Back your leader, hold your turf, and help your gang dominate the station before the rivals do.

gang-leader-role-greeting =
    You are a Gang Leader.
    Your bosses are not happy. Your recent failures have put you on thin ice, and patience is running out.
    This is your last chance to prove your worth. fail again and there won't be another.
    Hit the station hard. Recruit whoever you can, claim every corner you're able to, and make it clear who runs this place.
    Don't get comfortable, though. Word is, rival gangs are already moving in with the same idea.

gang-leader-action-summon-locker-name = Summon Gang Locker
gang-leader-action-summon-locker-desc = Summon your gang locker to your location. If already placed, relocate it instead.

gang-member-action-toggle-overlay-name = Toggle Territory Overlay
gang-member-action-toggle-overlay-desc = Show or hide the territory overlay.

gang-leader-locker-summoned = Your locker is on its way!

gang-leader-member-accepted = Your gang invitation was accepted.
gang-leader-member-denied = Your gang invitation was denied.
gang-invite-target-already-member = That person is already in a gang.
gang-invite-sent = {$name} has been invited! Awaiting their response.
gang-invite-already-outgoing = You already have an outgoing invitation!

gang-invite-window-title = Gang Invitation
gang-invite-window-prompt = {$leader} wants you to join the [color=#{$color}]{$gangName}[/color]!
gang-invite-window-timer = Time remaining: {$seconds}s
gang-invite-window-accept = Accept
gang-invite-window-deny = Deny

gang-creator-title = Create Your Gang
gang-creator-color-label = Pick a color to represent your gang:
gang-creator-confirm = Confirm
gang-creator-too-dark = This color is too transparent!
gang-creator-name-label = Pick a name for your gang:
gang-creator-name-placeholder = e.g. The Cobras
gang-creator-name-too-short = Name must be at least {$min} characters.
gang-creator-name-too-long = Name must be at most {$max} characters.
gang-creator-name-already-taken = That gang name is already taken!

gang-territory-too-close = You're too close to an existing territory!
gang-locker-too-close-to-crate = You're too close to an existing gang crate!
gang-color-already-taken = This color was already picked!

gang-spray-can-verb-paint = Spray Paint
gang-spray-can-no-gang = You don't belong to a gang!
gang-spray-can-empty = The spray can is empty.
gang-spray-can-clothing-painted = You spray your gang's color onto it.
gang-spray-can-points-earned = +{$points} Gang Points!
gang-spray-can-charges-remaining = {$charges ->
    [one] It has [color=yellow]{$charges}[/color] use left.
    *[other] It has [color=yellow]{$charges}[/color] uses left.
}

gang-store-title = Gang Store
gang-store-in-use = Someone else is already using the locker!

store-currency-display-gangpoint = Gang Points

gang-store-category-starter-equipment = Starter Equipment
gang-store-category-consumables = Consumables
gang-store-category-weapons = Weapons
gang-store-category-equipment = Equipment

gang-store-listing-beginner-bundle-name = Beginner bundle
gang-store-listing-beginner-bundle-desc = Contains everything an aspiring gangster needs to get started. Highly recommended.

gang-store-listing-medkit-name = Medkit
gang-store-listing-medkit-desc = A standard medical kit. Patch yourself up before getting back in the fight.

gang-store-listing-omnizine-name = Omnizine
gang-store-listing-omnizine-desc = A syringe of omnizine. Heals most damage types quickly.

gang-store-listing-quickhack-name = Quickhack
gang-store-listing-quickhack-desc = A covert device that fakes AI "open" signals to open unbolted doors. Good for 5 uses.

gang-store-listing-knife-name = Combat Knife
gang-store-listing-knife-desc = A reliable close-quarters blade. Quick and deadly.

gang-store-listing-machete-name = Gangsters machete
gang-store-listing-machete-desc = Quite effective at cutting off limbs.

gang-store-listing-throwingknife-name = Throwing Knife
gang-store-listing-throwingknife-desc = A balanced knife made for throwing. Good for picking off enemies at range.

gang-store-listing-baseballbat-name = Baseball Bat
gang-store-listing-baseballbat-desc = A classic street weapon. Swing for the face.

gang-store-listing-propaint-name = Spray Paint
gang-store-listing-propaint-desc = A can of gang spray paint. Use it to claim territory for your crew.

gang-store-listing-clothes-bundle-name = Gang Clothes Bundle
gang-store-listing-clothes-bundle-desc = A spare set of gang threads: shoes, jumpsuit, jacket, and a flashy top hat. Rep your colors to earn territory perks.

gangwar-crate-drop-announcement = Word just came in a gang crate dropped near {$location}. It's got some extra valuable goodies. Every gang on the station wants it. Go get it.
gangwar-duffelbag-drop-announcement = Word just came in a duffel bag hit the floor near {$location}. Every gang on the station wants it. Go get it.
gangwar-tipoff-drop-announcement = Someone paid for a tip off. A duffel bag just dropped near {$location}. Word's already out. move fast.
gangwar-duffelbag-drop-unknown-location = an unknown location

gang-store-listing-light-armor-vest-name = Light Armor Vest
gang-store-listing-light-armor-vest-desc = A standard Type I armored vest. Not flashy, but it'll keep you breathing longer in a fight.

gang-crate-no-locker-nearby = You need to be near a gang locker to open this!
gang-crate-reward-popup = Your gang secured the crate! +{$points} gang points!

gang-duffel-bag-untrapped = You cut your hand on the bag's trap!
gang-duffel-bag-untrapping = You begin disarming the trap..
alerts-gang-trapped-name = Trapped!
alerts-gang-trapped-desc = The duffelbag was trapped! You're now slowed.

gang-store-listing-encryption-key-name = Gang Encryption Key
gang-store-listing-encryption-key-desc = An encryption key tuned to a special frequency used by gangs.

chat-radio-gang = Gang Chat

alerts-gang-bonus-name = Territory bonus
alerts-gang-bonus-desc = Shows if your territory bonus is currently active.

gang-locker-examine-header = [color=#D2691E][bold]Gang Leaderboard[/bold][/color]
gang-locker-examine-gang-entry = {$rank}. [color=#{$color}][bold]{$name}[/bold][/color] - {$score} pts
gang-locker-examine-update-timer = [italic]Updating in {$seconds}s[/italic]
gang-structure-revealed = The gang structure reveals itself.

gang-member-role-greeting =
    You've been brought into the fold.
    Your leader has vouched for you so don't make them regret it.
    Expand your turf, watch your gang's back, and help take control of this station.
    Rival gangs are already out there. Don't give them an inch.

objective-issuer-gang = Boss

gang-objective-recruit-title = Recruit {$count ->
    [one] {$count} member
    *[other] {$count} members
} to your gang
gang-objective-recruit-description = Use your offer action to bring loyal soldiers into the fold.

gang-objective-first-place-title = Be in first place
gang-objective-first-place-description = Ensure your gang is in first place by the end of the round.

gang-objective-earn-points-title = Earn {$count} gang points
gang-objective-earn-points-description = Help your gang accumulate points.

gang-store-listing-tipoff-name = Tip Off
gang-store-listing-tipoff-desc = Schedule an early duffel bag drop packed with goodies. A random civilian will be tipped off about the drop location via NanoChat.

gangwar-tipoff-nanochat-sender = Anonymous
gangwar-tipoff-nanochat-message = Don't ask how I know this, but there's a duffel bag stashed near {$location}. Grab it before the gangs do. you've got {$seconds} seconds before word gets out.

quickhack-no-charges = No charges remaining.
quickhack-failed = Failed to send signal.
