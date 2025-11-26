# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

#!/usr/bin/env python3

import subprocess
import os
import sys
import re
import argparse
import json
import fnmatch
from datetime import datetime, timezone
from collections import defaultdict

LICENSE_CONFIG = {
    "mit": {"id": "MIT", "path": "LICENSES/MIT.txt"},
    "agpl": {"id": "AGPL-3.0-or-later", "path": "LICENSES/AGPLv3.txt"},
    "mpl": {"id": "MPL-2.0", "path": "LICENSES/MPL-2.0.txt"},
}

DEFAULT_LICENSE_LABEL = "mit"
DEFAULT_AUTHOR = "Space Station 14 Contributors"

DIRECTORY_RULES = [
    {
        "pattern": "Content.Goobstation.*/",
        "author": "Goob Station Contributors",
        "license": "agpl"
    },
    {
        "pattern": "_Goobstation",
        "author": "Goob Station Contributors",
        "license": "agpl"
    },
]

COMMENT_STYLES = {
    # C-style single-line comments
    ".cs": ("//", None),
    ".js": ("//", None),
    ".ts": ("//", None),
    ".jsx": ("//", None),
    ".tsx": ("//", None),
    ".c": ("//", None),
    ".cpp": ("//", None),
    ".cc": ("//", None),
    ".h": ("//", None),
    ".hpp": ("//", None),
    ".java": ("//", None),
    ".scala": ("//", None),
    ".kt": ("//", None),
    ".swift": ("//", None),
    ".go": ("//", None),
    ".rs": ("//", None),
    ".dart": ("//", None),
    ".groovy": ("//", None),
    ".php": ("//", None),

    # Hash-style single-line comments
    ".yaml": ("#", None),
    ".yml": ("#", None),
    ".ftl": ("#", None),
    ".py": ("#", None),
    ".rb": ("#", None),
    ".pl": ("#", None),
    ".pm": ("#", None),
    ".sh": ("#", None),
    ".bash": ("#", None),
    ".zsh": ("#", None),
    ".fish": ("#", None),
    ".ps1": ("#", None),
    ".r": ("#", None),
    ".rmd": ("#", None),
    ".jl": ("#", None),
    ".tcl": ("#", None),
    ".perl": ("#", None),
    ".conf": ("#", None),
    ".toml": ("#", None),
    ".ini": ("#", None),
    ".cfg": ("#", None),
    ".gitignore": ("#", None),
    ".dockerignore": ("#", None),

    # Other single-line comment styles
    ".bat": ("REM", None),
    ".cmd": ("REM", None),
    ".vb": ("'", None),
    ".vbs": ("'", None),
    ".bas": ("'", None),
    ".asm": (";", None),
    ".s": (";", None),
    ".lisp": (";", None),
    ".clj": (";", None),
    ".f": ("!", None),
    ".f90": ("!", None),
    ".m": ("%", None),
    ".sql": ("--", None),
    ".ada": ("--", None),
    ".adb": ("--", None),
    ".ads": ("--", None),
    ".hs": ("--", None),
    ".lhs": ("--", None),
    ".lua": ("--", None),

    # Multi-line comment styles
    ".xaml": ("<!--", "-->"),
    ".xml": ("<!--", "-->"),
    ".html": ("<!--", "-->"),
    ".htm": ("<!--", "-->"),
    ".svg": ("<!--", "-->"),
    ".css": ("/*", "*/"),
    ".scss": ("/*", "*/"),
    ".sass": ("/*", "*/"),
    ".less": ("/*", "*/"),
    ".md": ("<!--", "-->"),
    ".markdown": ("<!--", "-->"),
}
REPO_PATH = "."

def matches_pattern(file_path, pattern):
    if '*' in pattern or '?' in pattern:
        if fnmatch.fnmatch(file_path, f"*{pattern}*"):
            return True
        path_parts = file_path.split('/')
        for i in range(len(path_parts)):
            partial_path = '/'.join(path_parts[i:]) + '/'
            if fnmatch.fnmatch(partial_path, pattern + '*'):
                return True
        return False
    else:
        return pattern in file_path

def get_author_and_license_for_file(file_path):
    normalized_path = file_path.replace("\\", "/")
    
    for rule in DIRECTORY_RULES:
        pattern = rule["pattern"]
        if matches_pattern(normalized_path, pattern):
            author = rule["author"]
            license_label = rule.get("license", DEFAULT_LICENSE_LABEL)
            print(f"  Matched pattern '{pattern}' -> Author: {author}, License: {license_label}")
            return author, license_label
    
    print(f"  No pattern match, using defaults -> Author: {DEFAULT_AUTHOR}, License: {DEFAULT_LICENSE_LABEL}")
    return DEFAULT_AUTHOR, DEFAULT_LICENSE_LABEL

def get_current_year():
    """Returns the current year."""
    return datetime.now(timezone.utc).year

