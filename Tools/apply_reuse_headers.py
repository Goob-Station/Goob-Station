# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later
#!/usr/bin/env python3
# apply_reuse_headers.py - A script to add REUSE headers to C# files

import os
import sys
import re
import subprocess
import argparse
from datetime import datetime, timezone
from collections import defaultdict
import concurrent.futures
import threading
import time

# --- Configuration ---
CUTOFF_COMMIT_HASH = "8270907bdc509a3fb5ecfecde8cc14e5845ede36"
LICENSE_BEFORE = "MIT"
LICENSE_AFTER = "AGPL-3.0-or-later"
FILE_PATTERNS = ["*.cs", "*.js", "*.ts", "*.jsx", "*.tsx", "*.c", "*.cpp", "*.cc", "*.h", "*.hpp",
                "*.java", "*.scala", "*.kt", "*.swift", "*.go", "*.rs", "*.dart", "*.groovy", "*.php",
                "*.yaml", "*.yml", "*.ftl", "*.py", "*.rb", "*.pl", "*.pm", "*.sh", "*.bash", "*.zsh",
                "*.fish", "*.ps1", "*.r", "*.rmd", "*.jl", "*.tcl", "*.perl", "*.conf", "*.toml",
                "*.ini", "*.cfg", "*.bat", "*.cmd", "*.vb", "*.vbs", "*.bas", "*.asm", "*.s", "*.lisp",
                "*.clj", "*.f", "*.f90", "*.m", "*.sql", "*.ada", "*.adb", "*.ads", "*.hs", "*.lhs",
                "*.lua", "*.xaml", "*.xml", "*.html", "*.htm", "*.svg", "*.css", "*.scss", "*.sass",
                "*.less", "*.md", "*.markdown", "*.csproj", "*.DotSettings"]
REPO_PATH = "."
MAX_WORKERS = os.cpu_count() or 4

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
    ".csproj": ("<!--", "-->"),
    ".DotSettings": ("<!--", "-->"),
}

# --- Shared State and Lock ---
progress_lock = threading.Lock()
processed_count = 0
skipped_count = 0
error_count = 0
mit_count = 0
agpl_count = 0
last_file_processed = ""
last_license_type = ""
all_warnings = []
total_files = 0

# --- Helper Functions (Copied from update_pr_reuse_headers.py) ---

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
        with progress_lock:
            all_warnings.append("FATAL: 'git' command not found. Make sure git is installed and in your PATH.")
        return None

def get_authors_from_git(file_path, cwd=REPO_PATH):
    """
    Gets authors and their contribution years for a specific file.
    Returns: dict like {"Author Name <email>": (min_year, max_year)}
    """
    # Always get all authors
    command = ["git", "log", "--pretty=format:%at|%an|%ae|%b", "--follow", "--", file_path]

    output = run_git_command(command, cwd=cwd, check=False)
    if not output:
        # Try to get the current user from git config as a fallback
        try:
            name_cmd = ["git", "config", "user.name"]
            email_cmd = ["git", "config", "user.email"]
            user_name = run_git_command(name_cmd, cwd=cwd, check=False)
            user_email = run_git_command(email_cmd, cwd=cwd, check=False)

            if user_name and user_email and user_name.strip() != "Unknown":
                # Use current year
                current_year = datetime.now(timezone.utc).year
                return {f"{user_name} <{user_email}>": (current_year, current_year)}
            else:
                print("Warning: Could not get current user from git config or name is 'Unknown'")
                return {}
        except Exception as e:
            print(f"Error getting git user: {e}")
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
            lines.append(f"{prefix} SPDX-FileCopyrightText: Contributors to the DoobStation14 project")

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
            lines.append(f"SPDX-FileCopyrightText: Contributors to the DoobStation14 project")

        # Add separator
        lines.append("")

        # Add license line
        lines.append(f"SPDX-License-Identifier: {license_id}")

        # End comment
        lines.append(f"{suffix}")

    return "\n".join(lines)

