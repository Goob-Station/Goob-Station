# Admin objectives panel

# Command
cmd-objectivespanel-desc = Opens the objectives panel to view, add and remove the objectives of a player.
cmd-objectivespanel-help = Usage: objectivespanel <username>
cmd-objectivespanel-player-completion = <username>
cmd-objectivespanel-player-not-found = Player not found.
cmd-objectivespanel-mind-not-found = That player has no mind.

# Verb
admin-verb-text-edit-objectives = Edit objectives
admin-verb-edit-objectives = Open the objectives panel to view, add and remove this mind's objectives.

# Window
admin-objectives-title = Objectives
admin-objectives-refresh = Refresh
admin-objectives-player = Objectives of {$player}
admin-objectives-no-mind = {$player} has no mind!
admin-objectives-tab-current = Current
admin-objectives-tab-prototype = Add objective
admin-objectives-tab-custom = Custom
admin-objectives-tab-steal = Steal
admin-objectives-none = This mind has no objectives.
admin-objectives-invalid = Broken objective ({$proto})
admin-objectives-remove = Remove
admin-objectives-add = Add
admin-objectives-issuer = Issuer
admin-objectives-unknown-name = Unknown

# Current tab
admin-objectives-row-info = {$issuer} | {$progress}% | {$proto}
admin-objectives-row-target = Target: {$target}

# Prototype tab
admin-objectives-search = Search objectives...
admin-objectives-nothing-selected = Select an objective to add.
admin-objectives-selected = {$proto} {$name}
admin-objectives-target = Target
admin-objectives-target-random = Random
admin-objectives-target-entry = {$dead ->
    [true] {$name} ({$job}) - dead
    *[false] {$name} ({$job})
}
admin-objectives-bypass = Bypass requirements
admin-objectives-bypass-tooltip = Skip the objective's requirements, such as needing the traitor role or not already having this objective.

# Custom tab
admin-objectives-custom-info = A custom objective is always complete and shows exactly what you type. Rewards no server currency.
admin-objectives-custom-title = Objective (shown in the character menu and round end)
admin-objectives-custom-description = Description

# Steal tab
admin-objectives-steal-info = The objective is complete while the player carries (or pulls) this exact entity, or it is within range of a thief beacon they have linked. The title and icon are filled in from it.
admin-objectives-steal-entity = Entity ID
admin-objectives-steal-placeholder = Entity ID, e.g. from the right-click verbs or VV

# Results
admin-objectives-added = Added objective: {$objective}
admin-objectives-removed = Removed objective: {$objective}
admin-objectives-error-no-mind = The player no longer has a mind.
admin-objectives-error-bad-prototype = {$proto} is not an objective prototype.
admin-objectives-error-bad-target = The chosen target no longer exists.
admin-objectives-error-bad-item = That entity doesn't exist or can't be stolen.
admin-objectives-error-bad-entity-id = That is not a valid entity ID.
admin-objectives-error-empty-title = The objective text can't be empty.
admin-objectives-error-not-found = That objective no longer exists.
admin-objectives-error-assign-failed = The objective couldn't be assigned. The player may not meet its requirements, already have it, or there is no valid target. Try "Bypass requirements".
admin-objectives-error-assign-failed-bypass = The objective couldn't be assigned, probably because there is no valid target.
