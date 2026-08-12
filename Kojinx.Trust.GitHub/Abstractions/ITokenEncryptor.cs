namespace Kojinx.Trust.GitHub.Abstractions;

public interface ITokenEncryptor
{
    string ProtectToken(string plainTextToken);
    string? UnprotectToken(string protectedToken);
}
