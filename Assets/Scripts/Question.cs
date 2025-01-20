using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

[BsonIgnoreExtraElements]
public class Question
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string _id { get; set; }

    public string question { get; set; }

    public string[] answers { get; set; }

    public int correctAnswerIndex { get; set; }

    public string catName { get; set; }

    public Question() { }
}
