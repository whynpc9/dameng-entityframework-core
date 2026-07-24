using Dm;

namespace W.EntityFrameworkCore.Dameng.Storage.Internal;

internal static class DamengTransientExceptionDetector
{
    // DmException.Number exposes both negative server codes and positive ADO.NET
    // provider codes. Keep this list deliberately narrow: configuration,
    // authentication, syntax, constraint, and data errors must never be retried.
    private static readonly HashSet<int> TransientErrorNumbers =
    [
        -3003, // EC_RN_DEADLOCK
        -3404, // EC_RN_STMT_TIMEOUT
        -6003, // EC_CONNECT_CAN_NOT_ESTABLISHED
        -6004, // EC_SNET_FAIL
        -6010, // EC_CONNECT_LOST
        6001, // ECNET_COMMUNITION_ERROR
        6027, // ECNET_NO_SOCKET_DATA
        6060, // ECNET_CONNECTION_CLOSED
        6089, // ECNET_COMMAND_TIME_OUT
        6123, // ECNET_CONNPOOL_TIMEOUT
    ];

    public static bool ShouldRetryOn(Exception exception)
        => exception is DmException dmException
            && TransientErrorNumbers.Contains(dmException.Number);
}
