# SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
#
# SPDX-License-Identifier: MIT-WIZARDS

﻿health-change-display =
    { $deltasign ->
        [-1] [color=green]{NATURALFIXED($amount, 2)}[/color] {$kind}
        *[1] [color=red]{NATURALFIXED($amount, 2)}[/color] {$kind}
    }
