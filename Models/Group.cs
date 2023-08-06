namespace chat_backend.Models;

public class Group {
	public Guid GroupId {get;set;}
	public string Name {get;set;} = string.Empty;

	//UserGroup join table
	public ICollection<UserGroup> UserGroups {get;} = new List<UserGroup>();

	//GroupMessage join table
	public ICollection<GroupMessage> GroupMessages {get;} = new List<GroupMessage>();
}
