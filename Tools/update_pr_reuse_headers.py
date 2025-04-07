#!/usr/bin/env python3

import subprocess
import os
import sys
import re
import argparse
import json
from datetime import datetime, timezone
from collections import defaultdict

# --- Configuration ---
LICENSE_CONFIG = {
    "mit": {"id": "MIT", "path": "LICENSES/MIT.txt"},
    "agpl": {"id": "AGPL-3.0-or-later", "path": "LICENSES/AGPLv3.txt"},
}
DEFAULT_LICENSE_LABEL = "agpl"

COMMENT_PREFIXES = {
    ".cs": "//",
    ".yaml": "#",
    ".yml": "#",
}
REPO_PATH = "."

def run_git_command(command, cwd=REPO_PATH, check=True):
    """Runs a git command and returns its output."""
    try:
        result = subprocess.run(
            command,
            capture_output=True,
            text=True,
            check=check,
            cwd=cwd,
            encoding='utf-8',
            errors='ignore'
        )
        return result.stdout.strip()
    except subprocess.CalledProcessError as e:
        if check:
            print(f"Error running git command {' '.join(command)}: {e.stderr}", file=sys.stderr)
        return None
    except FileNotFoundError:
        print("FATAL: 'git' command not found. Make sure git is installed and in your PATH.", file=sys.stderr)
        return None

def get_authors_from_git(file_path, base_sha=None, head_sha=None, cwd=REPO_PATH):
    """
    Gets authors and their contribution years for a specific file.
    If base_sha and head_sha are provided, only gets authors from that commit range.
    Returns: dict like {"Author Name <email>": (min_year, max_year)}
    """
    # Prepare git log command
    if base_sha and head_sha:
        # For PR commits only
        commit_range = f"{base_sha}..{head_sha}"
        command = ["git", "log", commit_range, "--pretty=format:%at|%an|%ae|%b", "--", file_path]
    else:
        # For all historical commits
        command = ["git", "log", "--pretty=format:%at|%an|%ae|%b", "--follow", "--", file_path]

    output = run_git_command(command, cwd=cwd, check=False)
    if not output:
        return {}

    # Process the output
    author_timestamps = defaultdict(list)
    co_author_regex = re.compile(r"^Co-authored-by:\s*(.*?)\s*<([^>]+)>", re.MULTILINE)

    for line in output.splitlines():
        if not line.strip():
            continue

        parts = line.split('|', 3)
        if len(parts) < 4:
            continue

        timestamp_str, author_name, author_email, body = parts

        try:
            timestamp = int(timestamp_str)
        except ValueError:
            continue

        # Add main author
        if author_name and author_email:
            author_key = f"{author_name.strip()} <{author_email.strip()}>"
            author_timestamps[author_key].append(timestamp)

        # Add co-authors
        for match in co_author_regex.finditer(body):
            co_author_name = match.group(1).strip()
            co_author_email = match.group(2).strip()
            if co_author_name and co_author_email:
                co_author_key = f"{co_author_name} <{co_author_email}>"
                author_timestamps[co_author_key].append(timestamp)

    # Convert timestamps to years
    author_years = {}
    for author, timestamps in author_timestamps.items():
        if not timestamps:
            continue
        min_ts = min(timestamps)
        max_ts = max(timestamps)
        min_year = datetime.fromtimestamp(min_ts, timezone.utc).year
        max_year = datetime.fromtimestamp(max_ts, timezone.utc).year
        author_years[author] = (min_year, max_year)

    return author_years

def parse_existing_header(content, comment_prefix):
    """
    Parses an existing REUSE header to extract authors and license.
    Returns: (authors_dict, license_id, header_lines)
    """
    lines = content.splitlines()
    authors = {}
    license_id = None
    header_lines = []

    # Regular expressions for parsing
    copyright_regex = re.compile(f"^{re.escape(comment_prefix)} SPDX-FileCopyrightText: (\\d{{4}}) (.+)$")
    license_regex = re.compile(f"^{re.escape(comment_prefix)} SPDX-License-Identifier: (.+)$")

    # Find the header section
    in_header = True
    for i, line in enumerate(lines):
        if in_header:
            header_lines.append(line)

            # Check for copyright line
            copyright_match = copyright_regex.match(line)
            if copyright_match:
                year = int(copyright_match.group(1))
                author = copyright_match.group(2).strip()
                authors[author] = (year, year)
                continue

            # Check for license line
            license_match = license_regex.match(line)
            if license_match:
                license_id = license_match.group(1).strip()
                continue

            # Empty comment line or separator
            if line.strip() == comment_prefix:
                continue

            # If we get here, we've reached the end of the header
            if i > 0:  # Only if we've processed at least one line
                header_lines.pop()  # Remove the non-header line
                in_header = False
        else:
            break

    return authors, license_id, header_lines

