namespace be.Application.Interfaces.Security;

public interface IEncryption
{
    string Encrypt(string text);
    string Decrypt(string text);
}