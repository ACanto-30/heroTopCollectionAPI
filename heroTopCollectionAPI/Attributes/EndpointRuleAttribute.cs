namespace heroTopCollectionAPI.Attributes
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
    public class EndpointRuleAttribute : Attribute
    {
        public string ContentType { get; }
        public bool RequiresAuthorization { get; }

        public EndpointRuleAttribute(string contentType, bool requiresAuthorization = true)
        {
            ContentType = contentType;
            RequiresAuthorization = requiresAuthorization;
        }
    }
}