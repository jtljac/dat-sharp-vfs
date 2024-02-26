using System.IO.MemoryMappedFiles;
using dat_sharp_vfs.Util;

namespace dat_sharp_vfs.test;

public class TestVfs {
    private readonly DatSharpVfs populatedVfs;
    private readonly DatSharpVfs vfs;

    public TestVfs() {
        populatedVfs = new DatSharpVfs();
        populatedVfs.CreateDirectory("testingDir");
        populatedVfs.CreateDirectory("testingDir2/nestTest", true);
        populatedVfs.CreateDirectory("testingDir2/nestTest2", true);
        populatedVfs.CreateDirectory("testingDir2/nestTest3/Nest2Test", true);

        populatedVfs.MountFile("testFile", new MockDVfsFile());
        populatedVfs.MountFile("testFile2", new MockDVfsFile());
        populatedVfs.MountFile("testingDir2/nestTestFile", new MockDVfsFile());
        populatedVfs.MountFile("testingDir2/nestTestFile2", new MockDVfsFile());
        populatedVfs.MountFile("testingDir2/nestTest/nestTestFile2", new MockDVfsFile());

        vfs = new DatSharpVfs();
    }

    /* --------------------------------------- */
    /* Root                                    */
    /* --------------------------------------- */

    /// <summary>
    /// Test is Root at root is true
    /// </summary>
    [Fact]
    public void TestIsRootRoot() {
        Assert.True(vfs.IsRoot);
    }

    /// <summary>
    /// Test is Root not at root is false
    /// </summary>
    [Fact]
    public void TestIsRootNotRoot() {
        const string filePath = "testingDir";
        var folder = populatedVfs.GetDirectory(filePath);
        Assert.NotNull(folder);
        Assert.False(folder.IsRoot);
    }

    /* --------------------------------------- */
    /* Depth                                   */
    /* --------------------------------------- */

    /// <summary>
    /// Test depth at root is 0
    /// </summary>
    [Fact]
    public void TestIsDepthRoot() {
        Assert.Equal(0, vfs.Depth);
    }

    /// <summary>
    /// Test depth at one level down is 1
    /// </summary>
    [Fact]
    public void TestIsDepth1() {
        const string filePath = "testingDir";
        var folder = populatedVfs.GetDirectory(filePath);
        Assert.NotNull(folder);
        Assert.Equal(1, folder.Depth);
    }

    /// <summary>
    /// Test depth at two levels down is 2
    /// </summary>
    [Fact]
    public void TestIsDepth2() {
        const string filePath = "testingDir2/nestTest";
        var folder = populatedVfs.GetDirectory(filePath);
        Assert.NotNull(folder);
        Assert.Equal(2, folder.Depth);
    }

    /* --------------------------------------- */
    /* Folder Access                           */
    /* --------------------------------------- */

    /// <summary>
    /// Test getting a folder
    /// </summary>
    [Fact]
    public void TestGetFolder() {
        const string filePath = "testingDir";
        Assert.NotNull(populatedVfs.GetDirectory(filePath));
    }

    /// <summary>
    /// Test getting a folder that ends in a slash
    /// </summary>
    [Fact]
    public void TestGetFolderSlash() {
        const string filePath = "testingDir/";
        Assert.NotNull(populatedVfs.GetDirectory(filePath));
    }

    /// <summary>
    /// Test getting a folder that doesn't exist
    /// </summary>
    [Fact]
    public void TestGetFolderNotExist() {
        const string filePath = "testingDirThatDoesntExist";
        Assert.Null(populatedVfs.GetDirectory(filePath));
    }

    /// <summary>
    /// Test getting a folder in a sub folder
    /// </summary>
    [Fact]
    public void TestGetFolderSubDir() {
        const string filePath = "testingDir2/nestTest";
        Assert.NotNull(populatedVfs.GetDirectory(filePath));
    }

    /// <summary>
    /// Test folder getting itself
    /// </summary>
    [Fact]
    public void TestGetFolderSelf() {
        const string filePath = ".";
        Assert.Equal(vfs, vfs.GetDirectory(filePath));
    }

