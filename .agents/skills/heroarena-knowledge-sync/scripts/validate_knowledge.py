#!/usr/bin/env python3
"""Read-only structural checks for the HeroArena knowledge base; Python 3 stdlib only."""
import argparse
import hashlib
import json
import re
from pathlib import Path
from urllib.parse import unquote


def validate(root):
    root = Path(root).resolve()
    errors = []
    docs = [root / "AGENTS.md"] + sorted((root / "ProjectKnowledge").glob("*.md"))
    skill_files = sorted((root / ".agents/skills").glob("*/SKILL.md"))
    references = sorted((root / ".agents/skills").glob("*/references/*.md"))
    docs += [root / "ProjectKnowledge/archive/README.md"]
    for path in docs + skill_files + references:
        if not path.is_file():
            errors.append(f"Missing entry: {path.relative_to(root)}")
            continue
        text = path.read_text(encoding="utf-8-sig")
        if "\ufffd" in text:
            errors.append(f"Replacement character: {path.relative_to(root)}")
        # Current files use simple inline links. Archives preserve original bytes and are not interpreted as current guidance.
        text = re.sub(r"```.*?```", "", text, flags=re.S)
        for target in re.findall(r"\]\(([^)]+)\)", text):
            target = target.strip().strip("<>")
            if re.match(r"[A-Za-z][A-Za-z0-9+.-]*:", target) or target.startswith("#"):
                continue
            local = unquote(target.split("#", 1)[0])
            if local and not (path.parent / local).is_file():
                errors.append(f"Broken link: {path.relative_to(root)} -> {target}")
    for path in skill_files:
        text = path.read_text(encoding="utf-8-sig")
        match = re.match(r"^---\n(.*?)\n---\n", text, re.S)
        if not match:
            errors.append(f"Missing frontmatter: {path}")
            continue
        name = re.search(r"^name: (.+)$", match[1], re.M)
        description = re.search(r"^description: (.+)$", match[1], re.M)
        if not name or name[1] != path.parent.name or not re.fullmatch(r"[a-z0-9]+(?:-[a-z0-9]+)*", name[1]) or len(name[1]) > 64:
            errors.append(f"Invalid skill name: {path.parent.name}")
        if not description or not description[1].strip():
            errors.append(f"Missing skill description: {path.parent.name}")
    index = (root / "ProjectKnowledge/06_代码索引.md").read_text(encoding="utf-8")
    runtime = sorted((root / "Assets/Scripts").rglob("*.cs"))
    for path in runtime:
        if "../" + path.relative_to(root).as_posix() not in index:
            errors.append(f"Unindexed runtime script: {path.relative_to(root)}")
    archive = root / "ProjectKnowledge/archive/2026-10-17"
    manifest_path = archive / "manifest.json"
    if not manifest_path.is_file():
        errors.append("Missing archive manifest")
        manifest = {}
    else:
        manifest = json.loads(manifest_path.read_text(encoding="utf-8"))
    for name, expected in manifest.items():
        path = archive / name
        if not path.is_file():
            errors.append(f"Missing archive: {name}")
            continue
        raw = path.read_bytes()
        if len(raw) != expected["bytes"] or hashlib.sha256(raw).hexdigest() != expected["sha256"]:
            errors.append(f"Archive changed: {name}")
    route = (root / "ProjectKnowledge/10_AI协作规范.md").read_text(encoding="utf-8")
    for path in skill_files:
        if path.parent.name not in route:
            errors.append(f"Unrouted skill: {path.parent.name}")
    if errors:
        print("FAIL")
        for error in errors:
            print("- " + error)
        return 1
    print(f"PASS: {len(docs)} knowledge entries, {len(skill_files)} skills, {len(references)} references, {len(runtime)} runtime scripts, {len(manifest)} intact archives")
    print("Structural checks only; no Unity runtime tests or external license verification.")
    return 0


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--root", type=Path, default=Path(__file__).resolve().parents[4])
    raise SystemExit(validate(parser.parse_args().root))
