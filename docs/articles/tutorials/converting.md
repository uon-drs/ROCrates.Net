# Converting a directory to an RO-Crate

## Convert a Directory on disk to an RO-Crate

- Before:
```
| MyDirectory
|- file1.txt
|- subdir
|-- file1.txt
|-- file2.txt
```

```csharp
using ROCrates;

// Create a blank `ROCrate` object
var rocrate = new ROCrate();

// Convert the RO-Crate and add the entities to the `ROCrate` object
rocrate.Convert("MyDirectory");
```

- After:
```
| MyDirectory
|- file1.txt
|- ro-crate-metadata.json
|- ro-crate-preview.html
|- subdir
|-- file1.txt
|-- file2.txt
```