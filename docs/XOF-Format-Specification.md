# XOF — Xiangqi Open Format

**English** | [简体中文](XOF-Format-Specification.zh-CN.md) | [Tiếng Việt](XOF-Format-Specification.vi.md)

**Specification, version 1.0 (2026-11-16)**

XOF is a plain-text file format for storing Xiangqi (Chinese Chess) games. It covers the
moves, nested variations, per-move comments, per-move timestamps and any game information
you want to add. It replaces the 2025 Vietnamese draft (`XOF Format Specifications.pdf`).
Where the two disagree, this document wins (see §3.9).

Contents

1. [What is wrong with the current formats](#1-what-is-wrong-with-the-current-formats)
2. [The world needs a better format: why XOF](#2-the-world-needs-a-better-format-why-xof)
3. [XOF Format Specification](#3-xof-format-specification)
4. [Coding with XOF](#4-coding-with-xof)

---

## 1. What is wrong with the current formats

A game record has two layers: the **move notation** (how one move is written) and the
**file format** (how a whole game, with its information, variations and comments, is
stored). Both layers have serious problems today.

### 1.1 Move notation: ambiguous, inconsistent and rule-dependent

The traditional notation (`C2=5`, `N8+7`, 炮二平五, 马8进7 …) is the one players actually
use. It is easy to picture on the board. But every federation writes it in its own way. We
checked the published rule books (WXF 2018, AXF 1996, the Vietnamese Laws of 2004 and 2021,
the Chinese Laws of 2020) and found these problems:

1. **Omitting the front/rear mark makes moves ambiguous.** An AXF book prints `C9+1` in a
   position where Blue has *two* Cannons on street 9, two ranks apart, and *both* can advance
   one rank. Nobody can tell which Cannon moved; the WXF form would be `+C+1`. A move like
   this can only be resolved by luck, when just one of the two pieces happens to have a
   legal move. So a reader (human or program) needs the position and a legal-move generator
   just to understand the text.
2. **"Front / middle / rear" only handles three pieces.** Up to five Pawns can stand on one
   street. The traditional marks cannot name the 4th and 5th Pawn.
3. **The same text means different things under different rules.** For example, `P2-5` in
   the Vietnamese 2004 notation is a *sideways* move, while in the 2021 notation the same
   string is a *retreat*. To read a game you must know (or guess) which rule book it was
   written with, and that must hold for the whole game.
4. **Two-character piece symbols** (e.g. Vietnamese 2004 `Tg`) make moves different lengths
   and break simple parsing.
5. **The official books are not consistent.** The WXF book writes the same situation (two
   Rooks on one street) sometimes as `R4-2` and sometimes as `+R-2`. Advisors and Elephants
   use the street number instead of front/rear. One book switches between upper and lower
   case for Blue. None of the rules say what to do when *two* streets each hold doubled
   Pawns.
6. **Mixed digit and character systems.** Chinese records write Red's streets as 一…九 and
   Black's as 1–9, sometimes in full-width digits １…９. They also mix Traditional and
   Simplified characters (車/车, 進/进, 後/后).
7. **Printing errors cannot be detected from the text.** In one AXF book we found 13
   diagrams with wrong moves (wrong piece letter or wrong number). They showed up only when
   the moves were replayed on a board.

Coordinate notations such as ICCS (`h2e2`) are unambiguous, but they are hard for people to
read, and almost no player records or thinks in them.

### 1.2 File formats: closed, flat or loosely defined

- **Closed binary formats.** Most Xiangqi programs save games in their own binary formats
  (XQF, CCBridge CBL/CBR, …). Some of them are encrypted or undocumented. Only the program
  that wrote the file can read it. When the program dies, the archive dies with it.
- **PGN for Xiangqi has no variations.** In practice, the Xiangqi PGN files you meet never
  contain branches. Analysis, the most valuable part of a commented game, is lost.
- **PGN move text is not standardised for Xiangqi.** Each writer uses its own piece letters,
  language and notation (see §1.1). The file does not say which key it uses, so the reader
  must be told the key by hand before importing.
- **PGN is hard to parse well.** Variations, comments, move numbers and annotations all share
  one free-flowing text stream. Writing a correct reader takes far more effort than it
  should.
- **No place for move timing.** Electronic boards and online play record *when* each move was
  made, but there is no standard place to keep it.

---

## 2. The world needs a better format: why XOF

The name **XOF** stands for **Xiangqi Open Format**. The goals: anyone can read it, anyone
can write it, nothing is ambiguous.

| Problem today | What XOF does |
|---|---|
| Closed binary formats | Plain UTF-8 text. Opens in any text editor and is readable without software. |
| Ambiguous moves (§1.1 items 1, 2, 5) | **The street number is always written.** When two or more pieces of the same type share a street, a position prefix `1`…`5` is **required**. This handles all five Pawns, and every move can be decoded from the position without guessing. |
| Same text, different meaning across rule books (item 3) | One meaning for each sign: `+` advance, `-` retreat, `=` move sideways. This never depends on a rule book. |
| Piece letters differ by language (items 4, 6) | Each game declares its own **Pieces Key** (7 single-character symbols). English, Vietnamese, Chinese, or any other language can be used and read without ambiguity. |
| No variations (PGN) | Variations of any depth, any number of alternatives for one move. |
| Hard to parse | **One move = one line**, with tab-separated columns. A reader is a line splitter plus one small recursive function (§4). |
| No move timing | Every move line has its own timestamp column. |
| Comments mixed into the move stream | Every move line has its own comment column. |
| Free-form game information | 12 required header names plus any number of your own headers. |

Plain-text lines bring other benefits:

- **Version-control friendly.** One move per line gives clean `git diff` output. A change to
  one move or comment shows as a one-line change.
- **Searchable with standard tools.** `grep` or any text search can find a move, a comment or
  a player across thousands of files.
- **Database friendly.** A whole game is a single string. Store it in one text column and
  parse it again with the same reader (§3.8).
- **Easy to fix by hand.** A printing error from a book can be corrected in a text editor.

---

## 3. XOF Format Specification

The key words **MUST**, **MUST NOT**, **SHOULD** and **MAY** are used as in RFC 2119.

### 3.1 Encoding and line endings

- A file MUST be encoded in **UTF-8**. Writers SHOULD NOT write a BOM.
- Line endings MAY be `\r\n`, `\n` or `\r`. Readers MUST accept all three.
- Readers MUST ignore empty lines. Readers SHOULD trim leading and trailing white space
  from every line before interpreting it. Do not trim tabs *inside* a move line, because
  they separate its columns.

### 3.2 File structure

A file contains one or more **games**. Each game has a **header section** and a
**move-text section**:

```
<header section of game 1>
---------                       ← section separator: exactly 9 dashes
<move-text section of game 1>
------------------              ← game separator: exactly 18 dashes
<header section of game 2>
---------
<move-text section of game 2>
------------------
```

- The **section separator** is a line containing exactly **9** dashes (`---------`).
- The **game separator** is a line containing exactly **18** dashes (`------------------`).
  Writers MUST put it **after** every game, including the last one. It never appears at the
  start of the file.
- Readers SHOULD still accept a final game that is not followed by a game separator.
- No header value or comment MAY contain a run of 9 or more dashes. Otherwise a reader may
  take it for a separator.

### 3.3 Header section

One header per line:

```
<Header Name>: <Header Value>
```

- Name and value are separated by a **colon** (`:`). Leading and trailing white space of both
  is ignored.
- **A header name or value MUST NOT contain a colon.** A writer that needs to store a time
  in a header must replace the colons with another character (e.g. `20?53?00` or `20.53.00`).
  Move timestamps are not affected, because move lines use tabs (§3.4).
- The value MAY be empty. Many old book games have no date, place or player.

**Required headers.** Every game MUST contain these 12 header names, even when their values
are empty. Writers SHOULD write them in this order:

| Header name | Meaning |
|---|---|
| `Event` | Name of the event or tournament (or of the problem, for a book position) |
| `Address` | Place where the game was played |
| `Date` | Date of the game, recommended format `YYYY-MM-DD` |
| `Round` | Round, for tournaments played in rounds |
| `Game Number` | Number of the game within the round or match |
| `Red Player` | Red player: a person, an engine, a robot… |
| `Blue Player` | Blue player (the side many books call "Black") |
| `Red Team` | Team, club or federation of the Red player |
| `Blue Team` | Team, club or federation of the Blue player |
| `Starting FEN` | Starting position in FEN (see below) |
| `Result` | `1-0` Red wins, `0-1` Blue wins, `1/2-1/2` draw, `*` unknown or unfinished |
| `Pieces Key` | Symbols used for the pieces in the move text (§3.5) |

`Starting FEN` for the normal starting position is:

```
rnbakabnr/9/1c5c1/p1p1p1p1p/9/9/P1P1P1P1P/1C5C1/9/RNBAKABNR w - - 0 1
```

Handicap games, endgames and problems use their own position. The side to move in the FEN
(`w` = Red, `b` = Blue) decides who makes move 1.

**Custom headers.** After the required headers a game MAY contain any number of extra
headers that follow the same `Name: Value` rule (e.g. `ECCO`, `Commentator`, `Time Control`,
`Source`). Readers MUST read and keep **all** headers, including ones they do not know.

### 3.4 Move-text section

Each line records **one move by one side** (one ply). A move line has 6 columns separated by
a **tab** character:

```
[ML|VA] <TAB> [Number] <TAB> [Move] <TAB> [R|B] <TAB> [Date time] <TAB> [Comment]
```

| Column | Content |
|---|---|
| 1 | `ML` for a main-line move, `VA` for a move inside a variation |
| 2 | Ply number, counted from 1. Each line is one ply, so Red and Blue each have their own number (1 Red, 2 Blue, 3 Red…). |
| 3 | The move, written as in §3.6 with the game's Pieces Key |
| 4 | Side that makes the move: `R` Red, `B` Blue |
| 5 | Time the move was made, MAY be empty. Recommended format `YYYY-MM-DD HH:MM:SS` (colons are allowed here). Useful for games from electronic boards or online play, for example to study a player's speed. |
| 6 | Comment on the move, MAY be empty. MUST NOT contain a tab or a line break. Writers must replace them with spaces. |

Columns 5 and 6 may be empty, but writers SHOULD always write all 6 columns (empty columns
are just consecutive tabs). Readers MUST accept lines that have only the first 4 columns.

Example (English Pieces Key):

```
ML	1	C2=5	R	2025-07-08 20:53:00	Central cannon
```

**The result is stored only in the `Result` header.** Writers MUST NOT add a result line
(`1-0`, `0-1`, …) after the last move. The move-text section ends with the last move line
(or `VARIANT]`), followed by the game separator.

### 3.5 Pieces Key

The `Pieces Key` header lists the symbol of each of the 7 piece types as
`<Piece name> = <symbol>` pairs, separated by commas, in this order:

```
Pieces Key: King = K, Assistant = A, Elephant = E, Rook = R, Cannon = C, Knight = N, Pawn = P
```

- The piece names are fixed English words: `King`, `Assistant` (Advisor), `Elephant`,
  `Rook`, `Cannon`, `Knight`, `Pawn`.
- Each symbol MUST be **exactly one character**, and the 7 symbols MUST all be different.
- A symbol MUST NOT be a digit, `+`, `-`, `=`, a tab or white space. Latin letters SHOULD
  be upper case.
- The same symbol is used for both sides. Column 4 (`R` / `B`) tells which side moved.

Recommended keys:

| Language | Pieces Key value |
|---|---|
| English | `King = K, Assistant = A, Elephant = E, Rook = R, Cannon = C, Knight = N, Pawn = P` |
| Vietnamese | `King = T, Assistant = S, Elephant = V, Rook = X, Cannon = P, Knight = M, Pawn = C` |
| Chinese (Simplified, Red's characters for both sides) | `King = 帅, Assistant = 仕, Elephant = 相, Rook = 车, Cannon = 炮, Knight = 马, Pawn = 兵` |

Any other set is valid if it follows the rules above. A reader must never assume a key; it
must always use the key declared by the game.

### 3.6 Move text

A move is written as:

```
[position prefix] <piece> <street> <sign> <number>
```

- **Streets** are numbered **1 to 9 from each player's own right-hand side**, the usual way
  in Xiangqi. Red counts from Red's right, Blue from Blue's right.
- **Signs** (fixed in every language):
  - `+` **advance**: move toward the opponent
  - `-` **retreat**: move toward your own side
  - `=` **sideways**: move along the same rank
- **The number after the sign:**
  - **Rook, Cannon, Pawn, King**: after `+` / `-` it is the **number of ranks moved**. After
    `=` it is the **street the piece arrives on**.
  - **Knight, Elephant, Assistant**: after `+` / `-` it is the **street the piece arrives
    on**. These pieces never move sideways, so `=` is never used for them.

Examples (Vietnamese key: `P` Cannon, `M` Knight):

| Move | Meaning |
|---|---|
| `P2=5` | Cannon on street 2 moves sideways to street 5 |
| `M8+7` | Knight on street 8 advances to street 7 |
| `P8-2` | Cannon on street 8 retreats 2 ranks |

**Several pieces of the same type on one street.** When two or more pieces of the same type
and side stand on the **same street**, the move MUST start with a position prefix from `1`
to `5`:

- `1` is the piece **farthest from its own back rank** (the most advanced one), then `2`,
  `3`, `4`, and `5` is the one **nearest its own back rank**.
- This applies to Advisors, Elephants, Rooks, Cannons, Knights and up to five Pawns. The
  street number is still written.
- Examples (English key): `1C4+2` is the front Cannon on street 4 advancing 2 ranks.
  `2A4+5` is the rear Assistant on street 4 advancing to street 5. `3P5=4` is the third Pawn
  on street 5 moving sideways to street 4.
- A prefix MUST NOT be written when the piece is alone on its street.

Because the street is always written and the prefix is required, **one move text matches
exactly one move** on the board. This holds even when two different streets each hold
doubled pieces.

### 3.7 Variations

A variation is an alternative to the move written **just before** it. The example below is
taken from a real endgame file and uses the Vietnamese key (`S` Assistant, `P` Cannon):

```
ML	7	C7+1	R
ML	8	1S4-5	B			← main-line move 8
[VARIANT
VA	8	2S4+5	B			← alternative to move 8
VA	9	P4=5	R
VARIANT]
ML	9	...
```

- A variation is enclosed between a line `[VARIANT` and a line `VARIANT]`. It is written
  **immediately after** the move it replaces.
- The first move of a variation has the **same ply number and side** as the move it
  replaces. Numbering then continues inside the variation (9, 10, …).
- All moves inside a variation use `VA` in column 1, at every depth.
- **Several alternatives**: write several `[VARIANT … VARIANT]` blocks one after another,
  right after the same move. Their order is the order of preference.
- **Nested variations**: a `[VARIANT … VARIANT]` block may follow any `VA` line. It is then
  an alternative to that move inside the variation. There is no depth limit.
- Writers MUST NOT write an empty variation (a block with no move line).
- After the closing `VARIANT]`, the main line (or the parent variation) continues with the
  next move after the one that was replaced.

### 3.8 Storing XOF in a database

A whole game, from its first header line to its game separator, can be stored in a single
text field together with its line endings. Read the field back and split it into lines in
the same way as a file.

### 3.9 Changes from the 2025 draft

- The draft ended the move text with a result line. **This line is no longer written.** The
  result is only in the `Result` header, and `Result` is now a required header.
- The draft used `[VARIANT … VARIANT]` blocks together with level codes `Vx`. Only `VA`
  is used now, and nesting is shown by the blocks themselves.
- The recommended Chinese key now uses Simplified characters for both sides
  (帅仕相车炮马兵).

### 3.10 Complete example

Two games in one file (English key; `→|` marks a tab):

```
Event: Example Open
Address: Hanoi
Date: 2026-11-16
Round: 1
Game Number: 1
Red Player: Player A
Blue Player: Player B
Red Team:
Blue Team:
Starting FEN: rnbakabnr/9/1c5c1/p1p1p1p1p/9/9/P1P1P1P1P/1C5C1/9/RNBAKABNR w - - 0 1
Result: *
Pieces Key: King = K, Assistant = A, Elephant = E, Rook = R, Cannon = C, Knight = N, Pawn = P
ECCO: B10
---------
ML→|1→|C2=5→|R→|2026-11-16 09:00:05→|Central cannon→|
ML→|2→|N8+7→|B→|2026-11-16 09:00:31→|→|
[VARIANT
VA→|2→|C8=5→|B→|→|Same-direction cannons→|
VA→|3→|N2+3→|R→|→|→|
[VARIANT
VA→|3→|N8+7→|R→|→|→|
VARIANT]
VA→|4→|N8+7→|B→|→|→|
VARIANT]
ML→|3→|N2+3→|R→|2026-11-16 09:01:10→|→|
ML→|4→|R9=8→|B→|2026-11-16 09:01:42→|→|
------------------
Event: Second game
...
---------
...
------------------
```

---

## 4. Coding with XOF

Reading and writing XOF is simple. It is only text parsing, and there is no binary layout,
escaping or tokenizer to write. A reader needs only:

1. **Split the file into games** at lines equal to the 18-dash game separator.
2. **Split each game into header and move text** at the 9-dash section separator.
3. **Headers:** split each line at the colon, then trim both parts.
4. **Moves:** split each line at the tabs to get its 6 columns.
5. **Variations:** one **recursive function**. When the line after a move is `[VARIANT`, call
   the function. It collects `VA` lines into a list until it reaches `VARIANT]`, and calls
   itself again whenever it meets another `[VARIANT`. Each list it returns is attached to
   the move just before the block.

In pseudocode:

```
parseVariant(lines, i, parentMove):
    i = i + 1                                  # skip "[VARIANT"
    list = []
    while lines[i] != "VARIANT]":
        if lines[i] == "[VARIANT":
            i = parseVariant(lines, i, last(list))   # nested: alternative to the last move
        else:
            list.add(parseMoveLine(lines[i]))
        i = i + 1
    parentMove.variants.add(list)              # one move may have several variations
    return i                                   # index of "VARIANT]"
```

A move therefore holds a **list of variations**, and each variation is a list of moves
(`List<List<Move>>`), which may hold variations of their own.

Writing is the reverse: write the headers, the 9-dash line, then each move line. After a
move that has variations, write each variation as `[VARIANT`, its moves (recursively) and
`VARIANT]`. Finish with the 18-dash line.

### 4.1 Reference implementation: XOFWorker (C#)

**XOFWorker** is the open-source C# (.NET) library that [SuperXiangQi](https://superxiangqi.appsixtyone.com/) uses to read and write
XOF:

> **https://github.com/KevinDev166/XOFWorker**

| Class | Role |
|---|---|
| `XOFReader` | Reads a file game by game and raises an event for each game parsed (and for errors), so large files are processed without loading everything first |
| `XOFWriter` | Writes one game or a list of games to a file |
| `XOFGame` | One game: headers, Pieces Key and the main-line moves |
| `XOFMove` | One move line. `Variants` is a `List<List<XOFMove>>`. |
| `XOFHeader` | One `Name: Value` pair |
| `XOFPiecesKey` | The Pieces Key, with the English / Vietnamese / Chinese presets |
| `XOFKeywords` | All separators, keywords and required header names in one place |

The recursive variation parser is `XOFReader.ParseVariant`. See the project's source code
for the details.

XOFWorker checks the **syntax** of the file. It does not check whether moves are **legal**
on the board, because that needs a Xiangqi move generator. Your application should replay
the moves from the `Starting FEN` and check them.

### 4.2 Other languages

XOFWorker is currently available only in C#. A reader in another language is a short piece
of work if you follow §4 and the reference code. If many developers need it, ports to other
languages may be published later.