    /// <summary>
    /// Test root folder path traversal getting itself
    /// </summary>
    [Fact]
    public void TestGetFolderPathTraversalRoot() {
        const string filePath = "..";
        Assert.Equal(vfs, vfs.GetDirectory(filePath));
    }

    /// <summary>
    /// Test folder path traversal getting parent
    /// </summary>
    [Fact]
    public void TestGetFolderPathTraversal() {
        const string filePath = "..";

        var startPoint = populatedVfs.GetDirectory("testingDir2");
        Assert.NotNull(startPoint);

        Assert.Equal(populatedVfs, startPoint.GetDirectory(filePath));
    }

    /// <summary>
    /// Test getting using absolute path from root
    /// </summary>
    [Fact]
    public void TestGetFolderAbsoluteRoot() {
        const string filePathFromRoot = "testingDir";
        const string filePath = "/testingDir";
        Assert.Equal(populatedVfs.GetDirectory(filePathFromRoot), populatedVfs.GetDirectory(filePath));
    }

    /// <summary>
    /// Test getting using absolute path from a file one level deep
    /// </summary>
    [Fact]
    public void TestGetFolderAbsolute1Deep() {
        const string filePathFromRoot = "testingDir";
        const string filePath = "/testingDir";

        var startPoint = populatedVfs.GetDirectory("testingDir2");
        Assert.NotNull(startPoint);

        Assert.Equal(populatedVfs.GetDirectory(filePathFromRoot), startPoint.GetDirectory(filePath));
    }

    /// <summary>
    /// Test getting using absolute path from a file two levels deep
    /// </summary>
    [Fact]
    public void TestGetFolderAbsolute2Deep() {
        const string filePathFromRoot = "testingDir";
        const string filePath = "/testingDir";

        var startPoint = populatedVfs.GetDirectory("testingDir2/nestTest");
        Assert.NotNull(startPoint);

        Assert.Equal(populatedVfs.GetDirectory(filePathFromRoot), startPoint.GetDirectory(filePath));
    }

    /* --------------------------------------- */
    /* Folder Creation                         */
    /* --------------------------------------- */

    /// <summary>
    /// Test creating a folder that ends in a slash
    /// </summary>
    [Fact]
    public void TestCreateFolderSlash() {
        const string filePath = "testDir/";
        Assert.False(vfs.DirectoryExists(filePath));
        Assert.Null(Record.Exception(() => vfs.CreateDirectory(filePath)));
        Assert.True(vfs.DirectoryExists(filePath));
    }

    /// <summary>
    /// Test creating a folder
    /// </summary>
    [Fact]
    public void TestCreateFolder() {
        const string filePath = "testDir";
        Assert.False(vfs.DirectoryExists(filePath));
        Assert.Null(Record.Exception(() => vfs.CreateDirectory(filePath)));
        Assert.True(vfs.DirectoryExists(filePath));
    }

    /// <summary>
    /// Test creating a folder with an empty path
    /// </summary>
    [Fact]
    public void TestCreateFolderEmpty() {
        const string filePath = "";
        Assert.Throws<ArgumentException>(() => vfs.CreateDirectory(filePath));
    }

    /// <summary>
    /// Test creating a folder in a subdirectory
    /// </summary>
    [Fact]
    public void TestCreateFolderSubDir() {
        const string filePath = "testingDir/testDir";
        Assert.False(populatedVfs.DirectoryExists(filePath));
        Assert.Null(Record.Exception(() => populatedVfs.CreateDirectory(filePath)));
        Assert.True(populatedVfs.DirectoryExists(filePath));
    }

    /// <summary>
    /// Test creating a folder with recursion
    /// </summary>
    [Fact]
    public void TestCreateFolderRecursive() {
        const string filePath = "testDir/recursed";
        Assert.False(vfs.DirectoryExists(filePath));
        Assert.Null(Record.Exception(() => vfs.CreateDirectory(filePath, true)));
        Assert.True(vfs.DirectoryExists(filePath));
    }

