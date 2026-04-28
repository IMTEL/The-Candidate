
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

public class NavArbeidsPlass : IJobSelectionService
{

    readonly HttpClient httpClient = new();

    string nextpage;
    private bool fetchingDone = false;

    ConcurrentBag<JobContainer> cache = new();
    HashSet<string> JobUUID = new();

    SemaphoreSlim threadpools = new(10);

    SemaphoreSlim requestLocker = new(1);

    Task setAccesstoken;

    public NavArbeidsPlass()
    {

        httpClient.BaseAddress = new Uri("https://pam-stilling-feed.nav.no/api/");
        //Find the correct time date format
        DateTime time = DateTime.Now.AddMonths(-2);

        httpClient.DefaultRequestHeaders.Add("If-Modified-Since", time.ToString("R"));
        setAccesstoken = GetAndSetAccessToken();
    }

    public async Task<(IEnumerable<JobContainer>, bool)> GetJobs(int page = 1, int amount = 9)
    {
        await setAccesstoken;
        if (!fetchingDone && page * amount > cache.Count)
        {   
            await requestLocker.WaitAsync();
            HttpResponseMessage response = await httpClient.GetAsync("v1/feed" + (nextpage != null ? "/" + nextpage : ""));
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                JObject json = JObject.Parse(await response.Content.ReadAsStringAsync());
                nextpage = json["next_id"].ToString();
                if (nextpage == "")
                {
                    fetchingDone = true;
                }
                List<JToken> ads = json["items"].Children().ToList();
                List<Task> tasks = new();
                foreach (JToken ad in ads)
                {
                    //This should be multithreaded... It is taking way to long
                    string uuid = ad["id"].ToString();
                    if (!JobUUID.Contains(uuid))
                    {
                        JobUUID.Add(uuid);
                        tasks.Add(FetchAdData(uuid));
                    }
                }
                await Task.WhenAll(tasks.ToArray());
            }
            requestLocker.Release();
        }

        bool hasNextPage = false;
        if (cache.Count() < (page - 1) * amount)
        {
            throw new ArgumentException("Out of scope with the amount of jobs");
        }
        if (cache.Count() > page * amount)
        {
            hasNextPage = true;
        }
        int startIndex = (page - 1) * amount;
        int endIndex = Math.Min(cache.Count(), page * amount);
        return (cache.ToList().GetRange(startIndex, endIndex - startIndex), hasNextPage);
    }

    private async Task FetchAdData(string id)
    {
        await threadpools.WaitAsync();
        HttpResponseMessage jobresponse = await httpClient.GetAsync("v1/feedentry/" + id);
        threadpools.Release();
        if (jobresponse.StatusCode == System.Net.HttpStatusCode.OK)
        {
            JObject Jjob = JObject.Parse(await jobresponse.Content.ReadAsStringAsync());
            JToken AdContent = Jjob["ad_content"];
            if (AdContent != null)
            {
                string title = AdContent["jobtitle"].ToString() == "" ? AdContent["title"].ToString() : AdContent["jobtitle"].ToString();
                Environment.Job job = new(title, new() { Environment.Actor.Manager }, AdContent["description"].ToString());
                cache.Add(new(job, null));
            }
        }
    }

    private async Task GetAndSetAccessToken()
    {
        Task<HttpResponseMessage> response = httpClient.GetAsync("publicToken");
        await response;
        string responseString = await response.Result.Content.ReadAsStringAsync();
        //Find offset of the beginning of the JWT token... if the type or algorithm is changed this line probably has to as well
        int offset = responseString.IndexOf("ey");
        string apiToken = responseString[offset..];
        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiToken}");
    }

}
