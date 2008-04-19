﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Ionic.Utils.Zip
{
    /// <summary>
    /// Issued when an <c>ZipEntry.ExtractWithPassword()</c> method is invoked
    /// with an incorrect password.
    /// </summary>
    public class BadPasswordException : System.Exception
    {
        /// <summary>
        /// Default ctor.
        /// </summary>
        public BadPasswordException() { }

        /// <summary>
        /// Come on, you know how exceptions work. Why are you looking at this documentation?
        /// </summary>
        /// <param name="message">The message in the exception.</param>
        public BadPasswordException(String message)
            : base(message)
        { }
    }

    /// <summary>
    /// Indicates that a read was attempted on a stream, and bad or incomplete data was
    /// received.  
    /// </summary>
    public class BadReadException : System.Exception
    {
        /// <summary>
        /// Default ctor.
        /// </summary>
        public BadReadException () { }

        /// <summary>
        /// Come on, you know how exceptions work. Why are you looking at this documentation?
        /// </summary>
        /// <param name="message">The message in the exception.</param>
        public BadReadException (String message)
            : base(message)
        { }
    }

    
    /// <summary>
    /// Indicates that an operation was attempted on a ZipFile which was not possible
    /// given the state of the instance. For example, if you call <c>Save()</c> on a ZipFile 
    /// which has no filename set, you can get this exception. 
    /// </summary>
    public class BadStateException: System.Exception
    {
        /// <summary>
        /// Default ctor.
        /// </summary>
        public BadStateException() { }

        /// <summary>
        /// Come on, you know how exceptions work. Why are you looking at this documentation?
        /// </summary>
        /// <param name="message">The message in the exception.</param>
        public BadStateException(String message)
            : base(message)
        { }
    }

}
