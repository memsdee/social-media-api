namespace be.Domain.Attributes;

public class BsonCollectionAttribute
{
    [AttributeUsage(AttributeTargets.Class)]
    public class BsonCollection(string collectionName) : Attribute
    {
        public string CollectionName { get; } = collectionName;
    }
}