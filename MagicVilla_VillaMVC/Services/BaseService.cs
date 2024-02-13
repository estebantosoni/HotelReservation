
using MagicVilla_VillaMVC.Models;
using MagicVilla_VillaMVC.Services.IServices;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json.Serialization;
using Utility;

namespace MagicVilla_VillaMVC.Services
{
    public class BaseService : IBaseService
    {
        public APIResponse APIResponse { get; set; }

        //client to send a message (response) to the API
        IHttpClientFactory httpClient { get; set; }

        public BaseService(IHttpClientFactory httpClientFactory)
        {
            APIResponse = new();
            this.httpClient = httpClientFactory;
        }

        public async Task<T> SendAsync<T>(APIRequest request)
        {

            try
            {
                //client configuration
                var client = httpClient.CreateClient("MagicAPI");
                    //client message
                HttpRequestMessage message = new();
                message.Headers.Add("Accept", "application/json");
                message.RequestUri = new(request.Url);
                //check if data request to send to the API is null
                if (request.Data != null)
                {
                    message.Content = new StringContent(
                        JsonConvert.SerializeObject(request.Data),
                        Encoding.UTF8, "application/json");
                }

                //check method from the request
                switch (request.ApiType)
                {
                    case SD.ApiType.POST:
                        message.Method = HttpMethod.Post;
                        break;
                    case SD.ApiType.PUT:
                        message.Method = HttpMethod.Put;
                        break;
                    case SD.ApiType.DELETE:
                        message.Method = HttpMethod.Delete;
                        break;
                    default:
                        message.Method = HttpMethod.Get;
                        break;
                }

                //create response message for receive API response
                HttpResponseMessage responseMessage = null;

                //we pass the token to the header on the request
                if (!string.IsNullOrEmpty(request.Token))
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", request.Token);
                }

                //send a message to API
                responseMessage = await client.SendAsync(message);
                //convert response in a correct format to be able to read it
                var apiContent = await responseMessage.Content.ReadAsStringAsync();

                try
                {
                    //this code is for give more info about errors
                    APIResponse APIResponse = JsonConvert.DeserializeObject<APIResponse>(apiContent);
                    if(APIResponse != null && (responseMessage.StatusCode == System.Net.HttpStatusCode.BadRequest
                        || responseMessage.StatusCode == System.Net.HttpStatusCode.NotFound))
                    {
                        APIResponse.StatusCode = System.Net.HttpStatusCode.BadRequest;
                        APIResponse.IsSuccess = false;

                        //this is needed for maintain the generics that we have
                        var res = JsonConvert.SerializeObject(APIResponse);
                        var returnObj = JsonConvert.DeserializeObject<T>(res);
                        return returnObj;
                    }
                }
                catch (Exception e)
                {
                    var exResponse = JsonConvert.DeserializeObject<T>(apiContent);
                    return exResponse;
                }
                var response = JsonConvert.DeserializeObject<T>(apiContent);
                return response;
            }
            catch (Exception e)
            {
                //API response returns an error to the client
                var dto = new APIResponse
                {
                    ErrorMessages = new List<string>
                    {
                        Convert.ToString(e.Message)
                    },
                    IsSuccess = false
                };
                var res = JsonConvert.SerializeObject(dto);
                var APIResponse = JsonConvert.DeserializeObject<T>(res);
                return APIResponse;

            }

        }
    }
}
