using EnvDTE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Company.VSPackage1.Classes
{
    public class FileListObject
    {
        public string Name { get; private set; }
        public string Path { get; private set; }
        public ProjectItem Item { get; private set; }

        public FileListObject(string name, string path, ProjectItem item)
        {
            Name = name;
            Path = path;
            Item = item;
        }

        public override string ToString()
        {
            return Path + " - " + Name;
        }
    }
}
