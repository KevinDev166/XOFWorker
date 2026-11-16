# XOFWorker

[English](README.md) | **简体中文** | [Tiếng Việt](README.vi.md)

一个用于读写 **XOF（Xiangqi Open Format，象棋开放格式）** 文件的小型 C#（.NET）库。XOF 是一种
人类可读的纯文本象棋对局格式，支持嵌套变着、每步棋的注释和每步棋的时间戳。

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

完整的格式说明见
**[XOF 格式规范](docs/XOF-Format-Specification.zh-CN.md)**。

## 功能

- 读取包含任意数量对局的文件，一次一盘，每解析完一盘（以及每遇到一个错误）触发一个事件。
  大文件无需一次全部载入。
- 写入一盘或多盘对局。
- 变着深度不限（`XOFMove.Variants` 是 `List<List<XOFMove>>`）。
- 保留所有头字段，包括自定义头字段。
- 内置英文、越南文和中文的 Pieces Key 预设，也可以使用您自己的符号表。
- 接受 `\r\n`、`\n` 和 `\r` 换行符。写入不带 BOM 的 UTF-8。
- 没有依赖：只使用 `System.*`，可在 Windows、Linux 和 macOS 上运行。

## 环境要求

- .NET 9 SDK 或更高版本

## 仓库结构

```
XOFWorker/           库本身
XOFWorker.Tests/     xUnit 测试（往返读写、Pieces Key、换行符…）
docs/                XOF 格式规范
```

获取代码、构建并运行测试：

```bash
git clone https://github.com/KevinDev166/XOFWorker.git
cd XOFWorker
dotnet build
dotnet test
```

要在您的项目中使用这个库，添加对 `XOFWorker/XOFWorker.csproj` 的项目引用即可（或者直接复制
源文件：总共只有 13 个）。

## 快速入门

### 读取文件

```csharp
using XOFWorker;

var reader = new XOFReader("games.xof");

// 可选：每解析完一盘就立即处理。
reader.GameParsedSuccess += (s, e) => Console.WriteLine("Read game: " + e.Game!.GetHeader(XOFKeywords.Event)?.Value);
reader.ErrorParsing      += (s, e) => Console.WriteLine("Error near line " + e.Info.ReadingLine + ": " + e.Info.Message);

reader.ParseFile();   // 不会抛出异常：错误通过事件和 LatestException 报告

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

一盘损坏的对局不会使读取停止：它会计入 `GameParsedFailed`，然后继续读取下一盘。

### 写入对局

```csharp
using XOFWorker;

var game = new XOFGame { Keys = new XOFPiecesKey("en") };   // K A E R C N P

// 12 个必需的头字段（值可以为空）。
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

// 任意自定义头字段。
game.Headers.Add(new XOFHeader("ECCO", "B10"));

// 着法：每个半回合一个 XOFMove。
var move1 = new XOFMove { IsMainLine = true, Number = 1, PlayerColor = XOFKeywords.R, MoveText = "C2=5", Comment = "Central cannon" };
var move2 = new XOFMove { IsMainLine = true, Number = 2, PlayerColor = XOFKeywords.B, MoveText = "N8+7" };

// 变着是第 2 步的替代：它以相同的序号和走子方开始。
move2.Variants.Add(new List<XOFMove>
{
    new XOFMove { IsMainLine = false, Number = 2, PlayerColor = XOFKeywords.B, MoveText = "C8=5" },
    new XOFMove { IsMainLine = false, Number = 3, PlayerColor = XOFKeywords.R, MoveText = "N2+3" },
});

game.Moves.Add(move1);
game.Moves.Add(move2);

XOFWriter.WriteGame(game, "game.xof");   // 出错时抛出异常（例如缺少必需的头字段）
```

要把多盘对局写入一个文件，请使用 `new XOFWriter().WriteGames(list, path)`。它通过
`ErrorWriting` 事件和 `LatestException` 报告错误，而不是抛出异常。

如果只想得到 XOF 文本而不写文件（例如把对局存入数据库），请调用
`game.ExportEntireGameToString()`。

### 遍历所有变着

变着是一棵树，所以用一个简单的递归函数就能遍历：

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

## 主要的类

| 类 | 作用 |
|---|---|
| `XOFReader` | 逐盘读取文件。事件：`GameParsedSuccess`、`ErrorParsing`、`CompletedParsing`。 |
| `XOFWriter` | 写入一盘（`WriteGame`）或多盘对局（`WriteGames`）。 |
| `XOFGame` | 一盘对局：`Headers`、`Keys`（Pieces Key）和主线 `Moves`。 |
| `XOFMove` | 一行着法：`IsMainLine`、`Number`、`MoveText`、`PlayerColor`、`Date`、`Comment`、`Variants`。 |
| `XOFHeader` | 一个 `Name: Value` 头字段。 |
| `XOFPiecesKey` | 7 个棋子符号。预设：`"en"`、`"vi"`、`"zh"`。 |
| `XOFKeywords` | 分隔符、关键字和必需的头字段名。 |

递归的变着解析器是 `XOFReader.ParseVariant`。

## 这个库不做什么

XOFWorker 检查 XOF 文件的**语法**。它**不**检查着法在棋盘上是否**合法**，因为那需要象棋着法
生成器。您的应用程序应当从 `Starting FEN` 开始重放着法来进行检查。

## 其他语言

XOFWorker 目前只有 C# 版本。这个格式很简单，用任何语言写读取器都只是文本解析加一个递归函数
（见规范第 4 节）。如果有需求，以后可能会发布其他语言的移植版本。

## 许可证

XOFWorker 以 [MIT 许可证](LICENSE) 发布。您可以自由地在个人和商业项目中使用它，也可以修改和
再分发。
