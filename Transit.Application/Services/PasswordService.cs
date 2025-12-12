namespace Transit.Application.Services;

public class PasswordService
{
    public string HashPassword(string password) => BCrypt.Net.BCrypt.HashPassword(password);
    public bool ValidatePassword(string encrypted, string password) => BCrypt.Net.BCrypt.Verify(password, encrypted);

}
