using ROCrates.Exceptions;
using ROCrates.Models;
using File = ROCrates.Models.File;

namespace ROCrates.Tests;

public class TestRoCrate
{
    [Fact]
    public void Add_Adds_ObjetsOfDifferentTypes()
    {
        var roCrate = new ROCrate();
        var person = new Person(roCrate);
        var file = new File(roCrate, source: "my-test-file.txt");
        var dataset = new Dataset(roCrate, source: "my-data-dir/");

        roCrate.Add(person, file, dataset);

        Assert.True(roCrate.Entities.ContainsKey(person.Id));
        Assert.True(roCrate.Entities.TryGetValue(person.Id, out var recoveredPerson));
        Assert.IsType<Person>(recoveredPerson);

        Assert.True(roCrate.Entities.ContainsKey(file.Id));
        Assert.True(roCrate.Entities.TryGetValue(file.Id, out var recoveredFile));
        Assert.IsType<File>(recoveredFile);

        Assert.True(roCrate.Entities.ContainsKey(dataset.Id));
        Assert.True(roCrate.Entities.TryGetValue(dataset.Id, out var recoveredDataset));
        Assert.IsType<Dataset>(recoveredDataset);
    }

    [Fact]
    public void Add_Updates_EntitiesWithSameName()
    {
        // Arrange
        var roCrate = new ROCrate();
        var (testKey, testValue) = ("key", "value");
        var file1 = new File(crate: roCrate, source: "file.txt");
        roCrate.Add(file1);
        var file2 = new File(crate: roCrate, source: file1.Id);
        file2.SetProperty(testKey, testValue);

        // Act
        roCrate.Add(file2);

        // Assert
        Assert.Equal(file1.Id, file2.Id);
        Assert.Contains(file1.Id, roCrate.Entities.Keys);
        Assert.Contains(testKey, roCrate.Entities[file1.Id].Properties);
    }

    [Fact]
    public void Save_Creates_DirectoryWithFiles()
    {
        // Arrange
        var file1 = new FileInfo(Guid.NewGuid().ToString());
        file1.Create().Close();
        var file2 = new FileInfo(Guid.NewGuid().ToString());
        file2.Create().Close();
        var outputDir = Guid.NewGuid().ToString();

        var roCrate = new ROCrate();
        var fileObject1 = new File(crate: roCrate, source: file1.ToString());
        var fileObject2 = new File(crate: roCrate, source: file2.ToString());
        roCrate.Add(fileObject1, fileObject2);

        // Act
        roCrate.Save(outputDir);

        // Assert
        Assert.True(System.IO.File.Exists(Path.Combine(outputDir, file1.Name)));
        Assert.True(System.IO.File.Exists(Path.Combine(outputDir, file2.Name)));

        // Clean up
        if (file1.Exists) file1.Delete();
        if (file2.Exists) file2.Delete();
        if (Directory.Exists(outputDir)) Directory.Delete(outputDir, recursive: true);
    }

    [Fact]
    public void Save_Creates_ZipWithFiles()
    {
        // Arrange
        var file1 = new FileInfo(Guid.NewGuid().ToString());
        file1.Create().Close();
        var file2 = new FileInfo(Guid.NewGuid().ToString());
        file2.Create().Close();
        var outputDir = Guid.NewGuid().ToString();
        var outputZipFile = $"{outputDir}.zip";

        var roCrate = new ROCrate();
        var fileObject1 = new File(crate: roCrate, source: file1.ToString());
        var fileObject2 = new File(crate: roCrate, source: file2.ToString());
        roCrate.Add(fileObject1, fileObject2);

        // Act
        roCrate.Save(outputDir, zip: true);

        // Assert
        Assert.True(System.IO.File.Exists(outputZipFile));

        // Clean up
        if (file1.Exists) file1.Delete();
        if (file2.Exists) file2.Delete();
        if (Directory.Exists(outputDir)) Directory.Delete(outputDir, recursive: true);
        if (System.IO.File.Exists(outputZipFile)) System.IO.File.Delete(outputZipFile);
    }

