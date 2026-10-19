import sys

patch_file = r"C:\Users\Tyler\.gemini\antigravity-ide\brain\0906803d-92d8-48b2-9098-798a0b2caf34\.system_generated\steps\147\content.md"
target_file = sys.argv[1]

with open(patch_file, "r", encoding="utf-8") as f:
    printing = False
    for line in f:
        if line.startswith("diff --git a/"):
            if target_file in line:
                printing = True
            else:
                printing = False
        if printing:
            print(line, end="")
