using Microsoft.AspNetCore.SignalR;

using chat_backend.Data;

namespace chat_backend.Hubs;

public class ChatHub : Hub<IClient> {
    
  public async Task SendMessage(string user,string group,string message){
      await Clients.Group(group).RecieveMessage(user,message);
  }

  public async Task AddToGroup(string user,string group,DatabaseContext _context){
    var alreadyRegisteredUser = _context.Users.SingleOrDefault(usr => usr.Username == user);

    if (alreadyRegisteredUser is null)
      return;

    await Groups.AddToGroupAsync(Context.ConnectionId,group);
  }
}
