namespace BoltBrain.Services
{
    public interface IAiService
    {
        Task<string> GenerateContentAsync(string prompt);
    }
}
