using System;
using System.Net.Sockets;

namespace MongoDB.Driver.Internal
{
    /// <summary>
    /// There may be stale connections in the connection pool,
    /// but the MongoDB driver doesn't handle it and just throw an exception.
    /// https://groups.google.com/forum/#!msg/mongodb-user/YFcZOWcZxKY/2N-jPkrNnpsJ
    /// </summary>
    internal class StaleConnection
    {
        public static void Retry(Action action)
        {
            try
            {
                action();
            }
            catch (MongoConnectionException ex)
            {
                if (ShouldRetry(ex))
                {
                    action();
                }
                else
                {
                    throw;
                }
            }
        }

        public static T Retry<T>(Func<T> func)
        {
            try
            {
                return func();
            }
            catch (Exception ex)
            {
                if (ShouldRetry(ex))
                {
                    return func();
                }
                throw;
            }
        }

        private static bool ShouldRetry(Exception exception)
        {
            var innerException = exception;
            while (innerException != null)
            {
                var socketException = innerException as SocketException;
                if (socketException != null)
                {
                    if (socketException.SocketErrorCode == SocketError.ConnectionReset)
                    {
                        return true;
                    }
                }

                innerException = innerException.InnerException;
            }
            return false;
        }
    }
}
