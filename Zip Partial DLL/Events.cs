using System;
using System.Collections.Generic;
using System.Text;

namespace Ionic.Zip
{
    /// <summary>
    /// Delegate for the callback by which the application gives approval for multiple
    /// reads of the file stream. This callback is called only when the initial 
    /// compression operation inflates the size of the file data. 
    /// </summary>
    public delegate bool ReReadApprovalCallback(Int64 uncompressedSize, Int64 compressedSize, string filename);

    /// <summary>
    /// Delegate for the callback by which the application tells the libraary whether
    /// to use compression on the file or not.  Using this callback, the application can 
    /// specify that previously-compressed files (.mp3, .png, .docx, etc) should 
    /// not be compressed, for example, or can turn on or off compression based on any 
    /// other factor.
    /// </summary>
    public delegate bool WantCompressionCallback(string localFilename, string filenameInArchive);


    /// <summary>
    /// In an EventArgs type, indicates which sort of progress event is being reported.
    /// </summary>
    public enum ZipProgressEventType
    {
        /// <summary>
        /// Indicates that a Read() operation has started.
        /// </summary>
        Reading_Started,

        /// <summary>
        /// Indicates that an individual entry in the archive is about to be read.
        /// </summary>
        Reading_BeforeReadEntry,

        /// <summary>
        /// Indicates that an individual entry in the archive has just been read.
        /// </summary>
        Reading_AfterReadEntry,

        /// <summary>
        /// Indicates that a Read() operation has completed.
        /// </summary>
        Reading_Completed,

        /// <summary>
        /// The given event reports the number of bytes read so far
        /// during a Read() operation.
        /// </summary>
        Reading_ArchiveBytesRead,

        /// <summary>
        /// Indicates that a Save() operation has started.
        /// </summary>
        Saving_Started,

        /// <summary>
        /// Indicates that an individual entry in the archive is about to be written.
        /// </summary>
        Saving_BeforeWriteEntry,

        /// <summary>
        /// Indicates that an individual entry in the archive has just been saved.
        /// </summary>
        Saving_AfterWriteEntry,

        /// <summary>
        /// Indicates that a Save() operation has completed.
        /// </summary>
        Saving_Completed,

        /// <summary>
        /// Indicates that the zip archive has been created in a
        /// temporary location during a Save() operation.
        /// </summary>
        Saving_AfterSaveTempArchive,

        /// <summary>
        /// Indicates that the temporary file is about to be renamed to the final archive 
        /// name during a Save() operation.
        /// </summary>
        Saving_BeforeRenameTempArchive,

        /// <summary>
        /// Indicates that the temporary file is has just been renamed to the final archive 
        /// name during a Save() operation.
        /// </summary>
        Saving_AfterRenameTempArchive,

        /// <summary>
        /// Indicates that the self-extracting archive has been compiled
        /// during a Save() operation.
        /// </summary>
        Saving_AfterCompileSelfExtractor,

        /// <summary>
        /// The given event is reporting the number of source bytes that have run through the compressor so far
        /// during a Save() operation.
        /// </summary>
        Saving_EntryBytesRead,

        /// <summary>
        /// Indicates that an entry is about to be extracted. 
        /// </summary>
        Extracting_BeforeExtractEntry,

        /// <summary>
        /// Indicates that an entry has just been extracted. 
        /// </summary>
        Extracting_AfterExtractEntry,

        /// <summary>
        /// The given event is reporting the number of bytes written so far for the current entry
        /// during an Extract() operation.
        /// </summary>
        Extracting_EntryBytesWritten,

        /// <summary>
        /// Indicates that an ExtractAll operation is about to begin.
        /// </summary>
        Extracting_BeforeExtractAll,

        /// <summary>
        /// Indicates that an ExtractAll operation has completed.
        /// </summary>
        Extracting_AfterExtractAll,
    }


    /// <summary>
    /// Provides information about the progress of a save or extract operation.
    /// </summary>
    public class ZipProgressEventArgs : EventArgs
    {
        private int _entriesTotal;
        private bool _cancel;
        private ZipEntry _latestEntry;
        private ZipProgressEventType _flavor;
        private String _archiveName;
        private int _bytesTransferred;
        private Int64 _totalBytesToTransfer;


        internal ZipProgressEventArgs() { }

        internal ZipProgressEventArgs(string archiveName, ZipProgressEventType flavor)
        {
            this._archiveName = archiveName;
            this._flavor = flavor;
        }

        /// <summary>
        /// The total number of entries to be saved or extracted.
        /// </summary>
        public int EntriesTotal
        {
            get { return _entriesTotal; }
            set { _entriesTotal = value; }
        }

        /// <summary>
        /// The name of the last entry saved or extracted.
        /// </summary>
        public ZipEntry CurrentEntry
        {
            get { return _latestEntry; }
            set { _latestEntry = value; }
        }

