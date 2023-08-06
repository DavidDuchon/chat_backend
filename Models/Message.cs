using System.Collections.Generic;

namespace chat_backend.Models;

public class Message {
	public Guid MessageId {get;set;}
	public string Text {get;set;} = String.Empty;
	
	//User Relationship
	public User From {get;set;} = null!;
	public Guid UserId {get;set;}

	//GroupMessage join table
	public ICollection<GroupMessage> GroupMessages {get;set;} = new List<GroupMessage>();
}