def parse_existing_header(content, comment_style):
    prefix, suffix = comment_style
    lines = content.splitlines()
    authors = {}
    license_id = None
    header_lines = []

    if suffix is None:
        copyright_regex = re.compile(f"^{re.escape(prefix)} SPDX-FileCopyrightText: (\\d{{4}}) (.+)$")
        license_regex = re.compile(f"^{re.escape(prefix)} SPDX-License-Identifier: (.+)$")

        in_header = True
        for i, line in enumerate(lines):
            if in_header:
                header_lines.append(line)

                copyright_match = copyright_regex.match(line)
                if copyright_match:
                    year = int(copyright_match.group(1))
                    author = copyright_match.group(2).strip()
                    authors[author] = (year, year)
                    continue

                license_match = license_regex.match(line)
                if license_match:
                    license_id = license_match.group(1).strip()
                    continue

                if line.strip() == prefix:
                    continue

                if i > 0:
                    header_lines.pop()
                    in_header = False
            else:
                break
    else:
        copyright_regex = re.compile(r"^SPDX-FileCopyrightText: (\d{4}) (.+)$")
        license_regex = re.compile(r"^SPDX-License-Identifier: (.+)$")

        in_comment = False
        for i, line in enumerate(lines):
            stripped_line = line.strip()

            if stripped_line == prefix:
                in_comment = True
                header_lines.append(line)
                continue

            if stripped_line == suffix and in_comment:
                header_lines.append(line)
                break

            if in_comment:
                header_lines.append(line)

                copyright_match = copyright_regex.match(stripped_line)
                if copyright_match:
                    year = int(copyright_match.group(1))
                    author = copyright_match.group(2).strip()
                    authors[author] = (year, year)
                    continue

                license_match = license_regex.match(stripped_line)
                if license_match:
                    license_id = license_match.group(1).strip()
                    continue

    return authors, license_id, header_lines

def create_header(authors, license_id, comment_style):
    prefix, suffix = comment_style
    lines = []

    if suffix is None:
        if authors:
            for author, (_, year) in sorted(authors.items(), key=lambda x: (x[1][1], x[0])):
                lines.append(f"{prefix} SPDX-FileCopyrightText: {year} {author}")
        else:
            lines.append(f"{prefix} SPDX-FileCopyrightText: {get_current_year()} {DEFAULT_AUTHOR}")

        lines.append(f"{prefix}")

        lines.append(f"{prefix} SPDX-License-Identifier: {license_id}")
    else:
        lines.append(f"{prefix}")

        if authors:
            for author, (_, year) in sorted(authors.items(), key=lambda x: (x[1][1], x[0])):
                lines.append(f"SPDX-FileCopyrightText: {year} {author}")
        else:
            lines.append(f"SPDX-FileCopyrightText: {get_current_year()} {DEFAULT_AUTHOR}")

        lines.append("")

        lines.append(f"SPDX-License-Identifier: {license_id}")

        lines.append(f"{suffix}")

    return "\n".join(lines)

def process_file(file_path, pr_license_override=None, pr_base_sha=None, pr_head_sha=None):

    _, ext = os.path.splitext(file_path)
    comment_style = COMMENT_STYLES.get(ext)
    if not comment_style:
        print(f"Skipping unsupported file type: {file_path}")
        return False

    full_path = os.path.join(REPO_PATH, file_path)
    if not os.path.exists(full_path):
        print(f"File not found: {file_path}")
        return False

    with open(full_path, 'r', encoding='utf-8-sig', errors='ignore') as f:
        content = f.read()

    existing_authors, existing_license, header_lines = parse_existing_header(content, comment_style)

    if existing_license:
        print(f"Skipping {file_path} - already has REUSE header (License: {existing_license})")
        return False

    file_author, file_license_label = get_author_and_license_for_file(file_path)
    current_year = get_current_year()

    if pr_license_override:
        license_label = pr_license_override
        print(f"  Using PR license override: {license_label}")
    else:
        license_label = file_license_label

    if license_label not in LICENSE_CONFIG:
        print(f"  Warning: Unknown license '{license_label}', using default: {DEFAULT_LICENSE_LABEL}")
        license_label = DEFAULT_LICENSE_LABEL

    license_id = LICENSE_CONFIG[license_label]["id"]

    print(f"Adding new header to {file_path} (Author: {file_author}, License: {license_id})")

    authors = {file_author: (current_year, current_year)}
    new_header = create_header(authors, license_id, comment_style)

    if content.strip():
        prefix, suffix = comment_style
        if suffix and content.lstrip().startswith("<?xml"):
            xml_decl_end = content.find("?>") + 2
            xml_declaration = content[:xml_decl_end]
            rest_of_content = content[xml_decl_end:].lstrip()
            new_content = xml_declaration + "\n" + new_header + "\n\n" + rest_of_content
        else:
            new_content = new_header + "\n\n" + content
    else:
        new_content = new_header + "\n"

    if new_content == content:
        print(f"No changes needed for {file_path}")
        return False

    with open(full_path, 'w', encoding='utf-8', newline='\n') as f:
        f.write(new_content)

    print(f"Updated {file_path}")
    return True

def main():
    parser = argparse.ArgumentParser(description="Update REUSE headers for PR files")
    parser.add_argument("--files-added", nargs="*", default=[], help="List of added files")
    parser.add_argument("--files-modified", nargs="*", default=[], help="List of modified files")
    parser.add_argument("--pr-license", help="License override from PR (optional)")
    parser.add_argument("--pr-base-sha", help="Base SHA of the PR (unused)")
    parser.add_argument("--pr-head-sha", help="Head SHA of the PR (unused)")

    args = parser.parse_args()

    pr_license_override = None
    if args.pr_license:
        license_label = args.pr_license.lower()
        if license_label in LICENSE_CONFIG:
            pr_license_override = license_label
            print(f"Using PR license override for all files: {LICENSE_CONFIG[license_label]['id']}")
        else:
            print(f"Warning: Unknown PR license '{license_label}', will use directory-based defaults")

    files_changed = False

    print("\n--- Processing Added Files ---")
    for file in args.files_added:
        print(f"\nProcessing added file: {file}")
        if process_file(file, pr_license_override, args.pr_base_sha, args.pr_head_sha):
            files_changed = True

    print("\n--- Processing Modified Files ---")
    for file in args.files_modified:
        print(f"\nProcessing modified file: {file}")
        if process_file(file, pr_license_override, args.pr_base_sha, args.pr_head_sha):
            files_changed = True

    print("\n--- Summary ---")
    if files_changed:
        print("Files were modified")
    else:
        print("No files needed changes")

if __name__ == "__main__":
    main()

