namespace testIdentity.Models
{
    public interface IUserRepo
    {
        bool ValidateCredentials(string username, string password);

        User FindById(string id);

        User FindByUsername(string username);
    }
}