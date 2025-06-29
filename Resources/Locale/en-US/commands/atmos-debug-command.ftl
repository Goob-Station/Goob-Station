# SPDX-FileCopyrightText: 2024 PrPleGoo <PrPleGoo@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

cmd-atvrange-desc = Sets the atmos debug range (as two floats, start [red] and end [blue])
cmd-atvrange-help = Usage: {$command} <start> <end>
cmd-atvrange-error-start = Bad float START
cmd-atvrange-error-end = Bad float END
cmd-atvrange-error-zero = Scale cannot be zero, as this would cause a division by zero in AtmosDebugOverlay.

cmd-atvmode-desc = Sets the atmos debug mode. This will automatically reset the scale.
cmd-atvmode-help = Usage: {$command} <TotalMoles/GasMoles/Temperature> [<gas ID (for GasMoles)>]
cmd-atvmode-error-invalid = Invalid mode
cmd-atvmode-error-target-gas = A target gas must be provided for this mode.
cmd-atvmode-error-out-of-range = Gas ID not parsable or out of range.
cmd-atvmode-error-info = No further information is required for this mode.

cmd-atvcbm-desc = Changes from red/green/blue to greyscale
cmd-atvcbm-help = Usage: {$command} <true/false>
cmd-atvcbm-error = Invalid flag
