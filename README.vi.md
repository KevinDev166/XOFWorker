# XOFWorker

[English](README.md) | [简体中文](README.zh-CN.md) | **Tiếng Việt**

Thư viện C# (.NET) nhỏ gọn để đọc và ghi file **XOF (Xiangqi Open Format)**: định dạng văn bản
thuần, người đọc được, dùng cho các ván Cờ Tướng, có biến lồng nhau, lời bình cho từng nước và
thời điểm của từng nước.

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

Toàn bộ định dạng được mô tả trong
**[Đặc tả định dạng XOF](docs/XOF-Format-Specification.vi.md)**.

## Tính năng

- Đọc file có bao nhiêu ván cũng được, từng ván một, phát một sự kiện cho mỗi ván đã phân tích
  (và cho mỗi lỗi). Không phải nạp hết file lớn vào cùng một lúc.
- Ghi một ván hoặc một danh sách ván.
- Biến sâu bao nhiêu cũng được (`XOFMove.Variants` là một `List<List<XOFMove>>`).
- Giữ lại mọi header, kể cả header riêng.
- Có sẵn Pieces Key cho tiếng Anh, tiếng Việt và tiếng Trung, ngoài ra dùng được bộ ký hiệu bất
  kỳ của bạn.
- Chấp nhận ký tự xuống dòng `\r\n`, `\n` và `\r`. Ghi UTF-8 không BOM.
- Không phụ thuộc thư viện nào: chỉ dùng `System.*`, chạy trên Windows, Linux và macOS.

## Yêu cầu

- .NET 9 SDK trở lên

## Cấu trúc repository

```
XOFWorker/           thư viện
XOFWorker.Tests/     test xUnit (đọc-ghi khứ hồi, Pieces Key, ký tự xuống dòng…)
docs/                Đặc tả định dạng XOF
```

Lấy mã nguồn, build và chạy test:

```bash
git clone https://github.com/KevinDev166/XOFWorker.git
cd XOFWorker
dotnet build
dotnet test
```

Để dùng thư viện trong dự án của bạn, thêm project reference tới `XOFWorker/XOFWorker.csproj`
(hoặc chép các file nguồn: chỉ có 13 file).

## Bắt đầu nhanh

### Đọc file

```csharp
using XOFWorker;

var reader = new XOFReader("games.xof");

// Tuỳ chọn: xử lý mỗi ván ngay khi vừa phân tích xong.
reader.GameParsedSuccess += (s, e) => Console.WriteLine("Read game: " + e.Game!.GetHeader(XOFKeywords.Event)?.Value);
reader.ErrorParsing      += (s, e) => Console.WriteLine("Error near line " + e.Info.ReadingLine + ": " + e.Info.Message);

reader.ParseFile();   // không ném ngoại lệ: lỗi được báo qua sự kiện và LatestException

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

Một ván bị hỏng không làm bộ đọc dừng lại: ván đó được đếm vào `GameParsedFailed` và việc đọc tiếp
tục với ván kế tiếp.

### Ghi một ván

```csharp
using XOFWorker;

var game = new XOFGame { Keys = new XOFPiecesKey("en") };   // K A E R C N P

// 12 header bắt buộc (giá trị có thể để trống).
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

// Header riêng bất kỳ.
game.Headers.Add(new XOFHeader("ECCO", "B10"));

// Nước đi: mỗi ply một XOFMove.
var move1 = new XOFMove { IsMainLine = true, Number = 1, PlayerColor = XOFKeywords.R, MoveText = "C2=5", Comment = "Central cannon" };
var move2 = new XOFMove { IsMainLine = true, Number = 2, PlayerColor = XOFKeywords.B, MoveText = "N8+7" };

// Biến là phương án thay cho nước 2: bắt đầu với cùng số thứ tự và cùng bên.
move2.Variants.Add(new List<XOFMove>
{
    new XOFMove { IsMainLine = false, Number = 2, PlayerColor = XOFKeywords.B, MoveText = "C8=5" },
    new XOFMove { IsMainLine = false, Number = 3, PlayerColor = XOFKeywords.R, MoveText = "N2+3" },
});

game.Moves.Add(move1);
game.Moves.Add(move2);

XOFWriter.WriteGame(game, "game.xof");   // ném ngoại lệ khi lỗi (ví dụ thiếu header bắt buộc)
```

Để ghi nhiều ván vào một file, dùng `new XOFWriter().WriteGames(list, path)`. Hàm này báo lỗi qua
sự kiện `ErrorWriting` và `LatestException` thay vì ném ngoại lệ.

Để lấy văn bản XOF mà không ghi file (ví dụ để lưu ván vào cơ sở dữ liệu), gọi
`game.ExportEntireGameToString()`.

### Duyệt qua mọi biến

Các biến tạo thành một cây, nên duyệt chúng chỉ cần một hàm đệ quy đơn giản:

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

## Các lớp chính

| Lớp | Vai trò |
|---|---|
| `XOFReader` | Đọc file từng ván một. Sự kiện: `GameParsedSuccess`, `ErrorParsing`, `CompletedParsing`. |
| `XOFWriter` | Ghi một ván (`WriteGame`) hoặc một danh sách ván (`WriteGames`). |
| `XOFGame` | Một ván: `Headers`, `Keys` (Pieces Key) và `Moves` của mạch chính. |
| `XOFMove` | Một dòng nước đi: `IsMainLine`, `Number`, `MoveText`, `PlayerColor`, `Date`, `Comment`, `Variants`. |
| `XOFHeader` | Một header `Name: Value`. |
| `XOFPiecesKey` | 7 ký hiệu quân. Bộ có sẵn: `"en"`, `"vi"`, `"zh"`. |
| `XOFKeywords` | Các dòng ngăn, từ khoá và tên header bắt buộc. |

Bộ phân tích biến đệ quy là `XOFReader.ParseVariant`.

## Những gì thư viện không làm

XOFWorker kiểm tra **cú pháp** của file XOF. Nó **không** kiểm tra nước đi có **hợp lệ** trên bàn
cờ hay không, vì việc đó cần một bộ sinh nước đi Cờ Tướng. Ứng dụng của bạn nên đi lại các nước từ
`Starting FEN` để kiểm tra.

## Các ngôn ngữ khác

Hiện XOFWorker chỉ có bản C#. Định dạng này đơn giản, bộ đọc viết bằng ngôn ngữ nào cũng chỉ là
phân tích văn bản cộng một hàm đệ quy (xem mục 4 của đặc tả). Các bản chuyển sang ngôn ngữ khác có
thể được phát hành sau nếu có nhu cầu.

## Giấy phép

XOFWorker được phát hành theo [Giấy phép MIT](LICENSE). Bạn được tự do dùng nó trong dự án cá nhân
và thương mại, cũng như sửa đổi và phân phối lại.
