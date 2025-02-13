// Copyright (c) .NET Foundation and contributors. All rights reserved. Licensed under the Microsoft Reciprocal License. See LICENSE.TXT file in the project root for full license information.

namespace WixToolset.BuildTasks
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.IO;
    using System.Xml;
    using Microsoft.Build.Framework;
    using Microsoft.Build.Utilities;

    /// <summary>
    /// This task refreshes the generated file for bundle projects.
    /// </summary>
    public class RefreshBundleGeneratedFile : Task
    {
        /// <summary>
        /// The list of files to generate.
        /// </summary>
        [Required]
        public ITaskItem[] GeneratedFiles { get; set; }

        /// <summary>
        /// All the project references in the project.
        /// </summary>
        [Required]
        public ITaskItem[] ProjectReferencePaths { get; set; }

        /// <summary>
        /// Gets a complete list of external cabs referenced by the given installer database file.
        /// </summary>
        /// <returns>True upon completion of the task execution.</returns>
        public override bool Execute()
        {
            ArrayList payloadGroupRefs = new ArrayList();
            ArrayList packageGroupRefs = new ArrayList();
            for (int i = 0; i < this.ProjectReferencePaths.Length; i++)
            {
                ITaskItem item = this.ProjectReferencePaths[i];

                if (!String.IsNullOrEmpty(item.GetMetadata(ToolsCommon.DoNotHarvest)))
                {
                    continue;
                }

                string projectPath = item.GetMetadata("MSBuildSourceProjectFile");
                string projectName = Path.GetFileNameWithoutExtension(projectPath);
                string referenceName = ToolsCommon.GetIdentifierFromName(ToolsCommon.GetMetadataOrDefault(item, "Name", projectName));

                string[] pogs = item.GetMetadata("RefProjectOutputGroups").Split(';');
                foreach (string pog in pogs)
                {
                    if (!String.IsNullOrEmpty(pog))
                    {
                        // TODO: Add payload group references and package group references once heat is generating them
                        ////payloadGroupRefs.Add(String.Format(CultureInfo.InvariantCulture, "{0}.{1}", referenceName, pog));
                        packageGroupRefs.Add(String.Format(CultureInfo.InvariantCulture, "{0}.{1}", referenceName, pog));
                    }
                }
            }

            XmlDocument doc = new XmlDocument();

            XmlProcessingInstruction head = doc.CreateProcessingInstruction("xml", "version='1.0' encoding='UTF-8'");
            doc.AppendChild(head);

            XmlElement rootElement = doc.CreateElement("Wix");
            rootElement.SetAttribute("xmlns", "http://wixtoolset.org/schemas/v4/wxs");
            doc.AppendChild(rootElement);

            XmlElement fragment = doc.CreateElement("Fragment");
            rootElement.AppendChild(fragment);

            XmlElement payloadGroup = doc.CreateElement("PayloadGroup");
            payloadGroup.SetAttribute("Id", "Bundle.Generated.Payloads");
            fragment.AppendChild(payloadGroup);

            XmlElement packageGroup = doc.CreateElement("PackageGroup");
            packageGroup.SetAttribute("Id", "Bundle.Generated.Packages");
            fragment.AppendChild(packageGroup);

            foreach (string payloadGroupRef in payloadGroupRefs)
            {
                XmlElement payloadGroupRefElement = doc.CreateElement("PayloadGroupRef");
                payloadGroupRefElement.SetAttribute("Id", payloadGroupRef);
                payloadGroup.AppendChild(payloadGroupRefElement);
            }

            foreach (string packageGroupRef in packageGroupRefs)
            {
                XmlElement packageGroupRefElement = doc.CreateElement("PackageGroupRef");
                packageGroupRefElement.SetAttribute("Id", packageGroupRef);
                packageGroup.AppendChild(packageGroupRefElement);
            }

            foreach (ITaskItem item in this.GeneratedFiles)
            {
                string fullPath = item.GetMetadata("FullPath");

                payloadGroup.SetAttribute("Id", Path.GetFileNameWithoutExtension(fullPath) + ".Payloads");
                packageGroup.SetAttribute("Id", Path.GetFileNameWithoutExtension(fullPath) + ".Packages");
                try
                {
                    doc.Save(fullPath);
                }
                catch (Exception e)
                {
                    // e.Message will be something like: "Access to the path 'fullPath' is denied."
                    this.Log.LogMessage(MessageImportance.High, "Unable to save generated file to '{0}'. {1}", fullPath, e.Message);
                }
            }

            return true;
        }
    }
}
