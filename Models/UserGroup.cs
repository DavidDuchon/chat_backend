
namespace chat_backend.Models;

public class UserGroup {
    public Guid UserGroupId {get;set;}

    // User relationship
    public Guid UserId {get;set;}
    public User User {get;set;}

    // Group relationship
    public Guid GroupId {get;set;}
    public Group Group {get;set;}
}