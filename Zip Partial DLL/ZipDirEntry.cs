#define OPTIMIZE_WI6612

// ZipDirEntry.cs
//
// Copyright (c) 2006, 2007, 2008 Microsoft Corporation.  All rights reserved.
//
// Part of an implementation of a zipfile class library. 
// See the file ZipFile.cs for the license and for further information.
//
// Tue, 27 Mar 2007  15:30


using System;

namespace Ionic.Zip
{
    /// <summary>
    /// This class models an entry in the directory contained within the zip file.
    /// The class is generally not used from within application code, though it is
    /// used by the ZipFile class.
    /// </summary>
    internal class ZipDirEntry
    {
        private ZipDirEntry() { }

        ///// <summary>
        ///// The time at which the file represented by the given entry was last modified.
        ///// </summary>
        //public DateTime LastModified
        //{
        //    get { return _LastModified; }
        //}

        /// <summary>
        /// The filename of the file represented by the given entry.
        /// </summary>
        public string FileName
        {
            get { return _FileName; }
        }

        /// <summary>
        /// Any comment associated to the given entry. Comments are generally optional.
        /// </summary>
        public string Comment
        {
            get { return _Comment; }
        }

        ///// <summary>
        ///// The version of the zip engine this archive was made by.  
        ///// </summary>
        //public Int16 VersionMadeBy
        //{
        //    get { return _VersionMadeBy; }
        //}

        ///// <summary>
        ///// The version of the zip engine this archive can be read by.  
        ///// </summary>
        //public Int16 VersionNeeded
        //{
        //    get { return _VersionNeeded; }
        //}

        ///// <summary>
        ///// The compression method used to generate the archive.  Deflate is our favorite!
        ///// </summary>
        //public Int16 CompressionMethod
        //{
        //    get { return _CompressionMethod; }
        //}

        ///// <summary>
        ///// The size of the file, after compression. This size can actually be 
        ///// larger than the uncompressed file size, for previously compressed 
        ///// files, such as JPG files. 
        ///// </summary>
        //public Int32 CompressedSize
        //{
        //    get { return _CompressedSize; }
        //}

        ///// <summary>
        ///// The size of the file before compression.  
        ///// </summary>
        //public Int32 UncompressedSize
        //{
        //    get { return _UncompressedSize; }
        //}

        /// <summary>
        /// True if the referenced entry is a directory.  
        /// </summary>
        public bool IsDirectory
        {
            get { return ((_InternalFileAttrs == 0) && ((_ExternalFileAttrs & 0x0010) == 0x0010)); }
        }


        ///// <summary>
        ///// The calculated compression ratio for the given file. 
        ///// </summary>
        //public Double CompressionRatio
        //{
        //    get
        //    {
        //        return 100 * (1.0 - (1.0 * CompressedSize) / (1.0 * UncompressedSize));
        //    }
        //}


        //internal ZipDirEntry(ZipEntry ze) { }

#if OPTIMIZE_WI6612
        public ZipEntry AsZipEntry()
        {
            ZipEntry e = new ZipEntry();
            e._Comment = this.Comment;
            e.FileName = this.FileName;
            if (this.IsDirectory) e.MarkAsDirectory();  // may append a slash to filename if nec.
            e._VersionNeeded = _VersionNeeded;
            e._BitField = _BitField;
            e._CompressionMethod = _CompressionMethod;
            e._LastModified = Ionic.Zip.SharedUtilities.PackedToDateTime(this._TimeBlob);

            e._Crc32 = this._Crc32;
            e._CompressedSize = _CompressedSize;
            e._CompressedFileDataSize = _CompressedSize;
            e._UncompressedSize = _UncompressedSize;
            e._RelativeOffsetOfHeader = _RelativeOffsetOfLocalHeader;
            e._LocalFileName = e.FileName;

            // workitem 6898
            if (e._LocalFileName.EndsWith("/")) e.MarkAsDirectory();

            // The length of the "local header" for the ZipEntry is not necessarily the same as
            // the length of the header in the ZipDirEntry, therefore we cannot know the __FileDataPosition 
            // until we read the local header.
            //e._LengthOfHeader = 30 + _filenameLength + _extraFieldLength;

            //e.__FileDataPosition = e._RelativeOffsetOfHeader + 30 + _filenameLength + _extraFieldLength;
            e.__FileDataPosition = 0;

            if ((e._BitField & 0x01) == 0x01)
            {
                e._Encryption = EncryptionAlgorithm.PkzipWeak;
                e._CompressedFileDataSize -= 12;
            }

            // The length of the "local header" for the ZipEntry is not necessarily the same as
            // the length of the header in the ZipDirEntry.  
            //e._LengthOfHeader = 30 + _filenameLength + _extraFieldLength;
            e._LengthOfHeader = 0;  // mark as zero to indicate we need to read later

            return e;
        }
#endif