    /// <summary>
    /// Test creating a folder with recursion in a subdir
    /// </summary>
    [Fact]
    public void TestCreateFolderRecursiveSubDir() {
        const string filePath = "testingDir/testDir/recursed";
        Assert.False(populatedVfs.DirectoryExists(filePath));
        Assert.Null(Record.Exception(() => populatedVfs.CreateDirectory(filePath, true)));
        Assert.True(populatedVfs.DirectoryExists(filePath));
    }

    /// <summary>
    /// Test creating a nested folder with recursion disabled
    /// </summary>
    [Fact]
    public void TestCreateFolderNoRecursive() {
        const string filePath = "testDir/recursed";
        Assert.False(vfs.DirectoryExists(filePath));
        Assert.Throws<DirectoryNotFoundException>(() => vfs.CreateDirectory(filePath, false));
        Assert.False(vfs.DirectoryExists(filePath));
    }

    /// <summary>
    /// Test creating a folder that already exists
    /// </summary>
    [Fact]
    public void TestCreateFolderAlreadyExists() {
        const string filePath = "testingDir";
        var existingFolder = populatedVfs.GetDirectory(filePath);
        Assert.NotNull(existingFolder);
        DatSharpVfs? newFolder = null;
        Assert.Null(Record.Exception(() => newFolder = populatedVfs.CreateDirectory(filePath)));
        Assert.NotNull(newFolder);
        Assert.Equal(existingFolder, newFolder);
    }

    /* --------------------------------------- */
    /* Folder Removing                         */
    /* --------------------------------------- */

    /// <summary>
    /// Test removing a folder
    /// </summary>
    [Fact]
    public void TestRemoveFolder() {
        const string filePath = "testingDir";
        Assert.True(populatedVfs.DirectoryExists(filePath));
        Assert.Null(Record.Exception(() => populatedVfs.RemoveDirectory(filePath)));
        Assert.False(populatedVfs.DirectoryExists(filePath));
    }

    /// <summary>
    /// Test removing a folder using an empty path
    /// </summary>
    [Fact]
    public void TestRemoveFolderEmptyPath() {
        const string filePath = "";
        Assert.Throws<ArgumentException>(() => vfs.RemoveDirectory(filePath));
    }

    /// <summary>
    /// Test removing a folder using an path that doesn't exist
    /// </summary>
    [Fact]
    public void TestRemoveFolderBadPath() {
        Assert.Throws<DirectoryNotFoundException>(() => vfs.RemoveDirectory("test"));
        Assert.Throws<DirectoryNotFoundException>(() => vfs.RemoveDirectory("test/testing"));
    }

    /// <summary>
    /// Test removing a folder by referring to self
    /// </summary>
    [Fact]
    public void TestRemoveFolderSelf() {
        const string filePath = ".";
        Assert.True(vfs.DirectoryExists(filePath));
        Assert.Throws<ArgumentException>(() => vfs.RemoveDirectory(filePath));
        Assert.True(vfs.DirectoryExists(filePath));
    }

    /// <summary>
    /// Test removing a folder by referring to its parent
    /// </summary>
    [Fact]
    public void TestRemoveFolderParent() {
        const string filePath = "testingDir/..";
        Assert.True(populatedVfs.DirectoryExists(filePath));
        Assert.Throws<ArgumentException>(() => populatedVfs.RemoveDirectory(filePath));
        Assert.True(populatedVfs.DirectoryExists(filePath));
    }

    /// <summary>
    /// Test removing a folder in a sub directory
    /// </summary>
    [Fact]
    public void TestRemoveFolderSubDir() {
        const string filePath = "testingDir2/nestTest2";
        Assert.True(populatedVfs.DirectoryExists(filePath));
        Assert.Null(Record.Exception(() => populatedVfs.RemoveDirectory(filePath)));
        Assert.False(populatedVfs.DirectoryExists(filePath));
    }

    /// <summary>
    /// Test removing a folder that is not empty
    /// </summary>
    [Fact]
    public void TestRemoveFolderNotEmpty() {
        const string filePath = "testingDir2";
        Assert.True(populatedVfs.DirectoryExists(filePath));
        Assert.Throws<DirectoryNotEmptyException>(() => populatedVfs.RemoveDirectory(filePath));
        Assert.True(populatedVfs.DirectoryExists(filePath));
    }

