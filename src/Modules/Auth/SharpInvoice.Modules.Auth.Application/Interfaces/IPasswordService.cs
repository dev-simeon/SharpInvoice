namespace SharpInvoice.Modules.Auth.Application.Interfaces
{
    /// <summary>
    /// Service for handling password operations with enhanced security
    /// </summary>
    public interface IPasswordService
    {
        /// <summary>
        /// Hashes a password using secure algorithm with pepper
        /// </summary>
        /// <param name="password">The plain text password</param>
        /// <returns>The hashed password</returns>
        string HashPassword(string password);

        /// <summary>
        /// Verifies if a password matches the stored hash
        /// </summary>
        /// <param name="password">The plain text password to verify</param>
        /// <param name="hash">The stored hash to compare against</param>
        /// <returns>True if the password matches, false otherwise</returns>
        bool VerifyPassword(string password, string hash);
    }
}