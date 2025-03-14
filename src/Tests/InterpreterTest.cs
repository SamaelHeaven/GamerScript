using GamerScript.Core;

namespace Tests;

public class InterpreterTest
{
    [Theory]
    [InlineData("Print", """
        taunt("Hello, World!")
        """)]
    [InlineData("Function", """
        dlc func() {
            taunt("Functions works")
        }

        dlc calc(x, y) {
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

        dlc test(x) {
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
    [InlineData("Bill", """
        loot price = 28.5
        loot TPS = 0.05
        loot TVQ = 0.09975
        loot tps = price * TPS
        loot tvq = price * TVQ
        loot total = price + tps + tvq

        taunt("Price : " + price)
        taunt("TPS (5%) : " + tps)
        taunt("TVQ (9.975%) : " + tvq)
        taunt("Total : " + total)
        """)]
    [InlineData("Fibonacci", """
        dlc fibonacci(n) {
            clutch n <= 1 {
                spawn n
            }
        
            loot a = fibonacci(n - 1)
            loot b = fibonacci(n - 2)
        
            spawn a + b
        }

        loot i = 1
        farm i < 10 {
            taunt(fibonacci(i))
            buff i
        }
        """)]
    [InlineData("Triangle", """
        dlc displayTriangle(size) {
            loot i = 1
            farm i <= size {
                loot line = ""
                loot spaces = size - i
                loot j = 1
        
                farm j <= spaces {
                    line = line + " "
                    buff j
                }
        
                loot k = 1
                farm k <= 2 * i - 1 {
                    clutch k == 1 {
                        line = line + "*"
                    } retry k == 2 * i - 1 {
                        line = line + "*"
                    } retry i == size {
                        line = line + "*"
                    } ragequit {
                        line = line + " "
                    }
                    buff k
                }
        
                taunt(line)
                buff i
            }
        }

        loot size = 8
        displayTriangle(size)
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