    [Fact]
    public void Save_Saves_DatasetsFirst()
    {
        // Arrange
        var roCrate = new ROCrate();
        var dirInfo = new DirectoryInfo(Guid.NewGuid().ToString());
        dirInfo.Create();
        var fileInfo = new FileInfo(Path.Combine(dirInfo.FullName, Guid.NewGuid().ToString()));
        fileInfo.Create().Close();

        var file = new File(crate: roCrate, source: fileInfo.FullName);
        var dataset = new Dataset(crate: roCrate, source: dirInfo.FullName);

        roCrate.Add(file, dataset);

        // Act
        roCrate.Save();

        // Assert
        Assert.True(dirInfo.Exists);
        Assert.True(fileInfo.Exists);

        // Clean up
        if (fileInfo.Exists) fileInfo.Delete();
        if (dirInfo.Exists) dirInfo.Delete(recursive: true);
    }

    [Fact]
    public void Initialise_Throws_WithNoMetadataJsonFile()
    {
        // Arrange
        var testDir = new DirectoryInfo(Guid.NewGuid().ToString());
        testDir.Create();
        var roCrate = new ROCrate();

        // Act
        var action = () => roCrate.Initialise(testDir.FullName);

        // Assert
        Assert.Throws<CrateReadException>(action);

        // Clean up
        if (testDir.Exists) testDir.Delete(recursive: true);
    }

    [Fact]
    public void Initialise_Throws_WhenSourceNonExistent()
    {
        // Arrange
        var testDir = new DirectoryInfo(Guid.NewGuid().ToString());
        var roCrate = new ROCrate();

        // Act
        var action = () => roCrate.Initialise(testDir.FullName);

        // Assert
        Assert.Throws<CrateReadException>(action);
    }

    [Fact]
    public void Initialise_Throws_WhenSourceIsNotADirectory()
    {
        // Arrange
        var testFile = new FileInfo(Guid.NewGuid().ToString());
        testFile.Create();
        var roCrate = new ROCrate();

        // Act
        var action = () => roCrate.Initialise(testFile.FullName);

        // Assert
        Assert.Throws<CrateReadException>(action);

        // Clean up
        if (testFile.Exists) testFile.Delete();
    }

    [Fact]
    public void Initialise_Throws_WhenMetadataIsEmpty()
    {
        // Arrange
        var testDir = new DirectoryInfo(Guid.NewGuid().ToString());
        testDir.Create();
        var testFile = new FileInfo(Path.Combine(testDir.FullName, "ro-crate-metadata.json"));
        testFile.Create().Close();
        var roCrate = new ROCrate();

        // Act
        var action = () => roCrate.Initialise(testDir.FullName);

        // Assert
        Assert.Throws<MetadataException>(action);

        // Clean up
        if (testDir.Exists) testDir.Delete(recursive: true);
    }

    [Fact]
    public void Initialise_Throws_WhenMetadataMissingContentAndGraph()
    {
        // Arrange
        var testDir = new DirectoryInfo(Guid.NewGuid().ToString());
        testDir.Create();
        var testFile = new FileInfo(Path.Combine(testDir.FullName, "ro-crate-metadata.json"));
        using (var s = testFile.CreateText())
        {
            s.Write("{}");
        }

        var roCrate = new ROCrate();

        // Act
        var action = () => roCrate.Initialise(testDir.FullName);

        // Assert
        Assert.Throws<MetadataException>(action);

        // Clean up
        if (testDir.Exists) testDir.Delete(recursive: true);
    }

    [Fact]
    public void Initialise_Throws_WhenGraphElementIsInvalid()
    {
        // Arrange
        var testDir = new DirectoryInfo(Guid.NewGuid().ToString());
        testDir.Create();
        System.IO.File.Copy(
            "Fixtures/test-invalid-metadata.json",
            Path.Combine(testDir.FullName, "ro-crate-metadata.json")
        );
        var roCrate = new ROCrate();

        // Act
        var action = () => roCrate.Initialise(testDir.FullName);

        // Assert
        Assert.Throws<MetadataException>(action);

        // Clean up
        if (testDir.Exists) testDir.Delete(recursive: true);
    }