    /// <summary>
    /// Test removing a folder recursively
    /// </summary>
    [Fact]
    public void TestRemoveFolderRecursive() {
        const string filePath = "testingDir2";

        // Mount a file in a second place to ensure multi mounted files aren't broken
        var testFile = populatedVfs.GetFile("testFile");
        Assert.NotNull(testFile);
        populatedVfs.MountFile(filePath + "/testfileagain", testFile);
        Assert.Equal(2, testFile.References);

        Assert.True(populatedVfs.DirectoryExists(filePath));
        Assert.Null(Record.Exception(() => populatedVfs.RemoveDirectory(filePath, true)));
        Assert.False(populatedVfs.DirectoryExists(filePath));
        Assert.Equal(1, testFile.References);
    }

    // CleanDirectory

    /* --------------------------------------- */
    /* Folder Listing                          */
    /* --------------------------------------- */

    /// <summary>
    /// Test listing directories in an empty folder is empty
    /// </summary>
    [Fact]
    public void TestListFolderEmpty() {
        const string filePath = "";
        Assert.Empty(vfs.ListDirectories(filePath));
    }

    /// <summary>
    /// Test listing directories in an empty subfolder is empty
    /// </summary>
    [Fact]
    public void TestListFolderSubEmpty() {
        const string filePath = "testingDir";
        Assert.Empty(populatedVfs.ListDirectories(filePath));
    }

    /// <summary>
    /// Test listing directories has the right folders
    /// </summary>
    [Fact]
    public void TestListFolder() {
        const string filePath = "";
        string[] expected = ["testingDir", "testingDir2"];
        Assert.Equal(expected.ToHashSet(), populatedVfs.ListDirectories(filePath).ToHashSet());
    }

    /// <summary>
    /// Test listing directories with includeAll enabled has the right folders
    /// </summary>
    [Fact]
    public void TestListFolderAll() {
        const string filePath = "";
        string[] expected = [".", "..", "testingDir", "testingDir2"];
        Assert.Equal(expected.ToHashSet(), populatedVfs.ListDirectories(filePath, true).ToHashSet());
    }

    /// <summary>
    /// Test listing directories in an subfolder has the right folders
    /// </summary>
    [Fact]
    public void TestListFolderSubDir() {
        const string filePath = "testingDir2";
        string[] expected = ["nestTest", "nestTest2", "nestTest3"];
        Assert.Equal(expected.ToHashSet(), populatedVfs.ListDirectories(filePath).ToHashSet());
    }

    /* --------------------------------------- */
    /* File Access                             */
    /* --------------------------------------- */

    /// <summary>
    /// Test getting a file
    /// </summary>
    [Fact]
    public void TestGetFile() {
        const string filePath = "testFile";
        Assert.NotNull(populatedVfs.GetFile(filePath));
    }

    /// <summary>
    /// Test getting a file in a nested directory
    /// </summary>
    [Fact]
    public void TestGetFileNested() {
        const string filePath = "testingDir2/nestTestFile";
        Assert.NotNull(populatedVfs.GetFile(filePath));
    }

    /// <summary>
    /// Test getting a file that doesn't exist
    /// </summary>
    [Fact]
    public void TestGetFileNotExist() {
        const string filePath = "testingfileDoesntExist";
        Assert.Null(populatedVfs.GetFile(filePath));
    }

    /// <summary>
    /// Test getting a file that doesn't exist from a subdir
    /// </summary>
    [Fact]
    public void TestGetFileNotExistSubDir() {
        const string filePath = "testingDir/testingfileDoesntExist";
        Assert.Null(populatedVfs.GetFile(filePath));
    }

    /// <summary>
    /// Test getting a file that doesn't exist
    /// </summary>
    [Fact]
    public void TestGetFileEmpty() {
        const string filePath = "";
        Assert.Null(populatedVfs.GetFile(filePath));
    }

    /* --------------------------------------- */
    /* File References                         */
    /* --------------------------------------- */

    /// <summary>
    /// Test files that haven't been mounted have 0 references
    /// </summary>
    [Fact]
    public void TestFileReferencesUnmounted() {
        Assert.Equal(0, new MockDVfsFile().References);
    }

