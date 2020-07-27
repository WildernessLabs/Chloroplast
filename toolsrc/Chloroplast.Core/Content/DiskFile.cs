﻿using System;
using System.IO;
using System.Threading.Tasks;
using Chloroplast.Core.Extensions;

namespace Chloroplast.Core.Content
{
    public class DiskFile : IFile
    {
        public DiskFile (string fullPath, string relativePath)
        {
            this.FullPath = fullPath;

            FileInfo info = new FileInfo (fullPath);
            if (info.Exists)
                this.LastUpdated = info.LastWriteTime;
            else
                this.LastUpdated = DateTime.MinValue;
                
            this.RootRelativePath = relativePath;
        }

        public DateTime LastUpdated { get; set; }
        public string RootRelativePath { get; set; }
        public string FullPath { get; }

        public void CopyTo (IFile target) 
        {
            if (target is DiskFile)
            {
                try {
                    var dtarget = target as DiskFile;
                    dtarget.FullPath.EnsureFileDirectory();
                    File.Copy(this.FullPath, dtarget.FullPath, true);
                }
                catch(Exception ex) {
                    Console.WriteLine(ex.Message);
                }
            }
            else 
            {
                throw new NotImplementedException($"can't copy to a {target.GetType().Name}");
            }
        }

        public Task<string> ReadContentAsync () =>
            File.ReadAllTextAsync (this.FullPath);

        public Task WriteContentAsync (string content)
        {
            this.FullPath.EnsureFileDirectory();

            return File.WriteAllTextAsync (this.FullPath, content);
        }
    }
}