    [Fact]
    public void Initialise_Reads_EntitiesToROCrateObject()
    {
        // Arrange
        var testDir = new DirectoryInfo(Guid.NewGuid().ToString());
        testDir.Create();
        System.IO.File.Copy(
            "Fixtures/metadata-test.json",
            Path.Combine(testDir.FullName, "ro-crate-metadata.json")
        );
        var roCrate = new ROCrate();

        // Act
        roCrate.Initialise(testDir.FullName);

        // Assert
        Assert.Contains("./", roCrate.Entities.Keys);
        Assert.Contains("ro-crate-metadata.json", roCrate.Entities.Keys);
        Assert.Contains("ro-crate-preview.html", roCrate.Entities.Keys);
        Assert.Contains("cp7glop.ai", roCrate.Entities.Keys);
        Assert.Contains("lots_of_little_files/", roCrate.Entities.Keys);

        // Clean up
        if (testDir.Exists) testDir.Delete(recursive: true);
    }

    [Fact]
    public void Initialise_Reads_ObjectsWithCompoundTypes()
    {
        // Arrange
        var testDir = new DirectoryInfo(Guid.NewGuid().ToString());
        testDir.Create();
        System.IO.File.Copy(
            "Fixtures/test-compound-types.json",
            Path.Combine(testDir.FullName, "ro-crate-metadata.json")
        );
        var roCrate = new ROCrate();

        // Act
        roCrate.Initialise(testDir.FullName);

        // Assert
        Assert.Contains("./", roCrate.Entities.Keys);
        Assert.Contains("ro-crate-metadata.json", roCrate.Entities.Keys);
        Assert.Contains("ro-crate-preview.html", roCrate.Entities.Keys);
        Assert.Contains("alignment.knime", roCrate.Entities.Keys);

        // Clean up
        if (testDir.Exists) testDir.Delete(recursive: true);
    }

    [Fact]
    public void Convert_Creates_PreviewAndMetadata()
    {
        // Arrange
        var testDir = new DirectoryInfo(Guid.NewGuid().ToString());
        testDir.Create();
        var roCrate = new ROCrate();

        // Act
        roCrate.Convert(testDir.FullName);

        // Assert
        Assert.True(System.IO.File.Exists(Path.Combine(testDir.FullName, roCrate.Metadata.Id)));
        Assert.True(System.IO.File.Exists(Path.Combine(testDir.FullName, roCrate.Preview.Id)));

        // Clean up
        if (testDir.Exists) testDir.Delete(recursive: true);
    }

    [Fact]
    public void Convert_Creates_ROCrateWithAllEntities()
    {
        // Arrange
        var testDir = new DirectoryInfo(Guid.NewGuid().ToString());
        testDir.Create();
        var subDir = testDir.CreateSubdirectory(Guid.NewGuid().ToString());
        var topLevelFile = new FileInfo($"{Guid.NewGuid().ToString()}.json");
        topLevelFile.Create().Close();
        topLevelFile.MoveTo(Path.Combine(testDir.FullName, topLevelFile.Name));
        var subDirFile = new FileInfo($"{Guid.NewGuid().ToString()}.json");
        subDirFile.Create().Close();
        subDirFile.MoveTo(Path.Combine(subDir.FullName, subDirFile.Name));
        var roCrate = new ROCrate();

        // Act
        roCrate.Convert(testDir.FullName);

        // Assert
        Assert.Contains(
            Path.GetRelativePath(testDir.FullName, topLevelFile.FullName),
            roCrate.Entities.Keys
        );
        Assert.Contains(
            Path.GetRelativePath(testDir.FullName, subDir.FullName) + "/",
            roCrate.Entities.Keys
        );
        Assert.Contains(
            Path.GetRelativePath(testDir.FullName, subDirFile.FullName),
            roCrate.Entities.Keys
        );

        // Clean up
        if (testDir.Exists) testDir.Delete(recursive: true);
    }
}