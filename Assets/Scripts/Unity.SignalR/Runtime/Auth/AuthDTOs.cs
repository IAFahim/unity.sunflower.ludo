using System;

namespace Ludos.Client.Auth
{
    [Serializable]
    public class AuthRequest
    {
        public string email;
        public string password;
    }

    [Serializable]
    public class TokenResponse
    {
        public string accessToken;
        public string refreshToken;
        public int expiresIn;
    }
}