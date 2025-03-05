using GamerScript.Core;

namespace Tests;

public class InterpreterTest
{
    [Theory]
    [InlineData("Print", """
        taunt("Hello, World!")
        """)]
    [InlineData("Function", """
        strat func() {
            taunt("Functions works")
        }

        strat calc(x, y) {
            spawn x + y
        }

        func()
        taunt(calc(2, 5))
        """)]
    [InlineData("Condition", """
        clutch buffed {
            taunt("If statements works")
        }

        loot condition = nerfed
        clutch condition {
            taunt("Should not display")
        }

        clutch condition {
            taunt("Should not display")
        } ragequit {
            taunt("Should display")
        }

        strat test(x) {
            clutch x > 0 {
                taunt("X is bigger than 0")
            } retry x < 0 {
                taunt("X is smaller than 0")
            } ragequit {
                taunt("X is equal to 0")
            }
        }

        test(10)
        test(-4)
        test(0)
        """)]
    [InlineData("Loop", """
        loot i = 0
        farm i < 10 {
            taunt(i)
            buff i
        }
        """)]
    public Task Test(string testName, string sourceCode, bool autoVerify = false)
    {
        var settings = new VerifySettings();
        settings.UseMethodName(testName);
        if (autoVerify)
            settings.AutoVerify();
        var stdout = new StringWriter();
        var interpreter = new Interpreter(stdout);
        interpreter.Interpret(sourceCode);
        return Verify(stdout.ToString(), settings);
    }
}