// Filter.cs
// ------------------------------------------------------------------
//
// Copyright (c) 2006, 2007, 2008, 2009 Microsoft Corporation.  All rights reserved.
//
// This code is released under the Microsoft Public License . 
// See the License.txt for details.  
//
// ------------------------------------------------------------------
//
// This module implements a "file filter" that finds files based on a set of inclusion criteria,
// including filename, size, file time, and potentially file attributes.
//
// ------------------------------------------------------------------


using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace Ionic
{

    /// <summary>
    /// Enumerates the options for a logical conjunction. This enum is intended for use 
    /// internally by the FileFilter class.
    /// </summary>
    internal enum LogicalConjunction
    {
        NONE,
        AND,
        OR,
    }

    internal enum WhichTime
    {
        atime,
        mtime,
        ctime,
    }


    internal enum ComparisonConstraint
    {
        [Description(">")]
        GreaterThan,
        [Description(">=")]
        GreaterThanOrEqualTo,
        [Description("<")]
        LesserThan,
        [Description("<=")]
        LesserThanOrEqualTo,
        [Description("=")]
        EqualTo,
        [Description("!=")]
        NotEqualTo
    }


    internal abstract partial class FileCriterion
    {
        internal abstract bool Evaluate(string filename);
    }


    internal partial class SizeCriterion : FileCriterion
    {
        internal ComparisonConstraint Constraint;
        internal Int64 Size;

        public override String ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("size ").Append(EnumUtil.GetDescription(Constraint)).Append(" ").Append(Size.ToString());
            return sb.ToString();
        }

        internal override bool Evaluate(string filename)
        {
            System.IO.FileInfo fi = new System.IO.FileInfo(filename);
            return _Evaluate(fi.Length);
        }

        private bool _Evaluate(Int64 Length)
        {
            bool result = false;
            switch (Constraint)
            {
                case ComparisonConstraint.GreaterThanOrEqualTo:
                    result = Length >= Size;
                    break;
                case ComparisonConstraint.GreaterThan:
                    result = Length > Size;
                    break;
                case ComparisonConstraint.LesserThanOrEqualTo:
                    result = Length <= Size;
                    break;
                case ComparisonConstraint.LesserThan:
                    result = Length < Size;
                    break;
                case ComparisonConstraint.EqualTo:
                    result = Length == Size;
                    break;
                case ComparisonConstraint.NotEqualTo:
                    result = Length != Size;
                    break;
                default:
                    throw new ArgumentException("Constraint");
            }
            return result;
        }

    }



    internal partial class TimeCriterion : FileCriterion
    {
        internal ComparisonConstraint Constraint;
        internal WhichTime Which;
        internal DateTime Time;

        public override String ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(Which.ToString()).Append(" ").Append(EnumUtil.GetDescription(Constraint)).Append(" ").Append(Time.ToString("yyyy-MM-dd-HH:mm:ss"));
            return sb.ToString();
        }

        internal override bool Evaluate(string filename)
        {
            System.IO.FileInfo fi = new System.IO.FileInfo(filename);
            DateTime x;
            switch (Which)
            {
                case WhichTime.atime:
                    x = System.IO.File.GetLastAccessTime(filename);
                    break;
                case WhichTime.mtime:
                    x = System.IO.File.GetLastWriteTime(filename);
                    break;
                case WhichTime.ctime:
                    x = System.IO.File.GetCreationTime(filename);
                    break;
                default:
                    throw new ArgumentException("Constraint");
            }
            return _Evaluate(x);
        }


        private bool _Evaluate(DateTime x)
        {

            bool result = false;
            switch (Constraint)
            {
                case ComparisonConstraint.GreaterThanOrEqualTo:
                    result = (x >= Time);
                    break;
                case ComparisonConstraint.GreaterThan:
                    result = (x > Time);
                    break;
                case ComparisonConstraint.LesserThanOrEqualTo:
                    result = (x <= Time);
                    break;
                case ComparisonConstraint.LesserThan:
                    result = (x < Time);
                    break;
                case ComparisonConstraint.EqualTo:
                    result = (x == Time);
                    break;
                case ComparisonConstraint.NotEqualTo:
                    result = (x != Time);
                    break;
                default:
                    throw new ArgumentException("Constraint");
            }

            //Console.WriteLine("TimeCriterion[{2}]({0})= {1}", filename, result, Which.ToString());
            return result;
        }
    }



    internal partial class NameCriterion : FileCriterion
    {
        private Regex _re;
        private String _regexString;
        internal ComparisonConstraint Constraint;
        private string _MatchingFileSpec;
        internal string MatchingFileSpec
        {
            set
            {
                _MatchingFileSpec = value;
                _regexString = Regex.Escape(value).Replace(@"\*", ".*").Replace(@"\?", ".") + "$";
                _re = new Regex(_regexString, RegexOptions.IgnoreCase);
            }
        }


        public override String ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("name = ").Append(_MatchingFileSpec);
            return sb.ToString();
        }


        internal override bool Evaluate(string filename)
        {
            return _Evaluate(filename);
        }

        private bool _Evaluate(string filename)
        {
            String f = System.IO.Path.GetFileName(filename);
            bool result = _re.IsMatch(f);
            if (Constraint != ComparisonConstraint.EqualTo)
                result = !result;
            //Console.WriteLine("NameCriterion[{2}]({0})= {1}", filename, result, _regexString);
            return result;
        }
    }



    internal partial class AttributesCriterion : FileCriterion
    {
        private FileAttributes _Attributes;
        internal ComparisonConstraint Constraint;
        internal string AttributeString
        {
            get
            {
                string result = "";
                if ((_Attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
                    result += "H";
                if ((_Attributes & FileAttributes.System) == FileAttributes.System)
                    result += "S";
                if ((_Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                    result += "R";
                if ((_Attributes & FileAttributes.Archive) == FileAttributes.Archive)
                    result += "A";
                if ((_Attributes & FileAttributes.NotContentIndexed) == FileAttributes.NotContentIndexed)
                    result += "I";
                return result;
            }

            set
            {
                _Attributes = FileAttributes.Normal;
                foreach (char c in value.ToUpper())
                {
                    switch (c)
                    {
                        case 'H':
                            if ((_Attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
                                throw new ArgumentException(String.Format("Repeated flag. ({0})", c), "value");
                            _Attributes |= FileAttributes.Hidden;
                            break;

                        case 'R':
                            if ((_Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                                throw new ArgumentException(String.Format("Repeated flag. ({0})", c), "value");
                            _Attributes |= FileAttributes.ReadOnly;
                            break;

                        case 'S':
                            if ((_Attributes & FileAttributes.System) == FileAttributes.System)
                                throw new ArgumentException(String.Format("Repeated flag. ({0})", c), "value");
                            _Attributes |= FileAttributes.System;
                            break;

                        case 'A':
                            if ((_Attributes & FileAttributes.Archive) == FileAttributes.Archive)
                                throw new ArgumentException(String.Format("Repeated flag. ({0})", c), "value");
                            _Attributes |= FileAttributes.Archive;
                            break;

                        case 'I':
                            if ((_Attributes & FileAttributes.NotContentIndexed) == FileAttributes.NotContentIndexed)
                                throw new ArgumentException(String.Format("Repeated flag. ({0})", c), "value");
                            _Attributes |= FileAttributes.NotContentIndexed;
                            break;
                        default:
                            throw new ArgumentException(value);
                    }
                }
            }
        }


        public override String ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("attributes ").Append(EnumUtil.GetDescription(Constraint)).Append(" ").Append(AttributeString);
            return sb.ToString();
        }

        private bool _EvaluateOne(FileAttributes fileAttrs, FileAttributes criterionAttrs)
        {
            bool result = false;
            if ((_Attributes & criterionAttrs) == criterionAttrs)
                result = ((fileAttrs & criterionAttrs) == criterionAttrs);
            else
                result = true;
            return result;
        }



        internal override bool Evaluate(string filename)
        {
#if NETCF
		FileAttributes fileAttrs = NetCfFile.GetAttributes(filename);
#else
            FileAttributes fileAttrs = System.IO.File.GetAttributes(filename);
#endif

            return _Evaluate(fileAttrs);
        }

        private bool _Evaluate(FileAttributes fileAttrs)
        {
            //Console.WriteLine("fileattrs[{0}]={1}", filename, fileAttrs.ToString());

            bool result = _EvaluateOne(fileAttrs, FileAttributes.Hidden);
            if (result)
                result = _EvaluateOne(fileAttrs, FileAttributes.System);
            if (result)
                result = _EvaluateOne(fileAttrs, FileAttributes.ReadOnly);
            if (result)
                result = _EvaluateOne(fileAttrs, FileAttributes.Archive);

            if (Constraint != ComparisonConstraint.EqualTo)
                result = !result;

            //Console.WriteLine("AttributesCriterion[{2}]({0})= {1}", filename, result, AttributeString);

            return result;
        }
    }



    internal partial class CompoundCriterion : FileCriterion
    {
        internal LogicalConjunction Conjunction;
        internal FileCriterion Left;

        private FileCriterion _Right;
        internal FileCriterion Right
        {
            get { return _Right; }
            set
            {
                _Right = value;
                if (value == null)
                    Conjunction = LogicalConjunction.NONE;
                else if (Conjunction == LogicalConjunction.NONE)
                    Conjunction = LogicalConjunction.AND;
            }
        }


        internal override bool Evaluate(string filename)
        {
            bool result = Left.Evaluate(filename);
            switch (Conjunction)
            {
                case LogicalConjunction.AND:
                    if (result)
                        result = Right.Evaluate(filename);
                    break;
                case LogicalConjunction.OR:
                    if (!result)
                        result = Right.Evaluate(filename);
                    break;
                default:
                    throw new ArgumentException("Conjunction");
            }
            return result;
        }


        public override String ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("(")
            .Append((Left != null) ? Left.ToString() : "null")
            .Append(" ")
            .Append(Conjunction.ToString())
            .Append(" ")
            .Append((Right != null) ? Right.ToString() : "null")
            .Append(")");
            return sb.ToString();
        }
    }



    /// <summary>
    /// FileFilter encapsulates logic that selects files from a source based on a set
    /// of criteria.  
    /// </summary>
    /// <remarks>
    ///
    /// <para>
    /// Typically, an application that creates or manipulates Zip archives will not directly
    /// interact with the FileFilter class.  The FileFilter class is used internally by the
    /// ZipFile class for selecting files for inclusion into the ZipFile, when the <see
    /// cref="Ionic.Zip.ZipFile.AddSelectedFiles(String,String)"/> method is called.
    /// </para>
    ///
    /// <para>
    /// But, some applications may wish to use the FileFilter class directly, to select
    /// files from disk volumes based on a set of criteria, without creating or querying Zip
    /// archives.  The file selection criteria include: a pattern to match the filename; the
    /// last modified, created, or last accessed time of the file; the size of the file; and
    /// the attributes of the file.
    /// </para>
    /// </remarks>
    public partial class FileFilter
    {
        internal FileCriterion Include;
        internal FileCriterion Exclude;

        /// <summary>
        /// Constructor that allows the caller to specify file selection criteria, 
        /// as well as file exclusion criteria.
        /// </summary>
        /// 
        /// <remarks>
        /// <para>
        /// This constructor allows the caller to specify a set of criteria for inclusion of files, as
        /// well as a set of criteria for exclusion.  The set of files returned will 
        /// satisfy the inclusion criteria, but will not satisfy the exclusion criteria. 
        /// </para>
        /// 
        /// <para>
        /// See <see cref="FileFilter.SelectionCriteria"/> for a description of the syntax of 
        /// the SelectionCriteria string.
        /// </para>
        /// </remarks>
        /// 
        /// <param name="SelectionCriteria">The criteria for file selection.</param>
        /// 
        /// <param name="ExclusionCriteria">
        /// The criteria for exclusion.  Actually, the ExclusionCriteria is
        /// redundant. Any criteria specified in the ExclusionCriteria could also be specified in
        /// the SelectionCriteria, just by logically negating the criteria.  In other words, a
        /// SelectionCriteria of "size &gt; 50000" coupled with an ExclusionCriteria of "name =
        /// *.txt" is equivalent to a SelectionCriteria of "size &gt; 50000 AND name != *.txt"
        /// with no ExclusionCriteria.  Despite this, this method is provided to allow for
        /// clarity in the interface for those cases where it makes sense to clearly delineate
        /// the exclusion criteria in the application code.
        /// </param>
        public FileFilter(String SelectionCriteria, String ExclusionCriteria)
        {
            if (String.IsNullOrEmpty(SelectionCriteria))
                throw new ArgumentException("includeString");

            Include = _ParseCriterion(SelectionCriteria);
            Exclude = _ParseCriterion(ExclusionCriteria);
        }


        /// <summary>
        /// Constructor that allows the caller to specify file selection criteria.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This constructor allows the caller to specify a set of criteria for inclusion of files. 
        /// </para>
        /// 
        /// <para>
        /// See <see cref="FileFilter.SelectionCriteria"/> for a description of the syntax of 
        /// the SelectionCriteria string.
        /// </para>
        /// </remarks>
        public FileFilter(String SelectionCriteria)
        {
            if (String.IsNullOrEmpty(SelectionCriteria))
                throw new ArgumentException("SelectionCriteria");

            Include = _ParseCriterion(SelectionCriteria);
        }

        /// <summary>
        /// The string specifying which files to include when retrieving.
        /// </summary>
        /// <remarks>
        ///         
        /// <para>
        /// Specify the criteria in statements of 3 elements: a noun, an operator, and a value.
        /// Consider the string "name != *.doc" .  The noun is "name".  The operator is "!=",
        /// implying "Not Equal".  The value is "*.doc".  That criterion, in English, says "all
        /// files with a name that does not end in the .doc extension."
        /// </para> 
        ///
        /// <para>
        /// Supported nouns include "name" for the filename; "atime", "mtime", and "ctime" for
        /// last access time, last modfied time, and created time of the file, respectively;
        /// "attributes" for the file attributes; and "size" for the file length (uncompressed).
        /// The "attributes" and "name" nouns both support = and != as operators.  The "size",
        /// "atime", "mtime", and "ctime" nouns support = and !=, and &gt;, &gt;=, &lt;, &lt;=
        /// as well.
        /// </para> 
        ///
        /// <para>
        /// Specify values for the file attributes as a string with one or more of the
        /// characters H,R,S,A in any order, implying Hidden, ReadOnly, System, and Archive,
        /// respectively.  To specify a time, use YYYY-MM-DD-HH:mm:ss as the format.  If you
        /// omit the HH:mm:ss portion, it is assumed to be 00:00:00 (midnight). The value for a
        /// size criterion is expressed in integer quantities of bytes, kilobytes (use k or kb
        /// after the number), megabytes (m or mb), or gigabytes (g or gb).  The value for a
        /// name is a pattern to match against the filename, potentially including wildcards.
        /// The pattern follows CMD.exe glob rules: * implies one or more of any character,
        /// while ? implies one character.  Currently you cannot specify a pattern that includes
        /// spaces.
        /// </para> 
        ///
        /// <para>
        /// Some examples: a string like "attributes != H" retrieves all entries whose
        /// attributes do not include the Hidden bit.  A string like "mtime > 2009-01-01"
        /// retrieves all entries with a last modified time after January 1st, 2009.  For
        /// example "size &gt; 2gb" retrieves all entries whose uncompressed size is greater
        /// than 2gb.
        /// </para> 
        ///
        /// <para>
        /// You can combine criteria with the conjunctions AND or OR. Using a string like "name
        /// = *.txt AND size &gt;= 100k" for the SelectionCriteria retrieves entries whose names
        /// end in  .txt, and whose uncompressed size is greater than or equal to
        /// 100 kilobytes.
        /// </para>
        ///
        /// <para>
        /// For more complex combinations of criteria, you can use parenthesis to group clauses
        /// in the boolean logic.  Absent parenthesis, the precedence of the criterion atoms is
        /// determined by order of appearance.  Unlike the C# language, the AND conjunction does
        /// not take precendence over the logical OR.  This is important only in strings that
        /// contain 3 or more criterion atoms.  In other words, "name = *.txt and size &gt; 1000
        /// or attributes = H" implies "((name = *.txt AND size &gt; 1000) OR attributes = H)"
        /// while "attributes = H OR name = *.txt and size &gt; 1000" evaluates to "((attributes
        /// = H OR name = *.txt) AND size &gt; 1000)".  When in doubt, use parenthesis.
        /// </para>
        ///
        /// <para>
        /// Using time properties requires some extra care. If you want to retrieve all entries
        /// that were last updated on 2009 February 14, specify "mtime &gt;= 2009-02-14 AND
        /// mtime &lt; 2009-02-15".  Read this to say: all files updated after 12:00am on
        /// February 14th, until 12:00am on February 15th.  You can use the same bracketing
        /// approach to specify any time period - a year, a month, a week, and so on.
        /// </para>
        ///
        /// <para>
        /// The syntax allows one special case: if you provide a string with no spaces, it is treated as
        /// a pattern to match for the filename.  Therefore a string like "*.xls" will be equivalent to 
        /// specifying "name = *.xls".  
        /// </para>
        /// 
        /// <para>
        /// There is no logic in this class that insures that the inclusion criteria
        /// are internally consistent.  For example, it's possible to specify criteria that
        /// says the file must have a size of less than 100 bytes, as well as a size that
        /// is greater than 1000 bytes.  Obviously no file will ever satisfy such criteria,
        /// but this class does not check and find such inconsistencies.
        /// </para>
        /// 
        /// </remarks>
        ///
        /// <exception cref="System.Exception">
        /// Thrown in the setter if the value has an invalid syntax.
        /// </exception>
        public String SelectionCriteria
        {
            get
            {
                return Include.ToString();
            }
            set
            {
                if (String.IsNullOrEmpty(value))
                    throw new ArgumentException("value");

                Include = _ParseCriterion(value);
            }
        }


        private enum ParseState
        {
            Start,
            OpenParen,
            CriterionDone,
            ConjunctionPending,
            Whitespace,
        }



        private FileCriterion _ParseCriterion(String s)
        {
            if (s == null) return null;

            // shorthand for filename glob
            if (s.IndexOf(" ") == -1)
                s = "name = " + s;

            string[] elements = s.Replace("(", " ( ")
            .Replace(")", " ) ")
            .Replace("  ", " ")
            .Trim()
            .Split(' ', '\t');

            if (elements.Length < 3) throw new ArgumentException();

            FileCriterion current = null;

            LogicalConjunction pendingConjunction = LogicalConjunction.NONE;

            ParseState state;
            var stateStack = new System.Collections.Generic.Stack<ParseState>();
            var critStack = new System.Collections.Generic.Stack<FileCriterion>();
            stateStack.Push(ParseState.Start);

            for (int i = 0; i < elements.Length; i++)
            {
                switch (elements[i].ToLower())
                {
                    case "and":
                    case "or":
                        state = stateStack.Peek();
                        if (state != ParseState.CriterionDone)
                            throw new ArgumentException(elements[i]);

                        if (elements.Length <= i + 3) throw new ArgumentException(elements[i]);
                        pendingConjunction = (LogicalConjunction)Enum.Parse(typeof(LogicalConjunction), elements[i].ToUpper());
                        current = new CompoundCriterion { Left = current, Right = null, Conjunction = pendingConjunction };
                        //Console.WriteLine("Conjunction: ({0})  push state({1})", elements[i].ToUpper(), state.ToString());
                        stateStack.Push(state);
                        stateStack.Push(ParseState.ConjunctionPending);
                        //Console.WriteLine("Conjunction: ({1})   [{0}]", current.ToString(), pendingConjunction.ToString());
                        //Console.WriteLine("Push Criterion");
                        critStack.Push(current);
                        break;

                    case "(":
                        state = stateStack.Peek();
                        if (state != ParseState.Start && state != ParseState.ConjunctionPending && state != ParseState.OpenParen)
                            throw new ArgumentException(elements[i]);
                        if (elements.Length <= i + 4) throw new ArgumentException();
                        stateStack.Push(ParseState.OpenParen);
                        break;

                    case ")":
                        state = stateStack.Pop();
                        //Console.WriteLine("openParen  popped state({0})", state.ToString());
                        if (stateStack.Peek() != ParseState.OpenParen)
                            throw new ArgumentException(elements[i]);
                        stateStack.Pop(); //?
                        stateStack.Push(ParseState.CriterionDone);
                        break;

                    case "atime":
                    case "ctime":
                    case "mtime":
                        if (elements.Length <= i + 2) throw new ArgumentException(elements[i]);
                        DateTime t;
                        try
                        {
                            t = DateTime.ParseExact(elements[i + 2], "yyyy-MM-dd-HH:mm:ss", null);
                        }
                        catch
                        {
                            t = DateTime.ParseExact(elements[i + 2], "yyyy-MM-dd", null);
                        }
                        current = new TimeCriterion
                        {
                            Which = (WhichTime)Enum.Parse(typeof(WhichTime), elements[i]),
                            Constraint = (ComparisonConstraint)EnumUtil.Parse(typeof(ComparisonConstraint), elements[i + 1]),
                            Time = t
                        };
                        i += 2;
                        //Console.WriteLine("slurped time atom: [{0}]", current.ToString());
                        stateStack.Push(ParseState.CriterionDone);
                        break;


                    case "size":
                        if (elements.Length <= i + 2) throw new ArgumentException(elements[i]);
                        Int64 sz = 0;
                        string v = elements[i + 2];
                        if (v.ToUpper().EndsWith("K"))
                            sz = Int64.Parse(v.Substring(0, v.Length - 1)) * 1024;

                        else if (v.ToUpper().EndsWith("KB"))
                            sz = Int64.Parse(v.Substring(0, v.Length - 2)) * 1024;
                        else if (v.ToUpper().EndsWith("M"))
                            sz = Int64.Parse(v.Substring(0, v.Length - 1)) * 1024 * 1024;
                        else if (v.ToUpper().EndsWith("MB"))
                            sz = Int64.Parse(v.Substring(0, v.Length - 2)) * 1024 * 1024;
                        else if (v.ToUpper().EndsWith("G"))
                            sz = Int64.Parse(v.Substring(0, v.Length - 1)) * 1024 * 1024 * 1024;
                        else if (v.ToUpper().EndsWith("GB"))
                            sz = Int64.Parse(v.Substring(0, v.Length - 2)) * 1024 * 1024 * 1024;
                        else sz = Int64.Parse(elements[i + 2]);

                        current = new SizeCriterion
                        {
                            Size = sz,
                            Constraint = (ComparisonConstraint)EnumUtil.Parse(typeof(ComparisonConstraint), elements[i + 1])
                        };
                        i += 2;
                        //Console.WriteLine("slurped size atom: [{0}]", current.ToString());
                        stateStack.Push(ParseState.CriterionDone);
                        break;

                    case "name":
                        {
                            if (elements.Length <= i + 2) throw new ArgumentException(elements[i]);
                            ComparisonConstraint c =
                                (ComparisonConstraint)EnumUtil.Parse(typeof(ComparisonConstraint), elements[i + 1]);
                            if (c != ComparisonConstraint.NotEqualTo && c != ComparisonConstraint.EqualTo)
                                throw new ArgumentException(elements[i + 1]);
                            current = new NameCriterion
                            {
                                MatchingFileSpec = elements[i + 2],
                                Constraint = c
                            };
                            i += 2;
                            //Console.WriteLine("slurped name atom: [{0}]", current.ToString());
                            stateStack.Push(ParseState.CriterionDone);
                        }
                        break;
                    case "attributes":
                        {
                            if (elements.Length <= i + 2) throw new ArgumentException(elements[i]);
                            //Console.WriteLine("looking at constraint [{0}]", elements[i+1]);
                            ComparisonConstraint c =
                                (ComparisonConstraint)EnumUtil.Parse(typeof(ComparisonConstraint), elements[i + 1]);
                            if (c != ComparisonConstraint.NotEqualTo && c != ComparisonConstraint.EqualTo)
                                throw new ArgumentException(elements[i + 1]);
                            current = new AttributesCriterion
                            {
                                AttributeString = elements[i + 2],
                                Constraint = c
                            };
                            i += 2;
                            //Console.WriteLine("slurped attributes atom: [{0}]", current.ToString());
                            stateStack.Push(ParseState.CriterionDone);
                        }
                        break;

                    case "":
                        stateStack.Push(ParseState.Whitespace);
                        break;

                    default:
                        //state = stateStack.Peek();
                        //if (state != ParseState.Start && state != ParseState.OpenParen)
                        throw new ArgumentException("'" + elements[i] + "'");
                }

                state = stateStack.Peek();
                if (state == ParseState.CriterionDone)
                {
                    stateStack.Pop();
                    if (stateStack.Peek() == ParseState.ConjunctionPending)
                    {
                        while (stateStack.Peek() == ParseState.ConjunctionPending)
                        {
                            var cc = critStack.Pop() as CompoundCriterion;
                            cc.Right = current;
                            current = cc; // mark the parent as current (walk up the tree)
                            stateStack.Pop();   // the conjunction is no longer pending 

                            state = stateStack.Pop();
                            if (state != ParseState.CriterionDone)
                                throw new ArgumentException();
                        }
                    }
                    else stateStack.Push(ParseState.CriterionDone);  // not sure?
                }

                if (state == ParseState.Whitespace)
                    stateStack.Pop();
            }

            return current;
        }


        /// <summary>
        /// Returns a string representation of the FileFilter object.
        /// </summary>
        /// <returns>The string representation of the boolean logic statement of the file
        /// selection criteria for this instance. </returns>
        public override String ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Include: ").Append(Include.ToString());
            if (Exclude != null)
                sb.Append(" and Exclude: ").Append(Exclude.ToString());
            return sb.ToString();
        }


        private bool Evaluate(string filename)
        {
            bool result = Include.Evaluate(filename);
            if (Exclude != null)
                result = result && !Exclude.Evaluate(filename);
            return result;
        }


        /// <summary>
        /// Returns the names of the files in the specified directory
        /// that fit the selection criteria specified in the FileFilter.
        /// </summary>
        ///
        /// <remarks>
        /// This is equivalent to calling <see cref="SelectFiles(String, bool)"/> with RecurseDirectories = false.
        /// </remarks>
        ///
        /// <param name="Directory">
        /// The name of the directory over which to apply the FileFilter criteria.
        /// </param>
        ///
        /// <returns>
        /// An array of strings containing fully-qualified pathnames of files
        /// that match the criteria specified in the FileFilter instance.
        /// </returns>
        public String[] SelectFiles(String Directory)
        {
            return SelectFiles(Directory, false);
        }


        /// <summary>
        /// Returns the names of the files in the specified directory that fit the selection
        /// criteria specified in the FileFilter, optionally recursing through subdirectories.
        /// </summary>
        ///
        /// <remarks>
        /// This method applies the file selection criteria contained in the FileFilter to the 
        /// files contained in the given directory, and returns the names of files that 
        /// conform to the criteria. 
        /// </remarks>
        ///
        /// <param name="Directory">
        /// The name of the directory over which to apply the FileFilter criteria.
        /// </param>
        ///
        /// <param name="RecurseDirectories">
        /// Whether to recurse through subdirectories when applying the file selection criteria.
        /// </param>
        ///
        /// <returns>
        /// An array of strings containing fully-qualified pathnames of files
        /// that match the criteria specified in the FileFilter instance.
        /// </returns>
        public String[] SelectFiles(String Directory, bool RecurseDirectories)
        {
            String[] filenames = System.IO.Directory.GetFiles(Directory);
            var list = new System.Collections.Generic.List<String>();

            // add the files: 
            foreach (String filename in filenames)
            {
                if (Evaluate(filename))
                    list.Add(filename);
            }

            if (RecurseDirectories)
            {
                // add the subdirectories:
                String[] dirnames = System.IO.Directory.GetDirectories(Directory);
                foreach (String dir in dirnames)
                {
                    Array.ForEach(SelectFiles(dir), list.Add);
                }
            }
            return list.ToArray();
        }
    }




    /// <summary>
    /// Summary description for EnumUtil.
    /// </summary>
    internal sealed class EnumUtil
    {
        /// <summary>
        /// Returns the value of the DescriptionAttribute if the specified Enum value has one.
        /// If not, returns the ToString() representation of the Enum value.
        /// </summary>
        /// <param name="value">The Enum to get the description for</param>
        /// <returns></returns>
        internal static string GetDescription(System.Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());
            var attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
            if (attributes.Length > 0)
                return attributes[0].Description;
            else
                return value.ToString();
        }

        /// <summary>
        /// Converts the string representation of the name or numeric value of one or more enumerated constants to an equivilant enumerated object.
        /// Note: Utilised the DescriptionAttribute for values that use it.
        /// </summary>
        /// <param name="enumType">The System.Type of the enumeration.</param>
        /// <param name="value">A string containing the name or value to convert.</param>
        /// <returns></returns>
        internal static object Parse(Type enumType, string value)
        {
            return Parse(enumType, value, false);
        }

        /// <summary>
        /// Converts the string representation of the name or numeric value of one or more enumerated constants to an equivilant enumerated object.
        /// A parameter specified whether the operation is case-sensitive.
        /// Note: Utilised the DescriptionAttribute for values that use it.
        /// </summary>
        /// <param name="enumType">The System.Type of the enumeration.</param>
        /// <param name="value">A string containing the name or value to convert.</param>
        /// <param name="ignoreCase">Whether the operation is case-sensitive or not.</param>
        /// <returns></returns>
        internal static object Parse(Type enumType, string value, bool ignoreCase)
        {
            if (ignoreCase)
                value = value.ToLower();

            foreach (System.Enum val in System.Enum.GetValues(enumType))
            {
                string comparisson = GetDescription(val);
                if (ignoreCase)
                    comparisson = comparisson.ToLower();
                if (GetDescription(val) == value)
                    return val;
            }

            return System.Enum.Parse(enumType, value, ignoreCase);
        }
    }

}

