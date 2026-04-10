
using System;
using System.Net.Http;
using System.Threading.Tasks;

public class NavArbeidsPlass
{
    
    HttpClient httpClient = new();
    string apiToken;

    public NavArbeidsPlass()
    {
        httpClient.BaseAddress = new Uri("https://pam-stilling-feed.nav.no/api/");
        Task<HttpResponseMessage> response = httpClient.GetAsync("accesstoken");
        response.Wait();
        string responseString = response.Result.Content.ToString();
        //Find offset of the beginning of the JWT token... if the type or algorithm is changed this line probably has to as well
        int offset = responseString.IndexOf("eyJhbGciOiJ");
        apiToken = responseString.Substring(offset);
    }


    public Task<JobAds> GetJobAds(int page = 0)
    {
        throw new NotImplementedException();
    }
}

public class JobAds
{
}