using System;
using System.Text.RegularExpressions;

// ReSharper disable CheckNamespace
namespace Orientdb.Net
// ReSharper restore CheckNamespace
{
    public class OrientdbServerException : Exception
    {
        private static readonly Regex ExceptionSplitter = new Regex(@"^([^\[]*?)\[(.*)\]", RegexOptions.Singleline);
        private static readonly string _couldNotParseServerException = "Could not parse server exception";

        public int Status { get; set; }
        public string ExceptionType { get; set; }
        internal OrientdbServerException(int status, string exceptionType)
        {
            this.Status = status;
            this.ExceptionType = exceptionType;
        }
        public OrientdbServerException(OrientdbServerError error)
            : base(ParseError(error))
        {
            this.Status = error.Status;
            this.ExceptionType = error.ExceptionType;
        }
        //iffy side effect assignment to exceptionType needed so that we simply return message to the 
        //base constructor.
        private static string ParseError(OrientdbServerError error)
        {
            if (error == null) return _couldNotParseServerException;
            if (error.Error.IsNullOrEmpty()) return _couldNotParseServerException;
            var matches = ExceptionSplitter.Match(error.Error);
            if (matches.Groups.Count != 3) return _couldNotParseServerException;

            error.ExceptionType = matches.Groups[1].Value;
            return matches.Groups[2].Value;
        }
    }
}