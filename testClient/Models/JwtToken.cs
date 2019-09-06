namespace testClient.Models
{
    public class JwtToken
    {
        public string AccessToken { get; set; }

        public string RefreshToken { get; set; }

        public JwtToken()
        {
        }

        public JwtToken(string accessToken, string refreshToken)
        {
            AccessToken = accessToken;
            RefreshToken = refreshToken;
        }
    }
}