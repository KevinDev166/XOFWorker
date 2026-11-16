using System.Text;
using XOFWorker;

namespace XOFWorker.Tests;

/// <summary>
/// ROUND-TRIP for the XOF format: build a game in memory -> WRITE it to a file -> READ it back ->
/// it must match exactly.
///
/// Why: XOFWorker previously had NO TESTS AT ALL, yet it reads/writes XOF files —
/// i.e. the users' REAL DATA. Unlike SGR (moves written as 4 grid-coordinate digits),
/// XOF writes moves as NOTATION based on the piece symbol set (PiecesKey), which adds a risk:
/// changing the symbol set makes the whole file read wrongly.
/// </summary>
public class XofRoundTripTests : IDisposable
{
    private readonly string _dir;

    public XofRoundTripTests()
    {
        _dir = Path.Combine(Path.GetTempPath(), "sxq-xof-test-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_dir);
    }

    public void Dispose()
    {
        try { Directory.Delete(_dir, recursive: true); } catch { /* clean up */ }
    }

    private string File_(string name) => Path.Combine(_dir, name);

    private const string OpeningFen = "rnbakabnr/9/1c5c1/p1p1p1p1p/9/9/P1P1P1P1P/1C5C1/9/RNBAKABNR w - - 0 1";

    /// <summary>Header value; also asserts that the header EXISTS (GetHeader now returns nullable).</summary>
    private static string Val(XOFGame game, string headerName)
    {
        XOFHeader? h = game.GetHeader(headerName);
        Assert.True(h is not null, $"missing header \"{headerName}\"");
        return h!.Value;
    }

    private static XOFMove Move(int number, string color, string moveText, string comment = "")
        => new()
        {
            IsMainLine = true,
            Number = number,
            PlayerColor = color,
            MoveText = moveText,
            Comment = comment,
            Date = "",
        };

    /// <summary>A game with ALL 12 required XOF headers (SGR has only 11 — XOF adds PiecesKey).</summary>
    private static XOFGame BuildGame(params XOFMove[] moves)
    {
        var g = new XOFGame { Keys = new XOFPiecesKey("en") };   // K A E R C N P
        g.Headers.Add(new XOFHeader(XOFKeywords.Event, "全国象棋锦标赛"));
        g.Headers.Add(new XOFHeader(XOFKeywords.Address, "北京"));
        g.Headers.Add(new XOFHeader(XOFKeywords.Date, "2026.08.10"));
        g.Headers.Add(new XOFHeader(XOFKeywords.Round, "3"));
        g.Headers.Add(new XOFHeader(XOFKeywords.GameNumber, "7"));
        g.Headers.Add(new XOFHeader(XOFKeywords.RedPlayer, "王天一"));
        g.Headers.Add(new XOFHeader(XOFKeywords.BluePlayer, "郑惟桐"));
        g.Headers.Add(new XOFHeader(XOFKeywords.RedTeam, "北京队"));
        g.Headers.Add(new XOFHeader(XOFKeywords.BlueTeam, "上海队"));
        g.Headers.Add(new XOFHeader(XOFKeywords.StartingFEN, OpeningFen));
        g.Headers.Add(new XOFHeader(XOFKeywords.Result, "1-0"));
        g.Headers.Add(new XOFHeader(XOFKeywords.PiecesKey, g.Keys.ExportToString()));
        foreach (XOFMove m in moves) g.Moves.Add(m);
        return g;
    }

    private static XOFGame WriteThenRead(XOFGame game, string path)
    {
        XOFWriter.WriteGame(game, path);

        var reader = new XOFReader(path);
        reader.ParseFile();

        Assert.Null(reader.LatestException);
        Assert.Equal(0, reader.ParsingInfo.GameParsedFailed);
        Assert.Single(reader.Games);
        return reader.Games[0];
    }

    [Fact]
    public void WriteThenRead_KeepsHeaders()
    {
        var original = BuildGame(Move(1, XOFKeywords.R, "C2=5"));
        XOFGame readBack = WriteThenRead(original, File_("headers.xof"));

        Assert.Equal("全国象棋锦标赛", Val(readBack, XOFKeywords.Event));
        Assert.Equal("北京", Val(readBack, XOFKeywords.Address));
        Assert.Equal("2026.08.10", Val(readBack, XOFKeywords.Date));
        Assert.Equal("3", Val(readBack, XOFKeywords.Round));
        Assert.Equal("王天一", Val(readBack, XOFKeywords.RedPlayer));
        Assert.Equal("郑惟桐", Val(readBack, XOFKeywords.BluePlayer));
        Assert.Equal("1-0", Val(readBack, XOFKeywords.Result));
        Assert.Equal(OpeningFen, Val(readBack, XOFKeywords.StartingFEN));
    }

    [Fact]
    public void WriteThenRead_KeepsMoveText()
    {
        var original = BuildGame(
            Move(1, XOFKeywords.R, "C2=5"),
            Move(1, XOFKeywords.B, "N8+7"),
            Move(2, XOFKeywords.R, "N2+3"),
            Move(2, XOFKeywords.B, "R9=8"));

        XOFGame readBack = WriteThenRead(original, File_("moves.xof"));

        Assert.Equal(original.Moves.Count, readBack.Moves.Count);
        for (int i = 0; i < original.Moves.Count; i++)
        {
            Assert.Equal(original.Moves[i].Number, readBack.Moves[i].Number);
            Assert.Equal(original.Moves[i].PlayerColor, readBack.Moves[i].PlayerColor);
            Assert.Equal(original.Moves[i].MoveText, readBack.Moves[i].MoveText);
        }
    }

    [Fact]
    public void WriteThenRead_KeepsUnicodeComment()
    {
        const string comment = "Central cannon — the most common opening; Black usually replies 马8进7 (Knight 8 advances to 7).";

        var original = BuildGame(Move(1, XOFKeywords.R, "C2=5", comment));
        XOFGame readBack = WriteThenRead(original, File_("comment.xof"));

        Assert.Equal(comment, readBack.Moves[0].Comment);
    }

    [Fact]
    public void WriteThenRead_KeepsPiecesKey()
    {
        // PiecesKey decides how EVERY move in the file is read. A wrong key means the whole game is read wrongly.
        var original = BuildGame(Move(1, XOFKeywords.R, "C2=5"));
        XOFGame readBack = WriteThenRead(original, File_("key.xof"));

        Assert.Equal(original.Keys.ExportToString(), Val(readBack, XOFKeywords.PiecesKey));
    }

    [Fact]
    public void WriteManyGames_ReadsBackAllGames()
    {
        var g1 = BuildGame(Move(1, XOFKeywords.R, "C2=5"));
        var g2 = BuildGame(Move(1, XOFKeywords.R, "P3+1"), Move(1, XOFKeywords.B, "P7+1"));

        string path = File_("multi.xof");
        new XOFWriter().WriteGames(new List<XOFGame> { g1, g2 }, path);

        var reader = new XOFReader(path);
        reader.ParseFile();

        Assert.Equal(2, reader.Games.Count);
        Assert.Equal(0, reader.ParsingInfo.GameParsedFailed);
        Assert.Single(reader.Games[0].Moves);
        Assert.Equal(2, reader.Games[1].Moves.Count);
    }

    [Fact]
    public void ReadEmptyFile_NoException_ReturnsEmptyList()
    {
        string path = File_("empty.xof");
        System.IO.File.WriteAllText(path, "", new UTF8Encoding(false));

        var reader = new XOFReader(path);
        reader.ParseFile();

        Assert.Empty(reader.Games);
        Assert.Null(reader.LatestException);
    }

    /// <summary>An XOF file WITHOUT a separator line after the LAST game — the last game must still be read (before 2026-10-20
    /// it was silently dropped: batch-importing one-game XOF files that lack the last line lost the whole file).</summary>
    [Fact]
    public void LastGameWithoutTrailingSeparator_IsStillRead()
    {
        string path = File_("no-trailing-separator.xof");
        XOFWriter.WriteGame(BuildGame(Move(1, XOFKeywords.R, "C2=5")), path);
        var lines = File.ReadAllLines(path, System.Text.Encoding.UTF8).ToList();
        while (lines.Count > 0 && (lines[^1].Trim().Length == 0 || lines[^1].Trim() == XOFKeywords.GameSeparator)) lines.RemoveAt(lines.Count - 1);
        File.WriteAllLines(path, lines, new System.Text.UTF8Encoding(false));
        Assert.DoesNotContain(XOFKeywords.GameSeparator, File.ReadAllText(path));

        var reader = new XOFReader(path);
        reader.ParseFile();
        Assert.Single(reader.Games);
        Assert.Equal(1, reader.ParsingInfo.GameParsedSuccess);
    }

    /// <summary>Two games, the last one without a separator → both games read; garbage at the end of the file → counted as 1 bad game (not silently lost).</summary>
    [Fact]
    public void TwoGames_AndTrailingGarbage()
    {
        string path = File_("two.xof");
        new XOFWriter().WriteGames(new List<XOFGame> { BuildGame(Move(1, XOFKeywords.R, "C2=5")), BuildGame(Move(1, XOFKeywords.R, "C8=5")) }, path);
        string text = File.ReadAllText(path).TrimEnd();
        Assert.EndsWith(XOFKeywords.GameSeparator, text);
        File.WriteAllText(path, text[..^XOFKeywords.GameSeparator.Length]);   // drop the LAST separator line
        var r1 = new XOFReader(path);
        r1.ParseFile();
        Assert.Equal(2, r1.Games.Count);

        File.AppendAllText(path, Environment.NewLine + XOFKeywords.GameSeparator + Environment.NewLine + "this is not a game");
        var r2 = new XOFReader(path);
        r2.ParseFile();
        Assert.Equal(2, r2.Games.Count);
        Assert.Equal(1, r2.ParsingInfo.GameParsedFailed);
    }
}
