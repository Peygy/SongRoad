namespace MainApp.Models.Service
{
    public interface IJwtCheckService
    {
        bool CheckAccessToken();
        bool CheckRefreshToken();
        bool RevokeRefreshToken();
    }
}
