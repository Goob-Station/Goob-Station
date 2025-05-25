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
from datetime import datetime, timezone
from collections import defaultdict

# --- Configuration ---
LICENSE_CONFIG = {
    "mit": {"id": "MIT", "path": "LICENSES/MIT.txt"},
    "agpl": {"id": "AGPL-3.0-or-later", "path": "LICENSES/AGPLv3.txt"},
    "mpl": {"id": "MPL-2.0", "path": "LICENSES/MPL-2.0.txt"},
}

DEFAULT_LICENSE_LABEL = "agpl"

# Dictionary mapping file extensions to comment styles
# Format: {extension: (prefix, suffix)}
# If suffix is None, it's a single-line comment style
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
    ".jl": ("#", None),  # Julia
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
    ".s": (";", None),  # Assembly
    ".lisp": (";", None),
    ".clj": (";", None),  # Clojure
    ".f": ("!", None),   # Fortran
    ".f90": ("!", None), # Fortran
    ".m": ("%", None),   # MATLAB/Octave
    ".sql": ("--", None),
    ".ada": ("--", None),
    ".adb": ("--", None),
    ".ads": ("--", None),
    ".hs": ("--", None), # Haskell
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

def get_authors_from_git(file_path, cwd=REPO_PATH, pr_base_sha=None, pr_head_sha=None):
    """
    Gets authors and their contribution years for a specific file.
    If pr_base_sha and pr_head_sha are provided, also includes authors from the PR's commits.
    Returns: dict like {"Author Name <email>": (min_year, max_year)}
    """
    author_timestamps = defaultdict(list)

    # Get authors from the PR's commits if base and head SHAs are provided
    if pr_base_sha and pr_head_sha:
        print(f"Getting authors from PR commits for {file_path}")
        print(f"PR base SHA: {pr_base_sha}")
        print(f"PR head SHA: {pr_head_sha}")

        # First, let's log all commits in the PR
        all_commits_command = ["git", "log", f"{pr_base_sha}..{pr_head_sha}", "--pretty=format:%H|%an|%ae", "--", file_path]
        print(f"Running command: {' '.join(all_commits_command)}")
        all_commits_output = run_git_command(all_commits_command, cwd=cwd, check=False)

        if all_commits_output:
            print(f"Commits found in PR for {file_path}:")
            for line in all_commits_output.splitlines():
                print(f"  {line}")
        else:
            print(f"No commits found in PR for {file_path}")

        # Now get the authors with timestamps
        pr_command = ["git", "log", f"{pr_base_sha}..{pr_head_sha}", "--pretty=format:%H|%at|%an|%ae|%b", "--", file_path]
        print(f"Running command: {' '.join(pr_command)}")
        pr_output = run_git_command(pr_command, cwd=cwd, check=False)

        if pr_output:
            # Process PR authors
            print(f"Raw PR output for {file_path}:")
            for line in pr_output.splitlines():
                print(f"  {line}")

            process_git_log_output(pr_output, author_timestamps)
            print(f"Found {len(author_timestamps)} authors in PR commits for {file_path}")

            # Print the authors found
            print(f"Authors found in PR commits for {file_path}:")
            for author, timestamps in author_timestamps.items():
                print(f"  {author}: {timestamps}")
        else:
            print(f"No PR output found for {file_path}")

    # Get all historical authors
    print(f"Getting historical authors for {file_path}")
    command = ["git", "log", "--pretty=format:%H|%at|%an|%ae|%b", "--follow", "--", file_path]
    print(f"Running command: {' '.join(command)}")
    output = run_git_command(command, cwd=cwd, check=False)

    if output:
        # Process historical authors
        print(f"Processing historical authors for {file_path}")
        process_git_log_output(output, author_timestamps)

        # Print the authors found
        print(f"All authors found for {file_path} (after adding historical):")
        for author, timestamps in author_timestamps.items():
            print(f"  {author}: {timestamps}")
    else:
        print(f"No historical output found for {file_path}")

    if not author_timestamps:
        # Try to get the current user from git config as a fallback
        try:
            name_cmd = ["git", "config", "user.name"]
            email_cmd = ["git", "config", "user.email"]
            user_name = run_git_command(name_cmd, cwd=cwd, check=False)
            user_email = run_git_command(email_cmd, cwd=cwd, check=False)

            # Use current year
            current_year = datetime.now(timezone.utc).year
            if user_name and user_email and user_name.strip() != "Unknown":
                return {f"{user_name} <{user_email}>": (current_year, current_year)}
            else:
                print("Warning: Could not get current user from git config or name is 'Unknown'")
                return {}
        except Exception as e:
            print(f"Error getting git user: {e}")
        return {}

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

