using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Numerics;
using System.Threading.Tasks;

namespace GarlandToolsWrapper
{
    public sealed class WebRequests
    {
        private static string GARLAND_TOOLS_BASE_URL = "https://www.garlandtools.org/";
        private static Models.Data? dataObject = null;

        /// <summary>
        /// Retrieves the data object from Garland Tools
        /// </summary>
        public static Models.Data DataObject => RetrieveDataObject();

        private static Models.Data RetrieveDataObject()
        {
            return dataObject ??= GetData();
        }

        private static Models.Data GetData()
        {

            HttpClient httpClient = new();
            Task<HttpResponseMessage>? task = Task.Run(() => httpClient.GetAsync($"{GARLAND_TOOLS_BASE_URL}db/doc/core/en/3/data.json"));
            task.Wait();
            HttpResponseMessage? response = task.Result;
            httpClient.Dispose();

            if (response.IsSuccessStatusCode)
            {
                System.IO.Stream? thing = response.Content.ReadAsStream();
                System.IO.StreamReader reader = new(thing);
                string result = reader.ReadToEnd();
                Models.Data serializedResult = JsonConvert.DeserializeObject<Models.Data>(result)!;
                return serializedResult;
            }
            else
            {
                throw new Exception("Garland Tools did not return 200 status code");
            }
        }

        /// <summary>
        /// Searches Garland Tools for an item and returns serialized results
        /// </summary>
        public static List<Models.ItemSearchResult> ItemSearch(string itemName)
        {
            HttpClient httpClient = new();
            Task<HttpResponseMessage>? task = Task.Run(() => httpClient.GetAsync($"{GARLAND_TOOLS_BASE_URL}api/search.php?text={itemName}&lang=en"));
            task.Wait();
            HttpResponseMessage? response = task.Result;
            httpClient.Dispose();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                System.IO.StreamReader reader = new(response.Content.ReadAsStream());
                string result = reader.ReadToEnd();
                // Some item names have "-" in them, which errors out Garland Tools
                // As far as I can tell, if you type something that is already contained in the list,
                // it will filter out results client side without making a new request. We'll do something
                // similar here
                if (result.Contains("error") && itemName.Contains('-'))
                {
                    return ItemSearch(itemName.Split("-")[0]);
                }
                else if (result.Contains("error") && itemName.Contains('+'))
                {
                    return ItemSearch(itemName.Split("+")[0]);
                }
                else
                {
                    List<Models.ItemSearchResult> serializedResult = JsonConvert.DeserializeObject<List<Models.ItemSearchResult>>(result)!;
                    return serializedResult;
                }
            }
            else
            {
                throw new Exception("Garland Tools did not return 200 status code");
            }
        }

        public static Models.ItemDetails GetItemDetails(BigInteger itemId)
        {
            HttpClient httpClient = new();
            Task<HttpResponseMessage>? task = Task.Run(() => httpClient.GetAsync($"{GARLAND_TOOLS_BASE_URL}db/doc/item/en/3/{itemId}.json"));
            task.Wait();
            HttpResponseMessage? response = task.Result;
            httpClient.Dispose();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                System.IO.StreamReader reader = new(response.Content.ReadAsStream());
                string result = reader.ReadToEnd();
                Models.ItemDetails serializedResult = JsonConvert.DeserializeObject<Models.ItemDetails>(result)!;
                return serializedResult;
            }
            else
            {
                throw new Exception("Garland Tools did not return 200 status code");
            }
        }
    }
}
