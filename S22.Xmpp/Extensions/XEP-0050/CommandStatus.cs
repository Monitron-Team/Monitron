using System;

namespace S22.Xmpp.Extensions
{
    /// <summary>
    /// Describes the current status of a command.
    /// </summary>
    public enum CommandStatus
    {
        /// <summary>
        /// The command is being executed.
        /// </summary>
        Executing,
        /// <summary>
        /// The command has completed. The command session has ended.
        /// </summary>
        Completed,
        /// <summary>
        /// The command has been canceled. The command session has ended.
        /// </summary>
        Canceled
    }
}

