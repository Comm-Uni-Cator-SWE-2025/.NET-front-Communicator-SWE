using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Controller
{
    public class CloudStorageAdapter
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private readonly string _cloudApiBaseUrl;

        public CloudStorageAdapter(string cloudApiBaseUrl)
        {
            _cloudApiBaseUrl = cloudApiBaseUrl;
        }

        public async Task<MeetingSession?> CreateMeetingSessionAsync(UserProfile hostProfile)
        {
            var requestBody = JsonSerializer.Serialize(hostProfile);
            var requestContent = new StringContent(requestBody, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, $"{_cloudApiBaseUrl}/api/sessions")
            {
                Content = requestContent
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", GetCloudServiceAuthToken());

            var response = await _httpClient.SendAsync(request);

            if (response.StatusCode == System.Net.HttpStatusCode.Created)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                var session = JsonSerializer.Deserialize<MeetingSession>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return session;
            }
            return null;
        }

        public async Task<MeetingSession?> FindMeetingSessionByIdAsync(string sessionId)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"{_cloudApiBaseUrl}/api/sessions/{sessionId}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", GetCloudServiceAuthToken());

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode) // Checks for 2xx status codes
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                var session = JsonSerializer.Deserialize<MeetingSession>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return session;
            }
            return null;
        }

        private string GetCloudServiceAuthToken()
        {
            return "super-secret-service-to-service-token";
        }
    }
}
