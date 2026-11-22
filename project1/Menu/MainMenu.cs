using FoodExpress.Exceptions;
using FoodExpress.Models;
using FoodExpress.Repositories;
using FoodExpress.Services;
using static FoodExpress.Menu.MenuInput;

namespace FoodExpress.Menu
{
    /// <summary>Provides registration, verification, login, and application exit workflows.</summary>
    public sealed class MainMenu
    {
        private readonly AuthService _auth;
        private readonly EmailVerificationService _verification;
        private readonly IUserRepository _users;
        private readonly CustomerMenu _customerMenu;
        private readonly AdminMenu _adminMenu;
        private readonly LoggerService _logger;

        public MainMenu(AuthService auth, EmailVerificationService verification, IUserRepository users, CustomerMenu customerMenu, AdminMenu adminMenu, LoggerService logger)
        {
            _auth = auth;
            _verification = verification;
            _users = users;
            _customerMenu = customerMenu;
            _adminMenu = adminMenu;
            _logger = logger;
        }

        public async Task RunAsync()
        {
            try
            {
                while (true)
                {
                    int choice = Choose("FoodExpress",
                        (1, "Register"), (2, "Verify email"), (3, "Login"), (0, "Exit"));
                    switch (choice)
                    {
                        case 1:
                            await RegisterAsync();
                            break;
                        case 2:
                            await VerifyAsync();
                            break;
                        case 3:
                            await LoginAsync();
                            break;
                        case 0:
                            return;
                    }
                }
            }
            catch (EndOfInputException)
            {
                // The input stream closed while a menu or form was open.
            }
        }

        private async Task RegisterAsync()
        {
            try
            {
                string username = ReadText("Username: ");
                string email = ReadText("Email: ");
                string password = ReadPassword("Password: ");
                Customer customer = _auth.Register(username, email, password);
                Console.WriteLine("Account created. A verification code is being sent to your email.");
                await _verification.SendCodeAsync(customer);
                Console.WriteLine("Use Verify email from the main menu to activate your account.");
            }
            catch (Exception ex) when (ex is not EndOfInputException)
            {
                ShowError(ex);
            }
        }

        private async Task VerifyAsync()
        {
            try
            {
                string username = ReadText("Username: ");
                if (_users.FindByUsername(username) is not Customer customer)
                {
                    throw new ValidationException("Customer account was not found.");
                }

                int choice = Choose("Verify email", (1, "Enter verification code"), (2, "Resend code"));
                if (choice == 2)
                {
                    await _verification.SendCodeAsync(customer, isResend: true);
                    Console.WriteLine("A new code has been sent.");
                    return;
                }

                _verification.Verify(customer, ReadText("6-digit code: "));
                Console.WriteLine("Email verified. Your account is active.");
            }
            catch (Exception ex) when (ex is not EndOfInputException)
            {
                ShowError(ex);
            }
        }

        private async Task LoginAsync()
        {
            try
            {
                User user = _auth.Login(ReadText("Email: "), ReadPassword("Password: "));
                if (user is Customer customer)
                {
                    _customerMenu.LoadCart(customer);
                    await _customerMenu.RunAsync(customer);
                }
                else if (user is Admin admin)
                {
                    await _adminMenu.RunAsync(admin);
                }
            }
            catch (Exception ex) when (ex is not EndOfInputException)
            {
                ShowError(ex);
            }
        }

        private void ShowError(Exception exception) => Console.WriteLine(ErrorHandler.Handle(exception, _logger));
    }
}
