This folder is intended to contain a permissively-licensed TTF that the game will load at runtime.

Recommended font: Noto Sans (Apache 2.0) — place the file here and name it `NotoSans-Regular.ttf`.

Why bundle a font?
- Ensures consistent visuals across machines and CI
- Avoids relying on system fonts (which may not be present in CI)

If you add a font, also include its license text in this folder (e.g. `LICENSE-Noto.txt`).
