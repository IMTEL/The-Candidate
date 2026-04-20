
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

public class NavArbeidsPlass : IJobSelectionService
{
    
    HttpClient httpClient = new();

    public NavArbeidsPlass()
    {
        httpClient.BaseAddress = new Uri("https://pam-stilling-feed.nav.no/api/");
        //Find the correct time date format
        DateTime time = new();
        time = time.AddMonths(-2);

        httpClient.DefaultRequestHeaders.Add("If-Modified-Since", time.ToString("R"));
        GetAndSetAccessToken();
    }

    public async Task<(IEnumerable<JobContainer>, bool)> GetJobs(int page = 1, int amount = 9)
    {
        HttpResponseMessage response = await httpClient.GetAsync("v1/feed");
        JObject json = JObject.Parse(response.Content.ToString());
        throw new NotImplementedException();
    }

    private void GetAndSetAccessToken()
    {
        Task<HttpResponseMessage> response = httpClient.GetAsync("accesstoken");
        response.Wait();
        string responseString = response.Result.Content.ToString();
        //Find offset of the beginning of the JWT token... if the type or algorithm is changed this line probably has to as well
        int offset = responseString.IndexOf("eyJhbGciOiJ");
        string apiToken = responseString[offset..];
        httpClient.DefaultRequestHeaders.Add("Authorization", apiToken);
    }

}
