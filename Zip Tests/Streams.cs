// Streams.cs
// ------------------------------------------------------------------
//
// Copyright (c) 2009 Dino Chiesa 
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
// Time-stamp: <2009-October-08 14:43:08>
//
// ------------------------------------------------------------------
//
// This module defines tests for Streams interfaces into DotNetZip.  
// ZipOutputStream, ZipInputStream, etc
//
// ------------------------------------------------------------------

using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Ionic.Zip;
using Ionic.Zip.Tests.Utilities;


namespace Ionic.Zip.Tests.Streams
{
    /// <summary>
    /// Summary description for StreamsTests
    /// </summary>
    [TestClass]
    public class StreamsTests : IonicTestClass
    {
        public StreamsTests() : base() { }


        // Use TestCleanup to run code after each test has run
        [TestCleanup()]
        public void MyTestCleanupEx()
        {
        }

        EncryptionAlgorithm[] crypto =
            {
                EncryptionAlgorithm.None, 
                EncryptionAlgorithm.PkzipWeak,
                EncryptionAlgorithm.WinZipAes128,
                EncryptionAlgorithm.WinZipAes256,
            };

#if NOT
        EncryptionAlgorithm[] cryptoNoPkzip = 
            {
                EncryptionAlgorithm.None, 
                EncryptionAlgorithm.WinZipAes128,
                EncryptionAlgorithm.WinZipAes256,
            };
#endif

        Ionic.Zlib.CompressionLevel[] compLevels =
            {
                Ionic.Zlib.CompressionLevel.None,
                Ionic.Zlib.CompressionLevel.BestSpeed,
                Ionic.Zlib.CompressionLevel.Default,
                Ionic.Zlib.CompressionLevel.BestCompression,
            };

        Zip64Option[] z64 =
            {
                Zip64Option.Never,
                Zip64Option.AsNecessary, 
                Zip64Option.Always,
            };


        private string _sevenZip = null;
        private string sevenZip
        {
            get
            {
               if (_sevenZip == null)
               {
                   string testBin = TestUtilities.GetTestBinDir(CurrentDir);
                   _sevenZip = Path.Combine(testBin, "Resources\\7z.exe");
                   Assert.IsTrue(File.Exists(_sevenZip), "exe ({0}) does not exist", _sevenZip);
               }
               return _sevenZip;
            }
        }


        private string _wzzip = null;
        private string wzzip
        {
            get
            {
               if (_wzzip == null)
               {
                string progfiles = System.Environment.GetEnvironmentVariable("ProgramFiles");
                _wzzip = Path.Combine(progfiles, "winzip\\wzzip.exe");
                Assert.IsTrue(File.Exists(_wzzip), "exe ({0}) does not exist", _wzzip);
               }
               return _wzzip;
            }
        }
        