def process_git_log_output(output, author_timestamps):
    """
    Process git log output and add authors to author_timestamps.
    """
    co_author_regex = re.compile(r"^Co-authored-by:\s*(.*?)\s*<([^>]+)>", re.MULTILINE)

    for line in output.splitlines():
        if not line.strip():
            continue

        parts = line.split('|', 4)
        if len(parts) < 5:
            print(f"Skipping malformed line: {line}")
            continue

        commit_hash, timestamp_str, author_name, author_email, body = parts
        print(f"Processing commit {commit_hash[:8]} by {author_name} <{author_email}>")

        try:
            timestamp = int(timestamp_str)
        except ValueError:
            continue

        # Add main author
        if author_name and author_email and author_name.strip() != "Unknown":
            author_key = f"{author_name.strip()} <{author_email.strip()}>"
            author_timestamps[author_key].append(timestamp)

        # Add co-authors
        for match in co_author_regex.finditer(body):
            co_author_name = match.group(1).strip()
            co_author_email = match.group(2).strip()
            if co_author_name and co_author_email and co_author_name.strip() != "Unknown":
                co_author_key = f"{co_author_name} <{co_author_email}>"
                author_timestamps[co_author_key].append(timestamp)

    # No need to convert timestamps to years here, it's done in get_authors_from_git

def parse_existing_header(content, comment_style):
    """
    Parses an existing REUSE header to extract authors and license.
    Returns: (authors_dict, license_id, header_lines)

    comment_style is a tuple of (prefix, suffix)
    """
    prefix, suffix = comment_style
    lines = content.splitlines()
    authors = {}
    license_id = None
    header_lines = []

    if suffix is None:
        # Single-line comment style (e.g., //, #)
        # Regular expressions for parsing
        copyright_regex = re.compile(f"^{re.escape(prefix)} SPDX-FileCopyrightText: (\\d{{4}}) (.+)$")
        license_regex = re.compile(f"^{re.escape(prefix)} SPDX-License-Identifier: (.+)$")

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
                if line.strip() == prefix:
                    continue

                # If we get here, we've reached the end of the header
                if i > 0:  # Only if we've processed at least one line
                    header_lines.pop()  # Remove the non-header line
                    in_header = False
            else:
                break
    else:
        # Multi-line comment style (e.g., <!-- -->)
        # Regular expressions for parsing
        copyright_regex = re.compile(r"^SPDX-FileCopyrightText: (\d{4}) (.+)$")
        license_regex = re.compile(r"^SPDX-License-Identifier: (.+)$")

        # Find the header section
        in_comment = False
        for i, line in enumerate(lines):
            stripped_line = line.strip()

            # Start of comment
            if stripped_line == prefix:
                in_comment = True
                header_lines.append(line)
                continue

            # End of comment
            if stripped_line == suffix and in_comment:
                header_lines.append(line)
                break

            if in_comment:
                header_lines.append(line)

                # Check for copyright line
                copyright_match = copyright_regex.match(stripped_line)
                if copyright_match:
                    year = int(copyright_match.group(1))
                    author = copyright_match.group(2).strip()
                    authors[author] = (year, year)
                    continue

                # Check for license line
                license_match = license_regex.match(stripped_line)
                if license_match:
                    license_id = license_match.group(1).strip()
                    continue

    return authors, license_id, header_lines

