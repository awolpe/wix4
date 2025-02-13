// Copyright (c) .NET Foundation and contributors. All rights reserved. Licensed under the Microsoft Reciprocal License. See LICENSE.TXT file in the project root for full license information.

namespace WixToolsetTest.CoreIntegration
{
    using System;
    using System.IO;
    using System.Linq;
    using WixBuildTools.TestSupport;
    using WixToolset.Core.TestPackage;
    using Xunit;

    public class BadInputFixture
    {
        [Fact]
        public void HandleInvalidIds()
        {
            var folder = TestData.Get(@"TestData\BadInput");

            using (var fs = new DisposableFileSystem())
            {
                var baseFolder = fs.GetFolder();
                var intermediateFolder = Path.Combine(baseFolder, "obj");
                var wixlibPath = Path.Combine(intermediateFolder, @"test.wixlib");

                var result = WixRunner.Execute(new[]
                {
                    "build",
                    Path.Combine(folder, "InvalidIds.wxs"),
                    "-intermediateFolder", intermediateFolder,
                    "-o", wixlibPath,
                });

                Assert.Equal(330, result.ExitCode);
            }
        }

        [Fact]
        public void CantBuildSingleExeBundleWithInvalidArgument()
        {
            var folder = TestData.Get(@"TestData");

            using (var fs = new DisposableFileSystem())
            {
                var baseFolder = fs.GetFolder();
                var intermediateFolder = Path.Combine(baseFolder, "obj");
                var exePath = Path.Combine(baseFolder, @"bin\test.exe");

                var result = WixRunner.Execute(new[]
                {
                    "build",
                    Path.Combine(folder, "SingleExeBundle", "SingleExePackageGroup.wxs"),
                    Path.Combine(folder, "BundleWithPackageGroupRef", "Bundle.wxs"),
                    "-bindpath", Path.Combine(folder, "SimpleBundle", "data"),
                    "-bindpath", Path.Combine(folder, ".Data"),
                    "-intermediateFolder", intermediateFolder,
                    "-o", exePath,
                    "-nonexistentswitch", "param",
                });

                Assert.NotEqual(0, result.ExitCode);
                Assert.False(File.Exists(exePath));
            }
        }

        [Fact]
        public void RegistryKeyWithoutAttributesDoesntCrash()
        {
            var folder = TestData.Get(@"TestData\BadInput");

            using (var fs = new DisposableFileSystem())
            {
                var baseFolder = fs.GetFolder();
                var intermediateFolder = Path.Combine(baseFolder, "obj");
                var wixlibPath = Path.Combine(intermediateFolder, @"test.wixlib");

                var result = WixRunner.Execute(new[]
                {
                    "build",
                    Path.Combine(folder, "RegistryKey.wxs"),
                    "-intermediateFolder", intermediateFolder,
                    "-o", wixlibPath,
                });

                Assert.InRange(result.ExitCode, 2, Int32.MaxValue);
            }
        }

        [Fact]
        public void BundleExePackageWithNetfxProtocolIsRejected()
        {
            var folder = TestData.Get(@"TestData\BadInput");

            using (var fs = new DisposableFileSystem())
            {
                var baseFolder = fs.GetFolder();
                var intermediateFolder = Path.Combine(baseFolder, "obj");
                var wixlibPath = Path.Combine(intermediateFolder, @"test.wixlib");

                var result = WixRunner.Execute(new[]
                {
                    "build",
                    Path.Combine(folder, "BundleExePackageWithNetfxProtocol.wxs"),
                    "-intermediateFolder", intermediateFolder,
                    "-o", wixlibPath,
                });

                Assert.Equal(193, result.ExitCode);
            }
        }

        [Fact]
        public void BundleVariableWithBadTypeIsRejected()
        {
            var folder = TestData.Get(@"TestData\BadInput");

            using (var fs = new DisposableFileSystem())
            {
                var baseFolder = fs.GetFolder();
                var intermediateFolder = Path.Combine(baseFolder, "obj");
                var wixlibPath = Path.Combine(intermediateFolder, @"test.wixlib");

                var result = WixRunner.Execute(new[]
                {
                    "build",
                    Path.Combine(folder, "BundleVariable.wxs"),
                    "-intermediateFolder", intermediateFolder,
                    "-o", wixlibPath,
                });

                Assert.Equal(21, result.ExitCode);
            }
        }

        [Fact]
        public void BundleVariableWithHiddenPersistedIsRejected()
        {
            var folder = TestData.Get(@"TestData\BadInput");

            using (var fs = new DisposableFileSystem())
            {
                var baseFolder = fs.GetFolder();
                var intermediateFolder = Path.Combine(baseFolder, "obj");
                var wixlibPath = Path.Combine(intermediateFolder, @"test.wixlib");

                var result = WixRunner.Execute(new[]
                {
                    "build",
                    Path.Combine(folder, "HiddenPersistedBundleVariable.wxs"),
                    "-intermediateFolder", intermediateFolder,
                    "-o", wixlibPath,
                });

                Assert.Equal(193, result.ExitCode);
            }
        }

