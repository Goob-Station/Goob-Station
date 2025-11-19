# SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
#
# SPDX-License-Identifier: MIT-WIZARDS

### Interaction Messages

# Shown when repairing something
comp-repairable-repair = You repair {PROPER($target) ->
  [true] {""}
  *[false] the{" "}
}{$target} with {PROPER($tool) ->
  [true] {""}
  *[false] the{" "}
}{$tool}