def create_header(authors, license_id, comment_style):
    """
    Creates a REUSE header with the given authors and license.
    Returns: header string

    comment_style is a tuple of (prefix, suffix)
    """
    prefix, suffix = comment_style
    lines = []

    if suffix is None:
        # Single-line comment style (e.g., //, #)
        # Add copyright lines
        if authors:
            for author, (_, year) in sorted(authors.items(), key=lambda x: (x[1][1], x[0])):
                if not author.startswith("Unknown <"):
                    lines.append(f"{prefix} SPDX-FileCopyrightText: {year} {author}")
        else:
            lines.append(f"{prefix} SPDX-FileCopyrightText: Contributors to the GoobStation14 project")

        # Add separator
        lines.append(f"{prefix}")

        # Add license line
        lines.append(f"{prefix} SPDX-License-Identifier: {license_id}")
    else:
        # Multi-line comment style (e.g., <!-- -->)
        # Start comment
        lines.append(f"{prefix}")

        # Add copyright lines
        if authors:
            for author, (_, year) in sorted(authors.items(), key=lambda x: (x[1][1], x[0])):
                if not author.startswith("Unknown <"):
                    lines.append(f"SPDX-FileCopyrightText: {year} {author}")
        else:
            lines.append(f"SPDX-FileCopyrightText: Contributors to the GoobStation14 project")

        # Add separator
        lines.append("")

        # Add license line
        lines.append(f"SPDX-License-Identifier: {license_id}")

        # End comment
        lines.append(f"{suffix}")

    return "\n".join(lines)

def process_file(file_path, default_license_id, pr_base_sha=None, pr_head_sha=None):
    """
    Processes a file to add or update REUSE headers.
    Returns: True if file was modified, False otherwise
    """
    # Check file extension
    _, ext = os.path.splitext(file_path)
    comment_style = COMMENT_STYLES.get(ext)
    if not comment_style:
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
    existing_authors, existing_license, header_lines = parse_existing_header(content, comment_style)

    # Get all authors from git
    git_authors = get_authors_from_git(file_path, REPO_PATH, pr_base_sha, pr_head_sha)

    # Add current user to authors
    try:
        name_cmd = ["git", "config", "user.name"]
        email_cmd = ["git", "config", "user.email"]
        user_name = run_git_command(name_cmd, check=False)
        user_email = run_git_command(email_cmd, check=False)

        if user_name and user_email and user_name.strip() != "Unknown":
            # Use current year
            current_year = datetime.now(timezone.utc).year
            current_user = f"{user_name} <{user_email}>"

            # Add current user if not already present
            if current_user not in git_authors:
                git_authors[current_user] = (current_year, current_year)
                print(f"  Added current user: {current_user}")
            else:
                # Update year if necessary
                min_year, max_year = git_authors[current_user]
                git_authors[current_user] = (min(min_year, current_year), max(max_year, current_year))
        else:
            print("Warning: Could not get current user from git config or name is 'Unknown'")
    except Exception as e:
        print(f"Error getting git user: {e}")

    # Determine what to do based on existing header
    if existing_license:
        print(f"Updating existing header for {file_path} (License: {existing_license})")

        # Combine existing and git authors
        combined_authors = existing_authors.copy()
        for author, (git_min, git_max) in git_authors.items():
            if author.startswith("Unknown <"):
                continue
            if author in combined_authors:
                existing_min, existing_max = combined_authors[author]
                combined_authors[author] = (min(existing_min, git_min), max(existing_max, git_max))
            else:
                combined_authors[author] = (git_min, git_max)
                print(f"  Adding new author: {author}")

        # Create new header with existing license
        new_header = create_header(combined_authors, existing_license, comment_style)

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
        new_header = create_header(git_authors, default_license_id, comment_style)

        # Add header to file
        if content.strip():
            # For XML files, we need to add the header after the XML declaration if present
            prefix, suffix = comment_style
            if suffix and content.lstrip().startswith("<?xml"):
                # Find the end of the XML declaration
                xml_decl_end = content.find("?>") + 2
                xml_declaration = content[:xml_decl_end]
                rest_of_content = content[xml_decl_end:].lstrip()
                new_content = xml_declaration + "\n" + new_header + "\n\n" + rest_of_content
            else:
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

    # Print the PR base and head SHAs
    print(f"\nPR Base SHA: {args.pr_base_sha}")
    print(f"PR Head SHA: {args.pr_head_sha}")

    print("\n--- Processing Added Files ---")
    for file in args.files_added:
        print(f"\nProcessing added file: {file}")
        if process_file(file, license_id, args.pr_base_sha, args.pr_head_sha):
            files_changed = True

    print("\n--- Processing Modified Files ---")
    for file in args.files_modified:
        print(f"\nProcessing modified file: {file}")
        if process_file(file, license_id, args.pr_base_sha, args.pr_head_sha):
            files_changed = True

    print("\n--- Summary ---")
    if files_changed:
        print("Files were modified")
    else:
        print("No files needed changes")

if __name__ == "__main__":
    main()
