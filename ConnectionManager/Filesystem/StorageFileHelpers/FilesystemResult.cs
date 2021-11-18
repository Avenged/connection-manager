using ConnectionManager.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConnectionManager.Filesystem
{
    public class FilesystemResult
    {
        public FilesystemResult(FileSystemStatusCode errorCode)
        {
            ErrorCode = errorCode;
        }

        public FileSystemStatusCode ErrorCode { get; private set; }

        public static implicit operator FileSystemStatusCode(FilesystemResult res) => res.ErrorCode;

        public static implicit operator FilesystemResult(FileSystemStatusCode res) => new(res);

        public static implicit operator bool(FilesystemResult res) =>
            res.ErrorCode == FileSystemStatusCode.Success;

        public static explicit operator FilesystemResult(bool res) =>
            new FilesystemResult(res ? FileSystemStatusCode.Success : FileSystemStatusCode.Generic);
    }
}
