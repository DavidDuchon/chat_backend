namespace chat_backend.Models;

public class AuthenticationTokenPayload {
    public string User {get;set;} = String.Empty;
}

public class Expiration {
    public ulong exp {get;set;}
}

public class RefreshTokenPayload {
    public string For {get;set;} = String.Empty;
}
