using System.CommandLine;
using GamerScript.Core;
using SixLabors.ImageSharp;

var sourceArgument = new Argument<FileInfo>("source", "The path to the source file")
{
    Arity = ArgumentArity.ExactlyOne
};
sourceArgument.ExistingOnly();

var imageOption = new Option<string?>(
    "--image",
    "The name of the image to generate");
imageOption.AddAlias("-i");

var rootCommand =
    new RootCommand("Executes a GamerScript source file or generates an image showcasing the highlighted code")
    {
        sourceArgument,
        imageOption
    };
rootCommand.Name = "GamerScript.CLI";

rootCommand.SetHandler((sourceInfo, imageName) =>
{
    try
    {
        var sourceCode = File.ReadAllText(sourceInfo.FullName);
        var lexer = new Lexer(sourceCode);
        if (imageName is null)
        {
            var parser = new Parser(lexer);
            var interpreter = new Interpreter(parser);
            interpreter.Evaluate();
            return;
        }

        var image = ImageGenerator.Generate(lexer);
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
}, sourceArgument, imageOption);

return await rootCommand.InvokeAsync(args);