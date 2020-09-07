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

        public async Task WriteContentAsync (string content)
        {
            try
            {
                this.FullPath.EnsureFileDirectory ();

                // This is extremely strange, but I was getting
                // intermittent results (empty files) with the
                // await version. So this sync workaround
                // wfm
                if (true)
                    File.WriteAllText (this.FullPath, content);
                else
                    await File.WriteAllTextAsync (this.FullPath, content);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine (ex.ToString());
                Console.ResetColor ();
            }
        }

        public override string ToString ()
        {
            return this.FullPath;
        }
    }
}
