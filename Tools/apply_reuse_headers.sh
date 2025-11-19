# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
#
# SPDX-License-Identifier: MPL-2.0

#!/usr/bin/env bash

set -euo pipefail

# ============================================================================
# CONFIGURATION SECTION - Modify these to customize behavior
# ============================================================================

CURRENT_YEAR=$(date +%Y)
DEFAULT_AUTHOR="Space Station 14 Contributors"
DEFAULT_LICENSE="MIT-WIZARDS"

declare -A LICENSE_CONFIG=(
    ["mit-wizards"]="MIT-WIZARDS"
    ["mpl"]="MPL-2.0"
)

declare -a DIRECTORY_RULES=(
    "Content.Goobstation.*/|Goob Station Contributors|mpl"
    "_Goobstation|Goob Station Contributors|mpl"
)

declare -a EXCLUDE_DIRS=(
    "RobustToolbox"
    ".git"
    "bin"
    "obj"
)

declare -A COMMENT_STYLES=(
    # C-style single-line comments
    [".cs"]="//|"
    [".js"]="//|"
    [".ts"]="//|"
    [".jsx"]="//|"
    [".tsx"]="//|"
    [".c"]="//|"
    [".cpp"]="//|"
    [".cc"]="//|"
    [".h"]="//|"
    [".hpp"]="//|"
    [".java"]="//|"
    [".scala"]="//|"
    [".kt"]="//|"
    [".swift"]="//|"
    [".go"]="//|"
    [".rs"]="//|"
    [".dart"]="//|"
    [".groovy"]="//|"
    [".php"]="//|"

    # Hash-style single-line comments
    [".yaml"]="#|"
    [".yml"]="#|"
    [".ftl"]="#|"
    [".py"]="#|"
    [".rb"]="#|"
    [".pl"]="#|"
    [".pm"]="#|"
    [".sh"]="#|"
    [".bash"]="#|"
    [".zsh"]="#|"
    [".fish"]="#|"
    [".ps1"]="#|"
    [".r"]="#|"
    [".rmd"]="#|"
    [".jl"]="#|"
    [".tcl"]="#|"
    [".perl"]="#|"
    [".conf"]="#|"
    [".toml"]="#|"
    [".ini"]="#|"
    [".cfg"]="#|"
    [".gitignore"]="#|"
    [".dockerignore"]="#|"

    # Other single-line comment styles
    [".bat"]="REM|"
    [".cmd"]="REM|"
    [".vb"]="'|"
    [".vbs"]="'|"
    [".bas"]="'|"
    [".asm"]=";|"
    [".s"]=";|"
    [".lisp"]=";|"
    [".clj"]=";|"
    [".f"]="!|"
    [".f90"]="!|"
    [".m"]="%|"
    [".sql"]="--|"
    [".ada"]="--|"
    [".adb"]="--|"
    [".ads"]="--|"
    [".hs"]="--|"
    [".lhs"]="--|"
    [".lua"]="--|"

    # Multi-line comment styles
    [".xaml"]="<!--|-->"
    [".xml"]="<!--|-->"
    [".html"]="<!--|-->"
    [".htm"]="<!--|-->"
    [".svg"]="<!--|-->"
    [".css"]="/*|*/"
    [".scss"]="/*|*/"
    [".sass"]="/*|*/"
    [".less"]="/*|*/"
    [".md"]="<!--|-->"
    [".markdown"]="<!--|-->"
)

# Global variables for command-line options
VERBOSE=0
DRY_RUN=0
FORCE=0
LICENSE_OVERRIDE=""
SKIP_EXISTING=1
PARALLEL=0
NUM_JOBS=$(nproc 2>/dev/null || sysctl -n hw.ncpu 2>/dev/null || echo 4)

# ============================================================================
# HELPER FUNCTIONS
# ============================================================================

log_verbose() {
    if [ "$VERBOSE" -eq 1 ]; then
        echo "  $*" >&2
    fi
}