        /// <summary>
        /// In an event handler, set this to cancel the save or extract 
        /// operation that is in progress.
        /// </summary>
        public bool Cancel
        {
            get { return _cancel; }
            set { _cancel = _cancel || value; }
        }

        /// <summary>
        /// The type of event being reported.
        /// </summary>
        public ZipProgressEventType EventType
        {
            get { return _flavor; }
            set { _flavor = value; }
        }

        /// <summary>
        /// Returns the archive name.
        /// </summary>
        public String ArchiveName
        {
            get { return _archiveName; }
            set { _archiveName = value; }
        }


        /// <summary>
        /// The number of bytes read or written so far for this entry.  
        /// </summary>
        public int BytesTransferred
        {
            get { return _bytesTransferred; }
            set { _bytesTransferred = value; }
        }



        /// <summary>
        /// Total number of bytes that will be read or written for this entry.
        /// </summary>
        public Int64 TotalBytesToTransfer
        {
            get { return _totalBytesToTransfer; }
            set { _totalBytesToTransfer = value; }
        }
    }



    /// <summary>
    /// Provides information about the progress of a Read operation.
    /// </summary>
    public class ReadProgressEventArgs : ZipProgressEventArgs
    {

        internal ReadProgressEventArgs() { }

        private ReadProgressEventArgs(string archiveName, ZipProgressEventType flavor)
            : base(archiveName, flavor)
        { }

        internal static ReadProgressEventArgs Before(string archiveName, int entriesTotal)
        {
            var x = new ReadProgressEventArgs(archiveName, ZipProgressEventType.Reading_BeforeReadEntry);
            x.EntriesTotal = entriesTotal;
            return x;
        }

        internal static ReadProgressEventArgs After(string archiveName, ZipEntry entry, int entriesTotal)
        {
            var x = new ReadProgressEventArgs(archiveName, ZipProgressEventType.Reading_AfterReadEntry);
            x.EntriesTotal = entriesTotal;
            x.CurrentEntry = entry;
            return x;
        }

        internal static ReadProgressEventArgs Started(string archiveName)
        {
            var x = new ReadProgressEventArgs(archiveName, ZipProgressEventType.Reading_Started);
            return x;
        }

        internal static ReadProgressEventArgs ByteUpdate(string archiveName, ZipEntry entry, int bytesXferred, int totalBytes)
        {
            var x = new ReadProgressEventArgs(archiveName, ZipProgressEventType.Reading_ArchiveBytesRead);
            x.CurrentEntry = entry;
            x.BytesTransferred = bytesXferred;
            x.TotalBytesToTransfer = totalBytes;
            return x;
        }

        internal static ReadProgressEventArgs Completed(string archiveName)
        {
            var x = new ReadProgressEventArgs(archiveName, ZipProgressEventType.Reading_Completed);
            return x;
        }

    }

    /// <summary>
    /// Provides information about the progress of a save operation.
    /// </summary>
    public class SaveProgressEventArgs : ZipProgressEventArgs
    {
        private int _entriesSaved;

        /// <summary>
        /// Constructor for the SaveProgressEventArgs.
        /// </summary>
        /// <param name="archiveName">the name of the zip archive.</param>
        /// <param name="before">whether this is before saving the entry, or after</param>
        /// <param name="entriesTotal">The total number of entries in the zip archive.</param>
        /// <param name="entriesSaved">Number of entries that have been saved.</param>
        /// <param name="entry">The entry involved in the event.</param>
        internal SaveProgressEventArgs(string archiveName, bool before, int entriesTotal, int entriesSaved, ZipEntry entry)
            : base(archiveName, (before) ? ZipProgressEventType.Saving_BeforeWriteEntry : ZipProgressEventType.Saving_AfterWriteEntry)
        {
            this.EntriesTotal = entriesTotal;
            this.CurrentEntry = entry;
            this._entriesSaved = entriesSaved;
        }

        internal SaveProgressEventArgs() { }

        internal SaveProgressEventArgs(string archiveName, ZipProgressEventType flavor)
            : base(archiveName, flavor)
        { }


        internal static SaveProgressEventArgs ByteUpdate(string archiveName, ZipEntry entry, int bytesXferred, int totalBytes)
        {
            var x = new SaveProgressEventArgs(archiveName, ZipProgressEventType.Saving_EntryBytesRead);
            x.ArchiveName = archiveName;
            x.CurrentEntry = entry;
            x.BytesTransferred = bytesXferred;
            x.TotalBytesToTransfer = totalBytes;
            return x;
        }

        internal static SaveProgressEventArgs Started(string archiveName)
        {
            var x = new SaveProgressEventArgs(archiveName, ZipProgressEventType.Saving_Started);
            return x;
        }

