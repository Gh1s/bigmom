using System.Threading;
using System.Threading.Tasks;

namespace Csb.BigMom.App
{
    /// <summary>
    /// Provides an abstraction to manage tokens.
    /// </summary>
    public interface ITokenService
    {
        /// <summary>
        /// Returns the cached id_token, access_token and its expiry.
        /// If the access_token has expired, it's automatically refreshed with the IDP.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns>A task that contains the tokens.</returns>
        Task<Tokens> GetTokensAsync(CancellationToken cancellationToken);
    }
}
