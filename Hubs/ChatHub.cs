using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using chat_backend.Data;

namespace chat_backend.Hubs;

[Authorize(AuthenticationSchemes="signalr",Policy="AccessToken")]
public class ChatHub : Hub<IClient> {
    
  [Authorize(AuthenticationSchemes="signalr",Policy="AccessToken")]
  public async Task SendMessage(string group,string message){
      var identity = Context.User.Identity as ClaimsIdentity;

      var user = identity!.FindFirst("User")!.Value;
      await Clients.Group(group).RecieveMessage(user,message);
  }

  [Authorize(AuthenticationSchemes="signalr",Policy="AccessToken")]
  public async Task AddToGroup(string group/*,DatabaseContext _context*/){
  // var identity = Context.User.Identity as ClaimsIdentity;

  // var user = identity!.FindFirst("User")!.Value;
//   var alreadyRegisteredUser = _context.Users.SingleOrDefault(usr => usr.Username == user);
//
//   if (alreadyRegisteredUser is null)
//      return;
//
    await Groups.AddToGroupAsync(Context.ConnectionId,group);
  }
}
