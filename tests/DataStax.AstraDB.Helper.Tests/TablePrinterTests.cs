using FluentAssertions;
using RestApiSample;
using Xunit;

namespace DataStax.AstraDB.Helper.Tests;

public class TablePrinterTests
{
    [Fact]
    public void TablePrinter_prints_single_row()
    {
        var row = new UserDoc
        {
            email = "bob@example.com",
            name = "Bob",
            password = "secret",
            user_id = Guid.Parse("12f7edc0-6145-413a-8d1a-1c74b4545eb8")
        };

        using var sw = new StringWriter();
        var originalOut = Console.Out;
        Console.SetOut(sw);

        try
        {
            TablePrinter.PrintTable(new[] { row });
        }
        finally
        {
            Console.SetOut(originalOut);
        }

        var output = sw.ToString();
        output.Should().Contain("email")
              .And.Contain("name")
              .And.Contain("password")
              .And.Contain("user_id")
              .And.Contain("bob@example.com")
              .And.Contain("Bob")
              .And.Contain("secret")
              .And.Contain("12f7edc0-6145-413a-8d1a-1c74b4545eb8");
    }

    [Fact]
    public void TablePrinter_handles_empty_sequence()
    {
        using var sw = new StringWriter();
        var originalOut = Console.Out;
        Console.SetOut(sw);

        try
        {
            TablePrinter.PrintTable(Array.Empty<UserDoc>());
        }
        finally
        {
            Console.SetOut(originalOut);
        }

        sw.ToString().Trim().Should().Be("No rows.");
    }
}
