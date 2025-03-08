using System.CommandLine;
using GamerScript.Core;
using SixLabors.ImageSharp;
using TextCopy;

var sourceArgument = new Argument<FileInfo>("source", "The path to the source file")
{
    Arity = ArgumentArity.ExactlyOne
};
sourceArgument.ExistingOnly();

var imageOption = new Option<string?>(
    "--image",
    "Generates a PNG image of the highlighted code with the specified name");
imageOption.AddAlias("-i");

var htmlOption = new Option<bool>(
    "--html",
    "Generates HTML of the highlighted code and copy the result to the clipboard");
htmlOption.AddAlias("-h");

var rootCommand =
    new RootCommand("Executes a GamerScript source file or generates highlighted code")
    {
        sourceArgument,
        imageOption,
        htmlOption
    };
rootCommand.Name = "GamerScript.CLI";

rootCommand.SetHandler((sourceInfo, imageName, html) =>
{
    try
    {
        var sourceCode = File.ReadAllText(sourceInfo.FullName);
        var lexer = new Lexer(sourceCode);
        var tokens = lexer.Tokenize();
        if (imageName is null && !html)
        {
            var parser = new Parser(tokens);
            var interpreter = new Interpreter();
            interpreter.Interpret(parser.Parse());
            return;
        }

        if (html)
        {
            var htmlContent = CodeHighlighter.GenerateHtml(tokens);
            ClipboardService.SetText(htmlContent);
        }

        if (imageName is null)
            return;
        var image = CodeHighlighter.GenerateImage(tokens);
        image.SaveAsPng($"{imageName}.png");
    }
    catch (Exception e)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Error.WriteLine(e.Message);
        if (e is not GsException && e.StackTrace is not null)
            Console.Error.WriteLine(e.StackTrace);
        Console.ResetColor();
        Environment.Exit(1);
    }
}, sourceArgument, imageOption, htmlOption);

return await rootCommand.InvokeAsync(args);