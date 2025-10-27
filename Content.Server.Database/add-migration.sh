# SPDX-FileCopyrightText: 2021 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
# SPDX-FileCopyrightText: 2021 Saphire Lattice <lattice@saphi.re>
# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
#
# SPDX-License-Identifier: MIT

#!/usr/bin/env bash

if [ -z "$1" ] ; then
    echo "Must specify migration name"
    exit 1
fi

dotnet ef migrations add --context SqliteServerDbContext -o Migrations/Sqlite "$1"
dotnet ef migrations add --context PostgresServerDbContext -o Migrations/Postgres "$1"
