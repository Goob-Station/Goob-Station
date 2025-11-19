# SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
#
# SPDX-License-Identifier: MIT-WIZARDS

#!/usr/bin/env pwsh

Get-ChildItem release/*.zip | Get-FileHash -Algorithm SHA256 | ForEach-Object {
    $_.Hash > "$($_.Path).sha256";
}
