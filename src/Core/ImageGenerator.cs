using System.Reflection;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace GamerScript.Core;

public static class ImageGenerator
{
    private const int FontSize = 32;
    private const int LineSpacing = 8;
    private const int Dpi = 72;
    private const int PaddingX = 32;
    private const int PaddingY = 32;
    private static readonly Color BackgroundColor = Color.ParseHex("#151718");
    private static readonly Color KeywordColor = Color.ParseHex("#e6cd69");
    private static readonly Color IdentifierColor = Color.ParseHex("#55b5db");
    private static readonly Color OperatorColor = Color.ParseHex("#9fca56");
    private static readonly Color DelimiterColor = Color.ParseHex("#cfd2d1");
    private static readonly Color CommentColor = Color.ParseHex("#41535b");
    private static readonly Color NumberColor = Color.ParseHex("#cd3f45");
    private static readonly Color StringColor = Color.ParseHex("#55b5db");

    private static readonly string FontPath =
        Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "",
            "resources/Hack-Regular.ttf");

    public static Image Generate(Lexer lexer)
    {
        var tokens = lexer.Tokenize();
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
        image.Mutate(ctx => ctx.Fill(BackgroundColor));

        var xOffset = PaddingX;
        var yOffset = PaddingY;
        foreach (var token in tokens)
        {
            var finalXOffset = xOffset;
            var finalYOffset = yOffset;
            var color = token.Type switch
            {
                >= TokenType.KeywordsBegin and <= TokenType.KeywordsEnd => KeywordColor,
                >= TokenType.OperatorsBegin and <= TokenType.OperatorsEnd => OperatorColor,
                >= TokenType.DelimitersBegin and <= TokenType.DelimitersEnd => DelimiterColor,
                TokenType.Comment => CommentColor,
                TokenType.Number => NumberColor,
                TokenType.String => StringColor,
                _ => IdentifierColor
            };
            image.Mutate(ctx => ctx.DrawText(token.Lexeme, font, color, new PointF(finalXOffset, finalYOffset)));
            var textSize = TextMeasurer.MeasureAdvance(token.Lexeme, options);
            xOffset += (int)Math.Round(textSize.Width);
            if (token.Type != TokenType.NewLine)
                continue;

            xOffset = PaddingX;
            yOffset += lineHeight;
        }

        return image;
    }
}