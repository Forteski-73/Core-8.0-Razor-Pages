namespace OXF.Services
{
    public interface ILoginAttemptService
    {
        int GetAttempts(string key);
        void IncrementAttempts(string key);
        void ResetAttempts(string key);
    }
}
