using System;
using System.Collections.Generic;
using FubuCore;

namespace FubuMVC.Core.Packaging
{
    [MarkedForTermination]
    public interface IPackageFiles
    {
        void AddDirectory(string directory);
        void ForFiles(FileSet files, Action<string> readAction);
        IEnumerable<string> FindFiles(FileSet files);

        IEnumerable<string> Directories { get; }
    }
}