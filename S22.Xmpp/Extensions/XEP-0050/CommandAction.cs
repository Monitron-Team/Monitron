using System;

namespace S22.Xmpp.Extensions
{
    [Flags]
    public enum CommandAction
    {
        /// <summary>
        /// The command should be executed or continue to be executed.
        /// This is the default value.
        /// </summary>
        Execute,
        /// <summary>
        /// The command should be canceled.
        /// </summary>
        Cancel,
        /// <summary>
        /// The command should be digress to the previous stage of execution.
        /// </summary>
        Previous,
        /// <summary>
        /// The command should progress to the next stage of execution.
        /// </summary>
        Next,
        /// <summary>
        /// The command should be completed (if possible).
        /// </summary>
        Complete
    }
}

