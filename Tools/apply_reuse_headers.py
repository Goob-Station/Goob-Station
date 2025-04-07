# apply_reuse_headers.py
import subprocess
import os
import sys
from datetime import datetime, timezone
from collections import defaultdict
import time
import concurrent.futures
import threading
import re

# --- Configuration ---
CUTOFF_COMMIT_HASH = "8270907bdc509a3fb5ecfecde8cc14e5845ede36"
LICENSE_BEFORE = "MIT"
LICENSE_AFTER = "AGPL-3.0-or-later"
FILE_PATTERNS = ["*.cs", "*.yaml", "*.yml"]
REPO_PATH = "."
MAX_WORKERS = os.cpu_count() or 4

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

# --- Helper Functions ---

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
        with progress_lock:
            all_warnings.append("FATAL: 'git' command not found. Make sure git is installed and in your PATH.")
        return None

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

def get_author_contribution_years(file_path, cwd=REPO_PATH):
    """
    Gets a dictionary mapping authors (including co-authors) to their first and last
    contribution years for a file.
    Returns: (dict like {"Author Name <email>": (min_year, max_year)}, list_of_warnings)
    """
    # Use %x1E (Record Separator) to split commits reliably
    # Include commit body (%b) to parse Co-authored-by lines
    # Use %an for author name, %ae for email
    command = ["git", "log", "--pretty=format:%at%x00%an%x00%ae%x00%b%x1E", "--follow", "--", file_path]
    # Regex to find Co-authored-by lines
    co_author_regex = re.compile(r"^Co-authored-by:\s*(.*?)\s*<([^>]+)>", re.IGNORECASE | re.MULTILINE)

    output = run_git_command(command, cwd=cwd, check=False) # Don't fail if file has no history
    author_timestamps = defaultdict(list)
    warnings = []

    if output is None or not output.strip():
        return {}, warnings # No history found

    commits = output.strip().split('\x1E') # Split into individual commit records

    for commit_data in commits:
        if not commit_data.strip():
            continue

        parts = commit_data.strip().split('\x00')
        if len(parts) < 4: # timestamp, name, email, body
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

        # Process co-authors from commit body
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
        if not timestamps:
            continue
        try:
            min_ts = min(timestamps)
            max_ts = max(timestamps)
            min_year = datetime.fromtimestamp(min_ts, timezone.utc).year
            max_year = datetime.fromtimestamp(max_ts, timezone.utc).year
            author_years[author] = (min_year, max_year) # Store tuple (min_year, max_year)
        except Exception as e:
             warnings.append(f"Error calculating year range for author {author} on file {file_path}: {e}")

    return author_years, warnings


def create_reuse_header(author_years, license_id, comment_prefix):
    """
    Creates the REUSE compliant header string, sorted by first contribution year,
    displaying the latest contribution year.
    """
    header_lines = []
    copyright_prefix = f"{comment_prefix} SPDX-FileCopyrightText:"
    if not author_years:
        header_lines.append(f"{copyright_prefix} Contributors to the DoobStation14 project")
    else:
        # Sort authors by their minimum (first) contribution year
        sorted_authors = sorted(author_years.items(), key=lambda item: item[1][0])
        for author, (min_year, max_year) in sorted_authors:
            # Display the maximum (latest) year in the copyright line
            year_string = str(max_year)
            clean_author = author.replace('\n', ' ').replace('\r', '')
            header_lines.append(f"{copyright_prefix} {year_string} {clean_author}")

    header_lines.append(f"{comment_prefix}") # Separator line
    header_lines.append(f"{comment_prefix} SPDX-License-Identifier: {license_id}")
    return "\n".join(header_lines)

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
        if in_header and (not is_header_line or i >= 20):
             in_header = False
        cleaned_lines.append(line)

    # Trim leading whitespace after removing header
    first_content_line_index = 0
    for i, line in enumerate(cleaned_lines):
        if line.strip():
            first_content_line_index = i
            break

    return "\n".join(cleaned_lines[first_content_line_index:]) if cleaned_lines else ""


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
    license_id = None
    status = 'skipped'
    comment_prefix = None

    _, ext = os.path.splitext(file_path)
    if ext == '.cs': comment_prefix = '//'
    elif ext in ['.yaml', '.yml']: comment_prefix = '#'
    else:
        file_warnings.append(f"Skipped (Unsupported Extension): {file_path}")
        status = 'skipped_unsupported'
        with progress_lock:
            skipped_count += 1
            all_warnings.extend(file_warnings)
            progress_count_snapshot = processed_count + skipped_count + error_count
        print_progress(progress_count_snapshot)
        return status

    full_file_path = os.path.join(REPO_PATH, file_path)
    if not os.path.exists(full_file_path):
        file_warnings.append(f"Skipped (Not Found): {file_path}")
        status = 'skipped_not_found'
    else:
        last_commit_timestamp = get_last_commit_timestamp(file_path, REPO_PATH)
        if last_commit_timestamp is None:
            file_warnings.append(f"Warning (No Timestamp): Assuming AGPL for {file_path}")
            license_id = LICENSE_AFTER
        else:
            license_id = LICENSE_AFTER if last_commit_timestamp > cutoff_ts else LICENSE_BEFORE

        # Use the updated function which includes co-authors
        author_years, author_warnings = get_author_contribution_years(file_path, REPO_PATH)
        file_warnings.extend(author_warnings)
        if not author_years and not author_warnings:
             file_warnings.append(f"Warning (No Authors): Using generic copyright for {file_path}")

        # Pass the author_years dict (containing min/max tuples)
        reuse_header = create_reuse_header(author_years, license_id, comment_prefix)

        try:
            with open(full_file_path, 'r', encoding='utf-8-sig', errors='ignore') as f:
                original_content = f.read()
            cleaned_content = remove_existing_reuse_header(original_content, comment_prefix)
            separator = "\n\n" if cleaned_content.strip() else "" # Add extra newline for new files

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
                status = 'updated'
            else:
                status = 'skipped_no_change'
        except Exception as e:
            file_warnings.append(f"Error processing file {file_path}: {e}")
            status = 'error'

    with progress_lock:
        current_total_processed = processed_count + skipped_count + error_count + 1
        if status == 'updated':
            processed_count += 1
            last_file_processed = file_path
            last_license_type = license_id
            if license_id == LICENSE_BEFORE: mit_count += 1
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