    /// <summary>
    /// Test files that have been mounted once has one reference
    /// </summary>
    [Fact]
    public void TestFileReferencesMounted1() {
        const string filePath = "testFile";
        var testFile = new MockDVfsFile();
        Assert.Null(Record.Exception(() => vfs.MountFile(filePath, testFile)));
        Assert.Equal(1, testFile.References);
    }

    /// <summary>
    /// Test files that have been mounted twice has 2 referneces
    /// </summary>
    [Fact]
    public void TestFileReferencesMounted2() {
        const string filePath = "testFile";
        const string filePath2 = "testFile2";
        var testFile = new MockDVfsFile();
        Assert.Null(Record.Exception(() => vfs.MountFile(filePath, testFile)));
        Assert.Null(Record.Exception(() => vfs.MountFile(filePath2, testFile)));
        Assert.Equal(2, testFile.References);
    }

    /// <summary>
    /// Test files are unmounted decrement references
    /// </summary>
    [Fact]
    public void TestFileReferencesUnmount() {
        const string filePath = "testFile";
        var testFile = populatedVfs.GetFile(filePath);
        Assert.NotNull(testFile);
        Assert.Equal(1, testFile.References);
        Assert.NotNull(populatedVfs.UnmountFile(filePath));
        Assert.Equal(0, testFile.References);
    }

    /* --------------------------------------- */
    /* File Mounting                           */
    /* --------------------------------------- */

    /// <summary>
    /// Test mounting a file
    /// </summary>
    [Fact]
    public void TestMountFile() {
        const string filePath = "test";
        Assert.False(vfs.FileExists(filePath));
        Assert.Null(Record.Exception(() => vfs.MountFile(filePath, new MockDVfsFile())));
        Assert.True(vfs.FileExists(filePath));
    }

    /// <summary>
    /// Test mounting a file with an empty path
    /// </summary>
    [Fact]
    public void TestMountFileEmptyPath() {
        const string filePath = "";
        Assert.Throws<ArgumentException>(() => vfs.MountFile(filePath, new MockDVfsFile()));
        Assert.False(vfs.FileExists(filePath));
    }

    /// <summary>
    /// Test mounting a file with a trailing slash
    /// </summary>
    [Fact]
    public void TestMountFileTrailingSlash() {
        const string filePath = "testingDir/";
        Assert.Throws<ArgumentException>(() => vfs.MountFile(filePath, new MockDVfsFile()));
        Assert.False(vfs.FileExists(filePath));
    }

    /// <summary>
    /// Test mounting a file in a subdirectory
    /// </summary>
    [Fact]
    public void TestMountFileSubDir() {
        const string filePath = "testingDir/test";
        Assert.False(populatedVfs.FileExists(filePath));
        Assert.Null(Record.Exception(() => populatedVfs.MountFile(filePath, new MockDVfsFile())));
        Assert.True(populatedVfs.FileExists(filePath));
    }

    /// <summary>
    /// Test mounting a file with creating folders enabled
    /// </summary>
    [Fact]
    public void TestMountFileCreateFolders() {
        const string filePath = "test/testAgain";
        Assert.False(vfs.FileExists(filePath));
        Assert.Null(Record.Exception(() => vfs.MountFile(filePath, new MockDVfsFile(), true)));
        Assert.True(vfs.FileExists(filePath));
    }

    /// <summary>
    /// Test mounting a file at a nested path with creating folders disabled
    /// </summary>
    [Fact]
    public void TestMountFileCreateFoldersBad() {
        const string filePath = "test/testAgain";
        Assert.False(vfs.FileExists(filePath));
        Assert.Throws<DirectoryNotFoundException>(() => vfs.MountFile(filePath, new MockDVfsFile()));
        Assert.False(vfs.FileExists(filePath));
    }

    /// <summary>
    /// Test mounting a file where a file already exists
    /// </summary>
    [Fact]
    public void TestMountFileExists() {
        const string filePath = "testFile";
        var originalFile = populatedVfs.GetFile(filePath);
        Assert.True(populatedVfs.FileExists(filePath));
        Assert.Throws<FileExistsException>(() => populatedVfs.MountFile(filePath, new MockDVfsFile()));
        Assert.Equal(originalFile, populatedVfs.GetFile(filePath));
    }

