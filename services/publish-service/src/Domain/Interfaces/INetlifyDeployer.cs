namespace PublishService.Domain.Interfaces;

/// <summary>
/// Deploys to Netlify
/// </summary>
public interface INetlifyDeployer
{
    Task<string> DeployZipAsync(byte[] zipBytes, string netlifyToken);
}