        [Fact]
        public void GuardsAgainstVariousBundleValuesFromLoc()
        {
            var folder = TestData.Get(@"TestData");

            using (var fs = new DisposableFileSystem())
            {
                var baseFolder = fs.GetFolder();
                var intermediateFolder = Path.Combine(baseFolder, "obj");

                var result = WixRunner.Execute(new[]
                {
                    "build",
                    Path.Combine(folder, "BundleWithInvalid", "BundleWithInvalidLocValues.wxs"),
                    "-loc", Path.Combine(folder, "BundleWithInvalid", "BundleWithInvalidLocValues.wxl"),
                    "-bindpath", Path.Combine(folder, ".Data"),
                    "-bindpath", Path.Combine(folder, "DecompileSingleFileCompressed"),
                    "-bindpath", Path.Combine(folder, "SimpleBundle", "data"),
                    "-intermediateFolder", intermediateFolder,
                    "-o", Path.Combine(baseFolder, @"bin\test.exe")
                });

                Assert.InRange(result.ExitCode, 2, Int32.MaxValue);

                var messages = result.Messages.Select(m => m.ToString()).ToList();
                messages.Sort();

                WixAssert.CompareLineByLine(new[]
                {
                    "*Search/@Condition contains the built-in Variable 'WixBundleAction', which is not available when it is evaluated. (Unavailable Variables are: 'WixBundleAction'.). Rewrite the condition to avoid Variables that are never valid during its evaluation.",
                    "Bundle/@Condition contains the built-in Variable 'WixBundleInstalled', which is not available when it is evaluated. (Unavailable Variables are: 'RebootPending', 'WixBundleAction', or 'WixBundleInstalled'.). Rewrite the condition to avoid Variables that are never valid during its evaluation.",
                    "ExePackage/@DetectCondition contains the built-in Variable 'WixBundleAction', which is not available when it is evaluated. (Unavailable Variables are: 'WixBundleAction'.). Rewrite the condition to avoid Variables that are never valid during its evaluation.",
                    "The *Search/@Variable attribute's value, 'WixBundleInstalled', is one of the illegal options: 'AdminToolsFolder', 'AppDataFolder', 'CommonAppDataFolder', 'CommonFiles64Folder', 'CommonFilesFolder', 'CompatibilityMode', 'Date', 'DesktopFolder', 'FavoritesFolder', 'FontsFolder', 'InstallerName', 'InstallerVersion', 'LocalAppDataFolder', 'LogonUser', 'MyPicturesFolder', 'NativeMachine', 'NTProductType', 'NTSuiteBackOffice', 'NTSuiteDataCenter', 'NTSuiteEnterprise', 'NTSuitePersonal', 'NTSuiteSmallBusiness', 'NTSuiteSmallBusinessRestricted', 'NTSuiteWebServer', 'PersonalFolder', 'Privileged', 'ProgramFiles64Folder', 'ProgramFiles6432Folder', 'ProgramFilesFolder', 'ProgramMenuFolder', 'RebootPending', 'SendToFolder', 'ServicePackLevel', 'StartMenuFolder', 'StartupFolder', 'System64Folder', 'SystemFolder', 'TempFolder', 'TemplateFolder', 'TerminalServer', 'UserLanguageID', 'UserUILanguageID', 'VersionMsi', 'VersionNT', 'VersionNT64', 'WindowsFolder', 'WindowsVolume', 'WixBundleAction', 'WixBundleCommandLineAction', 'WixBundleForcedRestartPackage', 'WixBundleElevated', 'WixBundleInstalled', 'WixBundleProviderKey', 'WixBundleTag', or 'WixBundleVersion'.",
                    "The CommandLine/@Condition attribute's value '=' is not a valid bundle condition.",
                    "The MsiPackage/@InstallCondition attribute's value '=' is not a valid bundle condition.",
                    "The MsiProperty/@Condition attribute's value '=' is not a valid bundle condition.",
                    //"The Variable/@Name attribute's value, 'WixBundleInstalled', is one of the illegal options: 'AdminToolsFolder', 'AppDataFolder', 'CommonAppDataFolder', 'CommonFiles64Folder', 'CommonFilesFolder', 'CompatibilityMode', 'Date', 'DesktopFolder', 'FavoritesFolder', 'FontsFolder', 'InstallerName', 'InstallerVersion', 'LocalAppDataFolder', 'LogonUser', 'MyPicturesFolder', 'NativeMachine', 'NTProductType', 'NTSuiteBackOffice', 'NTSuiteDataCenter', 'NTSuiteEnterprise', 'NTSuitePersonal', 'NTSuiteSmallBusiness', 'NTSuiteSmallBusinessRestricted', 'NTSuiteWebServer', 'PersonalFolder', 'Privileged', 'ProgramFiles64Folder', 'ProgramFiles6432Folder', 'ProgramFilesFolder', 'ProgramMenuFolder', 'RebootPending', 'SendToFolder', 'ServicePackLevel', 'StartMenuFolder', 'StartupFolder', 'System64Folder', 'SystemFolder', 'TempFolder', 'TemplateFolder', 'TerminalServer', 'UserLanguageID', 'UserUILanguageID', 'VersionMsi', 'VersionNT', 'VersionNT64', 'WindowsFolder', 'WindowsVolume', 'WixBundleAction', 'WixBundleCommandLineAction', 'WixBundleForcedRestartPackage', 'WixBundleElevated', 'WixBundleInstalled', 'WixBundleProviderKey', 'WixBundleTag', or 'WixBundleVersion'.",
                    "The 'REINSTALLMODE' MsiProperty is controlled by the bootstrapper and cannot be authored. (Illegal properties are: 'ACTION', 'ADDLOCAL', 'ADDSOURCE', 'ADDDEFAULT', 'ADVERTISE', 'ALLUSERS', 'REBOOT', 'REINSTALL', 'REINSTALLMODE', or 'REMOVE'.) Remove the MsiProperty element.",
                }, messages.ToArray());
            }
        }
    }
}
