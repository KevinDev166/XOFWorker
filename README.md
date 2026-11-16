# XOFWorker

**English** | [简体中文](README.zh-CN.md) | [Tiếng Việt](README.vi.md)

A small C# (.NET) library for reading and writing **XOF (Xiangqi Open Format)** files: a
plain-text, human-readable format for Xiangqi (Chinese Chess) games, with nested
variations, per-move comments and per-move timestamps.

```
Event: Example Open
...
Pieces Key: King = K, Assistant = A, Elephant = E, Rook = R, Cannon = C, Knight = N, Pawn = P
---------
ML	1	C2=5	R	2026-11-16 09:00:05	Central cannon
ML	2	N8+7	B
[VARIANT
VA	2	C8=5	B		Same-direction cannons
VARIANT]
ML	3	N2+3	R
------------------
```

The full format is described in the
**[XOF Format Specification](docs/XOF-Format-Specification.md)**.

## Features

- Reads files with any number of games, one game at a time, with an event for each game
  parsed (and for each error). Large files do not have to be loaded all at once.
- Writes one game or a list of games.
- Variations of any depth (`XOFMove.Variants` is a `List<List<XOFMove>>`).
- All headers are kept, including custom ones.
- Pieces Key presets for English, Vietnamese and Chinese, plus any key of your own.
- Accepts `\r\n`, `\n` and `\r` line endings. Writes UTF-8 without BOM.
- No dependencies: uses only `System.*` and runs on Windows, Linux and macOS.

## Requirements

- .NET 9 SDK or later

## Repository layout

```
XOFWorker/           the library
XOFWorker.Tests/     xUnit tests (round-trip, Pieces Key, line endings…)
docs/                XOF Format Specification
```

Get the code, build and run the tests:

```bash
git clone https://github.com/KevinDev166/XOFWorker.git
cd XOFWorker
dotnet build
dotnet test
```

To use the library in your project, add a project reference to `XOFWorker/XOFWorker.csproj`
(or copy the source files: there are only 13 of them).

## Quick start

### Read a file

```csharp
using XOFWorker;

var reader = new XOFReader("games.xof");

// Optional: handle each game as soon as it is parsed.
reader.GameParsedSuccess += (s, e) => Console.WriteLine("Read game: " + e.Game!.GetHeader(XOFKeywords.Event)?.Value);
reader.ErrorParsing      += (s, e) => Console.WriteLine("Error near line " + e.Info.ReadingLine + ": " + e.Info.Message);

reader.ParseFile();   // does not throw: errors are reported by events and LatestException

if (reader.LatestException != null)
    Console.WriteLine("Cannot read the file: " + reader.LatestException.Message);

Console.WriteLine($"{reader.ParsingInfo.GameParsedSuccess} games read, {reader.ParsingInfo.GameParsedFailed} failed");

foreach (XOFGame game in reader.Games)
{
    string red  = game.GetHeader(XOFKeywords.RedPlayer)?.Value ?? "";
    string blue = game.GetHeader(XOFKeywords.BluePlayer)?.Value ?? "";
    Console.WriteLine($"{red} - {blue}: {game.Moves.Count} moves");
}
```

A broken game does not stop the reader: it is counted in `GameParsedFailed` and reading
continues with the next game.

### Write a game