        [TestMethod]
        public void ReadZip_OpenReader()
        {
            string[] passwords = { null, Path.GetRandomFileName(), "EE", "***()" };

            for (int j = 0; j < compLevels.Length; j++)
            {
                for (int k = 0; k < passwords.Length; k++)
                {
                    string zipFileToCreate = Path.Combine(TopLevelDir, String.Format("ReadZip_OpenReader-{0}-{1}.zip", j, k));

                    int entriesAdded = 0;
                    String filename = null;

                    string subdir = Path.Combine(TopLevelDir, String.Format("A{0}{1}", j, k));
                    Directory.CreateDirectory(subdir);

                    int fileCount = _rnd.Next(10) + 10;
                    for (int i = 0; i < fileCount; i++)
                    {
                        filename = Path.Combine(subdir, String.Format("file{0:D2}.txt", i));
                        int filesize = _rnd.Next(34000) + 5000;
                        TestUtilities.CreateAndFillFileText(filename, filesize);
                        entriesAdded++;
                    }

                    using (ZipFile zip1 = new ZipFile())
                    {
                        zip1.CompressionLevel = compLevels[j];
                        zip1.Password = passwords[k];
                        zip1.AddDirectory(subdir, Path.GetFileName(subdir));
                        zip1.Save(zipFileToCreate);
                    }

                    // Verify the files are in the zip
                    Assert.AreEqual<int>(TestUtilities.CountEntries(zipFileToCreate), entriesAdded,
                                         String.Format("Trial {0}-{1}: The Zip file has the wrong number of entries.", j, k));

                    // now extract the files and verify their contents
                    using (ZipFile zip2 = ZipFile.Read(zipFileToCreate))
                    {

                        for (int i = 0; i < 3; i++)
                        {
                            // try once with Password set on ZipFile,
                            // another with password on the entry, and
                            // a third time with password passed into the OpenReader() method.
                            if (i == 0)
                                zip2.Password = passwords[k];

                            foreach (string eName in zip2.EntryFileNames)
                            {
                                ZipEntry e1 = zip2[eName];

                                if (!e1.IsDirectory)
                                {

                                    Ionic.Zlib.CrcCalculatorStream s = null;
                                    try
                                    {
                                        if (i == 0)
                                            s = e1.OpenReader();
                                        else if (i == 1)
                                            s = e1.OpenReader(passwords[k]);
                                        else
                                        {
                                            e1.Password = passwords[k];
                                            s = e1.OpenReader();
                                        }
                                        string outFile = Path.Combine(TopLevelDir, String.Format("{0}.{1}.out", eName, i));
                                        using (var output = File.Create(outFile))
                                        {
                                            byte[] buffer = new byte[4096];
                                            int n, totalBytesRead = 0;
                                            do
                                            {
                                                n = s.Read(buffer, 0, buffer.Length);
                                                totalBytesRead += n;
                                                output.Write(buffer, 0, n);
                                            } while (n > 0);

                                            output.Flush();
                                            output.Close();
                                            TestContext.WriteLine("CRC expected({0:X8}) actual({1:X8})",
                                                                  e1.Crc, s.Crc);

                                            Assert.AreEqual<Int32>(s.Crc, e1.Crc,
                                                                   string.Format("The Entry {0} failed the CRC Check.", eName));

                                            Assert.AreEqual<Int32>(totalBytesRead, (int)e1.UncompressedSize,
                                                                   string.Format("We read an unexpected number of bytes. ({0})", eName));
                                        }
                                    }
                                    finally
                                    {
                                        if (s != null)
                                            s.Close();
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }



        
        [TestMethod]
        public void AddEntry_JitProvided()
        {
            for (int i = 0; i < crypto.Length; i++)
            {
                for (int k = 0; k < compLevels.Length; k++)
                {
                    string zipFileToCreate = Path.Combine(TopLevelDir, String.Format("AddEntry_JitProvided.{0}.{1}.zip", i, k));

                    Directory.SetCurrentDirectory(TopLevelDir);
                    string dirToZip = Path.GetFileNameWithoutExtension(Path.GetRandomFileName());
                    var files = TestUtilities.GenerateFilesFlat(dirToZip);

                    string password = Path.GetRandomFileName();

                    using (var zip = new ZipFile())
                    {
                        TestContext.WriteLine("=================================");
                        TestContext.WriteLine("Creating {0}...", Path.GetFileName(zipFileToCreate));
                        TestContext.WriteLine("Encryption({0})  Compression({1})  pw({2})",
                                              crypto[i].ToString(), compLevels[k].ToString(), password);

                        zip.Password = password;
                        zip.Encryption = crypto[i];
                        zip.CompressionLevel = compLevels[k];

                        foreach (var file in files)
                            zip.AddEntry(file,
                                         (name) => File.Open(name, FileMode.Open, FileAccess.Read, FileShare.ReadWrite),
                                         (name, stream) => stream.Close()
                                         );
                        zip.Save(zipFileToCreate);
                    }

                    if (crypto[i] == EncryptionAlgorithm.None)
                        WinzipVerify(zipFileToCreate);
                    else
                        WinzipVerify(zipFileToCreate, password);

                    Assert.AreEqual<int>(files.Length, TestUtilities.CountEntries(zipFileToCreate),
                                         "Trial ({0},{1}): The zip file created has the wrong number of entries.", i, k);
                }
            }
        }



        private delegate void TestCompressionLevels(string[] files, EncryptionAlgorithm crypto, bool seekable, int cycle, string format);

        [TestMethod]
        public void AddEntry_WriteDelegate()
        {
            _TestDriver(new TestCompressionLevels(_Internal_AddEntry_WriteDelegate), "WriteDelegate", true, false);
        }


        [TestMethod]
        public void AddEntry_WriteDelegate_NonSeekable()
        {
            _TestDriver(new TestCompressionLevels(_Internal_AddEntry_WriteDelegate), "WriteDelegate", false, false);
        }


        [TestMethod]
        public void AddEntry_WriteDelegate_ZeroBytes_wi8931()
        {
            _TestDriver(new TestCompressionLevels(_Internal_AddEntry_WriteDelegate), "WriteDelegate", true, true);
        }


        private void _TestDriver(TestCompressionLevels test, string label, bool seekable, bool zero)
        {
            Directory.SetCurrentDirectory(TopLevelDir);


            int[] fileCounts = new int[] { 1, 2, _rnd.Next(4) + 3, _rnd.Next(14) + 13 };

            for (int j = 0; j < fileCounts.Length; j++)
            {
                string dirToZip = String.Format("subdir{0}", j);
                string[] files = null;
                if (zero)
                {
                    // zero length files
                    Directory.CreateDirectory(dirToZip);
                    files = new string[fileCounts[j]];
                    for (int i = 0; i < fileCounts[j]; i++)
                        files[i] = TestUtilities.CreateUniqueFile("zerolength", dirToZip);
                }
                else
                    files = TestUtilities.GenerateFilesFlat(dirToZip, fileCounts[j], 40000, 72000);


                for (int i = 0; i < crypto.Length; i++)
                {
                    string format = String.Format("{0}.{1}.filecount{2}.Encryption.{3}.{4}seekable.{5}.zip",
                                                  label,
                                                  (zero) ? "ZeroBytes" : "regular",
                                                  fileCounts[j],
                                                  crypto[i].ToString(),
                                                  seekable ? "" : "non",
                                                  "{0}");

                    test(files, crypto[i], seekable, i, format);
                }
            }
        }



        private void _Internal_AddEntry_WriteDelegate(string[] files,
                                                      EncryptionAlgorithm crypto,
                                                      bool seekable,
                                                      int cycle,
                                                      string format)
        {
            int BufferSize = 2048;


            for (int k = 0; k < compLevels.Length; k++)
            {
                string zipFileToCreate = Path.Combine(TopLevelDir, String.Format(format, k));
                string password = Path.GetRandomFileName();

                using (var zip = new ZipFile())
                {
                    TestContext.WriteLine("=================================");
                    TestContext.WriteLine("Creating {0}...", Path.GetFileName(zipFileToCreate));
                    TestContext.WriteLine("Encryption({0})  Compression({1})  pw({2})",
                                          crypto.ToString(), compLevels[k].ToString(), password);

                    zip.Password = password;
                    zip.Encryption = crypto;
                    zip.CompressionLevel = compLevels[k];

                    foreach (var file in files)
                    {
                        zip.AddEntry(file, (name, output) =>
                            {
                                using (var input = File.Open(name, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                                {
                                    byte[] buffer = new byte[BufferSize];
                                    int n;
                                    while ((n = input.Read(buffer, 0, buffer.Length)) != 0)
                                    {
                                        output.Write(buffer, 0, n);
                                    }
                                }
                            });
                    }


                    if (!seekable)
                    {
                        // conditionally use a non-seekable output stream
                        using (Stream raw = File.Open(zipFileToCreate, FileMode.Create, FileAccess.ReadWrite))
                        {
                            using (var ns = new Ionic.Zip.Tests.NonSeekableOutputStream(raw))
                            {
                                zip.Save(ns);
                            }
                        }
                    }
                    else
                        zip.Save(zipFileToCreate);
                }

                WinzipVerify(zipFileToCreate, password);

                Assert.AreEqual<int>(files.Length, TestUtilities.CountEntries(zipFileToCreate),
                                     "Trial ({0},{1}): The zip file created has the wrong number of entries.", cycle, k);
            }
        }



        [TestMethod]
        [ExpectedException(typeof(ZipException))]
        public void Create_ZipOutputStream_ZeroBytes_NonSeekable()
        {
            Directory.SetCurrentDirectory(TopLevelDir);

            string dirToZip = Path.GetFileNameWithoutExtension(Path.GetRandomFileName());
            Directory.CreateDirectory(dirToZip);
            int fileCount = 3;
            string[] files = new string[fileCount];
            for (int i = 0; i < fileCount; i++)
                files[i] = TestUtilities.CreateUniqueFile("zerolength", dirToZip);

            _Internal_Create_ZipOutputStream(files, EncryptionAlgorithm.PkzipWeak, false, 99,
                                             "ZipOutputStream.ZeroBytes.Nonseekable.PkzipWeak.{0}.zip");
        }



        [TestMethod]
        [ExpectedException(typeof(System.InvalidOperationException))]
        public void Create_ZipOutputStream_WriteBeforePutNextEntry()
        {
            string zipFileToCreate = "Create_ZipOutputStream_WriteBeforePutNextEntry.zip";
            using (FileStream fs = File.Open(zipFileToCreate, FileMode.Create, FileAccess.ReadWrite))
            {
                using (var output = new ZipOutputStream(fs))
                {
                    //output.PutNextEntry("entry1.txt");
                    byte[] buffer = Encoding.ASCII.GetBytes("This is the content for entry #1.");
                    output.Write(buffer, 0, buffer.Length);
                }
            }
        }


        
                
        [TestMethod]
        public void Create_ZipOutputStream_EmptyEntries()
        {
            Directory.SetCurrentDirectory(TopLevelDir);
            for (int i = 0; i < crypto.Length; i++)
            {
                for (int j = 0; j < compLevels.Length; j++)
                {
                    string password = Path.GetRandomFileName();

                    for (int k = 0; k < 2; k++)
                    {

                        string zipFileToCreate = String.Format("Create_ZipOutputStream_EmptyEntries.Encryption.{0}.{1}.{2}.zip",
                                                              crypto[i].ToString(), compLevels[j].ToString(), k);

                        using (FileStream fs = File.Open(zipFileToCreate, FileMode.Create, FileAccess.ReadWrite))
                        {
                            using (var output = new ZipOutputStream(fs))
                            {
                                byte[] buffer;
                                output.Password = password;
                                output.Encryption = crypto[i];
                                output.CompressionLevel = compLevels[j];
                                output.PutNextEntry("entry1.txt");
                                if (k == 0)
                                {
                                    buffer = Encoding.ASCII.GetBytes("This is the content for entry #1.");
                                    output.Write(buffer, 0, buffer.Length);
                                }

                                output.PutNextEntry("entry2.txt");  // this will be zero length
                                output.PutNextEntry("entry3.txt");
                                if (k == 0)
                                {
                                    buffer = Encoding.ASCII.GetBytes("This is the content for entry #3.");
                                    output.Write(buffer, 0, buffer.Length);
                                }
                                output.PutNextEntry("entry4.txt");  // this will be zero length
                                output.PutNextEntry("entry5.txt");  // this will be zero length
                            }
                        }

                        WinzipVerify(zipFileToCreate, password);

                        Assert.AreEqual<int>(5, TestUtilities.CountEntries(zipFileToCreate),
                                             "Trial ({0},{1}): The zip file created has the wrong number of entries.", i, j);
                    }
                }

            }
        }




        [TestMethod]
        [ExpectedException(typeof(System.ArgumentException))]
        public void Create_ZipOutputStream_DuplicateEntry()
        {
            string zipFileToCreate = "Create_ZipOutputStream_DuplicateEntry.zip";

            string entryName = Path.GetRandomFileName();

            using (FileStream fs = File.Open(zipFileToCreate, FileMode.Create, FileAccess.ReadWrite))
            {
                using (var output = new ZipOutputStream(fs))
                {
                    output.PutNextEntry(entryName);
                    output.PutNextEntry(entryName);
                }
            }
        }



        [TestMethod]
        public void Create_ZipOutputStream()
        {
            _TestDriver(new TestCompressionLevels(_Internal_Create_ZipOutputStream), "ZipOutputStream", true, false);
        }

        [TestMethod]
        public void Create_ZipOutputStream_NonSeekable()
        {
            _TestDriver(new TestCompressionLevels(_Internal_Create_ZipOutputStream), "ZipOutputStream", false, false);
        }



        [TestMethod]
        public void Create_ZipOutputStream_ZeroLength_wi8933()
        {
            _TestDriver(new TestCompressionLevels(_Internal_Create_ZipOutputStream), "ZipOutputStream", true, true);
        }



        private void _Internal_Create_ZipOutputStream(string[] files,
                                                      EncryptionAlgorithm crypto,
                                                      bool seekable,
                                                      int cycle,
                                                      string format)
        {
            int BufferSize = 2048;

            for (int k = 0; k < compLevels.Length; k++)
            {
                string zipFileToCreate = Path.Combine(TopLevelDir, String.Format(format, k));
                string password = Path.GetRandomFileName();

                Stream raw = File.Open(zipFileToCreate, FileMode.Create, FileAccess.ReadWrite);

                // conditionally use a non-seekable output stream
                if (!seekable)
                    raw = new Ionic.Zip.Tests.NonSeekableOutputStream(raw);

                using (raw)
                {
                    TestContext.WriteLine("=================================");
                    TestContext.WriteLine("Creating {0}...", Path.GetFileName(zipFileToCreate));
                    TestContext.WriteLine("Encryption({0})  Compression({1})  pw({2})",
                                          crypto.ToString(), compLevels[k].ToString(), password);

                    using (var output = new ZipOutputStream(raw))
                    {
                        if (crypto != EncryptionAlgorithm.None)
                        {
                            output.Password = password;
                            output.Encryption = crypto;
                        }
                        output.CompressionLevel = compLevels[k];

                        foreach (var file in files)
                        {
                            TestContext.WriteLine("file: {0}", file);

                            output.PutNextEntry(file);
                            using (var input = File.Open(file, FileMode.Open, FileAccess.Read, FileShare.Read | FileShare.Write))
                            {
                                byte[] buffer = new byte[BufferSize];
                                int n;
                                while ((n = input.Read(buffer, 0, buffer.Length)) > 0)
                                {
                                    output.Write(buffer, 0, n);
                                }
                            }
                        }

                    }
                }

                WinzipVerify(zipFileToCreate, password);

                Assert.AreEqual<int>(files.Length, TestUtilities.CountEntries(zipFileToCreate),
                                     "Trial ({0},{1}): The zip file created has the wrong number of entries.", cycle, k);
            }
        }




        [TestMethod]
        public void Streams_7z_Zip_ZeroLength()
        {
            _Internal_Streams_7z_Zip(0, "zero");
        }

        [TestMethod]
        public void Streams_7z_Zip()
        {
            _Internal_Streams_7z_Zip(1, "nonzero");
        }

        [TestMethod]
        public void Streams_7z_Zip_Mixed()
        {
            _Internal_Streams_7z_Zip(2, "mixed");
        }
        
        [TestMethod]
        public void Streams_Winzip_Zip_Mixed_Password()
        {
            string password = Path.GetFileNameWithoutExtension(Path.GetRandomFileName());
            _Internal_Streams_WinZip_Zip(2, password, "mixed");
        }

        [TestMethod]
        public void Streams_Winzip_Zip()
        {
            _Internal_Streams_WinZip_Zip(1, null, "nonzero");
        }

        private string CreateZeroLengthFile(int ix, string directory)
        {
            string nameOfFileToCreate = Path.Combine(directory, String.Format("ZeroLength{0:D4}.txt", ix));
            using (var fs = File.Create(nameOfFileToCreate)) { }
            return nameOfFileToCreate;
        }

        
        public void _Internal_Streams_7z_Zip(int flavor, string label)
        {
            Directory.SetCurrentDirectory(TopLevelDir);

            int[] fileCounts = { 1, 2, _rnd.Next(8) + 6, _rnd.Next(18) + 16, _rnd.Next(48) + 56 } ;

            for (int m=0; m < fileCounts.Length; m++)
            {
                string dirToZip = String.Format("trial{0:D2}",m);
                if (!Directory.Exists(dirToZip)) Directory.CreateDirectory(dirToZip);

                int fileCount = fileCounts[m];
                string zipFileToCreate = Path.Combine(TopLevelDir,
                                                      String.Format("Streams_7z_Zip.{0}.{1}.{2}.zip", flavor, label, m));
            
                string[] files = null;
                if (flavor == 0)
                {
                    // zero length files
                    files = new string[fileCount];
                    for (int i = 0; i < fileCount; i++)
                        files[i] = CreateZeroLengthFile(i,dirToZip);
                }
                else if (flavor == 1)
                    files = TestUtilities.GenerateFilesFlat(dirToZip, fileCount, 100, 72000);
                else
                {
                    // mixed
                    files = new string[fileCount];
                    for (int i = 0; i < fileCount; i++)
                    {
                        if (_rnd.Next(3) == 0)
                            files[i] = CreateZeroLengthFile(i,dirToZip);
                        else
                        {
                            files[i] = Path.Combine(dirToZip, String.Format("nonzero{0:D4}.txt", i));
                            TestUtilities.CreateAndFillFileText(files[i], _rnd.Next(60000)+100);
                        }
                    }
                }

                // Create the zip archive via 7z.exe
                this.Exec(sevenZip, String.Format("a {0} {1}", zipFileToCreate, dirToZip));

                // Verify the number of files in the zip
                Assert.AreEqual<int>(TestUtilities.CountEntries(zipFileToCreate), files.Length,
                                     "Incorrect number of entries in the zip file.");

                // extract the files
                string extractDir = String.Format("extract{0:D2}",m);
                byte[] buffer= new byte[2048];
                int n;
                using (var raw = File.Open(zipFileToCreate, FileMode.Open, FileAccess.Read ))
                {
                    using (var input= new ZipInputStream(raw))
                    {
                        ZipEntry e;
                        while (( e = input.GetNextEntry()) != null)
                        {
                            TestContext.WriteLine("entry: {0}", e.FileName);

                            string outputPath = Path.Combine(extractDir, e.FileName);

                            if (e.IsDirectory)
                            {
                                // create the directory
                                Directory.CreateDirectory(outputPath);
                            }
                            else
                            {
                                // create the file
                                using (var output = File.Open(outputPath, FileMode.Create, FileAccess.ReadWrite ))
                                {
                                    while ((n= input.Read(buffer,0,buffer.Length)) > 0)
                                    {
                                        output.Write(buffer,0,n);
                                    }
                                }
                            }

                            // we don't set the timestamps or attributes on the file/directory.                        
                        }
                    }
                }

                // winzip does not include the base path in the filename;
                // 7zip does. 
                string[] filesUnzipped = Directory.GetFiles(Path.Combine(extractDir, dirToZip));

                // Verify the number of files extracted
                Assert.AreEqual<int>(files.Length, filesUnzipped.Length,
                                     "Incorrect number of files extracted.");
            }
        }



        public void _Internal_Streams_WinZip_Zip(int flavor, string password, string label)
        {
            Directory.SetCurrentDirectory(TopLevelDir);

            int[] fileCounts = { 1, 2, _rnd.Next(8) + 6, _rnd.Next(18) + 16, _rnd.Next(48) + 56 } ;

            for (int m=0; m < fileCounts.Length; m++)
            {
                string dirToZip = String.Format("trial{0:D2}",m);
                if (!Directory.Exists(dirToZip)) Directory.CreateDirectory(dirToZip);

                int fileCount = fileCounts[m];
                string zipFileToCreate = Path.Combine(TopLevelDir,
                                                      String.Format("Streams_Winzip_Zip.{0}.{1}.{2}.zip", flavor, label, m));
            
                string[] files = null;
                if (flavor == 0)
                {
                    // zero length files
                    files = new string[fileCount];
                    for (int i = 0; i < fileCount; i++)
                        files[i] = CreateZeroLengthFile(i,dirToZip);
                }
                else if (flavor == 1)
                    files = TestUtilities.GenerateFilesFlat(dirToZip, fileCount, 100, 72000);
                else
                {
                    // mixed
                    files = new string[fileCount];
                    for (int i = 0; i < fileCount; i++)
                    {
                        if (_rnd.Next(3) == 0)
                            files[i] = CreateZeroLengthFile(i,dirToZip);
                        else
                        {
                            files[i] = Path.Combine(dirToZip, String.Format("nonzero{0:D4}.txt", i));
                            TestUtilities.CreateAndFillFileText(files[i], _rnd.Next(60000)+100);
                        }
                    }
                }

                // Create the zip archive via WinZip.exe
                string pwdOption = String.IsNullOrEmpty(password) ? "" : "-s"+password;
                string formatString = "-a -p {0} -yx {1} {2}\\*.*";
                string wzzipOut = this.Exec(wzzip, String.Format(formatString, pwdOption, zipFileToCreate, dirToZip));

                
                // Verify the number of files in the zip
                Assert.AreEqual<int>(TestUtilities.CountEntries(zipFileToCreate), files.Length,
                                     "Incorrect number of entries in the zip file.");

                // extract the files
                string extractDir = String.Format("extract{0:D2}",m);
                            Directory.CreateDirectory(extractDir);
                byte[] buffer= new byte[2048];
                int n;
                using (var raw = File.Open(zipFileToCreate, FileMode.Open, FileAccess.Read ))
                {
                    using (var input= new ZipInputStream(raw))
                    {
                        input.Password = password;
                        ZipEntry e;
                        while (( e = input.GetNextEntry()) != null)
                        {
                            TestContext.WriteLine("entry: {0}", e.FileName);

                            string outputPath = Path.Combine(extractDir, e.FileName);

                            if (e.IsDirectory)
                            {
                                // create the directory
                                Directory.CreateDirectory(outputPath);
                            }
                            else
                            {
                                // create the file
                                using (var output = File.Open(outputPath, FileMode.Create, FileAccess.ReadWrite ))
                                {
                                    while ((n= input.Read(buffer,0,buffer.Length)) > 0)
                                    {
                                        output.Write(buffer,0,n);
                                    }
                                }
                            }

                            // we don't set the timestamps or attributes on the file/directory.                        
                        }
                    }
                }

                // winzip does not include the base path in the filename;
                // 7zip does. 
                string[] filesUnzipped = Directory.GetFiles(extractDir);

                // Verify the number of files extracted
                Assert.AreEqual<int>(files.Length, filesUnzipped.Length,
                                     "Incorrect number of files extracted.");
            }
        }

        
    }
}