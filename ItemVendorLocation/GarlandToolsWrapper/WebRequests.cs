using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Text;
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
        public static Models.Data DataObject
        {
            get => retrieveDataObject();
        }

        private static Models.Data retrieveDataObject()
        {
            return dataObject ??= GetData();
        }

        private static Models.Data GetData()
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create($"{GARLAND_TOOLS_BASE_URL}db/doc/core/en/3/data.json");
            request.Method = "GET";
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            if (response.StatusCode == HttpStatusCode.OK)
            {
                System.IO.Stream resultStream = response.GetResponseStream();
                System.IO.StreamReader reader = new(response.GetResponseStream());
                string result = reader.ReadToEnd();
                Models.Data serializedResult = JsonConvert.DeserializeObject<Models.Data>(result)!;
                //Models.Data serializedResult = serializer.Deserialize<Models.Data>(result);
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
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create($"{GARLAND_TOOLS_BASE_URL}api/search.php?text={itemName}&lang=en");
            request.Method = "GET";
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            if (response.StatusCode == HttpStatusCode.OK)
            {
                System.IO.Stream resultStream = response.GetResponseStream();
                System.IO.StreamReader reader = new(response.GetResponseStream());
                string result = reader.ReadToEnd();
                List<Models.ItemSearchResult> serializedResult = JsonConvert.DeserializeObject<List<Models.ItemSearchResult>>(result)!;
                return serializedResult;
            }
            else
            {
                throw new Exception("Garland Tools did not return 200 status code");
            }
        }

        public static Models.ItemDetails GetItemDetails(BigInteger itemId)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create($"{GARLAND_TOOLS_BASE_URL}db/doc/item/en/3/{itemId}.json");
            request.Method = "GET";
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            if (response.StatusCode == HttpStatusCode.OK)
            {
                System.IO.Stream resultStream = response.GetResponseStream();
                System.IO.StreamReader reader = new (response.GetResponseStream());
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
