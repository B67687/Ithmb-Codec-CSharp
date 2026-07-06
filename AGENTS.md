# AGENTS.md — AI Agent Guide for Ithmb-Codec-CSharp

This file tells AI coding agents (Claude Code, Copilot, Cursor, Codex) how to work with this repository. Read this first before editing any code.

## Repository Status: ARCHIVED

This C# reference implementation is **archived and set in stone**. All active development has moved to the Rust port at [B67687/Ithmb-Codec](https://github.com/B67687/Ithmb-Codec). Do not make changes here unless fixing a critical decoding bug.

## Repository Purpose

C# Native AOT codec plugin for [ImageGlass v10](https://imageglass.org) that decodes Apple `.ithmb` thumbnail-cache files (iPod Classic/Nano/Photo/Video, iPhone 2G, iPod Touch). This was the original reference from which the Rust port was derived.

## Key Facts

- **Language**: C# (.NET 10, Native AOT)
- **Decoders**: 7 (RGB565, RGB555, ReorderedRGB555, UYVY, YCbCr420, CLCL, CL)
- **Profiles**: 53 built-in (+ 1 speculative disabled)
- **SIMD**: SSE2 + AVX-512 + ARM64 NEON
- **PhotoDB**: Read, write, integrity checking
- **Tests**: 594 (roundtrip, fuzz, SIMD, parsers, PhotoDB)
- **Platform**: Windows (ImageGlass), cross-platform builds available

## Source Layout (24 files by domain)

| File | Purpose |
|------|---------|
| `IthmbCodecPlugin.cs` | ABI entry point, init, API surface |
| `*Decode*.cs` (6 files) | Decode pipeline + individual format decoders |
| `*SimdConstants.cs` | Shared SIMD masks and coefficients |
| `*Profile*.cs` (3 files) | Profile resolution, variant record, embedded JSON |
| `PhotoDb/` (3 files) | PhotoDB/ArtworkDB parser, writer, types |

## Relationship to Rust Port

The Rust workspace ([B67687/Ithmb-Codec](https://github.com/B67687/Ithmb-Codec)) is the canonical version. When in doubt about format behavior, the C# version is the authoritative algorithm reference but should not be modified independently.

## For Agents

- **Cross-reference the Rust `AGENTS.md`** for the full workspace layout and development conventions — the architecture is nearly identical.
- Do NOT add features here — add them to the Rust port first.
- If you need to understand decode flow, start with `DecodePipeline.cs` and the format decoder files.
- See the main `README.md` (366 lines) for architecture diagrams, benchmark data, and the full profile reference.
