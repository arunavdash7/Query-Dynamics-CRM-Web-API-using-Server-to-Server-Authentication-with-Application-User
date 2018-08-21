using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace S2SAuthenticationwithD365
{
    class S2SAuthenticationwithD365
    {
        public static void Main(string[] args)
        {
            S2SAuthenticationwithD365 s = new S2SAuthenticationwithD365();
            try
            {
                GetAccountsAsync().Wait();
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine("There was an exception:"+ ex.ToString());
            }
        }
        private static async Task GetAccountsAsync()
        {
            try
            {
                S2SAuthenticationwithD365 s = new S2SAuthenticationwithD365();
                string[] accountProperties = { "name" };
                JObject collection;
                ClientCredential credential = new ClientCredential("provide client Id", "provide secret key");

                string authorityUri = "https://login.microsoftonline.com/abc.onmicrosoft.com/oauth2/authorize"; // here abc.onmicrosoft.com is my crm domain. Please replace with your domain 
                TokenCache tokenCache = new TokenCache();

                AuthenticationContext context = new AuthenticationContext(authorityUri);
                AuthenticationResult result = await context.AcquireTokenAsync("Provide crm credential", credential); 

                var authToken = result.AccessToken;
                var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12; //
                var response = httpClient.GetAsync("crmurl/api/data/v9.0/accounts?$select=name").Result;
                var accountJSON = await response.Content.ReadAsStringAsync();
                collection = JsonConvert.DeserializeObject<JObject>(response.Content.ReadAsStringAsync().Result);
                s.DisplayFormattedEntities("Saved query (Active Accounts):", collection, accountProperties);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception ex is" + ex.Message);
            }
        }

        private void DisplayFormattedEntities(string label, JArray entities, string[] properties)
        {
            Console.Write(label);
            int lineNum = 0;
            foreach (JObject entity in entities)
            {
                lineNum++;
                List<string> propsOutput = new List<string>();
                //Iterate through each requested property and output either formatted value if one 
                //exists, otherwise output plain value.
                foreach (string prop in properties)
                {
                    string propValue;
                    string formattedProp = prop + "@OData.Community.Display.V1.FormattedValue";
                    if (null != entity[formattedProp])
                    { propValue = entity[formattedProp].ToString(); }
                    else
                    { propValue = entity[prop].ToString(); }
                    propsOutput.Add(propValue);
                }
                Console.Write("\n\t{0}) {1}", lineNum, String.Join(", ", propsOutput));
            }
            Console.Write("\n");
        }
        ///<summary>Overloaded helper version of method that unpacks 'collection' parameter.</summary>
        private void DisplayFormattedEntities(string label, JObject collection, string[] properties)
        {
            JToken valArray;
            //Parameter collection contains an array of entities in 'value' member.
            if (collection.TryGetValue("value", out valArray))
            {
                DisplayFormattedEntities(label, (JArray)valArray, properties);
            }
            //Otherwise it just represents a single entity.
            else
            {
                JArray singleton = new JArray(collection);
                DisplayFormattedEntities(label, singleton, properties);
            }
        }
    }
}
