namespace chat_backend.Hubs;

public interface IClient {
    Task RecieveMessage(string user,string message);
}