def create_header(authors, license_id, comment_prefix):
    """
    Creates a REUSE header with the given authors and license.
    Returns: header string
    """
    lines = []

    # Add copyright lines
    if authors:
        for author, (_, year) in sorted(authors.items(), key=lambda x: (x[1][1], x[0])):
            lines.append(f"{comment_prefix} SPDX-FileCopyrightText: {year} {author}")
    else:
        lines.append(f"{comment_prefix} SPDX-FileCopyrightText: Contributors to the GoobStation14 project")

    # Add separator
    lines.append(f"{comment_prefix}")

    # Add license line
    lines.append(f"{comment_prefix} SPDX-License-Identifier: {license_id}")

    return "\n".join(lines)

def process_file(file_path, default_license_id, base_sha=None, head_sha=None):
    """
    Processes a file to add or update REUSE headers.
    Returns: True if file was modified, False otherwise
    """
    # Check file extension
    _, ext = os.path.splitext(file_path)
    comment_prefix = COMMENT_PREFIXES.get(ext)
    if not comment_prefix:
        print(f"Skipping unsupported file type: {file_path}")
        return False

    # Check if file exists
    full_path = os.path.join(REPO_PATH, file_path)
    if not os.path.exists(full_path):
        print(f"File not found: {file_path}")
        return False

    # Read file content
    with open(full_path, 'r', encoding='utf-8-sig', errors='ignore') as f:
        content = f.read()

    # Parse existing header if any
    existing_authors, existing_license, header_lines = parse_existing_header(content, comment_prefix)

    # Get authors from git
    if base_sha and head_sha:
        # For modified files, get both historical and PR authors
        all_authors = get_authors_from_git(file_path)
        pr_authors = get_authors_from_git(file_path, base_sha, head_sha)

        # Combine all authors
        git_authors = all_authors.copy()
        for author, (pr_min, pr_max) in pr_authors.items():
            if author in git_authors:
                all_min, all_max = git_authors[author]
                git_authors[author] = (min(all_min, pr_min), max(all_max, pr_max))
            else:
                git_authors[author] = (pr_min, pr_max)
    else:
        # For new files, just get PR authors
        git_authors = get_authors_from_git(file_path, base_sha, head_sha)

    # Determine what to do based on existing header
    if existing_license:
        print(f"Updating existing header for {file_path} (License: {existing_license})")

        # Combine existing and git authors
        combined_authors = existing_authors.copy()
        for author, (git_min, git_max) in git_authors.items():
            if author in combined_authors:
                existing_min, existing_max = combined_authors[author]
                combined_authors[author] = (min(existing_min, git_min), max(existing_max, git_max))
            else:
                combined_authors[author] = (git_min, git_max)

        # Create new header with existing license
        new_header = create_header(combined_authors, existing_license, comment_prefix)

        # Replace old header with new header
        if header_lines:
            old_header = "\n".join(header_lines)
            new_content = content.replace(old_header, new_header, 1)
        else:
            # No header found (shouldn't happen if existing_license is set)
            new_content = new_header + "\n\n" + content
    else:
        print(f"Adding new header to {file_path} (License: {default_license_id})")

        # Create new header with default license
        new_header = create_header(git_authors, default_license_id, comment_prefix)

        # Add header to file
        if content.strip():
            new_content = new_header + "\n\n" + content
        else:
            new_content = new_header + "\n"

    # Check if content changed
    if new_content == content:
        print(f"No changes needed for {file_path}")
        return False

    # Write updated content
    with open(full_path, 'w', encoding='utf-8', newline='\n') as f:
        f.write(new_content)

    print(f"Updated {file_path}")
    return True

def main():
    parser = argparse.ArgumentParser(description="Update REUSE headers for PR files")
    parser.add_argument("--files-added", nargs="*", default=[], help="List of added files")
    parser.add_argument("--files-modified", nargs="*", default=[], help="List of modified files")
    parser.add_argument("--pr-license", default=DEFAULT_LICENSE_LABEL, help="License to use for new files")
    parser.add_argument("--pr-base-sha", help="Base SHA of the PR")
    parser.add_argument("--pr-head-sha", help="Head SHA of the PR")

    args = parser.parse_args()

    # Validate license
    license_label = args.pr_license.lower()
    if license_label not in LICENSE_CONFIG:
        print(f"Warning: Unknown license '{license_label}', using default: {DEFAULT_LICENSE_LABEL}")
        license_label = DEFAULT_LICENSE_LABEL

    license_id = LICENSE_CONFIG[license_label]["id"]
    print(f"Using license for new files: {license_id}")

    # Process files
    files_changed = False

    print("\n--- Processing Added Files ---")
    for file in args.files_added:
        if process_file(file, license_id, args.pr_base_sha, args.pr_head_sha):
            files_changed = True

    print("\n--- Processing Modified Files ---")
    for file in args.files_modified:
        if process_file(file, license_id, args.pr_base_sha, args.pr_head_sha):
            files_changed = True

    print("\n--- Summary ---")
    if files_changed:
        print("Files were modified")
    else:
        print("No files needed changes")

if __name__ == "__main__":
    main()
