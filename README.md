# JustAMusicFileRenamer (JAMFR)

A small Windows utility that cleans up MP3 album folders in bulk: it reads ID3 tags, repairs corrupted Cyrillic text, and renames both files and folders into a consistent, filesystem-safe format.

## What it does

Point it at a folder — either a single album or a whole library — and it will:

1. **Sanitize the folder name.** Transliterates Unicode to ASCII, replaces spaces with dashes, strips characters that aren't valid in filenames, and renames the folder if it changed.
2. **Sort the tracks.** All `.mp3` files in the folder are ordered alphabetically.
3. **Read each track's ID3 tag.** Tries ID3v2 first; if no artist is set there, it falls back to ID3v1.
4. **Repair mojibake.** If the artist/title text looks like Cyrillic (CP1251) that was mistakenly decoded as Latin-1 — e.g. `Êèíî` instead of `Кино` — it's automatically detected and fixed.
5. **Rename each file** to the pattern:
   ```
   (NN)-Artist-Title.mp3
   ```
   where `NN` is the track number from the tag (zero-padded to 2 digits), or the track's position in the folder if the tag has no track number. The artist/title portion is transliterated and sanitized the same way as folder names.

If the folder you select contains subfolders, each subfolder is treated as its own album and processed in parallel. If it doesn't, the selected folder itself is treated as a single album.

## Project structure

| Project | Purpose |
|---|---|
| `JAMFR.Logic` | Core logic: interfaces (`IAlbumFixer`, `IFileRenamer`, `IStringLocalizer`) and implementations (`AlbumFixer`, `Mp3Renamer`, `StringLocalizer`, `TagTextRepair`) |
| `JAMFR.WinGui` | Entry point (`Program.cs`) — opens a Windows folder-picker dialog and kicks off the fix |

### Key classes

- **`AlbumFixer`** — orchestrates a single album folder: renames the folder, then renames every track inside it.
- **`Mp3Renamer`** — reads a track's ID3 tag and renames the file accordingly.
- **`TagTextRepair`** — detects and fixes CP1251-as-Latin1 mojibake in tag text.
- **`StringLocalizer`** — transliterates any string to a safe, ASCII-only filename fragment (via [UnidecodeSharpFork](https://www.nuget.org/packages/UnidecodeSharpFork)).

## Requirements

- Windows (uses `System.Windows.Forms.FolderBrowserDialog`)
- .NET 6+ (the entry point uses top-level statements)

## Dependencies

- [`Id3`](https://www.nuget.org/packages/Id3) — reading ID3v1/ID3v2 tags
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

This tool renames files and folders in place with no undo/dry-run option. Back up your library (or test on a copy) before running it on anything you care about.

## License

[MIT](LICENSE)
