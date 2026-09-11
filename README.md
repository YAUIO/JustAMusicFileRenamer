# JustAMusicFileRenamer (JAMFR)

A small Windows utility that cleans up MP3 album folders in bulk: it reads ID3 tags, repairs corrupted Cyrillic text, renames both files and folders into a consistent, filesystem-safe format, and fixes track order on FAT-based devices that don't sort by filename.

## What it does

Point it at a folder — either a single album or a whole library — and it will:

1. **Sanitize the folder name.** Transliterates Unicode to ASCII, replaces spaces with dashes, strips characters that aren't valid in filenames, and renames the folder if it changed.
2. **Sort the tracks.** All `.mp3` files in the folder are ordered alphabetically.
3. **Read each track's tag** via TagLib# (performer, title, track number), which transparently handles ID3v1/ID3v2 and other supported tag formats.
4. **Repair mojibake.** If the artist/title text looks like Cyrillic (CP1251) that was mistakenly decoded as Latin-1 — e.g. `Êèíî` instead of `Кино` — it's automatically detected and fixed.
5. **Rename each file** to the pattern:
   ```
   NN-Artist-Title.mp3
   ```
   where `NN` is the track number from the tag (zero-padded to 2 digits, capped at 99), or the track's position in the folder if the tag has no track number. The artist segment is omitted if the title already contains `" - "` (avoids duplicating the artist when it's already embedded in the title). The artist/title portion is transliterated and sanitized the same way as folder names. If the computed track number or sort title differs from what's stored, the tag's `Track` and `TitleSort` fields are updated and saved back to the file.
6. **Fix on-disk track order for FAT-based devices.** Many FAT32 devices (car stereos, older MP3 players) list tracks by directory-entry order or file timestamp rather than filename. After renaming, each file is moved out to a temporary subfolder and back in the desired order to rewrite its directory entry, and its creation/modified/accessed timestamps are stamped sequentially (starting `2007-01-01`, one minute apart) so order-sensitive devices play tracks in the right sequence.

If the folder you select contains subfolders, each subfolder is treated as its own album and processed in parallel. If it doesn't, the selected folder itself is treated as a single album.

## Project structure

| Project | Purpose |
|---|---|
| `JAMFR.Logic` | Core logic: interfaces (`IAlbumFixer`, `IFileRenamer`, `IStringLocalizer`, `IFatModificationSorter`) and implementations (`AlbumFixer`, `Mp3Renamer`, `StringLocalizer`, `TagTextRepair`, `FatModificationSorter`) |
| `JAMFR.WinGui` | Entry point (`Program.cs`) — opens a Windows folder-picker dialog and kicks off the fix |

### Key classes

- **`AlbumFixer`** — orchestrates a single album folder: renames the folder, renames every track inside it, then fixes on-disk track order.
- **`Mp3Renamer`** — reads a track's tag via TagLib#, renames the file accordingly, and returns the resulting filename. May also update and save the track's `Track` number and `TitleSort` fields if they've changed.
- **`TagTextRepair`** — detects and fixes CP1251-as-Latin1 mojibake in tag text.
- **`StringLocalizer`** — transliterates any string to a safe, ASCII-only filename fragment (via [UnidecodeSharpFork](https://www.nuget.org/packages/UnidecodeSharpFork)), trims whitespace, strips stray null characters, and collapses repeated dashes (`---` → `-`).
- **`FatModificationSorter`** — rewrites file directory entries and stamps sequential timestamps so FAT-based, order-sensitive devices play tracks in the intended sequence.

## Requirements

- Windows (uses `System.Windows.Forms.FolderBrowserDialog`)
- .NET 6+ (the entry point uses top-level statements)

## Dependencies

- [`TagLibSharp`](https://www.nuget.org/packages/TagLibSharp) — reading and writing ID3 (and other) tags
- [`UnidecodeSharpFork`](https://www.nuget.org/packages/UnidecodeSharpFork) — Unicode → ASCII transliteration

## Build & run

```bash
git clone https://github.com/YAUIO/JustAMusicFileRenamer.git
cd JustAMusicFileRenamer
dotnet restore
dotnet build
```

Then run the `JAMFR.WinGui` project (F5 in Visual Studio, or launch the built executable). A dialog will prompt you to **select your library root or a single album folder** — the rest happens automatically.

## ⚠️ Note

This tool renames files and folders in place with no undo/dry-run option, and can also modify a track's tag metadata (`Track`, `TitleSort`) and its filesystem timestamps. Back up your library (or test on a copy) before running it on anything you care about.