    /* --------------------------------------- */
    /* File Unmounting                         */
    /* --------------------------------------- */

    /// <summary>
    /// Test unmounting a file
    /// </summary>
    [Fact]
    public void TestUnmountFile() {
        const string filePath = "testFile";
        Assert.True(populatedVfs.FileExists(filePath));
        Assert.NotNull(populatedVfs.UnmountFile(filePath));
        Assert.False(populatedVfs.FileExists(filePath));
    }

    /// <summary>
    /// Test unmounting a file that doesn't exist
    /// </summary>
    [Fact]
    public void TestUnmountFileNotExist() {
        const string filePath = "testFilethatdoesntexist";
        Assert.False(populatedVfs.FileExists(filePath));
        Assert.Null(populatedVfs.UnmountFile(filePath));
        Assert.False(populatedVfs.FileExists(filePath));
    }

    /// <summary>
    /// Test unmounting a file with an empty path
    /// </summary>
    [Fact]
    public void TestUnmountFileEmptyPath() {
        const string filePath = "";
        Assert.Null(populatedVfs.UnmountFile(filePath));
    }

    /// <summary>
    /// Test unmounting a file in a sub directory
    /// </summary>
    [Fact]
    public void TestUnmountFileSubDir() {
        const string filePath = "testingDir2/nestTestFile";
        Assert.True(populatedVfs.FileExists(filePath));
        Assert.NotNull(populatedVfs.UnmountFile(filePath));
        Assert.False(populatedVfs.FileExists(filePath));
    }

    /* --------------------------------------- */
    /* MountFiles                              */
    /* --------------------------------------- */

    /// <summary>
    /// Test mounting files with an inserter
    /// </summary>
    [Fact]
    public void TestInserter() {
        const string filePath = "";
        List<string> files = ["test", "test2", "test3"];
        var inserter = new MockDVfsFileInserter(files);
        Assert.Equal(files.Count, vfs.MountFiles(filePath, inserter));
        Assert.Equal(files, vfs.ListFiles(filePath));
        Assert.Empty(inserter.rejectedFiles);
    }

    /// <summary>
    /// Test mounting files with an inserter with a bad path
    /// </summary>
    [Fact]
    public void TestInserterBadPath() {
        const string filePath = "Test";
        List<string> files = ["test", "test2", "test3"];
        Assert.Throws<DirectoryNotFoundException>(() => vfs.MountFiles(filePath, new MockDVfsFileInserter(files), false));
    }

    /// <summary>
    /// Test mounting files with an inserter in a sub directory
    /// </summary>
    [Fact]
    public void TestInserterSubDir() {
        const string filePath = "testingDir";
        List<string> files = ["test", "test2", "test3"];
        var inserter = new MockDVfsFileInserter(files);
        Assert.Equal(files.Count, populatedVfs.MountFiles(filePath, inserter));
        Assert.Equal(files, populatedVfs.ListFiles(filePath));
        Assert.Empty(inserter.rejectedFiles);
    }

    /// <summary>
    /// Test mounting files with an inserter creating subdirectories for the base path
    /// </summary>
    [Fact]
    public void TestInserterCreateDir() {
        const string filePath = "testingDirdoesntexist";
        List<string> files = ["test", "test2", "test3"];
        var inserter = new MockDVfsFileInserter(files);
        Assert.Equal(files.Count, vfs.MountFiles(filePath, inserter));
        Assert.Equal(files, vfs.ListFiles(filePath));
        Assert.Empty(inserter.rejectedFiles);
    }

    /// <summary>
    /// Test mounting files with an inserter where files define subdirectories that don't exist
    /// </summary>
    [Fact]
    public void TestInserterFilesBadDir() {
        const string filePath = "";
        // Path doesn't exist
        // Empty Path
        // Should work
        // Duplicate file
        List<string> files = ["test/test", "", "test", "test"];
        var inserter = new MockDVfsFileInserter(files);
        Assert.Equal(1, vfs.MountFiles(filePath, inserter, false));
        Assert.Equal(3, inserter.rejectedFiles.Count);

        for (var index = 0; index < 2; index++) {
            var file = files[index];
            Assert.False(vfs.FileExists(file));
        }
    }