        internal static SaveProgressEventArgs Completed(string archiveName)
        {
            var x = new SaveProgressEventArgs(archiveName, ZipProgressEventType.Saving_Completed);
            return x;
        }

        /// <summary>
        /// Number of entries saved so far.
        /// </summary>
        public int EntriesSaved
        {
            get { return _entriesSaved; }
        }

    }


    /// <summary>
    /// Provides information about the progress of the extract operation.
    /// </summary>
    public class ExtractProgressEventArgs : ZipProgressEventArgs
    {
        private int _entriesExtracted;
        private bool _overwrite;
        private string _target;

        /// <summary>
        /// Constructor for the ExtractProgressEventArgs.
        /// </summary>
        /// <param name="archiveName">the name of the zip archive.</param>
        /// <param name="before">whether this is before saving the entry, or after</param>
        /// <param name="entriesTotal">The total number of entries in the zip archive.</param>
        /// <param name="entriesExtracted">Number of entries that have been extracted.</param>
        /// <param name="entry">The entry involved in the event.</param>
        /// <param name="extractLocation">The location to which entries are extracted.</param>
        /// <param name="wantOverwrite">indicates whether the extract operation will overwrite existing files.</param>
        internal ExtractProgressEventArgs(string archiveName, bool before, int entriesTotal, int entriesExtracted, ZipEntry entry, string extractLocation, bool wantOverwrite)
            : base(archiveName, (before) ? ZipProgressEventType.Extracting_BeforeExtractEntry : ZipProgressEventType.Extracting_AfterExtractEntry)
        {
            this.EntriesTotal = entriesTotal;
            this.CurrentEntry = entry;
            this._entriesExtracted = entriesExtracted;
            this._overwrite = wantOverwrite;
            this._target = extractLocation;
        }

        internal ExtractProgressEventArgs(string archiveName, ZipProgressEventType flavor)
            : base(archiveName, flavor)
        { }

        internal ExtractProgressEventArgs()
        { }


        internal static ExtractProgressEventArgs BeforeExtractEntry(string archiveName, ZipEntry entry, string extractLocation, bool wantOverwrite)
        {
            var x = new ExtractProgressEventArgs();
            x.ArchiveName = archiveName;
            x.EventType = ZipProgressEventType.Extracting_BeforeExtractEntry;
            x.CurrentEntry = entry;
            x._target = extractLocation;
            x._overwrite = wantOverwrite;
            return x;
        }

        internal static ExtractProgressEventArgs AfterExtractEntry(string archiveName, ZipEntry entry, string extractLocation, bool wantOverwrite)
        {
            var x = new ExtractProgressEventArgs();
            x.ArchiveName = archiveName;
            x.EventType = ZipProgressEventType.Extracting_AfterExtractEntry;
            x.CurrentEntry = entry;
            x._target = extractLocation;
            x._overwrite = wantOverwrite;
            return x;
        }

        internal static ExtractProgressEventArgs ExtractAllStarted(string archiveName, string extractLocation, bool wantOverwrite)
        {
            var x = new ExtractProgressEventArgs(archiveName, ZipProgressEventType.Extracting_BeforeExtractAll);
            x._overwrite = wantOverwrite;
            x._target = extractLocation;
            return x;
        }

        internal static ExtractProgressEventArgs ExtractAllCompleted(string archiveName, string extractLocation, bool wantOverwrite)
        {
            var x = new ExtractProgressEventArgs(archiveName, ZipProgressEventType.Extracting_AfterExtractAll);
            x._overwrite = wantOverwrite;
            x._target = extractLocation;
            return x;
        }


        internal static ExtractProgressEventArgs ByteUpdate(string archiveName, ZipEntry entry, int bytesWritten, Int64 totalBytes)
        {
            var x = new ExtractProgressEventArgs(archiveName, ZipProgressEventType.Extracting_EntryBytesWritten);
            x.ArchiveName = archiveName;
            x.CurrentEntry = entry;
            x.BytesTransferred = bytesWritten;
            x.TotalBytesToTransfer = totalBytes;
            return x;
        }



        /// <summary>
        /// Number of entries extracted so far.  This is set only if the 
        /// EventType is Extracting_BeforeExtractEntry or Extracting_AfterExtractEntry, and 
        /// the Extract() is occurring witin the scope of a call to ExtractAll().
        /// </summary>
        public int EntriesExtracted
        {
            get { return _entriesExtracted; }
        }


        /// <summary>
        /// True if the extract operation overwrites existing files.
        /// </summary>
        public bool Overwrite
        {
            get { return _overwrite; }
        }

        /// <summary>
        /// Returns the extraction target location, a filesystem path. 
        /// </summary>
        public String ExtractLocation
        {
            get { return _target; }
        }

    }

}