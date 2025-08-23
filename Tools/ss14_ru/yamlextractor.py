
import os
import logging
import glob
from file import YAMLFile
from project import Project

class YAMLExtractor:
    def __init__(self, yaml_files):
        self.yaml_files = yaml_files

    def execute(self):
        for yaml_file in self.yaml_files:
            self.create_en_fluent_file(yaml_file)


    def create_en_fluent_file(self, yaml_file):
        import re
        from fluent.syntax import FluentSerializer

        serializer = FluentSerializer(with_junk=True)
        relative_parent_dir = yaml_file.get_relative_parent_dir(project.prototypes_dir_path).lower()
        parsed_data = yaml_file.parse_data(yaml_file.read_data())
        serialized = serializer.serialize(parsed_data)

        # Получаем подпапку-префикс _ПОДПАПКА
        en_locale_subdir = yaml_file.full_path.split(os.sep)
        try:
            subdir = next(p for p in en_locale_subdir if p.startswith("_"))
        except StopIteration:
            subdir = "_general"

        # Полный путь к новому файлу
        en_new_dir_path = os.path.join(
            project.base_dir_path, "Resources", "Locale", "en-US", subdir, "prototypes", relative_parent_dir
        )
        os.makedirs(en_new_dir_path, exist_ok=True)

        en_file_name = os.path.splitext(os.path.basename(yaml_file.full_path))[0] + ".ftl"
        en_file_path = os.path.join(en_new_dir_path, en_file_name)

        with open(en_file_path, "w", encoding="utf-8") as f:
            f.write(serialized)

        logging.info(f"Создан en-файл: {en_file_path}")
        return en_file_path

logging.basicConfig(level = logging.INFO)
project = Project()

yaml_files_paths = project.get_files_paths_by_dir(project.prototypes_dir_path, 'yml')
yaml_files = list(map(lambda yaml_file_path: YAMLFile(yaml_file_path), yaml_files_paths))

if __name__ == "__main__":
    yaml_files = []  # Заглушка
    extractor = YAMLExtractor(yaml_files)
    extractor.execute()