show_usage() {
    cat << EOF
Usage: $(basename "$0") [OPTIONS] [DIRECTORY]

Apply REUSE-compliant SPDX headers to source files recursively.

ARGUMENTS:
  DIRECTORY              Target directory to process (default: current directory)

OPTIONS:
  -h, --help            Show this help message
  -v, --verbose         Enable verbose output
  -n, --dry-run         Show what would be done without making changes
  -f, --force           Force overwrite existing headers
  -l, --license LICENSE Override license for all files (mit or mpl)
  -a, --author AUTHOR   Override author for all files
  -p, --parallel        Enable parallel processing for faster execution
  -j, --jobs NUM        Number of parallel jobs (default: auto-detect CPU cores)
  --no-skip             Process files even if they already have headers

EXAMPLES:
  $(basename "$0") .                    # Process current directory
  $(basename "$0") -v /path/to/code     # Process with verbose output
  $(basename "$0") -l mpl -a "Me" src/  # Override license and author
  $(basename "$0") -n Resources/        # Dry run on Resources/
  $(basename "$0") -f Content.Client/   # Force reprocess existing headers

CONFIGURATION:
  Edit the CONFIGURATION SECTION at the top of this script to:
  - Add/modify license types
  - Configure directory-based rules
  - Add support for new file extensions
  - Change default author/license
  - Configure excluded directories (EXCLUDE_DIRS)

EOF
}

get_comment_style() {
    local ext="$1"
    local style="${COMMENT_STYLES[$ext]:-}"

    if [ -z "$style" ]; then
        echo "" ""
        return
    fi

    local prefix="${style%%|*}"
    local suffix="${style#*|}"

    echo "$prefix" "$suffix"
}

