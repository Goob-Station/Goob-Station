import pathlib
import os
import glob
from file import FluentFile

class Project:
    def __init__(self):
        self.base_dir_path = pathlib.Path(os.path.abspath(os.curdir)).parent.parent.resolve()
        self.resources_dir_path = os.path.join(self.base_dir_path, 'Resources')
        self.locales_dir_path = os.path.join(self.resources_dir_path, 'Locale')
        self.ru_locale_dir_path = os.path.join(self.locales_dir_path, 'ru-RU')
        self.en_locale_dir_path = os.path.join(self.locales_dir_path, 'en-US')
        self.prototypes_dir_path = os.path.join(self.resources_dir_path, "Prototypes")
        self.en_locale_prototypes_dir_path = os.path.join(self.en_locale_dir_path)
        self.ru_locale_prototypes_dir_path = os.path.join(self.ru_locale_dir_path)

    def get_files_paths_by_dir(self, dir_path, files_extenstion):
        all_files = glob.glob(f'{dir_path}/**/*.{files_extenstion}', recursive=True)
        # Фильтруем только те, где есть подпапка с префиксом "_"
        filtered = [f for f in all_files if any(part.startswith('_') for part in pathlib.Path(f).parts)]
        return filtered


    def get_fluent_files_by_dir(self, dir_path):
        files_paths_list = glob.glob(f'{dir_path}/**/*.ftl', recursive=True)
        # Только те файлы, которые лежат в подпапках с префиксом "_"
        filtered_paths = [p for p in files_paths_list if any(part.startswith('_') for part in pathlib.Path(p).parts)]

        files = []
        for file_path in filtered_paths:
            try:
                files.append(FluentFile(file_path))
            except:
                continue

        return files

    def get_lang_file(self, en_file: FluentFile, lang_code: str) -> FluentFile:
        rel_path = os.path.relpath(en_file.full_path, self.en_locale_dir_path)
        target_path = os.path.join(self.locales_dir_path, lang_code, rel_path)
        return FluentFile(target_path)

