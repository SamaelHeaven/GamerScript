using System.CommandLine;
using GamerScript.Core;

var sourceArgument = new Argument<FileInfo>("source", "The path to the source file")
{
    Arity = ArgumentArity.ExactlyOne
};
sourceArgument.ExistingOnly();

var rootCommand = new RootCommand("Executes a GamerScript source file")
{
    sourceArgument
};
rootCommand.Name = "GamerScriptCLI";

rootCommand.SetHandler(source =>
{
    try
    {
        var sourceCode = File.ReadAllText(source.FullName);
        var lexer = new Lexer(sourceCode);
        var parser = new Parser(lexer);
        var programNode = parser.Parse();
        var interpreter = new Interpreter();
        interpreter.Evaluate(programNode);
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
}, sourceArgument);

return await rootCommand.InvokeAsync(args);