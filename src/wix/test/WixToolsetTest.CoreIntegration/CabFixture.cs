// Copyright (c) .NET Foundation and contributors. All rights reserved. Licensed under the Microsoft Reciprocal License. See LICENSE.TXT file in the project root for full license information.

namespace WixToolsetTest.CoreIntegration
{
    using System;
    using System.IO;
    using System.Linq;
    using WixBuildTools.TestSupport;
    using WixToolset.Core.TestPackage;
    using Xunit;

    public class CabFixture
    {
        [Fact]
        public void CabinetFilesSequencedCorrectly()
        {
            var folder = TestData.Get(@"TestData\MultiFileCompressed");

            using (var fs = new DisposableFileSystem())
            {
                var baseFolder = fs.GetFolder();
                var intermediateFolder = Path.Combine(baseFolder, "obj");
                var msiPath = Path.Combine(baseFolder, @"bin\test.msi");
                var cabPath = Path.Combine(baseFolder, @"bin\cab1.cab");

                var result = WixRunner.Execute(new[]
                {
                    "build",
                    Path.Combine(folder, "Package.wxs"),
                    Path.Combine(folder, "PackageComponents.wxs"),
                    "-d", "MediaTemplateCompressionLevel",
                    "-loc", Path.Combine(folder, "Package.en-us.wxl"),
                    "-bindpath", Path.Combine(folder, "data"),
                    "-intermediateFolder", intermediateFolder,
                    "-o", msiPath
                });

                result.AssertSuccess();
                Assert.True(File.Exists(cabPath));

                var fileTable = Query.QueryDatabase(msiPath, new[] { "File" });
                var fileRows = fileTable.Select(r => new FileRow(r)).OrderBy(f => f.Sequence).ToList();

                Assert.Equal(new[] { 1, 2 }, fileRows.Select(f => f.Sequence).ToArray());
                WixAssert.CompareLineByLine(new[] { "Notepad.exe", "test.txt" }, fileRows.Select(f => f.Name).ToArray());

                var files = Query.GetCabinetFiles(cabPath);
                WixAssert.CompareLineByLine(fileRows.Select(f => f.Id).ToArray(), files.Select(f => f.Name).ToArray());
            }
        }

        [Fact]
        public void CanCabReallyOldFiles()
        {
            var folder = TestData.Get(@"TestData", "SingleFileCompressed");

            using (var fs = new DisposableFileSystem())
            {
                var baseFolder = fs.GetFolder(create: true);
                var intermediateFolder = Path.Combine(baseFolder, "obj");
                var msiPath = Path.Combine(baseFolder, @"bin\test.msi");
                var cabPath = Path.Combine(baseFolder, @"bin\cab1.cab");

                var old = new DateTime(1980, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                var oldTxt = Path.Combine(baseFolder, "test.txt");
                File.Copy(Path.Combine(folder, "data", "test.txt"), oldTxt);
                File.SetCreationTime(oldTxt, old);
                File.SetLastWriteTime(oldTxt, old);
                File.SetLastAccessTime(oldTxt, old);

                var result = WixRunner.Execute(new[]
                {
                    "build",
                    Path.Combine(folder, "Package.wxs"),
                    Path.Combine(folder, "PackageComponents.wxs"),
                    "-d", "MediaTemplateCompressionLevel",
                    "-loc", Path.Combine(folder, "Package.en-us.wxl"),
                    "-bindpath", baseFolder,
                    "-intermediateFolder", intermediateFolder,
                    "-o", msiPath
                });

                result.AssertSuccess();
                Assert.True(File.Exists(cabPath));

                var fileTable = Query.QueryDatabase(msiPath, new[] { "File" });
                var fileRows = fileTable.Select(r => new FileRow(r)).OrderBy(f => f.Sequence).ToList();

                Assert.Equal(new[] { 1 }, fileRows.Select(f => f.Sequence).ToArray());
                WixAssert.CompareLineByLine(new[] { "test.txt" }, fileRows.Select(f => f.Name).ToArray());

                var files = Query.GetCabinetFiles(cabPath);
                WixAssert.CompareLineByLine(fileRows.Select(f => f.Id).ToArray(), files.Select(f => f.Name).ToArray());
            }
        }

        private class FileRow
        {
            public FileRow(string row)
            {
                row = row.Substring("File:".Length);

                var split = row.Split('\t');
                this.Id = split[0];
                this.Name = split[2];
                this.Sequence = Convert.ToInt32(split[7]);
            }

            public string Id { get; set; }

            public string Name { get; set; }

            public int Sequence { get; set; }
        }
    }
}
