using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LeadMe
{
    /// <summary>
    /// Basic file class to hold references to the files found when searching the
    /// local directory.
    /// </summary>
    public class LocalFile
    {
        public readonly string fileName;
        public readonly string filePath;

        public LocalFile(string fileName, string filePath)
        {
            this.fileName = fileName;
            this.filePath = filePath;
        }
    }
}
