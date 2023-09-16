// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Extensions.EnvironmentAbstractions;

namespace Microsoft.DotNet.Cli.Clean.Tests
{
    public class GivenDotnetCleanCleansBuildArtifacts : SdkTest
    {
        public GivenDotnetCleanCleansBuildArtifacts(ITestOutputHelper log) : base(log)
        {
        }

        [Fact]
        public void ItCleansAProjectBuiltWithRuntimeIdentifier()
        {
            var testAppName = "MSBuildTestApp";
            var testInstance = _testAssetsManager.CopyTestAsset(testAppName)
                .WithSource();

            new DotnetBuildCommand(Log, testInstance.Path)
                .Execute("-r", $"{ToolsetInfo.LatestWinRuntimeIdentifier}-x64")
                .Should().Pass();

            var configuration = Environment.GetEnvironmentVariable("CONFIGURATION") ?? "Debug";
            var outputFolder = new DirectoryInfo(OutputPathCalculator.FromProject(testInstance.Path).GetOutputDirectory(configuration: configuration, runtimeIdentifier: $"{ToolsetInfo.LatestWinRuntimeIdentifier}-x64"));

            outputFolder.Should().NotBeEmpty();

            new DotnetCommand(Log, "clean", testInstance.Path)
                .Execute("-r", $"{ToolsetInfo.LatestWinRuntimeIdentifier}-x64")
                .Should().Pass();

            outputFolder.Should().BeEmpty();
        }

        [Fact]
        public void ItDoesNotShowErrorWhenInvokedWithoutRestore()
        {
            const string runtimeIdentifier = $"{ToolsetInfo.LatestWinRuntimeIdentifier}-x64";
            var directory = _testAssetsManager.CreateTestDirectory();

            new DotnetNewCommand(Log, "console")
                .WithVirtualHive()
                .Execute("-o", directory.Path)
                .Should().Pass();

            var projectPath = Directory.GetFiles(directory.Path, "*.csproj").Single();
            AddRuntimeIdentifierToProject(projectPath, runtimeIdentifier);

            new CleanCommand(Log, projectPath)
                .Execute("-restore")
                .Should().Pass();
        }

        private static void AddRuntimeIdentifierToProject(string projectPath, string runtimeIdentifier)
        {
            var xDoc = XDocument.Load(projectPath);
            var propertyGroup = xDoc.Root!.Elements().Single(e => e.Name.LocalName == "PropertyGroup");
            propertyGroup.Add(new XElement("RuntimeIdentifier", runtimeIdentifier));
            xDoc.Save(projectPath);
        }
    }
}