def get_commit_timestamp(commit_hash, cwd=REPO_PATH):
    """Gets the Unix timestamp of a specific commit."""
    output = run_git_command(["git", "show", "-s", "--format=%ct", commit_hash], cwd=cwd)
    if output:
        try:
            return int(output)
        except ValueError:
            with progress_lock:
                 all_warnings.append(f"Error: Could not parse timestamp from git output for commit {commit_hash}: {output}")
            return None
    return None

def get_last_commit_timestamp(file_path, cwd=REPO_PATH):
    """Gets the Unix timestamp of the last commit that modified the file."""
    output = run_git_command(["git", "log", "-1", "--format=%ct", "--follow", "--", file_path], cwd=cwd)
    if output:
        try:
            # Handle potential multiple lines if file history is complex, take the first timestamp
            return int(output.split('\n')[0])
        except (ValueError, IndexError):
            with progress_lock:
                all_warnings.append(f"Warning: Could not parse last commit timestamp for {file_path}")
            return None
    return None

def print_progress(current_processed_count, bar_length=40):
    """Prints the progress status block (thread-safe access to globals)."""
    if total_files == 0: percent = 0
    else: percent = 100 * (current_processed_count / float(total_files))
    filled_length = int(bar_length * current_processed_count // total_files) if total_files > 0 else 0
    bar = '#' * filled_length + '-' * (bar_length - filled_length)
    with progress_lock:
        mit = mit_count
        agpl = agpl_count
        last_f = last_file_processed
        last_l = last_license_type
    progress_str = (
        f"Processed: {current_processed_count}/{total_files} | "
        f"MIT: {mit} | AGPL: {agpl} | "
        f"Last: {os.path.basename(last_f) if last_f else 'N/A'} ({last_l if last_l else 'N/A'}) | "
        f"[{bar}] {percent:.1f}%"
    )
    sys.stdout.write(f"\x1b[2K{progress_str}\r")
    sys.stdout.flush()

def process_file(file_path_tuple):
    """Processes a single file. Designed to be run in a thread pool."""
    global processed_count, skipped_count, error_count, mit_count, agpl_count
    global last_file_processed, last_license_type, all_warnings

    file_path, cutoff_ts = file_path_tuple
    file_warnings = []
    status = 'skipped'
    comment_prefix = None

    # Check file extension
    _, ext = os.path.splitext(file_path)
    comment_style = COMMENT_STYLES.get(ext)
    if not comment_style:
        file_warnings.append(f"Skipped (Unsupported Extension): {file_path}")
        status = 'skipped_unsupported'
        with progress_lock:
            skipped_count += 1
            all_warnings.extend(file_warnings)
            progress_count_snapshot = processed_count + skipped_count + error_count
        print_progress(progress_count_snapshot)
        return status

    # Check if file exists
    full_path = os.path.join(REPO_PATH, file_path)
    if not os.path.exists(full_path):
        file_warnings.append(f"Skipped (Not Found): {file_path}")
        status = 'skipped_not_found'
    else:
        # Read file content
        with open(full_path, 'r', encoding='utf-8-sig', errors='ignore') as f:
            content = f.read()

        # Parse existing header if any
        existing_authors, existing_license, header_lines = parse_existing_header(content, comment_style)

        # Get all authors from git
        git_authors = get_authors_from_git(file_path)

        # Add current user to authors
        try:
            name_cmd = ["git", "config", "user.name"]
            email_cmd = ["git", "config", "user.email"]
            user_name = run_git_command(name_cmd, check=False)
            user_email = run_git_command(email_cmd, check=False)

            if user_name and user_email and user_name.strip() != "Unknown":
                current_year = datetime.now(timezone.utc).year
                current_user = f"{user_name} <{user_email}>"
                if current_user not in git_authors:
                    git_authors[current_user] = (current_year, current_year)
                    print(f"  Added current user: {current_user}")
                else:
                    min_year, max_year = git_authors[current_user]
                    git_authors[current_user] = (min(min_year, current_year), max(max_year, current_year))
            else:
                print("Warning: Could not get current user from git config or name is 'Unknown'")
        except Exception as e:
            print(f"Error getting git user: {e}")

        # Determine license based on cutoff commit
        last_commit_timestamp = get_last_commit_timestamp(file_path, REPO_PATH)
        if last_commit_timestamp is None:
            file_warnings.append(f"Warning (No Timestamp): Assuming AGPL for {file_path}")
            determined_license_id = LICENSE_AFTER
        else:
            determined_license_id = LICENSE_AFTER if last_commit_timestamp > cutoff_ts else LICENSE_BEFORE

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

            license_id_used = existing_license
        else:
            print(f"Adding new header to {file_path} (License: {determined_license_id})")

            # Create new header with determined license
            new_header = create_header(git_authors, determined_license_id, comment_style)

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

            license_id_used = determined_license_id

        # Check if content changed
        if new_content == content:
            print(f"No changes needed for {file_path}")
            status = 'skipped_no_change'
        else:
            # Write updated content
            try:
                with open(full_path, 'w', encoding='utf-8', newline='\n') as f:
                    f.write(new_content)
                status = 'updated'
            except Exception as e:
                file_warnings.append(f"Error writing file {file_path}: {e}")
                status = 'error'

    # Update progress
    with progress_lock:
        current_total_processed = processed_count + skipped_count + error_count + 1
        if status == 'updated':
            processed_count += 1
            last_file_processed = file_path
            last_license_type = license_id_used
            if license_id_used == LICENSE_BEFORE: mit_count += 1
            else: agpl_count += 1
        elif status == 'error': error_count += 1
        else: skipped_count += 1
        all_warnings.extend(file_warnings)
        progress_count_snapshot = current_total_processed
    print_progress(progress_count_snapshot)
    return status

# --- Main Script ---
if __name__ == "__main__":
    print("Starting REUSE header update process (multithreaded)...")
    print("Fetching file list...")
    cutoff_timestamp = get_commit_timestamp(CUTOFF_COMMIT_HASH, REPO_PATH)
    if cutoff_timestamp is None:
        print(f"\nFATAL: Could not get timestamp for cutoff commit {CUTOFF_COMMIT_HASH}. Aborting.", file=sys.stderr)
        if any("FATAL: 'git' command not found" in w for w in all_warnings): print("Git command was not found.", file=sys.stderr)
        exit(1)
    cutoff_dt = datetime.fromtimestamp(cutoff_timestamp, timezone.utc)
    print(f"Cutoff commit: {CUTOFF_COMMIT_HASH} ({cutoff_dt.strftime('%Y-%m-%d %H:%M:%S %Z')})")
    git_command = ["git", "ls-files"] + FILE_PATTERNS
    files_output = run_git_command(git_command, cwd=REPO_PATH)
    if files_output is None:
        print("\nError: Could not list files using git ls-files. Aborting.", file=sys.stderr)
        exit(1)
    target_files = [line for line in files_output.splitlines() if line.strip()]
    total_files = len(target_files)
    if not target_files:
        print("No C#, YAML, or YML files found matching the patterns.")
        exit(0)
    print(f"Found {total_files} files to process using up to {MAX_WORKERS} workers.")
    time.sleep(1)
    tasks = [(file_path, cutoff_timestamp) for file_path in target_files]
    with concurrent.futures.ThreadPoolExecutor(max_workers=MAX_WORKERS) as executor:
        list(executor.map(process_file, tasks))
    sys.stdout.write("\n")
    if all_warnings:
        print("\n--- Warnings/Errors Encountered ---")
        max_warnings_to_show = 50
        shown_warnings = 0
        unique_warnings = sorted(list(set(all_warnings)))
        for warning in unique_warnings:
            if shown_warnings < max_warnings_to_show: print(warning, file=sys.stderr); shown_warnings += 1
            elif shown_warnings == max_warnings_to_show: print(f"... (truncated {len(unique_warnings) - max_warnings_to_show} more unique warnings)", file=sys.stderr); shown_warnings += 1; break
        print("---------------------------------")
    print("\n--- Processing Summary ---")
    print(f"Total Files Scanned: {total_files}")
    print(f"Headers Updated: {processed_count}")
    print(f"Skipped (not found/no change/unsupported): {skipped_count}")
    print(f"Errors during processing: {error_count}")
    print(f"MIT Licenses Applied: {mit_count}")
    print(f"AGPL Licenses Applied: {agpl_count}")
    print("--------------------------")
    print("Script finished.")
