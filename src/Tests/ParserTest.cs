using GamerScript.Core;

namespace Tests;

public class ParserTest
{
    private const string SourceCode = """
                                      strat main() {
                                          loot x = 10
                                          loot y = 5.3
                                          loot result
                                          loot message = "Hello, Gamer!"
                                      
                                          clutch x > y + 5 {
                                              result = x + y
                                              taunt("X is greater than Y")
                                          } retry x == y {
                                              result = x * y
                                              taunt("X equals Y")
                                          } ragequit {
                                              result = x - y
                                              taunt("X is less than Y")
                                          }
                                      
                                          farm result > 0 {
                                              result = result - 1
                                              taunt(result)
                                              afk(1)
                                          }
                                      
                                          spawn 0
                                      }

                                      """;

    [Fact]
    private Task Test()
    {
        var lexer = new Lexer(SourceCode);
        var parser = new Parser(lexer);
        return Verify(parser.Parse());
    }
}