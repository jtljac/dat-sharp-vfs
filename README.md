# Dat Sharp Vfs
A C# library for creating a virtual file system that allows mounting files of multiple sources into a directory tree and
accessing through a consistent interface.

# Usage
## Directories and entry point
The core of the library is be found in `DatSharpVfs`, which represents a directory. This can contain multiple 
directories and files.

## Files
Files are represented by `DVfsFile`, which is an abstract class that can be extended to represent any file or data you
wish to mount to the Vfs.

A simple example implementation for a loose file in the OS file system is `LooseDVfsFile`.

A simple example implementation for a DVfsFile representing data in memory is `MemoryDVfsFile`.

An example implementation for entries in a zip file is `ZipDVfsFile`.

## Inserters
For mass inserting files into a DVfs, you can use an implementation of DVfsFileInserter, which provides methods for
getting a list of files to insert and handling when a file fails to insert.

A good example of this is `LooseFileDVfsFileInserter`, which inserts every loose file in a directory from the OS
filesystem into the Vfs.

An example of inserting files from an archive file is `ZipFileDVfsFileInserter`, which inserts every entry in a zip file
into the Vfs.