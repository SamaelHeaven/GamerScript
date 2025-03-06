using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace GamerScript.Core;

public static partial class CodeHighlighter
{
    private const int FontSize = 32;
    private const int LineSpacing = 8;
    private const int Dpi = 72;
    private const int PaddingX = 32;
    private const int PaddingY = 32;
    private const string BackgroundColor = "#151718";
    private const string KeywordColor = "#e6cd69";
    private const string IdentifierColor = "#55b5db";
    private const string OperatorColor = "#9fca56";
    private const string DelimiterColor = "#cfd2d1";
    private const string CommentColor = "#41535b";
    private const string NumberColor = "#cd3f45";
    private const string StringColor = "#55b5db";

    private static readonly string FontPath =
        Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "",
            "resources/Hack-Regular.ttf");

    public static string GenerateHtml(IEnumerable<Token> tokens)
    {
        var htmlBuilder = new StringBuilder();
        htmlBuilder.Append("<code style='font-family:\"Hack-Regular\", monospace;'>");
        foreach (var token in tokens)
        {
            var color = GetColorByToken(token);
            var value = HttpUtility.HtmlEncode(token.Lexeme)
                .Replace(" ", "&nbsp;")
                .Replace("\n", "<br>");
            htmlBuilder.Append($"<span style='color:{color};'>{value}</span>");
        }

        htmlBuilder.Append("</code>");
        return htmlBuilder.ToString();
    }

    public static Image GenerateImage(IList<Token> tokens)
    {
        var fontCollection = new FontCollection();
        var fontFamily = fontCollection.Add(FontPath);
        var font = fontFamily.CreateFont(FontSize, FontStyle.Regular);
        var options = new TextOptions(font)
        {
            Dpi = Dpi
        };

        var imageWidth = 0;
        var imageHeight = 0;
        var source = tokens.Aggregate("", (current, token) => current + token.Lexeme);
        var lineHeight = FontSize - font.FontMetrics.VerticalMetrics.Descender / Dpi + LineSpacing;
        foreach (var line in source.Split('\n'))
        {
            var textSize = TextMeasurer.MeasureAdvance(line, options);
            imageWidth = (int)Math.Round(Math.Max(imageWidth, textSize.Width));
            imageHeight += lineHeight;
        }

        imageWidth += PaddingX * 2;
        imageHeight += PaddingY * 2;

        var image = new Image<Rgba32>(imageWidth, imageHeight);
        image.Mutate(ctx => ctx.Fill(Color.ParseHex(BackgroundColor)));

        var xOffset = PaddingX;
        var yOffset = PaddingY;
        foreach (var token in tokens)
        {
            var color = GetColorByToken(token);
            foreach (var text in NewLineRegex().Split(token.Lexeme))
            {
                var textSize = TextMeasurer.MeasureAdvance(text, options);
                if (text == "\n")
                {
                    xOffset = PaddingX;
                    yOffset += lineHeight;
                    continue;
                }

                var finalXOffset = xOffset;
                var finalYOffset = yOffset;
                image.Mutate(ctx =>
                    ctx.DrawText(text, font, Color.ParseHex(color), new PointF(finalXOffset, finalYOffset)));
                xOffset += (int)Math.Round(textSize.Width);
            }
        }

        return image;
    }

    [GeneratedRegex(@"(\n)")]
    private static partial Regex NewLineRegex();

    private static string GetColorByToken(Token token)
    {
        return token.Type switch
        {
            >= TokenType.KeywordsBegin and <= TokenType.KeywordsEnd => KeywordColor,
            >= TokenType.OperatorsBegin and <= TokenType.OperatorsEnd => OperatorColor,
            >= TokenType.DelimitersBegin and <= TokenType.DelimitersEnd => DelimiterColor,
            TokenType.Comment => CommentColor,
            TokenType.Number => NumberColor,
            TokenType.String => StringColor,
            _ => IdentifierColor
        };
    }
}