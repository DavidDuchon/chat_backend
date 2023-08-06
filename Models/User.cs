using System.ComponentModel.DataAnnotations;
namespace chat_backend.Models;

public class User {
    public Guid UserId {get;set;}
    [Required]
    public string Username {get;set;} = string.Empty;
    [Required]
    public string Password {get;set;} = string.Empty;

    //Messages relationship
    public ICollection<Message> Messages {get;} = new List<Message>(); 
    //UserGroup join table
    public ICollection<UserGroup> UserGroups {get;} = new List<UserGroup>();
}
