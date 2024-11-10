import os
import yaml
import logging
from datetime import datetime
from pathlib import Path

# Set up logging
logging.basicConfig(
    level=logging.INFO,
    format='%(asctime)s - %(levelname)s - %(message)s',
    datefmt='%Y-%m-%d %H:%M:%S'
)
logger = logging.getLogger(__name__)

# Define paths using Path
script_dir = Path(__file__).parent
parts_dir = Path('../Resources/Changelog/Parts')
changelog_dir = Path('../Resources/Changelog')

def load_yaml_file(file_path: Path) -> dict | None:
    try:
        logger.debug(f"Loading YAML file: {file_path}")
        return yaml.safe_load(file_path.read_text(encoding='utf-8'))
    except Exception as e:
        logger.error(f"Error loading YAML file {file_path}: {e}")
        return None

def save_yaml_file(data: dict, file_path: Path) -> bool:
    try:
        logger.debug(f"Saving YAML file: {file_path}")
        file_path.write_text(
            yaml.dump(data, default_flow_style=False, allow_unicode=True),
            encoding='utf-8'
        )
        return True
    except Exception as e:
        logger.error(f"Error saving YAML file {file_path}: {e}")
        return False

def get_next_id(entries):
    try:
        ids = [entry.get('id') for entry in entries if isinstance(entry, dict) and 'id' in entry]
        return max(ids) + 1 if ids else 1
    except Exception as e:
        print(f"Error getting next ID: {e}")
        return 1

def update_changelog(part: dict, changelog_path: Path, next_id: int, part_mod_time: str) -> bool:
    try:
        logger.info(f"Updating changelog: {changelog_path}")
        changelog = load_yaml_file(changelog_path)
        if not changelog or 'Entries' not in changelog or not isinstance(changelog['Entries'], list):
            logger.error(f"Unexpected format in changelog file {changelog_path}")
            return False

        part_entry = {
            'author': part.get('author', 'Unknown'),
            'changes': part.get('changes', []),
            'id': next_id,
            'time': part_mod_time
        }
        logger.info(f"Adding new entry (ID: {next_id}) from author: {part_entry['author']}")
        changelog['Entries'].insert(0, part_entry)
        return save_yaml_file(changelog, changelog_path)
    except Exception as e:
        logger.error(f"Error updating changelog {changelog_path}: {e}")
        return False

def main():
    logger.info("Starting changelog update process")
    parts_files = list(parts_dir.glob('*.yml'))
    processed_count = 0
    error_count = 0

    for part_path in parts_files:
        logger.info(f"Processing file: {part_path.name}")
        part = load_yaml_file(part_path)

        if part is None:
            logger.error(f"Failed to parse: {part_path.name}")
            error_count += 1
            continue

        category = part.get('category', 'ShibaChangelog')
        changelog_path = changelog_dir / f"{category}.yml"

        if not changelog_path.exists():
            logger.error(f"Changelog file not found for category '{category}': {part_path.name}")
            error_count += 1
            continue

        changelog = load_yaml_file(changelog_path)
        if changelog is None or 'Entries' not in changelog or not isinstance(changelog['Entries'], list):
            logger.error(f"Unexpected format in changelog file: {changelog_path}")
            error_count += 1
            continue

        next_id = get_next_id(changelog['Entries'])
        part_mod_time = datetime.fromtimestamp(part_path.stat().st_mtime).isoformat()

        if update_changelog(part, changelog_path, next_id, part_mod_time):
            part_path.unlink()
            processed_count += 1
            logger.info(f"Successfully processed and removed: {part_path.name}")
        else:
            logger.error(f"Failed to update changelog: {part_path.name}")
            error_count += 1

    logger.info(f"Changelog update completed. Processed: {processed_count}, Errors: {error_count}")

if __name__ == '__main__':
    main()
