using System;

namespace S22.Xmpp.Extensions
{
    public enum NoteType
    {
        /// <summary>
        /// The note is informational only. This is not really an exceptional condition.
        /// </summary>
        Info,
        /// <summary>
        /// The note indicates a warning. Possibly due to illogical (yet valid) data.
        /// </summary>
        Warn,
        /// <summary>
        /// The note indicates an error. The text should indicate the reason for the error.
        /// </summary>
        Error
    }
}

