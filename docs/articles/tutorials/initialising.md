# Initialising an `ROCrate`

## Initialise a `ROCrate` from a directory.
```csharp
using ROCrates;

// Create a blank `ROCrate` object
var roCrate = new ROCrate();

// Initialise the `ROCrate` object
roCrate.Initialise("MyROCrate")
```

## Initialise a `ROCrate` from a zipped RO-Crate.
```csharp
using System.IO.Compression;
using ROCrates;

// Unzip the directory
Zipfile.ExtractToDirectory("MyROCrate.zip", "MyROCrate");

// Create a blank `ROCrate` object
var roCrate = new ROCrate();

// Initialise the `ROCrate` object
roCrate.Initialise("MyROCrate")
```