```csharp
using XOFWorker;

var game = new XOFGame { Keys = new XOFPiecesKey("en") };   // K A E R C N P

// The 12 required headers (values may be empty).
game.Headers.Add(new XOFHeader(XOFKeywords.Event,       "Example Open"));
game.Headers.Add(new XOFHeader(XOFKeywords.Address,     ""));
game.Headers.Add(new XOFHeader(XOFKeywords.Date,        "2026-11-16"));
game.Headers.Add(new XOFHeader(XOFKeywords.Round,       "1"));
game.Headers.Add(new XOFHeader(XOFKeywords.GameNumber,  "1"));
game.Headers.Add(new XOFHeader(XOFKeywords.RedPlayer,   "Player A"));
game.Headers.Add(new XOFHeader(XOFKeywords.BluePlayer,  "Player B"));
game.Headers.Add(new XOFHeader(XOFKeywords.RedTeam,     ""));
game.Headers.Add(new XOFHeader(XOFKeywords.BlueTeam,    ""));
game.Headers.Add(new XOFHeader(XOFKeywords.StartingFEN, XOFKeywords.DefaultFEN));
game.Headers.Add(new XOFHeader(XOFKeywords.Result,      "*"));
game.Headers.Add(new XOFHeader(XOFKeywords.PiecesKey,   game.Keys.ExportToString()));

// Any custom header.
game.Headers.Add(new XOFHeader("ECCO", "B10"));

// Moves: one XOFMove per ply.
var move1 = new XOFMove { IsMainLine = true, Number = 1, PlayerColor = XOFKeywords.R, MoveText = "C2=5", Comment = "Central cannon" };
var move2 = new XOFMove { IsMainLine = true, Number = 2, PlayerColor = XOFKeywords.B, MoveText = "N8+7" };

// A variation is an alternative to move 2: it starts with the same number and side.
move2.Variants.Add(new List<XOFMove>
{
    new XOFMove { IsMainLine = false, Number = 2, PlayerColor = XOFKeywords.B, MoveText = "C8=5" },
    new XOFMove { IsMainLine = false, Number = 3, PlayerColor = XOFKeywords.R, MoveText = "N2+3" },
});

game.Moves.Add(move1);
game.Moves.Add(move2);

XOFWriter.WriteGame(game, "game.xof");   // throws on error (e.g. a required header is missing)
```

To write several games into one file, use `new XOFWriter().WriteGames(list, path)`. It
reports errors through the `ErrorWriting` event and `LatestException` instead of throwing.

To get the XOF text without writing a file (for example to store a game in a database),
call `game.ExportEntireGameToString()`.

### Walk through all variations

Variations are a tree, so visiting them is a simple recursive function:

```csharp
static void Walk(List<XOFMove> line, int depth)
{
    foreach (XOFMove m in line)
    {
        Console.WriteLine(new string(' ', depth * 2) + $"{m.Number}. {m.MoveText} ({m.PlayerColor})");
        foreach (List<XOFMove> variation in m.Variants)
            Walk(variation, depth + 1);
    }
}

Walk(game.Moves, 0);
```

## Main classes

| Class | Role |
|---|---|
| `XOFReader` | Reads a file game by game. Events: `GameParsedSuccess`, `ErrorParsing`, `CompletedParsing`. |
| `XOFWriter` | Writes one game (`WriteGame`) or a list of games (`WriteGames`). |
| `XOFGame` | One game: `Headers`, `Keys` (Pieces Key) and the main-line `Moves`. |
| `XOFMove` | One move line: `IsMainLine`, `Number`, `MoveText`, `PlayerColor`, `Date`, `Comment`, `Variants`. |
| `XOFHeader` | One `Name: Value` header. |
| `XOFPiecesKey` | The 7 piece symbols. Presets: `"en"`, `"vi"`, `"zh"`. |
| `XOFKeywords` | Separators, keywords and required header names. |

The recursive variation parser is `XOFReader.ParseVariant`.

## What the library does not do

XOFWorker checks the **syntax** of XOF files. It does **not** check whether the moves are
**legal** on the board, because that needs a Xiangqi move generator. Your application
should replay the moves from the `Starting FEN` to check them.

## Other languages

XOFWorker is currently available only in C#. The format is simple, and a reader in any
language is only text parsing plus one recursive function (see section 4 of the
specification). Ports to other languages may be published later if there is demand.

## License

XOFWorker is released under the [MIT License](LICENSE). You are free to use it in personal
and commercial projects, and to modify and redistribute it.