    /// <summary>
    /// Test mounting files with an inserter creating subdirectories for the file paths
    /// </summary>
    [Fact]
    public void TestInserterFilesCreateFolders() {
        const string filePath = "";
        List<string> files = ["test/test", "test2", "test2/test3"];
        var inserter = new MockDVfsFileInserter(files);
        Assert.Equal(3, vfs.MountFiles(filePath, inserter));
        foreach (var file in files) {
            vfs.FileExists(file);
        }
    }

    /* --------------------------------------- */
    /* List Files                              */
    /* --------------------------------------- */

    /// <summary>
    /// Test listing directories in an empty folder is empty
    /// </summary>
    [Fact]
    public void TestListFilesEmpty() {
        const string filePath = "";
        Assert.Empty(vfs.ListFiles(filePath));
    }

    /// <summary>
    /// Test listing files in an empty subfolder is empty
    /// </summary>
    [Fact]
    public void TestListFilesSubEmpty() {
        const string filePath = "testingDir";
        Assert.Empty(populatedVfs.ListFiles(filePath));
    }

    /// <summary>
    /// Test listing files has the right files
    /// </summary>
    [Fact]
    public void TestListFiles() {
        const string filePath = "";
        string[] expected = ["testFile", "testFile2"];
        Assert.Equal(expected.ToHashSet(), populatedVfs.ListFiles(filePath).ToHashSet());
    }

    /// <summary>
    /// Test listing files in an subfolder has the right files
    /// </summary>
    [Fact]
    public void TestListFilesSubDir() {
        const string filePath = "testingDir2";
        string[] expected = ["nestTestFile", "nestTestFile2"];
        Assert.Equal(expected.ToHashSet(), populatedVfs.ListFiles(filePath).ToHashSet());
    }

    /* --------------------------------------- */
    /* Empty                                   */
    /* --------------------------------------- */

    /// <summary>
    /// Test An empty folder is empty
    /// </summary>
    [Fact]
    public void TestEmptyEmpty() {
        Assert.True(vfs.IsEmpty);
    }

    /// <summary>
    /// Test a folder that has only folders is not empty
    /// </summary>
    [Fact]
    public void TestNotEmptyFolders() {
        const string filePath = "testingDir2/nestTest3";
        var startingDirectory = populatedVfs.GetDirectory(filePath);
        Assert.NotNull(startingDirectory);
        Assert.False(startingDirectory.IsEmpty);
    }

    /// <summary>
    /// Test a folder that has only files is not empty
    /// </summary>
    [Fact]
    public void TestNotEmptyFiles() {
        const string filePath = "testingDir2/nestTest";
        var startingDirectory = populatedVfs.GetDirectory(filePath);
        Assert.NotNull(startingDirectory);
        Assert.False(startingDirectory.IsEmpty);
    }

    /// <summary>
    /// Test a folder that has files and folders is not empty
    /// </summary>
    [Fact]
    public void TestNotEmptyFull() {
        Assert.False(populatedVfs.IsEmpty);
    }
}

internal class MockDVfsFile(long size = 0, bool valid = true) : DVfsFile {
    public override long Size => size;
    public override bool IsValid => valid;
    public override int GetFileContent(in byte[] buffer, int offset) {
        throw new NotImplementedException();
    }

    public override Stream GetFileStream() {
        throw new NotImplementedException();
    }

    public override MemoryMappedFile GetMemoryMappedFile() {
        throw new NotImplementedException();
    }
}

internal class MockDVfsFileInserter(List<string> files) : IVfsFileInserter {
    public readonly List<string> rejectedFiles = [];
    public IEnumerable<Tuple<string, DVfsFile>> GetFiles() {
        return files.Select(file => new Tuple<string, DVfsFile>(file, new MockDVfsFile()));
    }

    public void HandleInsertFailure(string path, DVfsFile file, InsertionFailureReason failureReason) {
        rejectedFiles.Add(path);
    }
}