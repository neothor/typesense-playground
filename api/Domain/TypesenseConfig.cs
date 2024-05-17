namespace Api.Domain
{
    public class TypesenseConfig
    {
        public static readonly string Section = "Typesense";

        public string ApiKey { get; set; }
        public Uri[] NodeUris { get; set; }
    }
}
