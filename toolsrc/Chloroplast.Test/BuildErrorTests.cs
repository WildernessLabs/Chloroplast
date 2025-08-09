using System;
using System.IO;
using Chloroplast.Core;
using Xunit;

namespace Chloroplast.Test
{
    public class BuildErrorTests
    {
        [Fact]
        public void BuildErrorCollectionShouldCollectErrors()
        {
            var collection = new BuildErrorCollection();
            Assert.False(collection.HasErrors);
            Assert.Equal(0, collection.ErrorCount);

            collection.AddError("test.md", "Test error message");

            Assert.True(collection.HasErrors);
            Assert.Equal(1, collection.ErrorCount);
            Assert.Single(collection.Errors);

            var error = collection.Errors[0];
            Assert.Equal("test.md", error.FilePath);
            Assert.Equal("Test error message", error.ErrorMessage);
        }

        [Fact]
        public void BuildErrorCollectionShouldHandleExceptions()
        {
            var collection = new BuildErrorCollection();
            var exception = new InvalidOperationException("Test exception");

            collection.AddError("test.xml", "XML parsing failed", exception);

            Assert.True(collection.HasErrors);
            var error = collection.Errors[0];
            Assert.Equal("test.xml", error.FilePath);
            Assert.Equal("XML parsing failed", error.ErrorMessage);
            Assert.Equal(exception, error.Exception);
        }

        [Fact]
        public void BuildErrorCollectionShouldClear()
        {
            var collection = new BuildErrorCollection();
            collection.AddError("test.md", "Test error");

            Assert.True(collection.HasErrors);

            collection.Clear();

            Assert.False(collection.HasErrors);
            Assert.Equal(0, collection.ErrorCount);
        }

        [Fact]
        public void BuildErrorShouldFormatCorrectly()
        {
            var error = new BuildError("example.md", "Parse failed");
            var errorString = error.ToString();

            Assert.Contains("example.md", errorString);
            Assert.Contains("Parse failed", errorString);
            Assert.Contains(error.Timestamp.ToString("HH:mm:ss"), errorString);
        }

        [Fact]
        public void BuildErrorCollectionShouldWriteToFile()
        {
            var collection = new BuildErrorCollection();
            collection.AddError("test1.md", "First error");
            collection.AddError("test2.xml", "Second error");

            var tempFile = Path.GetTempFileName();
            try
            {
                collection.WriteErrorsToFile(tempFile);

                var content = File.ReadAllText(tempFile);
                Assert.Contains("test1.md", content);
                Assert.Contains("First error", content);
                Assert.Contains("test2.xml", content);
                Assert.Contains("Second error", content);
            }
            finally
            {
                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }
            }
        }
    }
}