matches_pattern() {
    local file_path="$1"
    local pattern="$2"

    if [[ "$pattern" == *"*"* ]] || [[ "$pattern" == *"?"* ]]; then
        if [[ "$file_path" == *"$pattern"* ]]; then
            return 0
        fi

        local path_parts
        IFS='/' read -ra path_parts <<< "$file_path"

        for ((i=0; i<${#path_parts[@]}; i++)); do
            local partial_path=""
            for ((j=i; j<${#path_parts[@]}; j++)); do
                if [ -n "$partial_path" ]; then
                    partial_path="$partial_path/${path_parts[$j]}"
                else
                    partial_path="${path_parts[$j]}"
                fi
            done
            partial_path="$partial_path/"

            if [[ "$partial_path" == $pattern* ]]; then
                return 0
            fi
        done

        return 1
    else
        [[ "$file_path" == *"$pattern"* ]]
    fi
}

get_author_and_license() {
    local file_path="$1"
    local normalized_path="${file_path//\\//}"

    for rule in "${DIRECTORY_RULES[@]}"; do
        local pattern="${rule%%|*}"
        local rest="${rule#*|}"
        local author="${rest%%|*}"
        local license_label="${rest##*|}"

        if matches_pattern "$normalized_path" "$pattern"; then
            log_verbose "Matched pattern '$pattern' -> Author: $author, License: $license_label"

            local license_id="${LICENSE_CONFIG[$license_label]:-}"
            if [ -z "$license_id" ]; then
                log_verbose "Warning: Unknown license '$license_label', using default: $DEFAULT_LICENSE"
                license_id="$DEFAULT_LICENSE"
            fi

            printf "%s\t%s\n" "$author" "$license_id"
            return
        fi
    done

    log_verbose "No pattern match, using defaults -> Author: $DEFAULT_AUTHOR, License: $DEFAULT_LICENSE"
    printf "%s\t%s\n" "$DEFAULT_AUTHOR" "$DEFAULT_LICENSE"
}

has_reuse_header() {
    local file="$1"
    local prefix="$2"
    local suffix="$3"

    local lines_to_check=10
    if [ -z "$suffix" ]; then
        lines_to_check=5
    fi

    if head -n "$lines_to_check" "$file" 2>/dev/null | grep -q "SPDX-License-Identifier:"; then
        return 0
    fi

    return 1
}

create_header() {
    local author="$1"
    local license="$2"
    local prefix="$3"
    local suffix="$4"

    if [ -z "$suffix" ]; then
        echo "${prefix} SPDX-FileCopyrightText: ${CURRENT_YEAR} ${author}"
        echo "${prefix}"
        echo "${prefix} SPDX-License-Identifier: ${license}"
    else
        echo "${prefix}"
        echo "SPDX-FileCopyrightText: ${CURRENT_YEAR} ${author}"
        echo ""
        echo "SPDX-License-Identifier: ${license}"
        echo "${suffix}"
    fi
}

process_file() {
    local file="$1"
    local ext="${file##*.}"

    if [ "$file" = "$ext" ]; then
        local basename
        basename=$(basename "$file")
        case "$basename" in
            .gitignore|.dockerignore)
                ext="$basename"
                ;;
            *)
                log_verbose "Skipping $file - no extension"
                return 0
                ;;
        esac
    else
        ext=".$ext"
    fi

    local prefix suffix
    read -r prefix suffix < <(get_comment_style "$ext")

    if [ -z "$prefix" ]; then
        log_verbose "Skipping $file - unsupported file type"
        return 0
    fi

    if [ "$SKIP_EXISTING" -eq 1 ] && [ "$FORCE" -eq 0 ]; then
        if has_reuse_header "$file" "$prefix" "$suffix"; then
            echo "Skipping $file - already has REUSE header"
            return 0
        fi
    fi

    local author license

    if [ -n "$LICENSE_OVERRIDE" ]; then
        license="$LICENSE_OVERRIDE"
        log_verbose "Using license override: $license"
    fi

    if [ -n "${AUTHOR_OVERRIDE:-}" ]; then
        author="$AUTHOR_OVERRIDE"
        if [ -z "$license" ]; then
            IFS=$'\t' read -r _ license < <(get_author_and_license "$file")
        fi
        log_verbose "Using author override: $author"
    else
        IFS=$'\t' read -r author license < <(get_author_and_license "$file")
        if [ -n "$LICENSE_OVERRIDE" ]; then
            license="$LICENSE_OVERRIDE"
        fi
    fi

    echo "Adding header to $file (Author: $author, License: $license)"

    if [ "$DRY_RUN" -eq 1 ]; then
        return 0
    fi

    local header
    header=$(create_header "$author" "$license" "$prefix" "$suffix")

    local temp_file="${file}.tmp"

    if [ -s "$file" ]; then
        if [ -n "$suffix" ] && head -n 1 "$file" 2>/dev/null | grep -q "^<?xml"; then
            local xml_line
            xml_line=$(head -n 1 "$file")
            {
                echo "$xml_line"
                echo "$header"
                echo ""
                tail -n +2 "$file"
            } > "$temp_file"
        else
            {
                echo "$header"
                echo ""
                cat "$file"
            } > "$temp_file"
        fi
    else
        echo "$header" > "$temp_file"
    fi

    mv "$temp_file" "$file"
    echo "Updated $file"

    return 1
}

# ============================================================================
# MAIN FUNCTION
# ============================================================================

main() {
    local target_dir="."

    while [[ $# -gt 0 ]]; do
        case "$1" in
            -h|--help)
                show_usage
                exit 0
                ;;
            -v|--verbose)
                VERBOSE=1
                shift
                ;;
            -n|--dry-run)
                DRY_RUN=1
                shift
                ;;
            -f|--force)
                FORCE=1
                SKIP_EXISTING=0
                shift
                ;;
            -l|--license)
                if [ -n "${2:-}" ]; then
                    local license_key="${2,,}"
                    if [ -n "${LICENSE_CONFIG[$license_key]:-}" ]; then
                        LICENSE_OVERRIDE="${LICENSE_CONFIG[$license_key]}"
                        shift 2
                    else
                        echo "Error: Unknown license '$2'. Valid options: ${!LICENSE_CONFIG[*]}" >&2
                        exit 1
                    fi
                else
                    echo "Error: --license requires an argument" >&2
                    exit 1
                fi
                ;;
            -a|--author)
                if [ -n "${2:-}" ]; then
                    AUTHOR_OVERRIDE="$2"
                    shift 2
                else
                    echo "Error: --author requires an argument" >&2
                    exit 1
                fi
                ;;
            -p|--parallel)
                PARALLEL=1
                shift
                ;;
            -j|--jobs)
                if [ -n "${2:-}" ]; then
                    NUM_JOBS="$2"
                    PARALLEL=1
                    shift 2
                else
                    echo "Error: --jobs requires an argument" >&2
                    exit 1
                fi
                ;;
            --no-skip)
                SKIP_EXISTING=0
                shift
                ;;
            -*)
                echo "Error: Unknown option '$1'" >&2
                show_usage
                exit 1
                ;;
            *)
                target_dir="$1"
                shift
                ;;
        esac
    done

    if [ ! -d "$target_dir" ]; then
        echo "Error: Directory '$target_dir' does not exist" >&2
        exit 1
    fi

    echo "============================================"
    echo "REUSE Header Application Tool"
    echo "============================================"
    echo "Target directory: $target_dir"
    echo "Current year: $CURRENT_YEAR"
    [ "$DRY_RUN" -eq 1 ] && echo "Mode: DRY RUN (no changes will be made)"
    [ "$FORCE" -eq 1 ] && echo "Mode: FORCE (will overwrite existing headers)"
    [ "$PARALLEL" -eq 1 ] && echo "Parallel processing: enabled ($NUM_JOBS jobs)"
    [ -n "$LICENSE_OVERRIDE" ] && echo "License override: $LICENSE_OVERRIDE"
    [ -n "${AUTHOR_OVERRIDE:-}" ] && echo "Author override: $AUTHOR_OVERRIDE"
    echo "============================================"
    echo ""

    # Export functions and variables for parallel processing
    export CURRENT_YEAR DEFAULT_AUTHOR DEFAULT_LICENSE
    export VERBOSE DRY_RUN FORCE LICENSE_OVERRIDE SKIP_EXISTING
    export -f get_comment_style matches_pattern get_author_and_license
    export -f has_reuse_header create_header process_file log_verbose
    
    # Export arrays
    export LICENSE_CONFIG COMMENT_STYLES DIRECTORY_RULES
    
    local find_cmd="find \"$target_dir\""
    
    for exclude_dir in "${EXCLUDE_DIRS[@]}"; do
        find_cmd="$find_cmd -path \"*/$exclude_dir\" -prune -o"
    done
    
    find_cmd="$find_cmd -type f -print0"
    
    local file_count=0
    local processed_count=0
    
    if [ "$PARALLEL" -eq 1 ]; then
        # Parallel processing with xargs
        eval "$find_cmd" 2>/dev/null | xargs -0 -P "$NUM_JOBS" -I {} bash -c 'process_file "$@"' _ {}
        
        # Count files after processing
        file_count=$(eval "$find_cmd" 2>/dev/null | tr '\0' '\n' | wc -l)
        processed_count="N/A (parallel mode)"
    else
        # Sequential processing
        while IFS= read -r -d '' file; do
            if [ -f "$file" ]; then
                file_count=$((file_count + 1))
                if ! process_file "$file"; then
                    processed_count=$((processed_count + 1))
                fi
            fi
        done < <(eval "$find_cmd" 2>/dev/null)
    fi

    echo ""
    echo "============================================"
    echo "Summary"
    echo "============================================"
    echo "Total files found: $file_count"
    if [ "$PARALLEL" -eq 1 ]; then
        echo "Parallel processing completed"
    else
        echo "Files processed: $processed_count"
        echo "Files skipped: $((file_count - processed_count))"
    fi
    [ "$DRY_RUN" -eq 1 ] && echo "(No actual changes made - dry run mode)"
    echo "============================================"
}

if [ "${BASH_SOURCE[0]}" = "${0}" ]; then
    main "$@"
fi
