# SPDX-FileCopyrightText: 2021 20kdc <asdd2808@gmail.com>
# SPDX-FileCopyrightText: 2021 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
#
# SPDX-License-Identifier: MIT

#!/bin/sh

# Add this to .git/config:
# [merge "mapping-merge-driver"]
#         name = Merge driver for maps
#         driver = Tools/mapping-merge-driver.sh %A %O %B

dotnet run --project ./Content.Tools "$@"

