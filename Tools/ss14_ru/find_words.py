import os
import sys
import re

def clear_console():
    print("\033[H\033[J", end="")
clear_console()

folder_path = "../../Resources/Locale"

# Сбор всех .ftl
ftl_files = []
for root, dirs, files in os.walk(folder_path):
    for file in files:
        if file.endswith(".ftl"):
            ftl_files.append(os.path.join(root, file))

total_files = len(ftl_files)

# Крутой прогресс-бар
def print_progress(current, total, bar_length=20):
    fraction = current / total
    filled_length = int(bar_length * fraction)
    bar = "#" * filled_length + "-" * (bar_length - filled_length)
    percent = int(fraction * 100)
    sys.stdout.write(f"\r[{bar}] {percent}% ({current}/{total}) файлов проверено")
    sys.stdout.flush()

while True:
    search_word = input("Что ищем?\n").strip()
    if not search_word:
        print("Вы не ввели слово. Завершение работы.")
        break

    clear_console()
    print(f"Всего файлов для проверки: {total_files}\n")
    print(f"Ищем: '{search_word}'\n")

    # Регулярное выражение для точного совпадения
    pattern = re.compile(rf'\b{re.escape(search_word)}\b')

    matches = []

    # Проверка файлов
    for idx, file_path in enumerate(ftl_files, 1):
        try:
            with open(file_path, "r", encoding="utf-8", errors="ignore") as f:
                for i, line in enumerate(f, 1):
                    if pattern.search(line):
                        relative_path = os.path.join("Resources/Locale", os.path.relpath(file_path, folder_path))
                        matches.append(f"{relative_path} , строка {i}: {line.strip()}")
        except Exception as e:
            matches.append(f"Не удалось прочитать файл {file_path}: {e}")

        print_progress(idx, total_files)

    print("\n\n=== Совпадения ===")
    if matches:
        for match in matches:
            print(match)
    else:
        print("Совпадений не найдено.")
    print()
