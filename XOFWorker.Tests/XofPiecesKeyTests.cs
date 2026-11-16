using XOFWorker;

namespace XOFWorker.Tests;

/// <summary>XOF piece symbol set (Pieces Key), move text validation, line splitting.</summary>
public class XofPiecesKeyTests
{
    [Theory]
    [InlineData("en", "KAERCNP")]
    [InlineData("vi", "TSVXPMC")]
    [InlineData("EN", "KAERCNP")]
    public void LanguageKeys_ShortString(string lang, string expected)
    {
        var k = new XOFPiecesKey(lang);
        Assert.Equal(expected, k.ExportToShortString());
        Assert.True(k.Validate());
    }

    [Fact]
    public void ChineseKey_Valid()
    {
        var k = new XOFPiecesKey("zh");
        Assert.True(k.Validate());
        Assert.Equal(7, k.ExportToShortString().Length);
    }

    [Fact]
    public void ShortString_RoundTrip_AndSameAs()
    {
        var a = new XOFPiecesKey("vi");
        var b = new XOFPiecesKey();
        b.ParseFromShortString(a.ExportToShortString());
        Assert.True(a.IsTheSameAs(b));
        Assert.False(a.IsTheSameAs(new XOFPiecesKey("en")));
    }

    [Theory]
    [InlineData("")]
    [InlineData("KAERCN")]
    [InlineData("KAERCNPX")]
    public void ParseFromShortString_WrongLength_Throws(string s)
        => Assert.Throws<ArgumentException>(() => new XOFPiecesKey().ParseFromShortString(s));

    [Fact]
    public void Validate_DuplicateChar_False()
    {
        var k = new XOFPiecesKey("en") { Pawn = 'K' };
        Assert.False(k.Validate());
        Assert.False(new XOFPiecesKey().Validate());   // empty key: all '\0' characters are duplicates
    }

    [Fact]
    public void PieceType_CharMapping_IsInverse()
    {
        var k = new XOFPiecesKey("vi");
        foreach (char c in k.GetKeyList())
            Assert.Equal(c, k.GetPieceCharByPieceType(k.GetPieceTypeByPieceChar(c)));
        Assert.Throws<ArgumentException>(() => k.GetPieceTypeByPieceChar('Z'));
        Assert.Throws<ArgumentException>(() => k.GetPieceCharByPieceType("Dragon"));
    }

    [Fact]
    public void ExportToString_ListsEveryPiece()
        => Assert.Equal("King = K, Assistant = A, Elephant = E, Rook = R, Cannon = C, Knight = N, Pawn = P",
                        new XOFPiecesKey("en").ExportToString());

    [Theory]
    [InlineData("C2.5", true)]
    [InlineData("N8+7", true)]
    [InlineData("C2.", false)]   // too short
    [InlineData("Z2.5", false)]  // no piece symbol
    public void IsValidMoveText(string move, bool expected)
        => Assert.Equal(expected, XiangQiHelper.IsValidMoveText(move, new XOFPiecesKey("en")));

    [Theory]
    [InlineData("a\r\nb\r\nc")]
    [InlineData("a\nb\nc")]
    [InlineData("a\rb\rc")]
    public void SplitLinesByEOL_AllLineEndings(string text)
        => Assert.Equal(new[] { "a", "b", "c" }, StringWorker.SplitLinesByEOL(text));

    [Fact]
    public void SplitLinesByEOL_SingleLine_Throws()
        => Assert.Throws<ArgumentException>(() => StringWorker.SplitLinesByEOL("one line"));
}
