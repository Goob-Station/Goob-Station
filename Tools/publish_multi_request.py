#!/usr/bin/env python3
# SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
# SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

import requests
import os
import re
import subprocess
from datetime import datetime, timezone
from typing import Iterable

PUBLISH_TOKEN = os.environ["PUBLISH_TOKEN"]

RELEASE_DIR = "release"

ROBUST_CDN_URL = os.environ["ROBUST_CDN_URL"]
FORK_ID = os.environ["FORK_ID"]

# AltHub Space -> start
def sanitize_version_part(value: str) -> str:
    sanitized = re.sub(r"[^0-9A-Za-z._-]+", "-", value.strip())
    sanitized = sanitized.strip("-._")
    return sanitized or "unknown"


def get_publish_version() -> str:
    branch = sanitize_version_part(os.environ.get("GITHUB_REF_NAME", "unknown"))
    timestamp = datetime.now(timezone.utc).strftime("%Y%m%d%H%M%S")
    run_number = sanitize_version_part(os.environ.get("GITHUB_RUN_NUMBER", "0"))

    return "-".join(
        (
            sanitize_version_part(FORK_ID),
            branch,
            timestamp,
            run_number,
        )
    )


VERSION = get_publish_version()
# AltHub Space -> end

def main():
    session = requests.Session()
    session.headers = {
        "Authorization": f"Bearer {PUBLISH_TOKEN}",
    }

    print(f"Starting publish on Robust.Cdn for version {VERSION}")

    data = {
        "version": VERSION,
        "engineVersion": get_engine_version(),
    }
    headers = {
        "Content-Type": "application/json"
    }
    resp = session.post(f"{ROBUST_CDN_URL}fork/{FORK_ID}/publish/start", json=data, headers=headers)
    resp.raise_for_status()
    print("Publish successfully started, adding files...")

    for file in get_files_to_publish():
        print(f"Publishing {file}")
        with open(file, "rb") as f:
            headers = {
                "Content-Type": "application/octet-stream",
                "Robust-Cdn-Publish-File": os.path.basename(file),
                "Robust-Cdn-Publish-Version": VERSION
            }
            resp = session.post(f"{ROBUST_CDN_URL}fork/{FORK_ID}/publish/file", data=f, headers=headers)

        resp.raise_for_status()

    print("Successfully pushed files, finishing publish...")

    data = {
        "version": VERSION
    }
    headers = {
        "Content-Type": "application/json"
    }
    resp = session.post(f"{ROBUST_CDN_URL}fork/{FORK_ID}/publish/finish", json=data, headers=headers)
    resp.raise_for_status()

    print("SUCCESS!")


def get_files_to_publish() -> Iterable[str]:
    for file in os.listdir(RELEASE_DIR):
        yield os.path.join(RELEASE_DIR, file)


def get_engine_version() -> str:
    proc = subprocess.run(["git", "describe","--tags", "--abbrev=0"], stdout=subprocess.PIPE, cwd="RobustToolbox", check=True, encoding="UTF-8")
    tag = proc.stdout.strip()
    assert tag.startswith("v")
    return tag[1:] # Cut off v prefix.


if __name__ == '__main__':
    main()
