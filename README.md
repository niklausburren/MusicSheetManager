# Music Sheet Manager

Music Sheet Manager is a purpose-built .NET 9 WPF application that reduces the operational overhead of working with music sheets in orchestras of any size. It addresses the complete lifecycle: from importing PDFs, preparing them for rehearsal or performance, to organizing, assigning, and distributing parts to individual musicians. The app transforms raw score material into a structured, searchable, and shareable asset library—so librarians and conductors can focus on music rather than file management.

At the heart of the workflow is reliable metadata handling. The app performs OCR to identify instrument, parts (voices), and clef from the score pages, then enforces consistent, human-friendly file naming rules. This metadata is embedded directly into the PDF information fields, ensuring that sheets remain traceable across tools and storage systems. Whether you import a single PDF or a complete set, Music Sheet Manager cleans, classifies, and prepares content for large-scale orchestral use.

Distribution mirrors how orchestras actually operate. For each instrument, the app generates a folder; inside, each musician has a personal subfolder; and within that, one folder per playlist (e.g., concert program or rehearsal set) containing the musician’s assigned parts. Assignments are automatically inferred from metadata but can be manually adjusted to handle special cases, doublings, or last-minute changes. This structure keeps everything predictable and makes it simple to deliver the right sheets to the right people.

Once prepared, sharing becomes frictionless. The generated directory tree can be synced via OneDrive or any cloud storage provider, ensuring musicians receive their materials promptly and consistently. Playlists provide a practical layer for concert preparation: group selections, confirm assignments, export, and share—without reinventing your folder structure for every event. The result is fewer mistakes, faster preparation, and a scalable process that grows with your ensemble.

## Features

- Import PDFs and split pages (A3 into two A4, rotate pages).
- OCR-based metadata detection (Instrument, Parts, Clef).
- Consistent filename generation and metadata persisted in PDF info fields.
- Auto-assignment of sheets to musicians with manual overrides.
- Playlist creation for concerts and distribution to per-musician folders.
- Folder-based organization and import workflow with progress reporting.

## Requirements

- Windows with .NET 9 Desktop Runtime
- Visual Studio 2026 or newer
- IronOCR license key (see Licensing)

## Installation

1. Clone the repository.
2. Configure the IronOCR license key in app settings:
   - `MusicSheetManager/Properties/Settings.settings` -> `IronOcrLicenseKey`
3. Open the solution in Visual Studio and build.

## Build

- Startup project: `MusicSheetManager`
- Target framework: `.NET 9`
- NuGet packages restore automatically.

## Usage

1. Launch the app.
2. Use “Import” to select a PDF and configure options (e.g., “A3 to A4”, “Rotate”).
3. OCR detects metadata; files are renamed and exported.
4. Review automatic musician assignments; adjust manually if needed.
5. Create playlists and distribute sheets to musician folders.
6. Share the generated folder structure via your cloud storage provider (e.g., OneDrive).

## Licensing (IronOCR)

A valid IronOCR license key is required and is verified online at the first OCR call. If machine/developer limits are exceeded, IronOCR throws a `LicensingException`. Remedies:
- Deactivate old activations in the IronSoftware portal.
- Upgrade the license to cover all required machines/developers.
- Optionally disable OCR in CI/build environments.

## Notes

- OCR languages: English (Best), German (Best).
- Temporary processing folder: `%TEMP%\MusicSheetManager\Import`.
- Metadata keys in PDF info are prefixed with `MusicSheetManager_*`.

## Support

- IronOCR licensing: sales@ironsoftware.com
- General questions: use GitHub Issues in this repository