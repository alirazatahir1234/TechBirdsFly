namespace PublishService.Domain.Interfaces;

/// <summary>
/// Deploys to Vercel
/// </summary>
public interface IVercelDeployer
{
    Task<string> DeployAsync(string folderPath, string vercelToken);
}
