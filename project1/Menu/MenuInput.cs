using Spectre.Console;

namespace FoodExpress.Menu
{
    internal static class MenuInput
    {
        private static bool IsInteractive =>
            !Console.IsInputRedirected && !Console.IsOutputRedirected && AnsiConsole.Profile.Capabilities.Interactive;

        public static int Choose(string title, params (int Number, string Label)[] options)
        {
            if (IsInteractive)
            {
                var prompt = new SelectionPrompt<(int Number, string Label)>
                {
                    Title = Markup.Escape(title),
                    Converter = option => $"{option.Number}. {option.Label}"
                };
                prompt.AddChoices(options);
                return AnsiConsole.Prompt(prompt).Number;
            }

            Console.WriteLine($"\n=== {title} ===");
            foreach (var option in options)
            {
                Console.WriteLine($"{option.Number}. {option.Label}");
            }

            while (true)
            {
                int choice = ReadInt("Choose an option: ");
                if (options.Any(option => option.Number == choice))
                {
                    return choice;
                }

                Console.WriteLine("Select one of the listed options.");
            }
        }

        public static string ReadText(string prompt)
        {
            Console.Write(prompt);
            return Console.ReadLine() ?? throw new EndOfInputException();
        }

        public static string ReadPassword(string prompt)
        {
            if (!IsInteractive)
            {
                return ReadText(prompt);
            }

            var password = new TextPrompt<string>(Markup.Escape(prompt)).Secret();
            password.AllowEmpty = true;
            return AnsiConsole.Prompt(password);
        }

        public static int ReadInt(string prompt)
        {
            while (true)
            {
                if (int.TryParse(ReadText(prompt), out int value))
                {
                    return value;
                }

                Console.WriteLine("Enter a whole number.");
            }
        }

        internal sealed class EndOfInputException : Exception
        {
        }
    }
}
