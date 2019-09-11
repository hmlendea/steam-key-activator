using System;

namespace SteamKeyActivator.Service
{
    /// <summary>
    /// Key activation exception.
    /// </summary>
    public class KeyActivationException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="KeyActivationException"/> class.
        /// </summary>
        /// <param name="message">Exception message.</param>
        public KeyActivationException(string message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyActivationException"/> class.
        /// </summary>
        /// <param name="message">Exception message.</param>
        /// <param name="innerException">Inner exception.</param>
        public KeyActivationException(string message, Exception innerException)
        {
        }
    }
}
