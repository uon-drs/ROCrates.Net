using System.Text.Json.Nodes;
using ROCrates.Models;
using File = System.IO.File;

namespace ROCrates.Tests;

public class TestDataset : IClassFixture<TestDatasetFixture>
{
    private const string _testDirName = "my-test-dir/";
    private readonly string _testFileJsonFile = "Fixtures/test-dataset.json";
    private TestDatasetFixture _testDatasetFixture;

    public TestDataset(TestDatasetFixture testDatasetFixture)
    {
        _testDatasetFixture = testDatasetFixture;
    }

    [Fact]
    public void TestWrite_Creates_Dir_From_Source()
    {
        // Arrange
        var sourceDir = Path.Combine(_testDatasetFixture.TestBasePath, _testDirName);
        Directory.CreateDirectory(sourceDir);
        var dataset = new Models.Dataset(
            new ROCrate(),
            source: Path.Combine(_testDatasetFixture.TestBasePath, _testDirName));

        // Act
        dataset.Write(_testDatasetFixture.TestBasePath);

        // Assert
        Assert.True(Directory.Exists(Path.Combine(_testDatasetFixture.TestBasePath, sourceDir)));
    }

    [Fact]
    public void TestWrite_Creates_Dir_From_DestPath()
    {
        // Arrange
        var sourceDir = Path.Combine(_testDatasetFixture.TestBasePath, _testDirName);
        var destPath = Path.Combine(_testDatasetFixture.TestBasePath, "ext", _testDirName);
        Directory.CreateDirectory(sourceDir);
        Directory.CreateDirectory(destPath);
        var dataset = new Models.Dataset(
            new ROCrate(),
            source: Path.Combine(_testDatasetFixture.TestBasePath, _testDirName),
            destPath: destPath);

        // Act
        dataset.Write(_testDatasetFixture.TestBasePath);

        // Assert
        Assert.True(Directory.Exists(Path.Combine(_testDatasetFixture.TestBasePath, destPath)));
    }

    [Fact]
    public void TestDataset_Serialises_Correctly()
    {
        // Arrange
        var expectedJson = File.ReadAllText(_testFileJsonFile).TrimEnd();
        var jsonObject = JsonNode.Parse(expectedJson).AsObject();

        var dataset = new Models.Dataset(
            new ROCrate(),
            properties: jsonObject);

        // Act
        var actualJson = dataset.Serialize();

        // Assert
        Assert.Equal(expectedJson, actualJson);
    }

    [Fact]
    public void DatasetIdTag_Is_UnixPath()
    {
        // Arrange
        var datasetSource = _testDatasetFixture.TestBasePath.Replace("/", "\\");
        var expected = _testDatasetFixture.TestBasePath + '/';

        // Act
        var dataset = new Dataset(source: datasetSource);

        // Assert
        Assert.Equal(expected, dataset.Id);
    }

    [Fact]
    public void DatasetIdTag_Ends_WithSlash()
    {
        // Arrange
        var datasetSource = _testDatasetFixture.TestBasePath;
        var expected = _testDatasetFixture.TestBasePath + '/';

        // Act
        var dataset = new Dataset(source: datasetSource);

        // Assert
        Assert.Equal(expected, dataset.Id);
    }

    [Fact]
    public void DatasetIdTag_Ends_WithoutSlashForRemoteSources()
    {
        // Arrange
        var datasetSource = _testDatasetFixture.TestRemotePath;
        var expected = _testDatasetFixture.TestRemotePath;

        // Act
        var dataset = new Dataset(source: datasetSource);

        // Assert
        Assert.Equal(expected, dataset.Id);
    }
}

public class TestDatasetFixture : IDisposable
{
    public readonly string TestBasePath = Path.Combine("dataset", "test", "path");
    public readonly string TestRemotePath = "https://workflowhub.eu/workflows/471?version=1";

    public TestDatasetFixture()
    {
        Directory.CreateDirectory(TestBasePath);
    }

    public void Dispose()
    {
        Directory.Delete(TestBasePath, recursive: true);
    }
}