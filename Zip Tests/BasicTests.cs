// BasicTests.cs
// ------------------------------------------------------------------
//
// Copyright (c) 2009 Dino Chiesa and Microsoft Corporation.  
// All rights reserved.
//
// This code module is part of DotNetZip, a zipfile class library.
//
// ------------------------------------------------------------------
//
// This code is licensed under the Microsoft Public License. 
// See the file License.txt for the license details.
// More info on: http://dotnetzip.codeplex.com
//
// ------------------------------------------------------------------
//
// last saved (in emacs): 
// Time-stamp: <2009-July-01 09:47:53>
//
// ------------------------------------------------------------------
//
// This module defines basic unit tests for DotNetZip.
//
// ------------------------------------------------------------------

using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RE = System.Text.RegularExpressions;

using Ionic.Zip;
using Ionic.Zip.Tests.Utilities;
using System.IO;

namespace Ionic.Zip.Tests.Basic
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class BasicTests : IExec
    {
        private System.Random _rnd;

        public BasicTests()
        {
            _rnd = new System.Random();
        }

        #region Context
        private TestContext testContextInstance;

        /// <summary>
        /// Gets or sets the test context which provides
        /// information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }
        #endregion

        #region Test Init and Cleanup
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //


        private string CurrentDir = null;
        private string TopLevelDir = null;

        // Use TestInitialize to run code before running each test 
        [TestInitialize()]
        public void MyTestInitialize()
        {
            TestUtilities.Initialize(ref CurrentDir, ref TopLevelDir);
            _FilesToRemove.Add(TopLevelDir);
        }


        System.Collections.Generic.List<string> _FilesToRemove = new System.Collections.Generic.List<string>();

        // Use TestCleanup to run code after each test has run
        [TestCleanup()]
        public void MyTestCleanup()
        {
            TestUtilities.Cleanup(CurrentDir, _FilesToRemove);
            if (_txrx!=null)
            {
                _txrx.Send("stop");
                _txrx = null;
            }
        }

        #endregion

        [TestMethod]
        public void CreateZip_AddItem_WithDirectory()
        {
            int i;

            // select the name of the zip file
            string ZipFileToCreate = Path.Combine(TopLevelDir, "CreateZip_AddItem.zip");
            Assert.IsFalse(File.Exists(ZipFileToCreate), "The temporary zip file '{0}' already exists.", ZipFileToCreate);

            // create the subdirectory
            string Subdir = Path.Combine(TopLevelDir, "files");

            // create a bunch of files
            string[] FilesToZip = TestUtilities.GenerateFilesFlat(Subdir);

            // Create the zip archive
            Directory.SetCurrentDirectory(TopLevelDir);
            using (ZipFile zip1 = new ZipFile())
            {
                //zip.StatusMessageTextWriter = System.Console.Out;
                for (i = 0; i < FilesToZip.Length; i++)
                    zip1.AddItem(FilesToZip[i], "files");
                zip1.Save(ZipFileToCreate);
            }

            // Verify the number of files in the zip
            Assert.AreEqual<int>(TestUtilities.CountEntries(ZipFileToCreate), FilesToZip.Length,
                    "Incorrect number of entries in the zip file.");
        }

        [TestMethod]
        public void CreateZip_AddItem_NoDirectory()
        {
            // select the name of the zip file
            string ZipFileToCreate = Path.Combine(TopLevelDir, "CreateZip_AddItem.zip");
            Assert.IsFalse(File.Exists(ZipFileToCreate), "The temporary zip file '{0}' already exists.", ZipFileToCreate);

            Directory.SetCurrentDirectory(TopLevelDir);

            // create a bunch of files
            string[] FilesToZip = TestUtilities.GenerateFilesFlat(".");

            // Create the zip archive
            using (ZipFile zip1 = new ZipFile())
            {
                foreach (var f in FilesToZip)
                    zip1.AddItem(f);
                zip1.Save(ZipFileToCreate);
            }

            // Verify the number of files in the zip
            Assert.AreEqual<int>(TestUtilities.CountEntries(ZipFileToCreate), FilesToZip.Length,
                    "Incorrect number of entries in the zip file.");
        }
        



        [TestMethod]
        public void CreateZip_AddFile()
        {
            int i;
            // select the name of the zip file
            string ZipFileToCreate = Path.Combine(TopLevelDir, "CreateZip_AddFile.zip");
            Assert.IsFalse(File.Exists(ZipFileToCreate), "The temporary zip file '{0}' already exists.", ZipFileToCreate);

            // create the subdirectory
            string Subdir = Path.Combine(TopLevelDir, "files");

            // create a bunch of files
            string[] FilesToZip = TestUtilities.GenerateFilesFlat(Subdir);

            // Create the zip archive
            Directory.SetCurrentDirectory(TopLevelDir);
            using (ZipFile zip1 = new ZipFile())
            {
                //zip.StatusMessageTextWriter = System.Console.Out;
                for (i = 0; i < FilesToZip.Length; i++)
                    zip1.AddFile(FilesToZip[i], "files");
                zip1.Save(ZipFileToCreate);
            }

            // Verify the number of files in the zip
            Assert.AreEqual<int>(TestUtilities.CountEntries(ZipFileToCreate), FilesToZip.Length,
                    "Incorrect number of entries in the zip file.");
        }


        [TestMethod]
        public void CreateZip_AddFileInDirectory()
        {
            int i, j;
            // create the subdirectory
            string Subdir = Path.Combine(TopLevelDir, "files");

            // create a bunch of files
            string[] FilesToZip = TestUtilities.GenerateFilesFlat(Subdir);
            for (int m = 0; m < 2; m++)
            {
                string directoryName = "";
                for (int k = 0; k < 4; k++)
                {
                    // select the name of the zip file
                    string ZipFileToCreate = Path.Combine(TopLevelDir, String.Format("CreateZip_AddFileInDirectory-trial{0}.{1}.zip", m,k));
                    Assert.IsFalse(File.Exists(ZipFileToCreate), "The temporary zip file '{0}' already exists.", ZipFileToCreate);
                    TestContext.WriteLine("=====================");
                    TestContext.WriteLine("Trial {0}", k);
                    TestContext.WriteLine("Zipfile: {0}", ZipFileToCreate);

                    directoryName = Path.Combine(directoryName,
                                                           String.Format("{0:D2}", k));
                    
                    TestContext.WriteLine("using dirname: {0}", directoryName);

                    int n = _rnd.Next(FilesToZip.Length / 2) + 2;
                    TestContext.WriteLine("Zipping {0} files", n);

                    // Create the zip archive
                    var zippedFiles = new List<String>();
                    using (ZipFile zip1 = new ZipFile())
                    {
                        for (i = 0; i < n; i++)
                        {
                            do
                            {
                                j = _rnd.Next(FilesToZip.Length);
                            } while (zippedFiles.Contains(FilesToZip[j]));
                            zip1.AddFile(FilesToZip[j], directoryName);
                            zippedFiles.Add(FilesToZip[j]);
                        }
                        zip1.Save(ZipFileToCreate);
                    }

                    // Verify the number of files in the zip
                    Assert.AreEqual<int>(TestUtilities.CountEntries(ZipFileToCreate), n,
                            "Incorrect number of entries in the zip file.");

                    using (ZipFile zip2 = ZipFile.Read(ZipFileToCreate))
                    {
                        foreach (var e in zip2)
                        {
                            TestContext.WriteLine("Check entry: {0}", e.FileName);

                            Assert.AreEqual<String>(directoryName, Path.GetDirectoryName(e.FileName),
                                "unexpected directory on zip entry");
                        }
                    }
                }

                Directory.SetCurrentDirectory(Subdir);
                FilesToZip = TestUtilities.GenerateFilesFlat(".");
            }

            Directory.SetCurrentDirectory(TopLevelDir);
            
        }


        [TestMethod]
        public void CreateZip_AddFile_LeadingDot()
        {
            // select the name of the zip file
            string ZipFileToCreate = Path.Combine(TopLevelDir, "CreateZip_AddFile_LeadingDot.zip");
            Assert.IsFalse(File.Exists(ZipFileToCreate), "The temporary zip file '{0}' already exists.", ZipFileToCreate);

            Directory.SetCurrentDirectory(TopLevelDir);

            // create a bunch of files
            string[] FilesToZip = TestUtilities.GenerateFilesFlat(".");

            // Create the zip archive
            using (ZipFile zip1 = new ZipFile())
            {
                for (int i = 0; i < FilesToZip.Length; i++)
                {
                    zip1.AddFile(FilesToZip[i]);
                }
                zip1.Save(ZipFileToCreate);
            }

            // Verify the number of files in the zip
            Assert.AreEqual<int>(TestUtilities.CountEntries(ZipFileToCreate), FilesToZip.Length,
                    "Incorrect number of entries in the zip file.");
        }




        [TestMethod]
        public void CreateZip_AddFiles_LeadingDot_Array()
        {
            // select the name of the zip file
            string ZipFileToCreate = Path.Combine(TopLevelDir, "CreateZip_AddFiles_LeadingDot_Array.zip");
            Assert.IsFalse(File.Exists(ZipFileToCreate), "The temporary zip file '{0}' already exists.", ZipFileToCreate);

            Directory.SetCurrentDirectory(TopLevelDir);

            // create a bunch of files
            string[] FilesToZip = TestUtilities.GenerateFilesFlat(".");

            // Create the zip archive
            using (ZipFile zip1 = new ZipFile())
            {
                zip1.AddFiles(FilesToZip);
                zip1.Save(ZipFileToCreate);
            }

            // Verify the number of files in the zip
            Assert.AreEqual<int>(TestUtilities.CountEntries(ZipFileToCreate), FilesToZip.Length,
                    "Incorrect number of entries in the zip file.");
        }



        [TestMethod]
        public void CreateZip_AddFiles_PreserveDirHierarchy()
        {
            // select the name of the zip file
            string ZipFileToCreate = Path.Combine(TopLevelDir, "CreateZip_AddFiles_PreserveDirHierarchy.zip");
            Assert.IsFalse(File.Exists(ZipFileToCreate), "The temporary zip file '{0}' already exists.", ZipFileToCreate);

            Directory.SetCurrentDirectory(TopLevelDir);
            string dirToZip = Path.Combine(TopLevelDir, "zipthis");

            // create a bunch of files
            int subdirCount; 
            int entries = TestUtilities.GenerateFilesOneLevelDeep(TestContext, "PreserveDirHierarchy", dirToZip, null, out subdirCount);

            string[] FilesToZip = Directory.GetFiles(".", "*.*", SearchOption.AllDirectories);

            Assert.AreEqual<int>(FilesToZip.Length, entries,
                    "Incorrect number of entries in the directory.");
            
            // Create the zip archive
            using (ZipFile zip1 = new ZipFile())
            {
                zip1.AddFiles(FilesToZip, true, "");
                zip1.Save(ZipFileToCreate);
            }

            // Verify the number of files in the zip
            Assert.AreEqual<int>(TestUtilities.CountEntries(ZipFileToCreate), FilesToZip.Length,
                    "Incorrect number of entries in the zip file.");
        }

        private bool ArraysAreEqual(byte[] b1, byte[] b2)
        {
            return (CompareArrays(b1, b2)==0);
        }

        
        private int CompareArrays(byte[] b1, byte[] b2)
        {
            if (b1==null && b2 == null) return 0;
            if (b1==null || b2 == null) return 0;
            if (b1.Length > b2.Length) return 1;
            if (b1.Length < b2.Length) return -1;
            for (int i=0; i < b1.Length; i++)
            {
                if (b1[i] > b2[i]) return 1;
                if (b1[i] < b2[i]) return -1;
            }
            return 0;
        }

        
        [TestMethod]
        public void CreateZip_AddEntry_ByteArray()
        {
            // select the name of the zip file
            string ZipFileToCreate = Path.Combine(TopLevelDir, "CreateZip_AddEntry_ByteArray.zip");
            Assert.IsFalse(File.Exists(ZipFileToCreate), "The temporary zip file '{0}' already exists.", ZipFileToCreate);
            Directory.SetCurrentDirectory(TopLevelDir);

            int entriesToCreate = _rnd.Next(12)+12;
            var dict = new Dictionary<string,byte[]>();
            
            // Create the zip archive
            using (ZipFile zip1 = new ZipFile())
            {
                for (int i=0; i < entriesToCreate;  i++) 
                {
                    var b = new byte[_rnd.Next(1000)+1000];
                    _rnd.NextBytes(b);
                    string filename = String.Format("Filename{0:D3}.bin", i);
                    var e = zip1.AddEntry(filename, "data", b);
                    dict.Add(e.FileName, b);
                }
                zip1.Save(ZipFileToCreate);
            }

            // Verify the number of files in the zip
            Assert.AreEqual<int>(TestUtilities.CountEntries(ZipFileToCreate), entriesToCreate,
                    "Incorrect number of entries in the zip file.");

            using (ZipFile zip1 = ZipFile.Read(ZipFileToCreate))
            {
                foreach (var e in zip1)
                {
                    // extract to a stream
                    var ms1 = new MemoryStream();
                    e.Extract(ms1);
                    Assert.IsTrue(ArraysAreEqual(ms1.ToArray(), dict[e.FileName]));
                }
            }
        }




        [TestMethod]
        public void CreateZip_AddFile_AddItem()
        {
            // select the name of the zip file
            string ZipFileToCreate = Path.Combine(TopLevelDir, "CreateZip_AddFile_AddItem.zip");
            Assert.IsFalse(File.Exists(ZipFileToCreate), "The temporary zip file '{0}' already exists.", ZipFileToCreate);

            // create the subdirectory
            string Subdir = Path.Combine(TopLevelDir, "files");

            // create a bunch of files
            string[] FilesToZip = TestUtilities.GenerateFilesFlat(Subdir);

            // Create the zip archive
            Directory.SetCurrentDirectory(TopLevelDir);
            using (ZipFile zip1 = new ZipFile())
            {
                for (int i = 0; i < FilesToZip.Length; i++)
                {
                    if (_rnd.Next(2) == 0)
                        zip1.AddFile(FilesToZip[i], "files");
                    else
                        zip1.AddItem(FilesToZip[i], "files");
                }
                zip1.Save(ZipFileToCreate);
            }

            // Verify the number of files in the zip
            Assert.AreEqual<int>(TestUtilities.CountEntries(ZipFileToCreate), FilesToZip.Length,
                    "Incorrect number of entries in the zip file.");
        }


        private void DumpZipFile(ZipFile z)
        {
            TestContext.WriteLine("found {0} entries", z.Entries.Count);
            TestContext.WriteLine("RequiresZip64: '{0}'", z.RequiresZip64.HasValue ? z.RequiresZip64.Value.ToString() : "not set");
            TestContext.WriteLine("listing the entries in {0}...", String.IsNullOrEmpty(z.Name) ? "(zipfile)" : z.Name);
            foreach (var e in z)
            {
                TestContext.WriteLine("{0}", e.FileName);
            }
        }



        [TestMethod]
        public void CreateZip_ZeroEntries()
        {
            // select the name of the zip file
            string ZipFileToCreate = Path.Combine(TopLevelDir, "CreateZip_ZeroEntries.zip");
            Assert.IsFalse(File.Exists(ZipFileToCreate), "The temporary zip file '{0}' already exists.", ZipFileToCreate);

            // Create the zip archive
            Directory.SetCurrentDirectory(TopLevelDir);
            using (ZipFile zip1 = new ZipFile())
            {
                zip1.Save(ZipFileToCreate);
                DumpZipFile(zip1);
            }

            // workitem 7685
            using (ZipFile zip1 = new ZipFile(ZipFileToCreate))
            {
                DumpZipFile(zip1);
            }

            using (ZipFile zip1 = ZipFile.Read(ZipFileToCreate))
            {
                DumpZipFile(zip1);
            }

            // Verify the number of files in the zip
            Assert.AreEqual<int>(TestUtilities.CountEntries(ZipFileToCreate), 0,
                    "Incorrect number of entries in the zip file.");
        }



        [TestMethod]
        public void CreateZip_Basic_ParameterizedSave()
        {
            int i;
            // select the name of the zip file
            string ZipFileToCreate = Path.Combine(TopLevelDir, "CreateZip_Basic_ParameterizedSave.zip");
            Assert.IsFalse(File.Exists(ZipFileToCreate), "The temporary zip file '{0}' already exists.", ZipFileToCreate);

            // create the subdirectory
            string Subdir = Path.Combine(TopLevelDir, "files");
            Directory.CreateDirectory(Subdir);

            // create a bunch of files
            int NumFilesToCreate = _rnd.Next(23) + 14;
            string[] FilesToZip = new string[NumFilesToCreate];
            for (i = 0; i < NumFilesToCreate; i++)
                FilesToZip[i] =
                    TestUtilities.CreateUniqueFile("bin", Subdir, _rnd.Next(10000) + 5000);

            // Create the zip archive
            Directory.SetCurrentDirectory(TopLevelDir);
            using (ZipFile zip1 = new ZipFile())
            {
                //zip.StatusMessageTextWriter = System.Console.Out;
                for (i = 0; i < FilesToZip.Length; i++)
                {
                    if (_rnd.Next(2) == 0)
                        zip1.AddFile(FilesToZip[i], "files");
                    else
                        zip1.AddItem(FilesToZip[i], "files");
                }
                zip1.Save(ZipFileToCreate);
            }

            // Verify the number of files in the zip
            Assert.AreEqual<int>(TestUtilities.CountEntries(ZipFileToCreate), FilesToZip.Length,
                    "Incorrect number of entries in the zip file.");
        }


        [TestMethod]
        public void CreateZip_AddFile_OnlyZeroLengthFiles()
        {
            _Internal_ZeroLengthFiles(_rnd.Next(33) + 3, "CreateZip_AddFile_OnlyZeroLengthFiles");
        }

        [TestMethod]
        public void CreateZip_AddFile_OneZeroLengthFile()
        {
            _Internal_ZeroLengthFiles(1, "CreateZip_AddFile_OneZeroLengthFile");
        }


        private void _Internal_ZeroLengthFiles(int fileCount, string nameStub)
        {
            string ZipFileToCreate = Path.Combine(TopLevelDir, nameStub + ".zip");
            Assert.IsFalse(File.Exists(ZipFileToCreate), "The temporary zip file '{0}' already exists.", ZipFileToCreate);

            int i;
            string[] FilesToZip = new string[fileCount];
            for (i = 0; i < fileCount; i++)
                FilesToZip[i] = TestUtilities.CreateUniqueFile("zerolength", TopLevelDir);

            using (ZipFile zip = new ZipFile())
            {
                //zip.StatusMessageTextWriter = System.Console.Out;
                for (i = 0; i < FilesToZip.Length; i++)
                {
                    string pathToUse = Path.Combine(Path.GetFileName(TopLevelDir),
                        Path.GetFileName(FilesToZip[i]));
                    zip.AddFile(pathToUse);
                }
                zip.Save(ZipFileToCreate);
            }

            Assert.AreEqual<int>(TestUtilities.CountEntries(ZipFileToCreate), FilesToZip.Length,
                    "The zip file created has the wrong number of entries.");
        }

        [TestMethod]
        public void CreateZip_UpdateDirectory()
        {
            int i, j;

            string ZipFileToCreate = Path.Combine(TopLevelDir, "CreateZip_UpdateDirectory.zip");
            Assert.IsFalse(File.Exists(ZipFileToCreate), "The temporary zip file '{0}' already exists.", ZipFileToCreate);

            string DirToZip = Path.Combine(TopLevelDir, "zipthis");
            Directory.CreateDirectory(DirToZip);

            TestContext.WriteLine("\n------------\nCreating files ...");

            int entries = 0;
            int subdirCount = _rnd.Next(17) + 14;
            //int subdirCount = _rnd.Next(3) + 2;
            var FileCount = new Dictionary<string, int>();
            var checksums = new Dictionary<string, byte[]>();

            for (i = 0; i < subdirCount; i++)
            {
                string SubdirShort = String.Format("dir{0:D4}", i);
                string Subdir = Path.Combine(DirToZip, SubdirShort);
                Directory.CreateDirectory(Subdir);

                int filecount = _rnd.Next(11) + 17;
                //int filecount = _rnd.Next(2) + 2;
                FileCount[SubdirShort] = filecount;
                for (j = 0; j < filecount; j++)
                {
                    string filename = String.Format("file{0:D4}.x", j);
                    string fqFilename = Path.Combine(Subdir, filename);
                    TestUtilities.CreateAndFillFile(fqFilename, _rnd.Next(1000) + 100);

                    var chk = TestUtilities.ComputeChecksum(fqFilename);
                    var t1 = Path.GetFileName(DirToZip);
                    var t2 = Path.Combine(t1, SubdirShort);
                    var key = Path.Combine(t2, filename);
                    key = TestUtilities.TrimVolumeAndSwapSlashes(key);
                    TestContext.WriteLine("chk[{0}]= {1}", key, TestUtilities.CheckSumToString(chk));
                    checksums.Add(key, chk);
                    entries++;
                }
            }

            Directory.SetCurrentDirectory(TopLevelDir);

            TestContext.WriteLine("\n------------\nAdding files into the Zip...");

            // add all the subdirectories into a new zip
            using (ZipFile zip1 = new ZipFile())
            {
                zip1.AddDirectory(DirToZip, "zipthis");
                zip1.Save(ZipFileToCreate);
            }

            TestContext.WriteLine("\n");

            Assert.AreEqual<int>(TestUtilities.CountEntries(ZipFileToCreate), entries,
              "The Zip file has an unexpected number of entries.");

            TestContext.WriteLine("\n------------\nExtracting and validating checksums...");

            // validate all the checksums
            using (ZipFile zip2 = ZipFile.Read(ZipFileToCreate))
            {
                foreach (ZipEntry e in zip2)
                {
                    e.Extract("unpack");
                    string PathToExtractedFile = Path.Combine("unpack", e.FileName);

                    // if it is a file.... 
                    if (checksums.ContainsKey(e.FileName))
                    {
                        // verify the checksum of the file is correct
                        string expectedCheckString = TestUtilities.CheckSumToString(checksums[e.FileName]);
                        string actualCheckString = TestUtilities.CheckSumToString(TestUtilities.ComputeChecksum(PathToExtractedFile));
                        Assert.AreEqual<String>(expectedCheckString, actualCheckString, "Unexpected checksum on extracted filesystem file ({0}).", PathToExtractedFile);
                    }
                }
            }


            TestContext.WriteLine("\n------------\nCreating some new files ...");

            // now, update some of the existing files
            DirToZip = Path.Combine(TopLevelDir, "updates");
            Directory.CreateDirectory(DirToZip);

            for (i = 0; i < subdirCount; i++)
            {
                string SubdirShort = String.Format("dir{0:D4}", i);
                string Subdir = Path.Combine(DirToZip, SubdirShort);
                Directory.CreateDirectory(Subdir);

                int filecount = FileCount[SubdirShort];
                for (j = 0; j < filecount; j++)
                {
                    if (_rnd.Next(2) == 1)
                    {
                        string filename = String.Format("file{0:D4}.x", j);
                        TestUtilities.CreateAndFillFile(Path.Combine(Subdir, filename),
                            _rnd.Next(1000) + 100);
                        string fqFilename = Path.Combine(Subdir, filename);

                        var chk = TestUtilities.ComputeChecksum(fqFilename);
                        //var t1 = Path.GetFileName(DirToZip);
                        var t2 = Path.Combine("zipthis", SubdirShort);
                        var key = Path.Combine(t2, filename);
                        key = TestUtilities.TrimVolumeAndSwapSlashes(key);

                        TestContext.WriteLine("chk[{0}]= {1}", key, TestUtilities.CheckSumToString(chk));

                        checksums.Remove(key);
                        checksums.Add(key, chk);
                    }
                }
            }


            TestContext.WriteLine("\n------------\nUpdating some of the files in the zip...");
            // add some new content
            using (ZipFile zip3 = ZipFile.Read(ZipFileToCreate))
            {
                zip3.UpdateDirectory(DirToZip, "zipthis");
                //String[] dirs = Directory.GetDirectories(DirToZip);

                //foreach (String d in dirs)
                //{
                //    string dir = Path.Combine(Path.GetFileName(DirToZip), Path.GetFileName(d));
                //    //string root = Path.Combine("zipthis", Path.GetFileName(d));
                //    zip3.UpdateDirectory(dir, "zipthis");
                //}
                zip3.Save();
            }

            TestContext.WriteLine("\n------------\nValidating the checksums for all of the files ...");

            // validate all the checksums again
            using (ZipFile zip4 = ZipFile.Read(ZipFileToCreate))
            {
                foreach (ZipEntry e in zip4)
                    TestContext.WriteLine("Found entry: {0}", e.FileName);

                foreach (ZipEntry e in zip4)
                {
                    e.Extract("unpack2");
                    if (!e.IsDirectory)
                    {
                        string PathToExtractedFile = Path.Combine("unpack2", e.FileName);

                        // verify the checksum of the file is correct
                        string expectedCheckString = TestUtilities.CheckSumToString(checksums[e.FileName]);
                        string actualCheckString = TestUtilities.CheckSumToString(TestUtilities.ComputeChecksum(PathToExtractedFile));
                        Assert.AreEqual<String>(expectedCheckString, actualCheckString, "Unexpected checksum on extracted filesystem file ({0}).", PathToExtractedFile);
                    }
                }
            }
        }

        Ionic.CopyData.Transceiver _txrx;
        bool _pb1Set;
        void LNSF_SaveProgress(object sender, SaveProgressEventArgs e)
        {
            switch (e.EventType)
            {
                case ZipProgressEventType.Saving_Started:
                    _txrx.Send("status saving started...");
                    _pb1Set = false;
                    break;

                case ZipProgressEventType.Saving_BeforeWriteEntry:
                    _txrx.Send(String.Format("status Compressing {0}", e.CurrentEntry.FileName));
                    if (!_pb1Set)
                    {
                        _txrx.Send(String.Format("pb 1 max {0}", e.EntriesTotal));
                        _pb1Set = true;
                    }
                    break;
                    
                case ZipProgressEventType.Saving_EntryBytesRead:
                    Assert.IsTrue(e.BytesTransferred <= e.TotalBytesToTransfer);
                    break;

                case ZipProgressEventType.Saving_AfterWriteEntry:
                    _txrx.Send("pb 1 step");
                    break;

                case ZipProgressEventType.Saving_Completed:
                    _txrx.Send("status Save completed");
                    _pb1Set = false;
                    _txrx.Send("pb 1 max 1");
                    _txrx.Send("pb 1 value 1");
                    break;
            }
        }
        

        int _numEntriesToAdd= 0;
        int _numEntriesAdded= 0;
        void LNSF_AddProgress(object sender, AddProgressEventArgs e)
        {
            switch (e.EventType)
            {
                case ZipProgressEventType.Adding_Started:
                    _txrx.Send("status Adding files to the zip...");
                    _pb1Set = false;
                    break;

                case ZipProgressEventType.Adding_AfterAddEntry:
                    if (!_pb1Set)
                    {
                        _txrx.Send(String.Format("pb 1 max {0}", _numEntriesToAdd));
                        _pb1Set = true;
                    }
                    _numEntriesAdded++;
                    _txrx.Send(String.Format("status Adding file {0}/{1} :: {2}",
                                             _numEntriesAdded, _numEntriesToAdd, e.CurrentEntry.FileName));
                    _txrx.Send("pb 1 step");
                    break;
                    
            case ZipProgressEventType.Adding_Completed:
                _txrx.Send("status Added all files");
                _pb1Set = false;
                _txrx.Send("pb 1 max 1");
                _txrx.Send("pb 1 value 1");
                break;
            }
        }
        
        

        [Timeout(1500000), TestMethod]
        public void CreateZip_AddDirectory_LargeNumberOfSmallFiles()
        {
            // start the visible progress monitor
            string testBin = TestUtilities.GetTestBinDir(CurrentDir);
            string progressMonitorTool = Path.Combine(testBin, "Resources\\UnitTestProgressMonitor.exe");
            string requiredDll = Path.Combine(testBin, "Resources\\Ionic.CopyData.dll");
            
            Assert.IsTrue(File.Exists(progressMonitorTool), "progress monitor tool does not exist ({0})",  progressMonitorTool);
            Assert.IsTrue(File.Exists(requiredDll), "required DLL does not exist ({0})",  requiredDll);

            string progressChannel = "LargeNumberOfSmallFiles";
            // start the progress monitor
            this.Exec(progressMonitorTool, String.Format("-channel {0}", progressChannel), false);

            System.Threading.Thread.Sleep(1000);
            _txrx = new Ionic.CopyData.Transceiver();
            _txrx.Channel = progressChannel;
            _txrx.Send("test Large # of Small Files");
            _txrx.Send("bars 2");
            System.Threading.Thread.Sleep(120);
            
            int max1=0;
            Action<Int16,Int32> progressUpdate = (x,y) =>
                {
                    if (x==0)
                    {
                        _txrx.Send(String.Format("pb 1 max {0}", y));
                        max1 = y;
                    }
                    else if (x==2)
                    {
                        _txrx.Send(String.Format("pb 1 value {0}", y));
                        _txrx.Send(String.Format("status creating files in directory {0} of {1}", y, max1));
                    }
                    else if (x==4)
                    {
                        _txrx.Send(String.Format("status done creating {0} files", y));
                    }
                };
            
            
            
            int[][] settings = { 
                new int[] {71, 21, 97, 27, 200, 200 },
                new int[] {51, 171, 47, 197, 100, 100 },
            };
            _txrx.Send(String.Format("pb 0 max {0}", settings.Length * 2));

            TestContext.WriteLine("============================================");
            TestContext.WriteLine("Test beginning - {0}", System.DateTime.Now.ToString("G"));
            for (int m = 0; m < settings.Length; m++)
            {
                string ZipFileToCreate = Path.Combine(TopLevelDir, String.Format("CreateZip_AddDirectory_LargeNumberOfSmallFiles-{0}.zip", m));
                Assert.IsFalse(File.Exists(ZipFileToCreate), "The temporary zip file '{0}' already exists.", ZipFileToCreate);

                
                string DirToZip = Path.Combine(TopLevelDir, "zipthis" + m);
                Directory.CreateDirectory(DirToZip);

                TestContext.WriteLine("============================================");
                TestContext.WriteLine("Creating files, cycle {0}...", m);

                int subdirCount = 0;
                int entries = TestUtilities.GenerateFilesOneLevelDeep(TestContext, "LargeNumberOfFiles", DirToZip, settings[m], progressUpdate, out subdirCount);
                _numEntriesToAdd = entries;  // _numEntriesToAdd is used in LNSF_AddProgress
                
                _txrx.Send("pb 0 step");

                TestContext.WriteLine("============================================");
                TestContext.WriteLine("Total of {0} files in {1} subdirs", entries, subdirCount);
                TestContext.WriteLine("============================================");
                TestContext.WriteLine("Creating zip - {0}", System.DateTime.Now.ToString("G"));
                Directory.SetCurrentDirectory(TopLevelDir);
                using (ZipFile zip = new ZipFile())
                {
                    zip.AddProgress += LNSF_AddProgress;
                    zip.AddDirectory(Path.GetFileName(DirToZip));
                    zip.BufferSize = 4096;
                    zip.SaveProgress += LNSF_SaveProgress;
                    zip.Save(ZipFileToCreate);
                }

                _txrx.Send("pb 0 step");
                
                TestContext.WriteLine("Checking zip - {0}", System.DateTime.Now.ToString("G"));
                Assert.AreEqual<int>(TestUtilities.CountEntries(ZipFileToCreate), entries,
                             "The zip file created has the wrong number of entries.");

                // clean up for this cycle
                Directory.Delete(DirToZip, true);
            }
            TestContext.WriteLine("============================================");
            TestContext.WriteLine("Test end - {0}", System.DateTime.Now.ToString("G"));
            
            _txrx.Send("stop");
        }




        [TestMethod]
        public void CreateZip_AddDirectory_OnlyZeroLengthFiles()
        {
            string ZipFileToCreate = Path.Combine(TopLevelDir, "CreateZip_AddDirectory_OnlyZeroLengthFiles.zip");
            Assert.IsFalse(File.Exists(ZipFileToCreate), "The temporary zip file '{0}' already exists.", ZipFileToCreate);

            string DirToZip = Path.Combine(TopLevelDir, "zipthis");
            Directory.CreateDirectory(DirToZip);

            int entries = 0;
            int subdirCount = _rnd.Next(8) + 8;
            for (int i = 0; i < subdirCount; i++)
            {
                string SubDir = Path.Combine(DirToZip, "dir" + i);
                Directory.CreateDirectory(SubDir);

                // one empty file per subdir
                string file = TestUtilities.CreateUniqueFile("bin", SubDir);
                entries++;
            }

            Directory.SetCurrentDirectory(TopLevelDir);

            using (ZipFile zip = new ZipFile())
            {
                zip.AddDirectory(Path.GetFileName(DirToZip));
                zip.Save(ZipFileToCreate);
            }

            Assert.AreEqual<int>(TestUtilities.CountEntries(ZipFileToCreate), entries,
                    "The zip file created has the wrong number of entries.");
        }



        [TestMethod]
        public void CreateZip_AddDirectory_OneZeroLengthFile()
        {
            string ZipFileToCreate = Path.Combine(TopLevelDir, "CreateZip_AddDirectory_OneZeroLengthFile.zip");
            Assert.IsFalse(File.Exists(ZipFileToCreate), "The temporary zip file '{0}' already exists.", ZipFileToCreate);

            string DirToZip = Path.Combine(TopLevelDir, "zipthis");
            Directory.CreateDirectory(DirToZip);

            // one empty file
            string file = TestUtilities.CreateUniqueFile("ZeroLengthFile.txt", DirToZip);

            Directory.SetCurrentDirectory(TopLevelDir);

            using (ZipFile zip = new ZipFile())
            {
                zip.AddDirectory(Path.GetFileName(DirToZip));
                zip.Save(ZipFileToCreate);
            }

            Assert.AreEqual<int>(TestUtilities.CountEntries(ZipFileToCreate), 1,
                    "The zip file created has the wrong number of entries.");
        }


        [TestMethod]
        public void CreateZip_AddDirectory_OnlyEmptyDirectories()
        {
            string ZipFileToCreate = Path.Combine(TopLevelDir, "CreateZip_AddDirectory_OnlyEmptyDirectories.zip");
            Assert.IsFalse(File.Exists(ZipFileToCreate), "The temporary zip file '{0}' already exists.", ZipFileToCreate);

            string DirToZip = Path.Combine(TopLevelDir, "zipthis");
            Directory.CreateDirectory(DirToZip);

            int entries = 0;
            int subdirCount = _rnd.Next(8) + 8;
            for (int i = 0; i < subdirCount; i++)
            {
                string SubDir = Path.Combine(DirToZip, "EmptyDir" + i);
                Directory.CreateDirectory(SubDir);
            }

            Directory.SetCurrentDirectory(TopLevelDir);

            using (ZipFile zip = new ZipFile())
            {
                zip.AddDirectory(Path.GetFileName(DirToZip));
                zip.Save(ZipFileToCreate);
            }

            Assert.AreEqual<int>(TestUtilities.CountEntries(ZipFileToCreate), entries,
                    "The zip file created has the wrong number of entries.");
        }

        [TestMethod]
        public void CreateZip_AddDirectory_OneEmptyDirectory()
        {
            string ZipFileToCreate = Path.Combine(TopLevelDir, "CreateZip_AddDirectory_OneEmptyDirectory.zip");
            Assert.IsFalse(File.Exists(ZipFileToCreate), "The temporary zip file '{0}' already exists.", ZipFileToCreate);

            string DirToZip = Path.Combine(TopLevelDir, "zipthis");
            Directory.CreateDirectory(DirToZip);

            Directory.SetCurrentDirectory(TopLevelDir);

            using (ZipFile zip = new ZipFile())
            {
                zip.AddDirectory(Path.GetFileName(DirToZip));
                zip.Save(ZipFileToCreate);
            }

            Assert.AreEqual<int>(TestUtilities.CountEntries(ZipFileToCreate), 0,
                    "The zip file created has the wrong number of entries.");
        }


        [TestMethod]
        public void CreateZip_AddDirectory_CheckStatusTextWriter()
        {
            string ZipFileToCreate = Path.Combine(TopLevelDir, "CreateZip_AddDirectory_CheckStatusTextWriter.zip");
            Assert.IsFalse(File.Exists(ZipFileToCreate), "The temporary zip file '{0}' already exists.", ZipFileToCreate);

            string DirToZip = Path.Combine(TopLevelDir, "zipthis");
            Directory.CreateDirectory(DirToZip);

            int entries = 0;
            int subdirCount = _rnd.Next(8) + 8;
            for (int i = 0; i < subdirCount; i++)
            {
                string SubDir = Path.Combine(DirToZip, "Dir" + i);
                Directory.CreateDirectory(SubDir);
                // a few files per subdir
                int fileCount = _rnd.Next(12) + 4;
                for (int j = 0; j < fileCount; j++)
                {
                    string file = Path.Combine(SubDir, "File" + j);
                    TestUtilities.CreateAndFillFile(file, 100);
                    entries++;
                }
            }

            Directory.SetCurrentDirectory(TopLevelDir);

            var sw = new StringWriter();
            using (ZipFile zip = new ZipFile())
            {
                zip.StatusMessageTextWriter = sw;
                zip.AddDirectory(Path.GetFileName(DirToZip));
                zip.Save(ZipFileToCreate);
            }

            string status = sw.ToString();

            TestContext.WriteLine("status output: " + status);

            Assert.IsTrue(status.Length > 24 * entries, "Insufficient status messages on the StatusTexWriter?");

            Assert.AreEqual<int>(TestUtilities.CountEntries(ZipFileToCreate), entries,
                    "The zip file created has the wrong number of entries.");
        }


        struct TestTrial
        {
            public string arg;
            public string re;
        }


        [TestMethod]
        public void CreateZip_AddDirectory()
        {
            TestTrial[] trials = { 
                               new TestTrial { arg=null, re="^file(\\d+).ext$"},
                               new TestTrial { arg="", re="^file(\\d+).ext$"},
                               new TestTrial { arg=null, re="^file(\\d+).ext$"},
                               new TestTrial { arg="Xabf", re="(?s)^Xabf/(file(\\d+).ext)?$"},
                               new TestTrial { arg="AAAA/BBB", re="(?s)^AAAA/BBB/(file(\\d+).ext)?$"}
                               };

            Directory.SetCurrentDirectory(TopLevelDir);

            for (int k = 0; k < trials.Length; k++)
            {
                TestContext.WriteLine("\n--------------------------------\n\n\n");
                string ZipFileToCreate = Path.Combine(TopLevelDir, String.Format("CreateZip_AddDirectory-{0}.zip", k));

                Assert.IsFalse(File.Exists(ZipFileToCreate), "The temporary zip file '{0}' already exists.", ZipFileToCreate);

                string dirToZip = String.Format("DirectoryToZip.{0}.test", k);
                Directory.CreateDirectory(dirToZip);

                int fileCount = _rnd.Next(5) + 4;
                for (int i = 0; i < fileCount; i++)
                {
                    String file = Path.Combine(dirToZip, String.Format("file{0:D3}.ext", i));
                    TestUtilities.CreateAndFillFile(file, _rnd.Next(2000) + 500);
                }

                var sw = new StringWriter();
                using (ZipFile zip = new ZipFile())
                {
                    zip.StatusMessageTextWriter = sw;
                    if (k == 0)
                        zip.AddDirectory(dirToZip);
                    else
                        zip.AddDirectory(dirToZip, trials[k].arg);
                    zip.Save(ZipFileToCreate);
                }
                TestContext.WriteLine(sw.ToString());
                Assert.AreEqual<int>(TestUtilities.CountEntries(ZipFileToCreate), fileCount,
                        String.Format("The zip file created in cycle {0} has the wrong number of entries.", k));

                //TestContext.WriteLine("");
                // verify that the entries in the zip are in the top level directory!!
                using (ZipFile zip2 = ZipFile.Read(ZipFileToCreate))
                {
                    foreach (ZipEntry e in zip2)
                        TestContext.WriteLine("found entry: {0}", e.FileName);
                    foreach (ZipEntry e in zip2)
                    {
                        //Assert.IsFalse(e.FileName.StartsWith("dir"),
                        //       String.Format("The Zip entry '{0}' is not rooted in top level directory.", e.FileName));

                        // check the filename: 
                        //RE.Match m0 = RE.Regex.Match(e.FileName, fnameRegex[k]);
                        // Assert.IsTrue(m0 != null, "No match");
                        // Assert.AreEqual<int>(m0.Groups.Count, 2,
                        //    String.Format("In cycle {0}, Matching {1} against {2}, Wrong number of matches ({3})",
                        //        k, e.FileName, fnameRegex[k], m0.Groups.Count));

                        Assert.IsTrue(RE.Regex.IsMatch(e.FileName, trials[k].re),
                            String.Format("In cycle {0}, Matching {1} against {2}", k, e.FileName, trials[k].re));
                    }
                }
            }
        }


        [TestMethod]
        public void CreateZip_AddDirectory_Nested()
        {
            TestTrial[] trials = { 
                               new TestTrial { arg=null, re="^dir(\\d){3}/(file(\\d+).ext)?$"},
                               new TestTrial { arg="", re="^dir(\\d){3}/(file(\\d+).ext)?$"},
                               new TestTrial { arg=null, re="^dir(\\d){3}/(file(\\d+).ext)?$"},
                               new TestTrial { arg="rtdha", re="(?s)^rtdha/(dir(\\d){3}/(file(\\d+).ext)?)?$"},
                               new TestTrial { arg="sdfjk/BBB", re="(?s)^sdfjk/BBB/(dir(\\d){3}/(file(\\d+).ext)?)?$"}
                               };

            Directory.SetCurrentDirectory(TopLevelDir);

            for (int k = 0; k < trials.Length; k++)
            {
                TestContext.WriteLine("\n--------------------------------\n\n\n");
                string ZipFileToCreate = Path.Combine(TopLevelDir, String.Format("CreateZip_AddDirectory_Nested-{0}.zip", k));

                Assert.IsFalse(File.Exists(ZipFileToCreate), "The temporary zip file '{0}' already exists.", ZipFileToCreate);

                string dirToZip = String.Format("DirectoryToZip.{0}.test", k);
                Directory.CreateDirectory(dirToZip);

                int i, j;
                int entries = 0;

                int subdirCount = _rnd.Next(23) + 7;
                for (i = 0; i < subdirCount; i++)
                {
                    string Subdir = Path.Combine(dirToZip, String.Format("dir{0:D3}", i));
                    Directory.CreateDirectory(Subdir);

                    int fileCount = _rnd.Next(8);  // sometimes zero
                    for (j = 0; j < fileCount; j++)
                    {
                        String file = Path.Combine(Subdir, String.Format("file{0:D3}.ext", j));
                        TestUtilities.CreateAndFillFile(file, _rnd.Next(10750) + 50);
                        entries++;
                    }
                }


                //string dirToZip = Path.GetFileName(TopLevelDir);
                var sw = new StringWriter();
                using (ZipFile zip = new ZipFile())
                {
                    zip.StatusMessageTextWriter = sw;
                    if (k == 0)
                        zip.AddDirectory(dirToZip);
                    else
                        zip.AddDirectory(dirToZip, trials[k].arg);
                    zip.Save(ZipFileToCreate);
                }
                TestContext.WriteLine(sw.ToString());

                Assert.AreEqual<int>(TestUtilities.CountEntries(ZipFileToCreate), entries,
                        String.Format("The zip file created in cycle {0} has the wrong number of entries.", k));

                //TestContext.WriteLine("");
                // verify that the entries in the zip are in the top level directory!!
                using (ZipFile zip2 = ZipFile.Read(ZipFileToCreate))
                {
                    foreach (ZipEntry e in zip2)
                        TestContext.WriteLine("found entry: {0}", e.FileName);
                    foreach (ZipEntry e in zip2)
                    {
                        Assert.IsTrue(RE.Regex.IsMatch(e.FileName, trials[k].re),
                            String.Format("In cycle {0}, Matching {1} against {2}", k, e.FileName, trials[k].re));
                    }

                }
            }
        }


        [TestMethod]
        // [ExpectedException(typeof(FileNotFoundException))]
        public void Basic_SaveToFileStream()
        {
            // from small numbers of files to larger numbers of files
            for (int k = 0; k < 3; k++)
            {
                string ZipFileToCreate = Path.Combine(TopLevelDir, String.Format("Basic_SaveToFileStream-trial{0}.zip", k));
                Assert.IsFalse(File.Exists(ZipFileToCreate), "The temporary zip file '{0}' already exists.", ZipFileToCreate);

                string dirToZip = Path.Combine(TopLevelDir, Path.GetRandomFileName());
                Directory.CreateDirectory(dirToZip);

                int filesToAdd = _rnd.Next(k * 10 + 3) + k * 10 + 3;
                for (int i = 0; i < filesToAdd; i++)
                {
                    var s = Path.Combine(dirToZip, String.Format("tempfile-{0}.bin", i));
                    int sz = _rnd.Next(10000) + 5000;
                    TestContext.WriteLine("  Creating file: {0} sz({1})", s, sz);
                    TestUtilities.CreateAndFillFileBinary(s, sz);
                }

                var fileStream = File.Create(ZipFileToCreate);

                using (ZipFile zip1 = new ZipFile())
                {
                    zip1.AddDirectory(dirToZip);
                    zip1.Comment = "This is a Comment On the Archive (AM/PM)";
                    zip1.Save(fileStream);
                }

                fileStream.Close();

                // Verify the files are in the zip
                Assert.AreEqual<int>(TestUtilities.CountEntries(ZipFileToCreate), filesToAdd,
                    String.Format("In trial {0}, the Zip file {1} has the wrong number of entries.", k, ZipFileToCreate));
            }
        }


        [TestMethod]
        public void Basic_IsText()
        {
            // from small numbers of files to larger numbers of files
            for (int k = 0; k < 3; k++)
            {
                string ZipFileToCreate = Path.Combine(TopLevelDir, String.Format("Basic_IsText-trial{0}.zip", k));
                Assert.IsFalse(File.Exists(ZipFileToCreate), "The temporary zip file '{0}' already exists.", ZipFileToCreate);

                string dirToZip = Path.Combine(TopLevelDir, Path.GetRandomFileName());
                Directory.CreateDirectory(dirToZip);

                int filesToAdd = _rnd.Next(33) + 11;
                for (int i = 0; i < filesToAdd; i++)
                {
                    var s = Path.Combine(dirToZip, String.Format("tempfile-{0}.txt", i));
                    int sz = _rnd.Next(10000) + 5000;
                    TestContext.WriteLine("  Creating file: {0} sz({1})", s, sz);
                    TestUtilities.CreateAndFillFileText(s, sz);
                }

                using (ZipFile zip1 = new ZipFile())
                {
                    int count = 0;
                    var filesToZip = Directory.GetFiles(dirToZip);
                    foreach (var f in filesToZip)
                    {
                        var e = zip1.AddFile(f, "files");
                        switch (k)
                        {
                            case 0: break;
                            case 1: if ((count % 2) == 0) e.IsText = true; break;
                            case 2: if ((count % 2) != 0) e.IsText = true; break;
                            case 3: e.IsText = true;  break;
                        }
                        count++;
                    }
                    zip1.Comment = "This is a Comment On the Archive (AM/PM)";
                    zip1.Save(ZipFileToCreate);
                }

                // Verify the files are in the zip
                Assert.AreEqual<int>(TestUtilities.CountEntries(ZipFileToCreate), filesToAdd,
                    String.Format("In trial {0}, the Zip file {1} has the wrong number of entries.", k, ZipFileToCreate));

                // verify the isText setting
                using (ZipFile zip2 = ZipFile.Read(ZipFileToCreate))
                {
                    int count = 0;
                    foreach (var e in zip2)
                    {
                        switch (k)
                        {
                            case 0: Assert.IsFalse(e.IsText);  break;
                            case 1: Assert.AreEqual<bool>((count % 2) == 0,e.IsText); break;
                            case 2: Assert.AreEqual<bool>((count % 2) != 0, e.IsText); break;
                            case 3: Assert.IsTrue(e.IsText);  break;
                        }
                        count++;
                    }
                }
            }
        }


        [TestMethod]
        public void CreateZip_VerifyThatStreamRemainsOpenAfterSave()
        {
            bool[] ForceCompressionOptions = { true, false };
            string[] Passwords = { null, Path.GetRandomFileName() };

            for (int j = 0; j < Passwords.Length; j++)
            {
                for (int k = 0; k < ForceCompressionOptions.Length; k++)
                {
                    TestContext.WriteLine("\n\n---------------------------------\nTrial ({0},{1}):  Password='{2}' Compression={3}\n", j, k, Passwords[j], ForceCompressionOptions[k]);
                    Directory.SetCurrentDirectory(TopLevelDir);
                    string dirToZip = Path.GetRandomFileName();
                    Directory.CreateDirectory(dirToZip);

                    int filesAdded = _rnd.Next(3) + 3;
                    for (int i = 0; i < filesAdded; i++)
                    {
                        var s = Path.Combine(dirToZip, String.Format("tempfile-{0}-{1}-{2}.bin", j, k, i));
                        int sz = _rnd.Next(10000) + 5000;
                        TestContext.WriteLine("  Creating file: {0} sz({1})", s, sz);
                        TestUtilities.CreateAndFillFileBinary(s, sz);
                    }

                    TestContext.WriteLine("\n");

                    //string dirToZip = Path.GetFileName(TopLevelDir);
                    var ms = new MemoryStream();
                    Assert.IsTrue(ms.CanSeek, String.Format("Trial {0}: The output MemoryStream does not do Seek.", k));
                    using (ZipFile zip1 = new ZipFile())
                    {
                        zip1.ForceNoCompression = ForceCompressionOptions[k];
                        zip1.Password = Passwords[j];
                        zip1.Comment = String.Format("Trial ({0},{1}):  Password='{2}' Compression={3}\n", j, k, Passwords[j], ForceCompressionOptions[k]);
                        zip1.AddDirectory(dirToZip);
                        zip1.Save(ms);
                    }

                    Assert.IsTrue(ms.CanSeek, String.Format("Trial {0}: After writing, the OutputStream does not do Seek.", k));
                    Assert.IsTrue(ms.CanRead, String.Format("Trial {0}: The OutputStream cannot be Read.", k));

                    // seek to the beginning
                    ms.Seek(0, SeekOrigin.Begin);
                    int filesFound = 0;
                    using (ZipFile zip2 = ZipFile.Read(ms))
                    {
                        foreach (ZipEntry e in zip2)
                        {
                            TestContext.WriteLine("  Found entry: {0} isDir({1}) sz_c({2}) sz_unc({3})", e.FileName, e.IsDirectory, e.CompressedSize, e.UncompressedSize);
                            if (!e.IsDirectory)
                                filesFound++;
                        }
                    }
                    Assert.AreEqual<int>(filesFound, filesAdded, String.Format("Trial {0}, Found an incorrect number of files.", k));
                }
            }
        }


        [TestMethod]
        public void CreateZip_AddFile_VerifyCrcAndContents()
        {
            string filename = null;
            int entriesAdded = 0;
            string repeatedLine = null;
            int j;

            // select the name of the zip file
            string ZipFileToCreate = Path.Combine(TopLevelDir, "CreateZip_AddFile_VerifyCrcAndContents.zip");
            Assert.IsFalse(File.Exists(ZipFileToCreate), "The temporary zip file '{0}' already exists.", ZipFileToCreate);

            // create the subdirectory
            string Subdir = Path.Combine(TopLevelDir, "A");
            Directory.CreateDirectory(Subdir);

            // create the files
            int NumFilesToCreate = _rnd.Next(10) + 8;
            for (j = 0; j < NumFilesToCreate; j++)
            {
                filename = Path.Combine(Subdir, String.Format("file{0:D3}.txt", j));
                repeatedLine = String.Format("This line is repeated over and over and over in file {0}",
                    Path.GetFileName(filename));
                TestUtilities.CreateAndFillFileText(filename, repeatedLine, _rnd.Next(34000) + 5000);
                entriesAdded++;
            }

            // Create the zip file
            Directory.SetCurrentDirectory(TopLevelDir);
            using (ZipFile zip1 = new ZipFile())
            {
                String[] filenames = Directory.GetFiles("A");
                foreach (String f in filenames)
                    zip1.AddFile(f, "");
                zip1.Comment = "UpdateTests::CreateZip_AddFile_VerifyCrcAndContents(): This archive will be updated.";
                zip1.Save(ZipFileToCreate);
            }

            // Verify the files are in the zip
            Assert.AreEqual<int>(TestUtilities.CountEntries(ZipFileToCreate), entriesAdded,
                "The Zip file has the wrong number of entries.");

            // now extract the files and verify their contents
            using (ZipFile zip2 = ZipFile.Read(ZipFileToCreate))
            {
                foreach (string s in zip2.EntryFileNames)
                {
                    repeatedLine = String.Format("This line is repeated over and over and over in file {0}", s);
                    zip2[s].Extract("extract");

                    // verify the content of the updated file. 
                    var sr = new StreamReader(Path.Combine("extract", s));
                    string actualLine = sr.ReadLine();
                    sr.Close();

                    Assert.AreEqual<string>(repeatedLine, actualLine,
                            String.Format("The content of the Updated file ({0}) in the zip archive is incorrect.", s));
                }
            }
        }


        [TestMethod]
        public void CreateZip_WithEmptyDirectory()
        {
            // select the name of the zip file
            string ZipFileToCreate = Path.Combine(TopLevelDir, "Create_WithEmptyDirectory.zip");
            Assert.IsFalse(File.Exists(ZipFileToCreate), "The temporary zip file '{0}' already exists.", ZipFileToCreate);

            // create the subdirectory
            string Subdir = Path.Combine(TopLevelDir, "EmptyDirectory");
            Directory.CreateDirectory(Subdir);

            using (ZipFile zip = new ZipFile())
            {
                zip.AddDirectory(Subdir, "");
                zip.Save(ZipFileToCreate);
            }

            // Verify the files are in the zip
            Assert.AreEqual<int>(TestUtilities.CountEntries(ZipFileToCreate), 0,
                "The Zip file has the wrong number of entries.");

        }


        
        [TestMethod]
        public void Basic_Set_ZipEntry()
        {
            Directory.SetCurrentDirectory(TopLevelDir);
            string  zipFileToCreate = Path.Combine(TopLevelDir, "Basic_Set_ZipEntry.zip");
            string dirToZip = Path.GetFileNameWithoutExtension(Path.GetRandomFileName());
            var files = TestUtilities.GenerateFilesFlat(dirToZip);

            using (ZipFile zip = new ZipFile())
            {
                zip.AddFiles(files);
                zip.Save(zipFileToCreate);
            }
            
            int count = TestUtilities.CountEntries(zipFileToCreate);
            Assert.IsTrue(count>0);
            
            using (ZipFile zip = ZipFile.Read(zipFileToCreate))
            {
                // this should be fine
                zip[1]= null;
                zip.Save();
            }
            Assert.AreEqual<Int32>(count, TestUtilities.CountEntries(zipFileToCreate)+1);
        }

        
        

        [TestMethod]
        public void Extract_IntoMemoryStream()
        {
            string filename = null;
            int entriesAdded = 0;
            string repeatedLine = null;
            int j;

            // select the name of the zip file
            string ZipFileToCreate = Path.Combine(TopLevelDir, "Extract_IntoMemoryStream.zip");
            Assert.IsFalse(File.Exists(ZipFileToCreate), "The temporary zip file '{0}' already exists.", ZipFileToCreate);

            // create the subdirectory
            string Subdir = Path.Combine(TopLevelDir, "A");
            Directory.CreateDirectory(Subdir);

            // create the files
            int NumFilesToCreate = _rnd.Next(10) + 8;
            for (j = 0; j < NumFilesToCreate; j++)
            {
                filename = Path.Combine(Subdir, String.Format("file{0:D3}.txt", j));
                repeatedLine = String.Format("This line is repeated over and over and over in file {0}",
                    Path.GetFileName(filename));
                TestUtilities.CreateAndFillFileText(filename, repeatedLine, _rnd.Next(34000) + 5000);
                entriesAdded++;
            }

            // Create the zip file
            Directory.SetCurrentDirectory(TopLevelDir);
            using (ZipFile zip1 = new ZipFile())
            {
                String[] filenames = Directory.GetFiles("A");
                foreach (String f in filenames)
                    zip1.AddFile(f, "");
                zip1.Comment = "BasicTests::Extract_IntoMemoryStream()";
                zip1.Save(ZipFileToCreate);
            }

            // Verify the files are in the zip
            Assert.AreEqual<int>(TestUtilities.CountEntries(ZipFileToCreate), entriesAdded,
                "The Zip file has the wrong number of entries.");

            // now extract the files into memory streams, checking only the length of the file.
            using (ZipFile zip2 = ZipFile.Read(ZipFileToCreate))
            {
                foreach (string s in zip2.EntryFileNames)
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        zip2[s].Extract(ms);
                        byte[] a = ms.ToArray();
                        string f = Path.Combine(Subdir, s);
                        var fi = new FileInfo(f);
                        Assert.AreEqual<int>((int)(fi.Length), a.Length, "Unequal file lengths.");
                    }
                }
            }
        }


        [TestMethod]
        public void Retrieve_ViaIndexer()
        {
            // select the name of the zip file
            string ZipFileToCreate = Path.Combine(TopLevelDir, "Retrieve_ViaIndexer.zip");
            Assert.IsFalse(File.Exists(ZipFileToCreate), "The temporary zip file '{0}' already exists.", ZipFileToCreate);

            string filename = null;
            int entriesAdded = 0;
            string repeatedLine = null;
            int j;

            // create the subdirectory
            string Subdir = Path.Combine(TopLevelDir, "A");
            Directory.CreateDirectory(Subdir);

            // create the files
            int NumFilesToCreate = _rnd.Next(10) + 8;
            for (j = 0; j < NumFilesToCreate; j++)
            {
                filename = Path.Combine(Subdir, String.Format("File{0:D3}.txt", j));
                repeatedLine = String.Format("This line is repeated over and over and over in file {0}",
                    Path.GetFileName(filename));
                TestUtilities.CreateAndFillFileText(filename, repeatedLine, _rnd.Next(23000) + 4000);
                entriesAdded++;
            }

            // Create the zip file
            Directory.SetCurrentDirectory(TopLevelDir);
            using (ZipFile zip1 = new ZipFile())
            {
                String[] filenames = Directory.GetFiles("A");
                foreach (String f in filenames)
                    zip1.AddFile(f, "");
                zip1.Comment = "BasicTests::Retrieve_ViaIndexer()";
                zip1.Save(ZipFileToCreate);
            }

            // Verify the files are in the zip
            Assert.AreEqual<int>(TestUtilities.CountEntries(ZipFileToCreate), entriesAdded,
                "The Zip file has the wrong number of entries.");

            // now extract the files into memory streams, checking only the length of the file.
            // We do 4 combinations:  case-sensitive on or off, and filename conversion on or off.
            for (int m = 0; m < 2; m++)
            {
                for (int n = 0; n < 2; n++)
                {
                    using (ZipFile zip2 = ZipFile.Read(ZipFileToCreate))
                    {
                        if (n == 1) zip2.CaseSensitiveRetrieval = true;
                        foreach (string s in zip2.EntryFileNames)
                        {
                            var s2 = (m == 1) ? s.ToUpper() : s;
                            using (MemoryStream ms = new MemoryStream())
                            {
                                try
                                {
                                    zip2[s2].Extract(ms);
                                    byte[] a = ms.ToArray();
                                    string f = Path.Combine(Subdir, s2);
                                    var fi = new FileInfo(f);
                                    Assert.AreEqual<int>((int)(fi.Length), a.Length, "Unequal file lengths.");
                                }
                                catch
                                {
                                    Assert.AreEqual<int>(1, n * m, "Indexer retrieval failed unexpectedly.");
                                }
                            }
                        }
                    }
                }
            }
        }




        [TestMethod]
        public void CreateZip_SetFileComments()
        {
            string ZipFileToCreate = Path.Combine(TopLevelDir, "FileComments.zip");
            Assert.IsFalse(File.Exists(ZipFileToCreate), "The temporary zip file '{0}' already exists.", ZipFileToCreate);

            string FileCommentFormat = "Comment Added By Test to file '{0}'";
            string CommentOnArchive = "Comment added by FileComments() method.";


            int fileCount = _rnd.Next(3) + 3;
            string[] FilesToZip = new string[fileCount];
            for (int i = 0; i < fileCount; i++)
            {
                FilesToZip[i] = Path.Combine(TopLevelDir, String.Format("file{0:D3}.bin", i));
                TestUtilities.CreateAndFillFile(FilesToZip[i], _rnd.Next(10000) + 5000);
            }

            Directory.SetCurrentDirectory(TopLevelDir);

            using (ZipFile zip = new ZipFile())
            {
                //zip.StatusMessageTextWriter = System.Console.Out;
                for (int i = 0; i < FilesToZip.Length; i++)
                {
                    // use the local filename (not fully qualified)
                    ZipEntry e = zip.AddFile(Path.GetFileName(FilesToZip[i]));
                    e.Comment = String.Format(FileCommentFormat, e.FileName);
                }
                zip.Comment = CommentOnArchive;
                zip.Save(ZipFileToCreate);
            }

            int entries = 0;
            using (ZipFile z2 = ZipFile.Read(ZipFileToCreate))
            {
                Assert.AreEqual<String>(CommentOnArchive, z2.Comment, "Unexpected comment on ZipFile.");
                foreach (ZipEntry e in z2)
                {
                    string expectedComment = String.Format(FileCommentFormat, e.FileName);
                    Assert.AreEqual<string>(expectedComment, e.Comment, "Unexpected comment on ZipEntry.");
                    entries++;
                }
            }
            Assert.AreEqual<int>(entries, FilesToZip.Length, "Unexpected file count. Expected {0}, got {1}.",
                    FilesToZip.Length, entries);
        }


        [TestMethod]
        public void CreateZip_SetFileLastModified()
        {
            string ZipFileToCreate = Path.Combine(TopLevelDir, "CreateZip_SetFileLastModified.zip");
            Assert.IsFalse(File.Exists(ZipFileToCreate), "The temporary zip file '{0}' already exists.", ZipFileToCreate);

            int fileCount = _rnd.Next(13) + 23;
            string[] FilesToZip = new string[fileCount];
            for (int i = 0; i < fileCount; i++)
            {
                FilesToZip[i] = Path.Combine(TopLevelDir, String.Format("file{0:D3}.bin", i));
                TestUtilities.CreateAndFillFileBinary(FilesToZip[i], _rnd.Next(10000) + 5000);
            }

            Directory.SetCurrentDirectory(TopLevelDir);
            var Timestamp = new System.DateTime(2007, 9, 1, 15, 0, 0);
            using (ZipFile zip = new ZipFile())
            {
                for (int i = 0; i < FilesToZip.Length; i++)
                {
                    // use the local filename (not fully qualified)
                    ZipEntry e = zip.AddFile(Path.GetFileName(FilesToZip[i]));
                    e.LastModified = Timestamp;
                }
                zip.Comment = "All files in this archive have the same timestamp.";
                zip.Save(ZipFileToCreate);
            }

            int entries = 0;
            using (ZipFile z2 = ZipFile.Read(ZipFileToCreate))
            {
                foreach (ZipEntry e in z2)
                {
                    Assert.AreEqual<DateTime>(Timestamp, e.LastModified, "Unexpected timestamp on ZipEntry.");
                    entries++;
                    // now verify that the LastMod time on the filesystem file is set correctly
                    e.Extract("unpack");
                    DateTime ActualFilesystemLastMod = File.GetLastWriteTime(Path.Combine("unpack", e.FileName));
                    Assert.AreEqual<DateTime>(Timestamp, ActualFilesystemLastMod, "Unexpected timestamp on extracted filesystem file.");
                }
            }
            Assert.AreEqual<int>(entries, FilesToZip.Length, "Unexpected file count. Expected {0}, got {1}.",
                    FilesToZip.Length, entries);
        }

        [TestMethod]
        public void CreateAndExtract_VerifyAttributes()
        {

            try
            {
                string ZipFileToCreate = Path.Combine(TopLevelDir, "CreateAndExtract_VerifyAttributes.zip");
                Assert.IsFalse(File.Exists(ZipFileToCreate), "The temporary zip file '{0}' already exists.", ZipFileToCreate);

                string Subdir = Path.Combine(TopLevelDir, "A");
                Directory.CreateDirectory(Subdir);

                //int fileCount = _rnd.Next(13) + 23;
                FileAttributes[] attributeCombos = {
                                                   FileAttributes.ReadOnly,
                                                   FileAttributes.ReadOnly | FileAttributes.System,
                                                   FileAttributes.ReadOnly | FileAttributes.System | FileAttributes.Hidden,
                                                   FileAttributes.ReadOnly | FileAttributes.System | FileAttributes.Hidden | FileAttributes.Archive,
                                                   FileAttributes.ReadOnly | FileAttributes.Hidden,
                                                   FileAttributes.ReadOnly | FileAttributes.Hidden| FileAttributes.Archive,
                                                   FileAttributes.ReadOnly | FileAttributes.Archive,
                                                   FileAttributes.System,
                                                   FileAttributes.System | FileAttributes.Hidden,
                                                   FileAttributes.System | FileAttributes.Hidden | FileAttributes.Archive,
                                                   FileAttributes.System | FileAttributes.Archive,
                                                   FileAttributes.Hidden,
                                                   FileAttributes.Hidden | FileAttributes.Archive,
                                                   FileAttributes.Archive,
                                                   FileAttributes.Normal,
                                                   FileAttributes.NotContentIndexed | FileAttributes.ReadOnly,
                                                   FileAttributes.NotContentIndexed | FileAttributes.System,
                                                   FileAttributes.NotContentIndexed | FileAttributes.Hidden,
                                                   FileAttributes.NotContentIndexed | FileAttributes.Archive,
                                                   FileAttributes.Temporary,
                                                   FileAttributes.Temporary | FileAttributes.Archive,
                                              };
                int fileCount = attributeCombos.Length;
                string[] FilesToZip = new string[fileCount];
                TestContext.WriteLine("============\nCreating.");
                for (int i = 0; i < fileCount; i++)
                {
                    FilesToZip[i] = Path.Combine(Subdir, String.Format("file{0:D3}.bin", i));
                    TestUtilities.CreateAndFillFileBinary(FilesToZip[i], _rnd.Next(10000) + 5000);
                    TestContext.WriteLine("Creating {0}    [{1}]", FilesToZip[i], attributeCombos[i].ToString());
                    File.SetAttributes(FilesToZip[i], attributeCombos[i]);
                }

                TestContext.WriteLine("============\nZipping.");
                Directory.SetCurrentDirectory(TopLevelDir);
                using (ZipFile zip = new ZipFile())
                {
                    for (int i = 0; i < FilesToZip.Length; i++)
                    {
                        // use the local filename (not fully qualified)
                        ZipEntry e = zip.AddFile(FilesToZip[i], "");
                    }
                    zip.Save(ZipFileToCreate);
                }

                int entries = 0;
                TestContext.WriteLine("============\nExtracting.");
                using (ZipFile z2 = ZipFile.Read(ZipFileToCreate))
                {
                    foreach (ZipEntry e in z2)
                    {
                        TestContext.WriteLine("Extracting {0}", e.FileName);
                        Assert.AreEqual<FileAttributes>(attributeCombos[entries], e.Attributes,
                            String.Format("unexpected attributes value in the entry {0} 0x{1:X4}", e.FileName, (int)e.Attributes));
                        entries++;
                        e.Extract("unpack");
                        // now verify that the attributes are set correctly in the filesystem

                        var attrs = File.GetAttributes(Path.Combine("unpack", e.FileName));
                        Assert.AreEqual<FileAttributes>(attrs, e.Attributes,
                            "Unexpected attributes on the extracted filesystem file {0}.", e.FileName);
                    }
                }
                Assert.AreEqual<int>(entries, FilesToZip.Length, "Unexpected file count. Expected {0}, got {1}.",
                        FilesToZip.Length, entries);
            }
            catch (Exception ex1)
            {
                TestContext.WriteLine("Exception: " + ex1);
                throw;
            }
        }



        [TestMethod]
        public void CreateAndExtract_SetAndVerifyAttributes()
        {
            string ZipFileToCreate = Path.Combine(TopLevelDir, "CreateAndExtract_SetAndVerifyAttributes.zip");
            Assert.IsFalse(File.Exists(ZipFileToCreate), "The temporary zip file '{0}' already exists.", ZipFileToCreate);

            // Here, we build a list of combinations of FileAttributes to try. 
            // We cannot simply do an exhaustic combination because (a) not all combinations are valid, and (b)
            // if you SetAttributes(file,Compressed) (also with Encrypted, ReparsePoint) it does not "work."  So those attributes 
            // must be excluded.
            FileAttributes[] attributeCombos = {
                                                   FileAttributes.ReadOnly,
                                                   FileAttributes.ReadOnly | FileAttributes.System,
                                                   FileAttributes.ReadOnly | FileAttributes.System | FileAttributes.Hidden,
                                                   FileAttributes.ReadOnly | FileAttributes.System | FileAttributes.Hidden | FileAttributes.Archive,
                                                   FileAttributes.ReadOnly | FileAttributes.Hidden,
                                                   FileAttributes.ReadOnly | FileAttributes.Hidden| FileAttributes.Archive,
                                                   FileAttributes.ReadOnly | FileAttributes.Archive,
                                                   FileAttributes.System,
                                                   FileAttributes.System | FileAttributes.Hidden,
                                                   FileAttributes.System | FileAttributes.Hidden | FileAttributes.Archive,
                                                   FileAttributes.System | FileAttributes.Archive,
                                                   FileAttributes.Hidden,
                                                   FileAttributes.Hidden | FileAttributes.Archive,
                                                   FileAttributes.Archive,
                                                   FileAttributes.Normal,
                                                   FileAttributes.NotContentIndexed | FileAttributes.ReadOnly,
                                                   FileAttributes.NotContentIndexed | FileAttributes.System,
                                                   FileAttributes.NotContentIndexed | FileAttributes.Hidden,
                                                   FileAttributes.NotContentIndexed | FileAttributes.Archive,
                                                   FileAttributes.Temporary,
                                                   FileAttributes.Temporary | FileAttributes.Archive,
                                              };
            int fileCount = attributeCombos.Length;

            Directory.SetCurrentDirectory(TopLevelDir);
            TestContext.WriteLine("============\nZipping.");
            using (ZipFile zip = new ZipFile())
            {
                for (int i = 0; i < fileCount; i++)
                {
                    // use the local filename (not fully qualified)
                    ZipEntry e = zip.AddEntry("file" + i.ToString(), "",
                            "FileContent: This file has these attributes: " + attributeCombos[i].ToString());
                    TestContext.WriteLine("Adding {0}    [{1}]", e.FileName, attributeCombos[i].ToString());
                    e.Attributes = attributeCombos[i];
                }
                zip.Save(ZipFileToCreate);
            }

            int entries = 0;
            TestContext.WriteLine("============\nExtracting.");
            using (ZipFile z2 = ZipFile.Read(ZipFileToCreate))
            {
                foreach (ZipEntry e in z2)
                {
                    TestContext.WriteLine("Extracting {0}", e.FileName);
                    Assert.AreEqual<FileAttributes>(attributeCombos[entries], e.Attributes,
                        String.Format("unexpected attributes value in the entry {0} 0x{1:X4}", e.FileName, (int)e.Attributes));
                    entries++;
                    e.Extract("unpack");
                    // now verify that the attributes are set correctly in the filesystem

                    var attrs = File.GetAttributes(Path.Combine("unpack", e.FileName));
                    Assert.AreEqual<FileAttributes>(e.Attributes, attrs,
                        "Unexpected attributes on the extracted filesystem file {0}.", e.FileName);
                }
            }
            Assert.AreEqual<int>(fileCount, entries, "Unexpected file count.");
        }


        [TestMethod]
        public void CreateZip_VerifyFileLastModified()
        {
            string ZipFileToCreate = Path.Combine(TopLevelDir, "CreateZip_VerifyFileLastModified.zip");
            Assert.IsFalse(File.Exists(ZipFileToCreate), "The temporary zip file '{0}' already exists.", ZipFileToCreate);

            String[] PotentialFilenames = Directory.GetFiles(System.Environment.GetEnvironmentVariable("TEMP"));
            var checksums = new Dictionary<string, byte[]>();
            var timestamps = new Dictionary<string, DateTime>();
            var ActualFilenames = new List<string>();
            var ExcludedFilenames = new List<string>();

            int maxFiles = _rnd.Next(PotentialFilenames.Length / 2) + PotentialFilenames.Length / 3;
            maxFiles = Math.Min(maxFiles, 145);
            //maxFiles = Math.Min(maxFiles, 15);
            TestContext.WriteLine("\n-----------------------------\r\n{1}: Finding files in '{0}'...",
                System.Environment.GetEnvironmentVariable("TEMP"),
                DateTime.Now.ToString("HH:mm:ss"));
            do
            {
                string filename = null;
                bool foundOne = false;
                while (!foundOne)
                {
                    filename = PotentialFilenames[_rnd.Next(PotentialFilenames.Length)];
                    if (ExcludedFilenames.Contains(filename)) continue;
                    var fi = new FileInfo(filename);

                    if (Path.GetFileName(filename)[0] == '~'
                            || ActualFilenames.Contains(filename)
                        || fi.Length > 10000000
                        // there are some weird files on my system that cause this test to fail!
                        // the GetLastWrite() method returns the "wrong" time - does not agree with
                        // what is shown in Explorer or in a cmd.exe dir output.  So I exclude those 
                        // files here.
                        //|| filename.EndsWith(".cer")
                        //|| filename.EndsWith(".msrcincident")
                        //|| filename == "MSCERTS.ini"
                        )
                    {
                        ExcludedFilenames.Add(filename);
                    }
                    else
                    {
                        foundOne = true;
                    }
                }

                var key = Path.GetFileName(filename);

                // surround this in a try...catch so as to avoid grabbing a file that is open by someone else, or has disappeared
                try
                {
                    var lastWrite = File.GetLastWriteTime(filename);
                    var fi = new FileInfo(filename);

                    // Rounding to nearest even second was necessary when DotNetZip did 
                    // not process NTFS times in the NTFS Extra field. Since v1.8.0.5, this
                    // is no longer the case.
                    // var tm = TestUtilities.RoundToEvenSecond(lastWrite); 

                    var tm = lastWrite;
                    // hop out of the try block if the file is from TODAY.  (heuristic to avoid currently open files)
                    if ((tm.Year == DateTime.Now.Year) && (tm.Month == DateTime.Now.Month) && (tm.Day == DateTime.Now.Day))
                        throw new Exception();
                    var chk = TestUtilities.ComputeChecksum(filename);
                    checksums.Add(key, chk);
                    TestContext.WriteLine("  {4}:  {1}  {2}  {3,-9}  {0}",
                        Path.GetFileName(filename),
                        lastWrite.ToString("yyyy MMM dd HH:mm:ss"),
                        tm.ToString("yyyy MMM dd HH:mm:ss"),
                        fi.Length,
                        DateTime.Now.ToString("HH:mm:ss"));
                    timestamps.Add(key, this.AdjustTime_Win32ToDotNet(tm));
                    ActualFilenames.Add(filename);
                }
                catch
                {
                    ExcludedFilenames.Add(filename);
                }
            } while ((ActualFilenames.Count < maxFiles) && (ActualFilenames.Count < PotentialFilenames.Length) &&
                ActualFilenames.Count + ExcludedFilenames.Count < PotentialFilenames.Length);

            Directory.SetCurrentDirectory(TopLevelDir);

            TestContext.WriteLine("{0}: Creating zip...", DateTime.Now.ToString("HH:mm:ss"));

            // create the zip file
            using (ZipFile zip = new ZipFile())
            {
                foreach (string s in ActualFilenames)
                {
                    ZipEntry e = zip.AddFile(s, "");
                    e.Comment = File.GetLastWriteTime(s).ToString("yyyyMMMdd HH:mm:ss");
                }
                zip.Comment = "The files in this archive will be checked for LastMod timestamp and checksum.";
                TestContext.WriteLine("{0}: Saving zip....", DateTime.Now.ToString("HH:mm:ss"));
                zip.Save(ZipFileToCreate);
            }

            TestContext.WriteLine("{0}: Unpacking zip....", DateTime.Now.ToString("HH:mm:ss"));

            // unpack the zip, and verify contents
            int entries = 0;
            using (ZipFile z2 = ZipFile.Read(ZipFileToCreate))
            {
                foreach (ZipEntry e in z2)
                {
                    TestContext.WriteLine("{0}: Checking entry {1}....", DateTime.Now.ToString("HH:mm:ss"), e.FileName);
                    entries++;
                    // verify that the LastMod time on the filesystem file is set correctly
                    e.Extract("unpack");
                    string PathToExtractedFile = Path.Combine("unpack", e.FileName);
                    DateTime ActualFilesystemLastMod = AdjustTime_Win32ToDotNet(File.GetLastWriteTime(PathToExtractedFile));
                    TimeSpan delta = timestamps[e.FileName] - ActualFilesystemLastMod;

                    // get the delta as an absolute value:
                    if (delta < new TimeSpan(0, 0, 0))
                        delta = new TimeSpan(0, 0, 0) - delta;

                    TestContext.WriteLine("time delta: {0}", delta.ToString());
                    // The time delta can be at most, 1 second.
                    Assert.IsTrue(delta < new TimeSpan(0, 0, 1),
                        "Unexpected LastMod timestamp on extracted filesystem file ({0}) expected({1}) actual({2})  delta({3}).",
                        PathToExtractedFile,
                        timestamps[e.FileName].ToString("F"),
                        ActualFilesystemLastMod.ToString("F"),
                        delta.ToString()
                        );

                    // verify the checksum of the file is correct
                    string expectedCheckString = TestUtilities.CheckSumToString(checksums[e.FileName]);
                    string actualCheckString = TestUtilities.CheckSumToString(TestUtilities.ComputeChecksum(PathToExtractedFile));
                    Assert.AreEqual<String>(expectedCheckString, actualCheckString, "Unexpected checksum on extracted filesystem file ({0}).", PathToExtractedFile);
                }
            }
            Assert.AreEqual<int>(entries, ActualFilenames.Count, "Unexpected file count.");
        }



        private DateTime AdjustTime_Win32ToDotNet(DateTime time)
        {
            // If I read a time from a file with GetLastWriteTime() (etc), I need
            // to adjust it for display in the .NET environment.  
            DateTime adjusted = time;
            if (DateTime.Now.IsDaylightSavingTime() && !time.IsDaylightSavingTime())
                adjusted = time + new System.TimeSpan(1, 0, 0);

            else if (!DateTime.Now.IsDaylightSavingTime() && time.IsDaylightSavingTime())
                adjusted = time - new System.TimeSpan(1, 0, 0);

            return adjusted;
        }



        [TestMethod]
        public void CreateZip_AddDirectory_NoFilesInRoot()
        {
            string ZipFileToCreate = Path.Combine(TopLevelDir, "CreateZip_AddDirectory_NoFilesInRoot.zip");

            Assert.IsFalse(File.Exists(ZipFileToCreate), "The temporary zip file '{0}' already exists.", ZipFileToCreate);

            string ZipThis = Path.Combine(TopLevelDir, "ZipThis");
            Directory.CreateDirectory(ZipThis);

            int i, j;
            int entries = 0;

            int subdirCount = _rnd.Next(4) + 4;
            for (i = 0; i < subdirCount; i++)
            {
                string Subdir = Path.Combine(ZipThis, "DirectoryToZip.test." + i);
                Directory.CreateDirectory(Subdir);

                int fileCount = _rnd.Next(3) + 3;
                for (j = 0; j < fileCount; j++)
                {
                    String file = Path.Combine(Subdir, "file" + j);
                    TestUtilities.CreateAndFillFile(file, _rnd.Next(1000) + 1500);
                    entries++;
                }
            }

            Directory.SetCurrentDirectory(TopLevelDir);
            using (ZipFile zip = new ZipFile())
            {
                zip.AddDirectory("ZipThis");
                zip.Save(ZipFileToCreate);
            }
            Assert.AreEqual<int>(TestUtilities.CountEntries(ZipFileToCreate), entries,
                    "The Zip file has the wrong number of entries.");
        }


        [TestMethod]
        public void CreateZip_AddDirectory_OneCharOverrideName()
        {
            int entries = 0;
            String filename = null;

            // set the name of the zip file to create
            string ZipFileToCreate = Path.Combine(TopLevelDir, "CreateZip_AddDirectory_OneCharOverrideName.zip");
            Assert.IsFalse(File.Exists(ZipFileToCreate), "The temporary zip file '{0}' already exists.", ZipFileToCreate);

            String CommentOnArchive = "BasicTests::CreateZip_AddDirectory_OneCharOverrideName(): This archive override the name of a directory with a one-char name.";

            string Subdir = Path.Combine(TopLevelDir, "A");
            Directory.CreateDirectory(Subdir);

            int NumFilesToCreate = _rnd.Next(23) + 14;
            var checksums = new Dictionary<string, string>();
            for (int j = 0; j < NumFilesToCreate; j++)
            {
                filename = Path.Combine(Subdir, String.Format("file{0:D3}.txt", j));
                TestUtilities.CreateAndFillFileText(filename, _rnd.Next(12000) + 5000);
                var chk = TestUtilities.ComputeChecksum(filename);

                var relativePath = Path.Combine(Path.GetFileName(Subdir), Path.GetFileName(filename));
                //var key = Path.Combine("A", filename);
                var key = TestUtilities.TrimVolumeAndSwapSlashes(relativePath);
                checksums.Add(key, TestUtilities.CheckSumToString(chk));

                entries++;
            }

            Directory.SetCurrentDirectory(TopLevelDir);

            using (ZipFile zip1 = new ZipFile())
            {
                zip1.AddDirectory(Subdir, Path.GetFileName(Subdir));
                zip1.Comment = CommentOnArchive;
                zip1.Save(ZipFileToCreate);
            }

            Assert.AreEqual<int>(TestUtilities.CountEntries(ZipFileToCreate), entries,
                    "The created Zip file has an unexpected number of entries.");

            // validate all the checksums
            using (ZipFile zip2 = ZipFile.Read(ZipFileToCreate))
            {
                foreach (ZipEntry e in zip2)
                {
                    e.Extract("unpack");
                    if (checksums.ContainsKey(e.FileName))
                    {
                        string PathToExtractedFile = Path.Combine("unpack", e.FileName);

                        // verify the checksum of the file is correct
                        string expectedCheckString = checksums[e.FileName];
                        string actualCheckString = TestUtilities.CheckSumToString(TestUtilities.ComputeChecksum(PathToExtractedFile));
                        Assert.AreEqual<String>(expectedCheckString, actualCheckString, "Unexpected checksum on extracted filesystem file ({0}).", PathToExtractedFile);
                    }
                }
            }

        }



        [TestMethod]
        public void CreateZip_ForceNoCompressionAllEntries()
        {
            string ZipFileToCreate = Path.Combine(TopLevelDir, "ForceNoCompression.zip");
            //_FilesToRemove.Add(ZipFileToCreate);
            Assert.IsFalse(File.Exists(ZipFileToCreate), "The temporary zip file '{0}' already exists.", ZipFileToCreate);

            String CommentOnArchive = "BasicTests::ForceNoCompression(): This archive override the name of a directory with a one-char name.";

            int entriesAdded = 0;
            String filename = null;

            string Subdir = Path.Combine(TopLevelDir, "A");
            Directory.CreateDirectory(Subdir);

            int fileCount = _rnd.Next(10) + 10;
            for (int j = 0; j < fileCount; j++)
            {
                filename = Path.Combine(Subdir, String.Format("file{0:D3}.txt", j));
                TestUtilities.CreateAndFillFileText(filename, _rnd.Next(34000) + 5000);
                entriesAdded++;
            }

            using (ZipFile zip = new ZipFile())
            {
                zip.ForceNoCompression = true;
                zip.AddDirectory(Subdir, Path.GetFileName(Subdir));
                zip.Comment = CommentOnArchive;
                zip.Save(ZipFileToCreate);
            }

            int entriesFound = 0;
            using (ZipFile zip = ZipFile.Read(ZipFileToCreate))
            {
                foreach (ZipEntry e in zip)
                {
                    if (!e.IsDirectory) entriesFound++;
                    Assert.AreEqual<short>(0, e.CompressionMethod, "Unexpected compression method on zipped entry.");
                }
            }
            Assert.AreEqual<int>(entriesAdded, entriesFound,
             "The created Zip file has an unexpected number of entries.");
        }

        [TestMethod]
        public void CreateZip_ForceNoCompressionSomeEntries()
        {
            string ZipFileToCreate = Path.Combine(TopLevelDir, "ForceNoCompression.zip");
            Assert.IsFalse(File.Exists(ZipFileToCreate), "The temporary zip file '{0}' already exists.", ZipFileToCreate);

            int entriesAdded = 0;
            String filename = null;

            string Subdir = Path.Combine(TopLevelDir, "A");
            Directory.CreateDirectory(Subdir);

            int fileCount = _rnd.Next(13) + 13;
            for (int j = 0; j < fileCount; j++)
            {
                if (_rnd.Next(2) == 0)
                {
                    filename = Path.Combine(Subdir, String.Format("file{0:D3}.txt", j));
                    TestUtilities.CreateAndFillFileText(filename, _rnd.Next(34000) + 5000);
                }
                else
                {
                    filename = Path.Combine(Subdir, String.Format("file{0:D3}.bin", j));
                    TestUtilities.CreateAndFillFileBinary(filename, _rnd.Next(34000) + 5000);
                }
                entriesAdded++;
            }

            Directory.SetCurrentDirectory(TopLevelDir);

            using (ZipFile zip = new ZipFile())
            {
                String[] filenames = Directory.GetFiles("A");
                foreach (String f in filenames)
                {
                    ZipEntry e = zip.AddFile(f, "");
                    if (f.EndsWith(".bin"))
                        e.CompressionMethod = 0x0;
                }
                zip.Comment = "Some of these files do not use compression.";
                zip.Save(ZipFileToCreate);
            }

            int entriesFound = 0;
            using (ZipFile zip = ZipFile.Read(ZipFileToCreate))
            {
                foreach (ZipEntry e in zip)
                {
                    if (!e.IsDirectory) entriesFound++;
                    if (e.FileName.EndsWith(".txt"))
                        Assert.AreEqual<short>(0x08, e.CompressionMethod, "Unexpected compression method on zipped text file.");
                    else
                        Assert.AreEqual<short>(0x00, e.CompressionMethod, "Unexpected compression method on zipped binary file.");
                }
            }
            Assert.AreEqual<int>(entriesAdded, entriesFound,
             "The created Zip file has an unexpected number of entries.");
        }


    }
}
