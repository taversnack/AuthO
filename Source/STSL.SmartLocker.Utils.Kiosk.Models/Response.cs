namespace STSL.SmartLocker.Utils.Kiosk.Models;

public class Response
{
    public Response()
    {

    }
    public Response(object data, bool success, string message)
    {
        Data = data;
        Success = success;
        Message = message;
    }
    public Response(object data, bool success)
    {
        Data = data;
        Success = success;
    }

    public object Data { get; set; }
    public bool Success { get; set; }
    public string Message { get; set; }
    public string Handler { get; set; }

    public static Response Sucessfull(string message)
    {
        return new Response(null, true, message);
    }
    public static Response Sucessfull(object data)
    {
        return new Response(data, true, string.Empty);
    }
    public static Response Sucessfull(string message, object data)
    {
        return new Response(data, true, message);
    }
    public static Response Failure(string message)
    {
        return new Response(null, false, message);
    }
    public static Response Failure(string message, object data)
    {
        return new Response(data, false, message);
    }

}
