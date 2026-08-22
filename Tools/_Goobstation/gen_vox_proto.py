from pathlib import Path
import argparse
import yaml
import pyperclip

def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("audio_dir", type=Path)

    args = parser.parse_args()
    oggs = sorted(args.audio_dir.glob("*.ogg"), key=lambda p: p.stem.lower())

    if not oggs: parser.error(f"no ogg files found in {args.audio_dir}")

    words = [{"word": file.stem} for file in oggs]

    data = {
        "type": "voxVoice",
        "id": args.audio_dir.name,
        "basePath": str(args.audio_dir),
        "words": words
    }

    output = yaml.safe_dump(data, sort_keys=False, allow_unicode=True, default_flow_style=False)
    print(output)
    pyperclip.copy(output)


if __name__ == "__main__":
    main()