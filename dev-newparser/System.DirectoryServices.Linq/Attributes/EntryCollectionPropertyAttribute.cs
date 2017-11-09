
namespace System.DirectoryServices.Linq.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class EntryCollectionPropertyAttribute : DirectoryPropertyAttribute
    {
        private const string MatchingRuleBitAnd = ":1.2.840.113556.1.4.803:";
        private const string MatchingRuleBitOr = ":1.2.840.113556.1.4.804:";
        private const string MatchingRuleInChain = ":1.2.840.113556.1.4.1941:";

        public EntryCollectionPropertyAttribute() : base(string.Empty, true)
        {
        }

        public EntryCollectionPropertyAttribute(string name) : base(name, true)
        {
        }

        /// <summary>
        /// This rule is limited to filters that apply to the DN. This is
        /// a special "extended match operator that walks the chain of
        /// ancestry in objects all the way to the root until it finds a match.
        /// NOTE: Some such queries on subtrees may be more processor intensive,
        /// such as chasing links with a high fan-out; that is, listing all the
        /// groups that a user is a member of. 
        /// </summary>
        public MatchingRuleType MatchingRule { get; set; }

        /// <summary>
        /// Returns ":1.2.840.113556.1.4.1941:" if
        /// <see cref="MatchingRule"/> is true, "" otherwise.
        /// </summary>
        public string MatchingRuleValue
        {
            get
            {
                switch (MatchingRule)
                {
                    case MatchingRuleType.BitAnd:
                        {
                            return MatchingRuleBitAnd;
                        }
                    case MatchingRuleType.BitOr:
                        {
                            return MatchingRuleBitOr;
                        }
                    case MatchingRuleType.InChain:
                        {
                            return MatchingRuleInChain;
                        }
                    case MatchingRuleType.None:
                    default:
                        {
                            return string.Empty;
                        }
                }
            }
        }
    }
}
