# SPDX-FileCopyrightText: 2025 Goob Station Contributors
#
# SPDX-License-Identifier: MPL-2.0

#!/usr/bin/env sh

# make sure to start from script dir
if [ "$(dirname $0)" != "." ]; then
    cd "$(dirname $0)"
fi

sh -e runQuickServer.sh &
sh -e runQuickClient.sh

exit
