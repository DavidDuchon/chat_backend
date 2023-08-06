using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

using chat_backend.Models;
using chat_backend.Data;

namespace chat_backend.Controllers;

[ApiController]
[Route("[controller]")]
public class GroupController: ControllerBase {
    private readonly DatabaseContext _context;
    private readonly ILogger<GroupController> _logger;
    public GroupController(DatabaseContext context,ILogger<GroupController> logger){
        _context = context;
        _logger = logger;
    }

    [HttpPost]
    [Route("create")]
    [Authorize(AuthenticationSchemes="default",Policy ="AccessToken")]
    public async Task<IActionResult> Create(GroupModel Group){

        var alreadyRegisteredGroup = _context.Groups.SingleOrDefault(group => group.Name == Group.GroupName); 


        if (alreadyRegisteredGroup is not null)
            return BadRequest(new {error = "Bad data"});


        string username = string.Empty;

        var identity = HttpContext.User.Identity as ClaimsIdentity;

        username = identity!.FindFirst("User")!.Value;

        var alreadyRegisteredUser = _context.Users.SingleOrDefault(user => user.Username == username);

        if (alreadyRegisteredUser is null)
            return NotFound();

        Group newGroup = new()
        {
            Name = Group.GroupName
        };

        UserGroup newUserGroup = new()
        {
            User = alreadyRegisteredUser,
            Group = newGroup
        };

        newGroup.UserGroups.Add(newUserGroup);
        alreadyRegisteredUser.UserGroups.Add(newUserGroup); 

        _logger.LogInformation("Length of UserGroups collection: {}",alreadyRegisteredUser.UserGroups.Count);
        await _context.Groups.AddAsync(newGroup);
        
        try {
            await _context.SaveChangesAsync();
        } catch (DbUpdateConcurrencyException){
            return BadRequest(new {error = "Group already exists"});
        }
            
        return CreatedAtAction(nameof(Create),new { group = Group.GroupName});
    }

    [HttpPost]
    [Route("join")]
    [Authorize(AuthenticationSchemes="default",Policy = "AccessToken")]
    public async Task<IActionResult> Join(GroupModel Group){
        var alreadyRegisteredGroup = _context.Groups.SingleOrDefault(group => group.Name == Group.GroupName);

        if (alreadyRegisteredGroup is null)
            return NotFound();

        var identity = HttpContext.User.Identity as ClaimsIdentity;

        var username = identity!.FindFirst("User")!.Value;

        var alreadyRegisteredUser = _context.Users.SingleOrDefault(user => user.Username == username);

        if (alreadyRegisteredUser is null)
            return NotFound();

        var userGroups = _context.Entry(alreadyRegisteredUser).Collection(user => user.UserGroups).Query().Where(ug => ug.Group.Name == Group.GroupName).ToList();

        if (userGroups is null || userGroups.Count != 0)
            return BadRequest();

        _logger.LogInformation("Length of UserGroups collection: {length}",alreadyRegisteredUser.UserGroups.Count);
        UserGroup newUserGroup = new UserGroup {
            User = alreadyRegisteredUser,
            Group = alreadyRegisteredGroup
        };

        alreadyRegisteredUser.UserGroups.Add(newUserGroup);
        alreadyRegisteredGroup.UserGroups.Add(newUserGroup);

        try{
            await _context.SaveChangesAsync();
        } catch (DbUpdateConcurrencyException){
            return BadRequest(new {error = "Cannot add User to group"});
        }

        return Accepted( new {group = Group.GroupName});

    }

}