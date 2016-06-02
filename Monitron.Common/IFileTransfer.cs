using System;

namespace Monitron.Common
{
    public interface IFileTransfer
    {
        string Id { get; }

        Identity From { get; }

        Identity To { get; }

        string Name { get; }

        long Size { get; }

        long Transferred { get; }
    }
}