        /// <summary>
        /// Reads one entry from the zip directory structure in the zip file. 
        /// </summary>
        /// <param name="s">the stream from which to read.</param>
        /// <param name="expectedEncoding">
        /// The text encoding to use if the entry is not marked UTF-8.
        /// </param>
        /// <returns>the entry read from the archive.</returns>
        public static ZipDirEntry Read(System.IO.Stream s, System.Text.Encoding expectedEncoding)
        {
            int signature = Ionic.Zip.SharedUtilities.ReadSignature(s);
            // return null if this is not a local file header signature
            if (ZipDirEntry.IsNotValidSig(signature))
            {
                s.Seek(-4, System.IO.SeekOrigin.Current);

                // Getting "not a ZipDirEntry signature" here is not always wrong or an error. 
                // This can happen when walking through a zipfile.  After the last ZipDirEntry, 
                // we expect to read an EndOfCentralDirectorySignature.  When we get this is how we 
                // know we've reached the end of the central directory. 
                if (signature != ZipConstants.EndOfCentralDirectorySignature &&
                    signature != ZipConstants.Zip64EndOfCentralDirectoryRecordSignature)
                {
                    throw new BadReadException(String.Format("  ZipDirEntry::Read(): Bad signature (0x{0:X8}) at position 0x{1:X8}", signature, s.Position));
                }
                return null;
            }

            int bytesRead = 42 + 4;
            byte[] block = new byte[42];
            int n = s.Read(block, 0, block.Length);
            if (n != block.Length) return null;

            int i = 0;
            ZipDirEntry zde = new ZipDirEntry();

            zde._VersionMadeBy = (short)(block[i++] + block[i++] * 256);
            zde._VersionNeeded = (short)(block[i++] + block[i++] * 256);
            zde._BitField = (short)(block[i++] + block[i++] * 256);
            zde._CompressionMethod = (short)(block[i++] + block[i++] * 256);
            zde._TimeBlob = block[i++] + block[i++] * 256 + block[i++] * 256 * 256 + block[i++] * 256 * 256 * 256;
            zde._Crc32 = block[i++] + block[i++] * 256 + block[i++] * 256 * 256 + block[i++] * 256 * 256 * 256;

            zde._CompressedSize = (uint)(block[i++] + block[i++] * 256 + block[i++] * 256 * 256 + block[i++] * 256 * 256 * 256);
            zde._UncompressedSize = (uint)(block[i++] + block[i++] * 256 + block[i++] * 256 * 256 + block[i++] * 256 * 256 * 256);

            //DateTime lastModified = Ionic.Utils.Zip.SharedUtilities.PackedToDateTime(lastModDateTime);
            //i += 24;

            zde._filenameLength = (short)(block[i++] + block[i++] * 256);
            zde._extraFieldLength = (short)(block[i++] + block[i++] * 256);
            zde._commentLength = (short)(block[i++] + block[i++] * 256);
            //Int16 diskNumber = (short)(block[i++] + block[i++] * 256);
            i += 2;

            zde._InternalFileAttrs = (short)(block[i++] + block[i++] * 256);
            zde._ExternalFileAttrs = block[i++] + block[i++] * 256 + block[i++] * 256 * 256 + block[i++] * 256 * 256 * 256;

            zde._RelativeOffsetOfLocalHeader = (uint)(block[i++] + block[i++] * 256 + block[i++] * 256 * 256 + block[i++] * 256 * 256 * 256);

            block = new byte[zde._filenameLength];
            n = s.Read(block, 0, block.Length);
            bytesRead += n;
            if ((zde._BitField & 0x0800) == 0x0800)
            {
                // UTF-8 is in use
                zde._FileName = Ionic.Zip.SharedUtilities.Utf8StringFromBuffer(block, block.Length);
            }
            else
            {
                zde._FileName = Ionic.Zip.SharedUtilities.StringFromBuffer(block, block.Length, expectedEncoding);
            }


            if (zde._extraFieldLength > 0)
            {
                bool IsZip64Format = ((uint)zde._CompressedSize == 0xFFFFFFFF ||
                      (uint)zde._UncompressedSize == 0xFFFFFFFF ||
                      (uint)zde._RelativeOffsetOfLocalHeader == 0xFFFFFFFF);

                bytesRead += SharedUtilities.ProcessExtraField(zde._extraFieldLength, s, IsZip64Format,
                                           ref zde._Extra,
                                           ref zde._UncompressedSize,
                                           ref zde._CompressedSize,
                                           ref zde._RelativeOffsetOfLocalHeader);
            }
            if (zde._commentLength > 0)
            {
                block = new byte[zde._commentLength];
                n = s.Read(block, 0, block.Length);
                bytesRead += n;
                if ((zde._BitField & 0x0800) == 0x0800)
                {
                    // UTF-8 is in use
                    zde._Comment = Ionic.Zip.SharedUtilities.Utf8StringFromBuffer(block, block.Length);
                }
                else
                {
                    zde._Comment = Ionic.Zip.SharedUtilities.StringFromBuffer(block, block.Length, expectedEncoding);
                }
            }
            zde._LengthOfDirEntry = bytesRead;
            return zde;
        }

        /// <summary>
        /// Returns true if the passed-in value is a valid signature for a ZipDirEntry. 
        /// </summary>
        /// <param name="signature">the candidate 4-byte signature value.</param>
        /// <returns>true, if the signature is valid according to the PKWare spec.</returns>
        internal static bool IsNotValidSig(int signature)
        {
            return (signature != ZipConstants.ZipDirEntrySignature);
        }

        private string _FileName;
        private string _Comment;
        private Int16 _VersionMadeBy;
        private Int16 _VersionNeeded;
        private Int16 _CompressionMethod;
        private Int64 _CompressedSize;
        private Int64 _UncompressedSize;
        private Int64 _RelativeOffsetOfLocalHeader;
        private Int16 _InternalFileAttrs;
        private Int32 _ExternalFileAttrs;
        private Int16 _BitField;
        private Int32 _TimeBlob;
        private Int32 _Crc32;
        private Int32 _LengthOfDirEntry;
        private Int16 _filenameLength;
        private Int16 _extraFieldLength;
        private Int16 _commentLength;
        private byte[] _Extra;
    }


}