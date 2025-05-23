using NLog;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Polyglot.Core
{
    public interface ITraverseFilter
    {
        bool IsMatch(string value);
    }

    public class ListContainsFilter : ITraverseFilter
    {
        public List<string> Values { get; } = new List<string>();

        public ListContainsFilter(List<string> values)
        {
            Values = values;
        }

        public bool IsMatch(string value)
        {
            return Values.Contains(value);
        }

        public override string ToString()
        {
            return string.Join(" ", Values.Select(x => $"'{x}'"));
        }
    }

    public class RegExFilter : ITraverseFilter
    {
        public Regex RegEx { get; }
        private string _regExPattern;

        public RegExFilter(string regExPattern)
        {
            RegEx = new Regex(regExPattern);
            _regExPattern = regExPattern;
        }

        public bool IsMatch(string value)
        {
            return RegEx.IsMatch(value);
        }

        public override string ToString()
        {
            return _regExPattern;
        }
    }

    public class TraverseConfig
    {
        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        public string Locale { get; }
        public bool FetchValid { get; }
        public bool FetchInvalid { get; }
        public bool FetchUntranslated { get; }
        public Dictionary<TraverseFilterKind, ITraverseFilter> TraverseFilter { get; } = new Dictionary<TraverseFilterKind, ITraverseFilter>();

        public TraverseConfig(string locale, bool fetchValid, bool fetchInvalid, bool fetchUntranslated) 
        {
            Locale = locale;
            FetchValid = fetchValid;
            FetchInvalid = fetchInvalid;
            FetchUntranslated = fetchUntranslated;
        }

        public void AddFilter(TraverseFilterKind kind, ITraverseFilter filter)
        {
            TraverseFilter.Add(kind, filter);
            logger.Debug($"Add filter by {kind}: {filter.ToString()}");
        }
    }
}