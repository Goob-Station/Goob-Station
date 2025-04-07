# update_pr_reuse_headers.py
import subprocess
import os
import sys
from datetime import datetime, timezone
from collections import defaultdict
import argparse
import json
import re

# --- Configuration ---
# Maps PR label names (lowercase) to SPDX License IDs and optional license file paths
# Add new licenses here for future expansion
LICENSE_CONFIG = {
    "mit": {"id": "MIT", "path": "LICENSES/MIT.txt"},
    "agpl": {"id": "AGPL-3.0-or-later", "path": "LICENSES/AGPLv3.txt"},
    # Add more licenses like:
    # "apache-2.0": {"id": "Apache-2.0", "path": "LICENSES/Apache-2.0.txt"},
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
        # Don't print error if check=False and it returns non-zero, git log might return empty
        if check:
            print(f"Error running git command {' '.join(command)}: {e.stderr}", file=sys.stderr)
        return None
    except FileNotFoundError:
        print("FATAL: 'git' command not found. Make sure git is installed and in your PATH.", file=sys.stderr)
        return None

def get_pr_authors_for_file(file_path, base_sha, head_sha, cwd=REPO_PATH):
    """
    Gets authors (including co-authors) and their contribution years *within the PR's commit range*
    for a specific file.
    Returns: (dict like {"Author Name <email>": (min_year, max_year)}, list_of_warnings)
    """
    commit_range = f"{base_sha}..{head_sha}"
    command = ["git", "log", commit_range, "--pretty=format:%at%x00%an%x00%ae%x00%b%x1E", "--", file_path]
    co_author_regex = re.compile(r"^Co-authored-by:\s*(.*?)\s*<([^>]+)>", re.IGNORECASE | re.MULTILINE)

    output = run_git_command(command, cwd=cwd, check=False)
    author_timestamps = defaultdict(list)
    warnings = []

    if output is None or not output.strip():
        return {}, warnings

    commits = output.strip().split('\x1E')

    for commit_data in commits:
        if not commit_data.strip():
            continue

        parts = commit_data.strip().split('\x00')
        if len(parts) < 4:
            warnings.append(f"Skipping malformed commit data for {file_path}: {commit_data[:100]}...")
            continue

        timestamp_str, author_name, author_email, body = parts[0], parts[1], parts[2], parts[3]

        try:
            ts_int = int(timestamp_str)
        except ValueError:
            warnings.append(f"Skipping invalid timestamp for {file_path}: {timestamp_str}")
            continue

        # Process main author
        if author_name and author_email:
            author_key = f"{author_name.strip()} <{author_email.strip()}>"
            author_timestamps[author_key].append(ts_int)
        else:
            warnings.append(f"Skipping commit with missing author info for {file_path}: Name='{author_name}', Email='{author_email}'")

        # Process co-authors
        for match in co_author_regex.finditer(body):
            co_author_name = match.group(1).strip()
            co_author_email = match.group(2).strip()
            if co_author_name and co_author_email:
                co_author_key = f"{co_author_name} <{co_author_email}>"
                author_timestamps[co_author_key].append(ts_int)
            else:
                 warnings.append(f"Skipping malformed Co-authored-by line in commit body for {file_path}: Name='{co_author_name}', Email='{co_author_email}'")

    author_years = {}
    for author, timestamps in author_timestamps.items():
        if not timestamps: continue
        try:
            min_ts = min(timestamps)
            max_ts = max(timestamps)
            min_year = datetime.fromtimestamp(min_ts, timezone.utc).year
            max_year = datetime.fromtimestamp(max_ts, timezone.utc).year
            author_years[author] = (min_year, max_year)
        except Exception as e:
             warnings.append(f"Error calculating year range for author {author} on file {file_path}: {e}")

    return author_years, warnings

def get_all_authors_for_file(file_path, cwd=REPO_PATH):
    """
    Gets *all* historical authors (including co-authors) and their contribution years
    for a specific file.
    Returns: (dict like {"Author Name <email>": (min_year, max_year)}, list_of_warnings)
    """
    command = ["git", "log", "--pretty=format:%at%x00%an%x00%ae%x00%b%x1E", "--follow", "--", file_path]
    co_author_regex = re.compile(r"^Co-authored-by:\s*(.*?)\s*<([^>]+)>", re.IGNORECASE | re.MULTILINE)

    output = run_git_command(command, cwd=cwd, check=False)
    author_timestamps = defaultdict(list)
    warnings = []

    if output is None or not output.strip():
        return {}, warnings

    commits = output.strip().split('\x1E')

    for commit_data in commits:
        if not commit_data.strip():
            continue

        parts = commit_data.strip().split('\x00')
        if len(parts) < 4:
            warnings.append(f"Skipping malformed commit data for {file_path}: {commit_data[:100]}...")
            continue

        timestamp_str, author_name, author_email, body = parts[0], parts[1], parts[2], parts[3]

        try:
            ts_int = int(timestamp_str)
        except ValueError:
            warnings.append(f"Skipping invalid timestamp for {file_path}: {timestamp_str}")
            continue

        # Process main author
        if author_name and author_email:
            author_key = f"{author_name.strip()} <{author_email.strip()}>"
            author_timestamps[author_key].append(ts_int)
        else:
            warnings.append(f"Skipping commit with missing author info for {file_path}: Name='{author_name}', Email='{author_email}'")

        # Process co-authors
        for match in co_author_regex.finditer(body):
            co_author_name = match.group(1).strip()
            co_author_email = match.group(2).strip()
            if co_author_name and co_author_email:
                co_author_key = f"{co_author_name} <{co_author_email}>"
                author_timestamps[co_author_key].append(ts_int)
            else:
                 warnings.append(f"Skipping malformed Co-authored-by line in commit body for {file_path}: Name='{co_author_name}', Email='{co_author_email}'")

    author_years = {}
    for author, timestamps in author_timestamps.items():
        if not timestamps: continue
        try:
            min_ts = min(timestamps)
            max_ts = max(timestamps)
            min_year = datetime.fromtimestamp(min_ts, timezone.utc).year
            max_year = datetime.fromtimestamp(max_ts, timezone.utc).year
            author_years[author] = (min_year, max_year)
        except Exception as e:
             warnings.append(f"Error calculating year range for author {author} on file {file_path}: {e}")

    return author_years, warnings


def create_reuse_header(author_years, license_id, comment_prefix):
    """Creates the REUSE compliant header string."""
    header_lines = []
    copyright_prefix = f"{comment_prefix} SPDX-FileCopyrightText:"
    if not author_years:
        header_lines.append(f"{copyright_prefix} Contributors to the GoobStation14 project")
    else:
        sorted_authors = sorted(author_years.items(), key=lambda item: item[1][0])
        for author, (min_year, max_year) in sorted_authors:
            year_string = str(max_year)
            clean_author = author.replace('\n', ' ').replace('\r', '')
            header_lines.append(f"{copyright_prefix} {year_string} {clean_author}")

    header_lines.append(f"{comment_prefix}")
    header_lines.append(f"{comment_prefix} SPDX-License-Identifier: {license_id}")
    return "\n".join(header_lines)

def extract_existing_authors(content, comment_prefix):
    """Extracts existing authors from SPDX-FileCopyrightText headers."""
    copyright_prefix = f"{comment_prefix} SPDX-FileCopyrightText:"
    lines = content.splitlines()
    authors = {}

    author_regex = re.compile(r"(\d{4})\s+(.*)")

    for line in lines:
        stripped_line = line.strip()
        if stripped_line.startswith(copyright_prefix):
            # Extract year and author
            author_text = stripped_line[len(copyright_prefix):].strip()
            match = author_regex.match(author_text)
            if match:
                year = int(match.group(1))
                author = match.group(2).strip()
                if author in authors:
                    _, max_year = authors[author]
                    authors[author] = (min(year, max_year), max(year, max_year))
                else:
                    authors[author] = (year, year)

    return authors

def remove_existing_reuse_header(content, comment_prefix):
    """Removes existing SPDX comment lines from the start of the content."""
    lines = content.splitlines()
    cleaned_lines = []
    in_header = True
    header_removed = False
    spdx_prefix = f"{comment_prefix} SPDX-"
    copyright_prefix_long = f"{comment_prefix} SPDX-FileCopyrightText:"
    copyright_prefix_short = f"{comment_prefix} Copyright"
    separator = f"{comment_prefix}"

    for i, line in enumerate(lines):
        stripped_line = line.strip()
        is_spdx_comment = stripped_line.startswith(spdx_prefix)
        is_copyright_comment = stripped_line.startswith(copyright_prefix_long) or stripped_line.startswith(copyright_prefix_short)
        is_separator_comment = stripped_line == separator and i < 5 # Only consider separators early on
        is_header_line = is_spdx_comment or is_copyright_comment or is_separator_comment

        if in_header and is_header_line:
            header_removed = True
            continue
        # Stop considering it a header if we hit a non-header line or go too deep
        if in_header and (not is_header_line or i >= 50):
             in_header = False
        cleaned_lines.append(line)

    # Trim leading whitespace after removing header
    first_content_line_index = 0
    for i, line in enumerate(cleaned_lines):
        if line.strip():
            first_content_line_index = i
            break

    return "\n".join(cleaned_lines[first_content_line_index:]) if cleaned_lines else ""


def extract_license_identifier(content, comment_prefix):
    """Extracts the SPDX-License-Identifier from the header."""
    spdx_license_prefix = f"{comment_prefix} SPDX-License-Identifier:"
    lines = content.splitlines()
    for i, line in enumerate(lines):
        stripped_line = line.strip()
        if stripped_line.startswith(spdx_license_prefix):
            return stripped_line[len(spdx_license_prefix):].strip()
        if i > 50: # Stop searching after a reasonable number of lines
            break
    return None

def process_added_file(file_path, license_id, base_sha, head_sha):
    """Processes a newly added file."""
    print(f"Processing ADDED file: {file_path} with license {license_id}")
    _, ext = os.path.splitext(file_path)
    comment_prefix = COMMENT_PREFIXES.get(ext)
    if not comment_prefix:
        print(f"  Skipping (unsupported extension): {file_path}", file=sys.stderr)
        return False

    full_file_path = os.path.join(REPO_PATH, file_path)
    if not os.path.exists(full_file_path):
         print(f"  Skipping (file not found): {file_path}", file=sys.stderr)
         return False # Should not happen in workflow context

    # Get authors only from the PR commits for new files
    author_years, warnings = get_pr_authors_for_file(file_path, base_sha, head_sha, REPO_PATH)
    if warnings:
        for warn in warnings: print(f"  Warning: {warn}", file=sys.stderr)

    reuse_header = create_reuse_header(author_years, license_id, comment_prefix)

    try:
        with open(full_file_path, 'r', encoding='utf-8-sig', errors='ignore') as f:
            original_content = f.read()

        # Check if file *already* has a header (e.g., user added it manually)
        existing_license = extract_license_identifier(original_content, comment_prefix)
        if existing_license:
            print(f"  Skipping (already has header with license {existing_license}): {file_path}")
            return False

        cleaned_content = remove_existing_reuse_header(original_content, comment_prefix) # Should ideally do nothing if no header
        separator = "\n\n" if cleaned_content.strip() else "" # Add extra newline for new files

        # Handle shebangs or initial comments for YAML
        if comment_prefix == '#' and cleaned_content.startswith('#'):
             new_content = reuse_header + "\n" + cleaned_content
        else:
             new_content = reuse_header + separator + cleaned_content

        final_content_lf = new_content.replace('\r\n', '\n').replace('\r', '\n')

        with open(full_file_path, 'w', encoding='utf-8', newline='\n') as f:
            f.write(final_content_lf)
        print(f"  ADDED header to {file_path}")
        return True
    except Exception as e:
        print(f"  Error processing file {file_path}: {e}", file=sys.stderr)
        return False

def process_modified_file(file_path, base_sha, head_sha):
    """Processes a modified file, updating authors but preserving license."""
    print(f"Processing MODIFIED file: {file_path}")
    _, ext = os.path.splitext(file_path)
    comment_prefix = COMMENT_PREFIXES.get(ext)
    if not comment_prefix:
        print(f"  Skipping (unsupported extension): {file_path}", file=sys.stderr)
        return False

    full_file_path = os.path.join(REPO_PATH, file_path)
    if not os.path.exists(full_file_path):
         print(f"  Skipping (file not found): {file_path}", file=sys.stderr)
         return False

    try:
        with open(full_file_path, 'r', encoding='utf-8-sig', errors='ignore') as f:
            original_content = f.read()

        # Extract existing authors from the file header
        existing_authors = extract_existing_authors(original_content, comment_prefix)
        print(f"  Found {len(existing_authors)} existing authors in header")

        existing_license = extract_license_identifier(original_content, comment_prefix)
        if not existing_license:
            # File was modified but had no header. Treat as ADDED with default license.
            print(f"  Warning: Modified file {file_path} has no existing license header. Applying default ({DEFAULT_LICENSE_LABEL}).")
            default_license_id = LICENSE_CONFIG.get(DEFAULT_LICENSE_LABEL, {}).get("id", "ERROR-UNKNOWN-DEFAULT")
            # Get *all* authors for this case (includes co-authors now)
            author_years, warnings = get_all_authors_for_file(file_path, REPO_PATH)
            if warnings:
                for warn in warnings: print(f"  Warning: {warn}", file=sys.stderr)

            # Combine with existing authors from header
            combined_authors = author_years.copy()
            for author, (min_year, max_year) in existing_authors.items():
                if author in combined_authors:
                    hist_min, hist_max = combined_authors[author]
                    combined_authors[author] = (min(hist_min, min_year), max(hist_max, max_year))
                else:
                    combined_authors[author] = (min_year, max_year)

            reuse_header = create_reuse_header(combined_authors, default_license_id, comment_prefix)
            license_id_to_use = default_license_id
        else:
            # File has a header, preserve license, update authors
            print(f"  Found existing license: {existing_license}")
            # Get all historical authors (includes co-authors now)
            all_author_years, warnings_all = get_all_authors_for_file(file_path, REPO_PATH)
            # Get authors from PR commits (includes co-authors now)
            pr_author_years, warnings_pr = get_pr_authors_for_file(file_path, base_sha, head_sha, REPO_PATH)

            if warnings_all or warnings_pr:
                for warn in warnings_all + warnings_pr: print(f"  Warning: {warn}", file=sys.stderr)

            # Combine authors from git history, existing header, and PR commits
            # Start with existing authors from header
            combined_authors = existing_authors.copy()

            # Add authors from git history
            for author, (hist_min, hist_max) in all_author_years.items():
                if author in combined_authors:
                    existing_min, existing_max = combined_authors[author]
                    combined_authors[author] = (min(existing_min, hist_min), max(existing_max, hist_max))
                else:
                    combined_authors[author] = (hist_min, hist_max)

            # Add authors from PR commits
            for author, (pr_min, pr_max) in pr_author_years.items():
                if author in combined_authors:
                    existing_min, existing_max = combined_authors[author]
                    combined_authors[author] = (min(existing_min, pr_min), max(existing_max, pr_max))
                else:
                    combined_authors[author] = (pr_min, pr_max)

            reuse_header = create_reuse_header(combined_authors, existing_license, comment_prefix)
            license_id_to_use = existing_license # Keep existing

        cleaned_content = remove_existing_reuse_header(original_content, comment_prefix)

        # Check if there's still a license identifier in the cleaned content
        # This can happen if there are multiple license identifiers in the file
        license_prefix = f"{comment_prefix} SPDX-License-Identifier:"
        lines = cleaned_content.splitlines()
        filtered_lines = []

        for line in lines:
            if not line.strip().startswith(license_prefix):
                filtered_lines.append(line)
            else:
                print(f"  Removing additional license identifier: {line.strip()}")

        cleaned_content = "\n".join(filtered_lines)
        separator = "\n\n" if cleaned_content.strip() else ""

        # Handle shebangs or initial comments for YAML
        if comment_prefix == '#' and cleaned_content.startswith('#'):
             new_content = reuse_header + "\n" + cleaned_content
        else:
             new_content = reuse_header + separator + cleaned_content

        final_content_lf = new_content.replace('\r\n', '\n').replace('\r', '\n')
        original_content_lf = original_content.replace('\r\n', '\n').replace('\r', '\n')

        if final_content_lf != original_content_lf:
            with open(full_file_path, 'w', encoding='utf-8', newline='\n') as f:
                f.write(final_content_lf)
            print(f"  UPDATED header for {file_path} (License: {license_id_to_use})")
            return True
        else:
            print(f"  Skipping (no changes needed): {file_path}")
            return False

    except Exception as e:
        print(f"  Error processing file {file_path}: {e}", file=sys.stderr)
        return False

if __name__ == "__main__":
    parser = argparse.ArgumentParser(description="Update REUSE headers for PR.")
    parser.add_argument("--files-added", nargs='*', default=[], help="List of added files.")
    parser.add_argument("--files-modified", nargs='*', default=[], help="List of modified files.")
    parser.add_argument("--pr-license", default=DEFAULT_LICENSE_LABEL, help="License to use for new files (mit or agpl).")
    parser.add_argument("--pr-base-sha", required=True, help="Base SHA of the PR.")
    parser.add_argument("--pr-head-sha", required=True, help="Head SHA of the PR.")

    args = parser.parse_args()

    print("Starting REUSE header update for PR...")
    print(f"Base SHA: {args.pr_base_sha}")
    print(f"Head SHA: {args.pr_head_sha}")

    # Determine license for new files
    new_file_license_label = args.pr_license.lower()
    if new_file_license_label not in LICENSE_CONFIG:
        print(f"  Warning: Unrecognized license '{new_file_license_label}', using default: {DEFAULT_LICENSE_LABEL}", file=sys.stderr)
        new_file_license_label = DEFAULT_LICENSE_LABEL

    print(f"Using license for new files: {new_file_license_label}")

    new_file_license_id = LICENSE_CONFIG.get(new_file_license_label, {}).get("id")
    if not new_file_license_id:
        print(f"FATAL: Could not find SPDX ID for license label '{new_file_license_label}'. Check LICENSE_CONFIG.", file=sys.stderr)
        sys.exit(1)

    print(f"License ID for NEW files: {new_file_license_id}")

    files_changed = False

    # Process Added Files
    print("\n--- Processing Added Files ---")
    for file in args.files_added:
        if process_added_file(file, new_file_license_id, args.pr_base_sha, args.pr_head_sha):
            files_changed = True

    # Process Modified Files
    print("\n--- Processing Modified Files ---")
    for file in args.files_modified:
        if process_modified_file(file, args.pr_base_sha, args.pr_head_sha):
            files_changed = True

    print("\n--- Summary ---")
    if files_changed:
        print("Script finished: Files were modified.")
    else:
        print("Script finished: No files needed changes.")

    print("----------------")
