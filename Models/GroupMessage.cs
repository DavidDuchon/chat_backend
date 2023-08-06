namespace chat_backend.Models;

public class GroupMessage {
    public Guid GroupMessageId {get;set;}

    //Group relationship
    public Guid GroupId {get;set;}
    public Group Group {get;set;}

    //Message relationship
    public Guid MessageId {get;set;}
    public Message Message {get;set;}
}