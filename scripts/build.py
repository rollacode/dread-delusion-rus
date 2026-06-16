#!/usr/bin/env python3
"""Build resources.assets from source/ru/*.txt (RU translation source of truth).

Usage:
  py -3 scripts/build.py            # build -> build/resources.assets
  py -3 scripts/build.py --deploy   # also copy into the live game
  py -3 scripts/build.py --zip      # also (re)pack dist zip's resources.assets

The build is deterministic: it injects each source/ru/<TextAsset>.txt back into a
clean English base (resources.assets.orig_backup) via UnityPy. Editing the RU
text files and rebuilding is the whole workflow.
"""
import UnityPy, os, sys, glob, shutil

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
GAME = r"F:\SteamLibrary\steamapps\common\Dread Delusion\windows_content"
DATA = os.path.join(GAME, "Dread Delusion_Data")
BASE = os.path.join(DATA, "resources.assets.orig_backup")   # clean English assets
RU   = os.path.join(ROOT, "source", "ru")
OUT  = os.path.join(ROOT, "build", "resources.assets")

# The whole UI (menus, item names, stats) is drawn with the legacy dynamic Font
# `alagard_by_pix3m-d6awiwp`, whose embedded TTF has NO Cyrillic — so Unity auto-
# substitutes its system sans (LiberationSans) for Russian letters. We swap that TTF
# for an alagard build that includes Cyrillic, so the UI renders Russian natively in
# alagard. (TMP text uses a separate SDF font, handled by the BepInEx fallback plugin.)
FONT_NAME = "alagard_by_pix3m-d6awiwp"
FONT_TTF  = os.path.join(ROOT, "fonts", "alagard-unicode.ttf")

def read(path):
    with open(path, encoding="utf-8", newline="") as f:
        return f.read()

def main():
    if not os.path.exists(BASE):
        sys.exit("ERROR: clean base not found: " + BASE)
    ru = {}
    for f in glob.glob(os.path.join(RU, "*.txt")):
        ru[os.path.splitext(os.path.basename(f))[0]] = read(f)
    ttf = None
    if os.path.exists(FONT_TTF):
        with open(FONT_TTF, "rb") as f:
            ttf = f.read()
    else:
        print("WARNING: Cyrillic font not found, UI will fall back to system sans:", FONT_TTF)

    env = UnityPy.load(BASE)
    n = 0
    fonts = 0
    for o in env.objects:
        if o.type.name == "TextAsset":
            d = o.read()
            key = getattr(d, "m_Name", "").replace("/", "_").replace("\\", "_")
            if key in ru:
                d.m_Script = ru[key]
                d.save()
                n += 1
        elif o.type.name == "Font" and ttf is not None:
            tt = o.read_typetree()
            if tt.get("m_Name") == FONT_NAME:
                tt["m_FontData"] = list(ttf)   # replace embedded TTF with the Cyrillic build
                o.save_typetree(tt)
                fonts += 1
    os.makedirs(os.path.dirname(OUT), exist_ok=True)
    with open(OUT, "wb") as fp:
        fp.write(env.file.save())
    print(f"built {OUT}  ({n} TextAssets injected, {fonts} font(s) patched, {os.path.getsize(OUT)} bytes)")

    if "--deploy" in sys.argv:
        shutil.copy(OUT, os.path.join(DATA, "resources.assets"))
        print("deployed -> game")

main()
