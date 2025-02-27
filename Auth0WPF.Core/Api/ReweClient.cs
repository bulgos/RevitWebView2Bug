using IdentityModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Data.Entities;
using System;
using System.Net.Http;
using System.Threading;
using System.Net.Http.Headers;
using Core.Data.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Linq;
using Core.Data.Command;
using Core.Dto.Requests;
using Core.Dto.Responses;
using Core.Data.Entities.Common;
using Core.Data.Entities.Ancillary;
using Core.Data.Common;
using Core.Serializers.Json;

namespace Rewe.Revit.API;

#pragma warning disable 8601
#pragma warning disable 8604
#pragma warning disable 8618
#pragma warning disable 8625
public partial class ReweClient
{
    private string _baseUrl;


    private HttpClient _httpClient;
    private static Lazy<JsonSerializerSettings> _settings = new Lazy<JsonSerializerSettings>(CreateSerializerSettings, true);
    private JsonSerializerSettings _instanceSettings;

    public ReweClient(string baseUrl, HttpClient httpClient)
    {
        BaseUrl = baseUrl;
        _httpClient = httpClient;
        Initialize();
    }

    private static JsonSerializerSettings CreateSerializerSettings()
    {
        var settings = new JsonSerializerSettings()
        {
            //TypeNameHandling = TypeNameHandling.Auto, // Ensures type metadata is included
            //SerializationBinder = new MultiInterfaceBinder(),
            Converters =
            [
                new ModelObjectEntityJsonConverter(), new Point2dJsonConverter(), new GridPerimeterJsonConverter(),
                new InterfaceJsonConverter<UserEntity, IUserEntity>(),
                new InterfaceJsonConverter<TenantEntity, ITenantEntity>(),
                new InterfaceJsonConverter<Location, ILocation>()
            ]
        };
        UpdateJsonSerializerSettings(settings);
        return settings;
    }

    public string BaseUrl
    {
        get { return _baseUrl; }
        set
        {
            _baseUrl = value;
            if (!string.IsNullOrEmpty(_baseUrl) && !_baseUrl.EndsWith("/"))
                _baseUrl += '/';
        }
    }

    protected JsonSerializerSettings JsonSerializerSettings { get { return _instanceSettings ?? _settings.Value; } }

    static partial void UpdateJsonSerializerSettings(JsonSerializerSettings settings);

    partial void Initialize();

    partial void PrepareRequest(HttpClient client, HttpRequestMessage request, string url);
    partial void PrepareRequest(HttpClient client, HttpRequestMessage request, StringBuilder urlBuilder);
    partial void ProcessResponse(HttpClient client, HttpResponseMessage response);


    /// <returns>OK</returns>
    /// <exception cref="ApiException">A server side error occurred.</exception>
    public virtual Task<UserEntity> AuthenticateAsync()
    {
        return AuthenticateAsync(CancellationToken.None);
    }

    /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    /// <returns>OK</returns>
    /// <exception cref="ApiException">A server side error occurred.</exception>
    public virtual async Task<UserEntity> AuthenticateAsync(CancellationToken cancellationToken)
    {
        var client_ = _httpClient;
        var disposeClient_ = false;
        try
        {
            using (var request_ = new HttpRequestMessage())
            {
                request_.Content = new StringContent(string.Empty, Encoding.UTF8, "text/plain");
                request_.Method = new HttpMethod("POST");
                request_.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("text/plain"));

                var urlBuilder_ = new StringBuilder();
                if (!string.IsNullOrEmpty(_baseUrl)) urlBuilder_.Append(_baseUrl);
                // Operation Path: "auth/authenticate"
                urlBuilder_.Append("auth/authenticate");

                PrepareRequest(client_, request_, urlBuilder_);

                var url_ = urlBuilder_.ToString();
                request_.RequestUri = new Uri(url_, UriKind.RelativeOrAbsolute);

                PrepareRequest(client_, request_, url_);

                var response_ = await client_.SendAsync(request_, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
                var disposeResponse_ = true;
                try
                {
                    var headers_ = new Dictionary<string, IEnumerable<string>>();
                    foreach (var item_ in response_.Headers)
                        headers_[item_.Key] = item_.Value;
                    if (response_.Content != null && response_.Content.Headers != null)
                    {
                        foreach (var item_ in response_.Content.Headers)
                            headers_[item_.Key] = item_.Value;
                    }

                    ProcessResponse(client_, response_);

                    var status_ = (int)response_.StatusCode;
                    if (status_ == 200)
                    {
                        if (response_.Headers.TryGetValues("Set-Cookie", out var cookieValues))
                        {
                            foreach (var cookie in cookieValues)
                            {
                                if (cookie.StartsWith("session-id"))
                                {
                                    var sessionId = cookie.Split(';')[0]; // Extract "session-id=XYZ"
                                    if (_httpClient.DefaultRequestHeaders.Contains("Cookie"))
                                        _httpClient.DefaultRequestHeaders.Remove("Cookie");
                                    _httpClient.DefaultRequestHeaders.Add("Cookie", sessionId); // Store it for future requests
                                    Console.WriteLine($"Stored Session ID: {sessionId}");
                                }
                            }
                        }

                        var objectResponse_ = await ReadObjectResponseAsync<UserEntity>(response_, headers_, cancellationToken).ConfigureAwait(false);
                        if (objectResponse_.Object == null)
                        {
                            throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                        }
                        return objectResponse_.Object;
                    }
                    else
                    {
                        var responseData_ = response_.Content == null ? null : await response_.Content.ReadAsStringAsync().ConfigureAwait(false);
                        throw new ApiException("The HTTP status code of the response was not expected (" + status_ + ").", status_, responseData_, headers_, null);
                    }
                }
                finally
                {
                    if (disposeResponse_)
                        response_.Dispose();
                }
            }
        }
        finally
        {
            if (disposeClient_)
                client_.Dispose();
        }
    }

    /// <returns>OK</returns>
    /// <exception cref="ApiException">A server side error occurred.</exception>
    public virtual Task SignoutAsync()
    {
        return SignoutAsync(CancellationToken.None);
    }

    /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    /// <returns>OK</returns>
    /// <exception cref="ApiException">A server side error occurred.</exception>
    public virtual async Task SignoutAsync(CancellationToken cancellationToken)
    {
        var client_ = _httpClient;
        var disposeClient_ = false;
        try
        {
            using (var request_ = new HttpRequestMessage())
            {
                request_.Content = new StringContent(string.Empty, Encoding.UTF8, "application/json");
                request_.Method = new HttpMethod("POST");

                var urlBuilder_ = new StringBuilder();
                if (!string.IsNullOrEmpty(_baseUrl)) urlBuilder_.Append(_baseUrl);
                // Operation Path: "auth/signout"
                urlBuilder_.Append("auth/signout");

                PrepareRequest(client_, request_, urlBuilder_);

                var url_ = urlBuilder_.ToString();
                request_.RequestUri = new Uri(url_, UriKind.RelativeOrAbsolute);

                PrepareRequest(client_, request_, url_);

                var response_ = await client_.SendAsync(request_, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
                var disposeResponse_ = true;
                try
                {
                    var headers_ = new Dictionary<string, IEnumerable<string>>();
                    foreach (var item_ in response_.Headers)
                        headers_[item_.Key] = item_.Value;
                    if (response_.Content != null && response_.Content.Headers != null)
                    {
                        foreach (var item_ in response_.Content.Headers)
                            headers_[item_.Key] = item_.Value;
                    }

                    ProcessResponse(client_, response_);

                    var status_ = (int)response_.StatusCode;
                    if (status_ == 200)
                    {
                        return;
                    }
                    else
                    {
                        var responseData_ = response_.Content == null ? null : await response_.Content.ReadAsStringAsync().ConfigureAwait(false);
                        throw new ApiException("The HTTP status code of the response was not expected (" + status_ + ").", status_, responseData_, headers_, null);
                    }
                }
                finally
                {
                    if (disposeResponse_)
                        response_.Dispose();
                }
            }
        }
        finally
        {
            if (disposeClient_)
                client_.Dispose();
        }
    }

    /// <returns>OK</returns>
    /// <exception cref="ApiException">A server side error occurred.</exception>
    public virtual Task<IEnumerable<ProjectEntity>> ProjectsGETAsync()
    {
        return ProjectsGETAsync(CancellationToken.None);
    }

    /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    /// <returns>OK</returns>
    /// <exception cref="ApiException">A server side error occurred.</exception>
    public virtual async Task<IEnumerable<ProjectEntity>> ProjectsGETAsync(CancellationToken cancellationToken)
    {
        var client_ = _httpClient;
        var disposeClient_ = false;
        try
        {
            using (var request_ = new HttpRequestMessage())
            {
                request_.Method = new HttpMethod("GET");

                var urlBuilder_ = new StringBuilder();
                if (!string.IsNullOrEmpty(_baseUrl)) urlBuilder_.Append(_baseUrl);
                // Operation Path: "projects"
                urlBuilder_.Append("projects");

                PrepareRequest(client_, request_, urlBuilder_);

                var url_ = urlBuilder_.ToString();
                request_.RequestUri = new Uri(url_, UriKind.RelativeOrAbsolute);

                PrepareRequest(client_, request_, url_);

                var response_ = await client_.SendAsync(request_, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
                var disposeResponse_ = true;
                try
                {
                    var headers_ = new Dictionary<string, IEnumerable<string>>();
                    foreach (var item_ in response_.Headers)
                        headers_[item_.Key] = item_.Value;
                    if (response_.Content != null && response_.Content.Headers != null)
                    {
                        foreach (var item_ in response_.Content.Headers)
                            headers_[item_.Key] = item_.Value;
                    }

                    ProcessResponse(client_, response_);

                    var status_ = (int)response_.StatusCode;
                    if (status_ == 200)
                    {
                        var objectResponse_ = await ReadObjectResponseAsync<List<ProjectEntity>>(response_, headers_, cancellationToken).ConfigureAwait(false);
                        if (objectResponse_.Object == null)
                        {
                            throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                        }
                        return objectResponse_.Object;
                    }
                    else
                    {
                        var responseData_ = response_.Content == null ? null : await response_.Content.ReadAsStringAsync().ConfigureAwait(false);
                        throw new ApiException("The HTTP status code of the response was not expected (" + status_ + ").", status_, responseData_, headers_, null);
                    }
                }
                finally
                {
                    if (disposeResponse_)
                        response_.Dispose();
                }
            }
        }
        finally
        {
            if (disposeClient_)
                client_.Dispose();
        }
    }

    /// <exception cref="ApiException">A server side error occurred.</exception>
    public virtual Task<ProjectReturnDto> GetProjectAsync(string id)
    {
        return GetProjectAsync(id, CancellationToken.None);
    }

    /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    /// <exception cref="ApiException">A server side error occurred.</exception>
    public virtual async Task<ProjectReturnDto> GetProjectAsync(string id, CancellationToken cancellationToken)
    {
        if (id == null)
            throw new ArgumentNullException("id");

        var client_ = _httpClient;
        var disposeClient_ = false;
        try
        {
            using (var request_ = new HttpRequestMessage())
            {
                request_.Method = new HttpMethod("GET");
                request_.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/octet-stream"));

                var urlBuilder_ = new StringBuilder();
                if (!string.IsNullOrEmpty(_baseUrl)) urlBuilder_.Append(_baseUrl);
                // Operation Path: "projects/{id}"
                urlBuilder_.Append("projects/");
                urlBuilder_.Append(Uri.EscapeDataString(ConvertToString(id, System.Globalization.CultureInfo.InvariantCulture)));

                PrepareRequest(client_, request_, urlBuilder_);

                var url_ = urlBuilder_.ToString();
                request_.RequestUri = new Uri(url_, UriKind.RelativeOrAbsolute);

                PrepareRequest(client_, request_, url_);

                var response_ = await client_.SendAsync(request_, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
                var disposeResponse_ = true;
                try
                {
                    var headers_ = new Dictionary<string, IEnumerable<string>>();
                    foreach (var item_ in response_.Headers)
                        headers_[item_.Key] = item_.Value;
                    if (response_.Content != null && response_.Content.Headers != null)
                    {
                        foreach (var item_ in response_.Content.Headers)
                            headers_[item_.Key] = item_.Value;
                    }

                    ProcessResponse(client_, response_);

                    var status_ = (int)response_.StatusCode;
                    if (status_ == 200)
                    {
                        var objectResponse_ = await ReadObjectResponseAsync<ProjectReturnDto>(response_, headers_, cancellationToken).ConfigureAwait(false);
                        if (objectResponse_.Object == null)
                        {
                            throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                        }
                        return objectResponse_.Object;
                    }
                    else
                    {
                        var responseData_ = response_.Content == null ? null : await response_.Content.ReadAsStringAsync().ConfigureAwait(false);
                        throw new ApiException("The HTTP status code of the response was not expected (" + status_ + ").", status_, responseData_, headers_, null);
                    }
                }
                finally
                {
                    if (disposeResponse_)
                        response_.Dispose();
                }
            }
        }
        finally
        {
            if (disposeClient_)
                client_.Dispose();
        }
    }

    public virtual Task<string?> ModelObjectRawAsync(string? modelObjectId)
    {
        return ModelObjectRawAsync(modelObjectId, CancellationToken.None);
    }

    public virtual async Task<string?> ModelObjectRawAsync(string? modelObjectId, CancellationToken cancellationToken)
    {
        if (modelObjectId == null)
            throw new ArgumentNullException("modelObjectId");

        var client_ = _httpClient;
        var disposeClient_ = false;
        try
        {
            using (var request_ = new HttpRequestMessage())
            {
                request_.Method = new HttpMethod("GET");

                var urlBuilder_ = new StringBuilder();
                if (!string.IsNullOrEmpty(_baseUrl)) urlBuilder_.Append(_baseUrl);
                // Operation Path: "modelObject/{modelObjectId}"
                urlBuilder_.Append("modelObjects/");
                urlBuilder_.Append(Uri.EscapeDataString(ConvertToString(modelObjectId, System.Globalization.CultureInfo.InvariantCulture)));

                PrepareRequest(client_, request_, urlBuilder_);

                var url_ = urlBuilder_.ToString();
                request_.RequestUri = new Uri(url_, UriKind.RelativeOrAbsolute);

                PrepareRequest(client_, request_, url_);

                var response_ = await client_.SendAsync(request_, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
                var disposeResponse_ = true;
                try
                {
                    var headers_ = new Dictionary<string, IEnumerable<string>>();
                    foreach (var item_ in response_.Headers)
                        headers_[item_.Key] = item_.Value;
                    if (response_.Content != null && response_.Content.Headers != null)
                    {
                        foreach (var item_ in response_.Content.Headers)
                            headers_[item_.Key] = item_.Value;
                    }

                    ProcessResponse(client_, response_);

                    var status_ = (int)response_.StatusCode;
                    if (status_ == 200 && response_ != null && response_.Content != null)
                    {
                        var responseText = await response_.Content.ReadAsStringAsync().ConfigureAwait(false);
                        return JToken.Parse(responseText).ToString(Formatting.Indented);
                    }

                    //var responseData_ = response_.Content == null ? null : await response_.Content.ReadAsStringAsync().ConfigureAwait(false);
                    throw new ApiException("The HTTP status code of the response was not expected (" + status_ + ").", status_, "responseData_", headers_, null);
                }
                finally
                {
                    if (disposeResponse_)
                        response_.Dispose();
                }
            }
        }
        finally
        {
            if (disposeClient_)
                client_.Dispose();
        }
    }

    public virtual Task<ModelObjectEntity> ModelObjectAsync(string? modelObjectId)
    {
        return ModelObjectAsync(modelObjectId, CancellationToken.None);
    }

    public virtual async Task<ModelObjectEntity> ModelObjectAsync(string? modelObjectId, CancellationToken cancellationToken)
    {
        if (modelObjectId == null)
            throw new ArgumentNullException("modelObjectId");

        var client_ = _httpClient;
        var disposeClient_ = false;
        try
        {
            using (var request_ = new HttpRequestMessage())
            {
                request_.Method = new HttpMethod("GET");

                var urlBuilder_ = new StringBuilder();
                if (!string.IsNullOrEmpty(_baseUrl)) urlBuilder_.Append(_baseUrl);
                // Operation Path: "modelObject/{modelObjectId}"
                urlBuilder_.Append("modelObjects/");
                urlBuilder_.Append(Uri.EscapeDataString(ConvertToString(modelObjectId, System.Globalization.CultureInfo.InvariantCulture)));

                PrepareRequest(client_, request_, urlBuilder_);

                var url_ = urlBuilder_.ToString();
                request_.RequestUri = new Uri(url_, UriKind.RelativeOrAbsolute);

                PrepareRequest(client_, request_, url_);

                var response_ = await client_.SendAsync(request_, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
                var disposeResponse_ = true;
                try
                {
                    var headers_ = new Dictionary<string, IEnumerable<string>>();
                    foreach (var item_ in response_.Headers)
                        headers_[item_.Key] = item_.Value;
                    if (response_.Content != null && response_.Content.Headers != null)
                    {
                        foreach (var item_ in response_.Content.Headers)
                            headers_[item_.Key] = item_.Value;
                    }

                    ProcessResponse(client_, response_);

                    var status_ = (int)response_.StatusCode;
                    if (status_ == 200)
                    {
                        var objectResponse_ = await ReadObjectResponseAsync<ModelObjectEntity>(response_, headers_, cancellationToken).ConfigureAwait(false);
                        if (objectResponse_.Object == null)
                        {
                            throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                        }
                        return objectResponse_.Object;
                    }
                    else
                    {
                        var responseData_ = response_.Content == null ? null : await response_.Content.ReadAsStringAsync().ConfigureAwait(false);
                        throw new ApiException("The HTTP status code of the response was not expected (" + status_ + ").", status_, responseData_, headers_, null);
                    }
                }
                finally
                {
                    if (disposeResponse_)
                        response_.Dispose();
                }
            }
        }
        finally
        {
            if (disposeClient_)
                client_.Dispose();
        }
    }

    public virtual Task<IEnumerable<ModelObjectEntity>> Subtree2Async(string ancestorId, ObjectType type)
    {
        return Subtree2Async(ancestorId, type, CancellationToken.None);
    }

    public virtual async Task<IEnumerable<ModelObjectEntity>> Subtree2Async(string ancestorId, ObjectType type, CancellationToken cancellationToken)
    {
        if (ancestorId == null)
            throw new ArgumentNullException("ancestorId");

        var client_ = _httpClient;
        var disposeClient_ = false;
        try
        {
            using (var request_ = new HttpRequestMessage())
            {
                request_.Method = new HttpMethod("GET");

                var urlBuilder_ = new StringBuilder();
                if (!string.IsNullOrEmpty(_baseUrl)) urlBuilder_.Append(_baseUrl);
                // Operation Path: "modelObjects/{ancestorId}/subtree/{type}"
                urlBuilder_.Append("modelObjects/");
                urlBuilder_.Append(Uri.EscapeDataString(ConvertToString(ancestorId, System.Globalization.CultureInfo.InvariantCulture)));
                urlBuilder_.Append("/subtree/");
                urlBuilder_.Append(Uri.EscapeDataString(ConvertToString(type, System.Globalization.CultureInfo.InvariantCulture)));

                PrepareRequest(client_, request_, urlBuilder_);

                var url_ = urlBuilder_.ToString();
                request_.RequestUri = new Uri(url_, UriKind.RelativeOrAbsolute);

                PrepareRequest(client_, request_, url_);

                var response_ = await client_.SendAsync(request_, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
                var disposeResponse_ = true;
                try
                {
                    var headers_ = new Dictionary<string, IEnumerable<string>>();
                    foreach (var item_ in response_.Headers)
                        headers_[item_.Key] = item_.Value;
                    if (response_.Content != null && response_.Content.Headers != null)
                    {
                        foreach (var item_ in response_.Content.Headers)
                            headers_[item_.Key] = item_.Value;
                    }

                    ProcessResponse(client_, response_);

                    var status_ = (int)response_.StatusCode;
                    if (status_ == 200)
                    {
                        var objectResponse_ = await ReadObjectResponseAsync<List<ModelObjectEntity>>(response_, headers_, cancellationToken).ConfigureAwait(false);
                        if (objectResponse_.Object == null)
                        {
                            throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                        }
                        return objectResponse_.Object;
                    }
                    else
                    {
                        var responseData_ = response_.Content == null ? null : await response_.Content.ReadAsStringAsync().ConfigureAwait(false);
                        throw new ApiException("The HTTP status code of the response was not expected (" + status_ + ").", status_, responseData_, headers_, null);
                    }
                }
                finally
                {
                    if (disposeResponse_)
                        response_.Dispose();
                }
            }
        }
        finally
        {
            if (disposeClient_)
                client_.Dispose();
        }
    }

    /// <exception cref="ApiException">A server side error occurred.</exception>
    public virtual Task<CommandResult> UpdateServiceBoxAsync(string serviceBoxId, CommandRequest<ServiceBoxDto> moveServiceBox)
    {
        return UpdateServiceBoxAsync(serviceBoxId, moveServiceBox, CancellationToken.None);
    }

    /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    /// <exception cref="ApiException">A server side error occurred.</exception>
    public virtual async Task<CommandResult> UpdateServiceBoxAsync(string serviceBoxId, CommandRequest<ServiceBoxDto> updateServiceBox, CancellationToken cancellationToken)
    {
        if (serviceBoxId == null)
            throw new ArgumentNullException("serviceBoxId");

        if (updateServiceBox == null)
            throw new ArgumentNullException("moveServiceBox");

        var client_ = _httpClient;
        var disposeClient_ = false;
        try
        {
            using (var request_ = new HttpRequestMessage())
            {
                var json_ = JsonConvert.SerializeObject(updateServiceBox, JsonSerializerSettings);
                var content_ = new StringContent(json_);
                content_.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                request_.Content = content_;
                request_.Method = new HttpMethod("PUT");
                request_.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/octet-stream"));

                var urlBuilder_ = new StringBuilder();
                if (!string.IsNullOrEmpty(_baseUrl)) urlBuilder_.Append(_baseUrl);
                // Operation Path: "serviceBoxes/{serviceBoxId}/update"
                urlBuilder_.Append("serviceBoxes/");
                urlBuilder_.Append(Uri.EscapeDataString(ConvertToString(serviceBoxId, System.Globalization.CultureInfo.InvariantCulture)));
                urlBuilder_.Append("/update");

                PrepareRequest(client_, request_, urlBuilder_);

                var url_ = urlBuilder_.ToString();
                request_.RequestUri = new Uri(url_, UriKind.RelativeOrAbsolute);

                PrepareRequest(client_, request_, url_);

                var response_ = await client_.SendAsync(request_, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
                var disposeResponse_ = true;
                try
                {
                    var headers_ = new Dictionary<string, IEnumerable<string>>();
                    foreach (var item_ in response_.Headers)
                        headers_[item_.Key] = item_.Value;
                    if (response_.Content != null && response_.Content.Headers != null)
                    {
                        foreach (var item_ in response_.Content.Headers)
                            headers_[item_.Key] = item_.Value;
                    }

                    ProcessResponse(client_, response_);

                    var status_ = (int)response_.StatusCode;
                    if (status_ == 200)
                    {
                        var objectResponse_ = await ReadObjectResponseAsync<CommandResult>(response_, headers_, cancellationToken).ConfigureAwait(false);
                        if (objectResponse_.Object == null)
                        {
                            throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                        }
                        return objectResponse_.Object;
                    }
                    else
                    {
                        var responseData_ = response_.Content == null ? null : await response_.Content.ReadAsStringAsync().ConfigureAwait(false);
                        throw new ApiException("The HTTP status code of the response was not expected (" + status_ + ").", status_, responseData_, headers_, null);
                    }
                }
                finally
                {
                    if (disposeResponse_)
                        response_.Dispose();
                }
            }
        }
        finally
        {
            if (disposeClient_)
                client_.Dispose();
        }
    }

    /// <exception cref="ApiException">A server side error occurred.</exception>
    public virtual Task<CommandResult> CreateTreeColumnAsync(string buildingId, CommandRequest<TreeColumnDto> treeColumn)
    {
        return CreateTreeColumnAsync(buildingId, treeColumn, CancellationToken.None);
    }

    /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    /// <exception cref="ApiException">A server side error occurred.</exception>
    public virtual async Task<CommandResult> CreateTreeColumnAsync(string buildingId, CommandRequest<TreeColumnDto> treeColumn, CancellationToken cancellationToken)
    {
        if (buildingId == null)
            throw new ArgumentNullException("buildingId");

        if (treeColumn == null)
            throw new ArgumentNullException("treeColumn");

        var client_ = _httpClient;
        var disposeClient_ = false;
        try
        {
            using (var request_ = new HttpRequestMessage())
            {
                var json_ = JsonConvert.SerializeObject(treeColumn, JsonSerializerSettings);
                var content_ = new StringContent(json_);
                content_.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                request_.Content = content_;
                request_.Method = new HttpMethod("POST");
                request_.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));

                var urlBuilder_ = new StringBuilder();
                if (!string.IsNullOrEmpty(_baseUrl)) urlBuilder_.Append(_baseUrl);
                // Operation Path: "{buildingId}/treeColumns"
                urlBuilder_.Append(Uri.EscapeDataString(ConvertToString(buildingId, System.Globalization.CultureInfo.InvariantCulture)));
                urlBuilder_.Append("/treeColumns");

                PrepareRequest(client_, request_, urlBuilder_);

                var url_ = urlBuilder_.ToString();
                request_.RequestUri = new Uri(url_, UriKind.RelativeOrAbsolute);

                PrepareRequest(client_, request_, url_);

                var response_ = await client_.SendAsync(request_, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
                var disposeResponse_ = true;
                try
                {
                    var headers_ = new Dictionary<string, IEnumerable<string>>();
                    foreach (var item_ in response_.Headers)
                        headers_[item_.Key] = item_.Value;
                    if (response_.Content != null && response_.Content.Headers != null)
                    {
                        foreach (var item_ in response_.Content.Headers)
                            headers_[item_.Key] = item_.Value;
                    }

                    ProcessResponse(client_, response_);

                    var status_ = (int)response_.StatusCode;
                    if (status_ == 200)
                    {
                        var objectResponse_ = await ReadObjectResponseAsync<CommandResult>(response_, headers_, cancellationToken).ConfigureAwait(false);
                        if (objectResponse_.Object == null)
                        {
                            throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                        }
                        return objectResponse_.Object;
                    }
                    else
                    {
                        var responseData_ = response_.Content == null ? null : await response_.Content.ReadAsStringAsync().ConfigureAwait(false);
                        throw new ApiException("The HTTP status code of the response was not expected (" + status_ + ").", status_, responseData_, headers_, null);
                    }
                }
                finally
                {
                    if (disposeResponse_)
                        response_.Dispose();
                }
            }
        }
        finally
        {
            if (disposeClient_)
                client_.Dispose();
        }
    }

    /// <exception cref="ApiException">A server side error occurred.</exception>
    public virtual Task<CommandResult> GenerateAllAsync(string buildingId, CommandRequest commandRequest)
    {
        return GenerateAllAsync(buildingId, commandRequest, CancellationToken.None);
    }

    /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    /// <exception cref="ApiException">A server side error occurred.</exception>
    public virtual async Task<CommandResult> GenerateAllAsync(string buildingId, CommandRequest commandRequest, CancellationToken cancellationToken)
    {
        if (buildingId == null)
            throw new ArgumentNullException("buildingId");

        if (commandRequest == null)
            throw new ArgumentNullException("commandRequest");

        var client_ = _httpClient;
        var disposeClient_ = false;
        try
        {
            using (var request_ = new HttpRequestMessage())
            {
                var json_ = JsonConvert.SerializeObject(commandRequest, JsonSerializerSettings);
                var content_ = new StringContent(json_);
                content_.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                request_.Content = content_;
                request_.Method = new HttpMethod("POST");
                request_.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));

                var urlBuilder_ = new StringBuilder();
                if (!string.IsNullOrEmpty(_baseUrl)) urlBuilder_.Append(_baseUrl);
                // Operation Path: "{buildingId}/generateAll"
                urlBuilder_.Append(Uri.EscapeDataString(ConvertToString(buildingId, System.Globalization.CultureInfo.InvariantCulture)));
                urlBuilder_.Append("/generateAll");

                PrepareRequest(client_, request_, urlBuilder_);

                var url_ = urlBuilder_.ToString();
                request_.RequestUri = new Uri(url_, UriKind.RelativeOrAbsolute);

                PrepareRequest(client_, request_, url_);

                var response_ = await client_.SendAsync(request_, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
                var disposeResponse_ = true;
                try
                {
                    var headers_ = new Dictionary<string, IEnumerable<string>>();
                    foreach (var item_ in response_.Headers)
                        headers_[item_.Key] = item_.Value;
                    if (response_.Content != null && response_.Content.Headers != null)
                    {
                        foreach (var item_ in response_.Content.Headers)
                            headers_[item_.Key] = item_.Value;
                    }

                    ProcessResponse(client_, response_);

                    var status_ = (int)response_.StatusCode;
                    if (status_ == 200)
                    {
                        var objectResponse_ = await ReadObjectResponseAsync<CommandResult>(response_, headers_, cancellationToken).ConfigureAwait(false);
                        if (objectResponse_.Object == null)
                        {
                            throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                        }
                        return objectResponse_.Object;
                    }
                    else
                    {
                        var responseData_ = response_.Content == null ? null : await response_.Content.ReadAsStringAsync().ConfigureAwait(false);
                        throw new ApiException("The HTTP status code of the response was not expected (" + status_ + ").", status_, responseData_, headers_, null);
                    }
                }
                finally
                {
                    if (disposeResponse_)
                        response_.Dispose();
                }
            }
        }
        finally
        {
            if (disposeClient_)
                client_.Dispose();
        }
    }

    /// <exception cref="ApiException">A server side error occurred.</exception>
    public virtual Task<CommandResult> RegenerateTreeColumnsAsync(string buildingId, CommandRequest commandRequest)
    {
        return RegenerateTreeColumnsAsync(buildingId, commandRequest, CancellationToken.None);
    }

    /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    /// <exception cref="ApiException">A server side error occurred.</exception>
    public virtual async Task<CommandResult> RegenerateTreeColumnsAsync(string buildingId, CommandRequest commandRequest, CancellationToken cancellationToken)
    {
        if (buildingId == null)
            throw new ArgumentNullException("buildingId");

        if (commandRequest == null)
            throw new ArgumentNullException("commandRequest");

        var client_ = _httpClient;
        var disposeClient_ = false;
        try
        {
            using (var request_ = new HttpRequestMessage())
            {
                var json_ = JsonConvert.SerializeObject(commandRequest, JsonSerializerSettings);
                var content_ = new StringContent(json_);
                content_.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                request_.Content = content_;
                request_.Method = new HttpMethod("POST");
                request_.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));

                var urlBuilder_ = new StringBuilder();
                if (!string.IsNullOrEmpty(_baseUrl)) urlBuilder_.Append(_baseUrl);
                // Operation Path: "{buildingId}/regenerate"
                urlBuilder_.Append(Uri.EscapeDataString(ConvertToString(buildingId, System.Globalization.CultureInfo.InvariantCulture)));
                urlBuilder_.Append("/regenerate");

                PrepareRequest(client_, request_, urlBuilder_);

                var url_ = urlBuilder_.ToString();
                request_.RequestUri = new Uri(url_, UriKind.RelativeOrAbsolute);

                PrepareRequest(client_, request_, url_);

                var response_ = await client_.SendAsync(request_, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
                var disposeResponse_ = true;
                try
                {
                    var headers_ = new Dictionary<string, IEnumerable<string>>();
                    foreach (var item_ in response_.Headers)
                        headers_[item_.Key] = item_.Value;
                    if (response_.Content != null && response_.Content.Headers != null)
                    {
                        foreach (var item_ in response_.Content.Headers)
                            headers_[item_.Key] = item_.Value;
                    }

                    ProcessResponse(client_, response_);

                    var status_ = (int)response_.StatusCode;
                    if (status_ == 200)
                    {
                        var objectResponse_ = await ReadObjectResponseAsync<CommandResult>(response_, headers_, cancellationToken).ConfigureAwait(false);
                        if (objectResponse_.Object == null)
                        {
                            throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                        }
                        return objectResponse_.Object;
                    }
                    else
                    {
                        var responseData_ = response_.Content == null ? null : await response_.Content.ReadAsStringAsync().ConfigureAwait(false);
                        throw new ApiException("The HTTP status code of the response was not expected (" + status_ + ").", status_, responseData_, headers_, null);
                    }
                }
                finally
                {
                    if (disposeResponse_)
                        response_.Dispose();
                }
            }
        }
        finally
        {
            if (disposeClient_)
                client_.Dispose();
        }
    }

    protected struct ObjectResponseResult<T>
    {
        public ObjectResponseResult(T responseObject, string responseText)
        {
            this.Object = responseObject;
            this.Text = responseText;
        }

        public T Object { get; }

        public string Text { get; }
    }

    public bool ReadResponseAsString { get; set; }

    protected virtual async Task<ObjectResponseResult<T>> ReadObjectResponseAsync<T>(HttpResponseMessage response, IReadOnlyDictionary<string, IEnumerable<string>> headers, CancellationToken cancellationToken)
    {
        if (response == null || response.Content == null)
        {
            return new ObjectResponseResult<T>(default(T), string.Empty);
        }

        if (ReadResponseAsString)
        {
            var responseText = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            try
            {
                var typedBody = JsonConvert.DeserializeObject<T>(responseText, JsonSerializerSettings);
                return new ObjectResponseResult<T>(typedBody, responseText);
            }
            catch (JsonException exception)
            {
                var message = "Could not deserialize the response body string as " + typeof(T).FullName + ".";
                throw new ApiException(message, (int)response.StatusCode, responseText, headers, exception);
            }
        }
        else
        {
            try
            {
                using (var responseStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
                using (var streamReader = new System.IO.StreamReader(responseStream))
                using (var jsonTextReader = new JsonTextReader(streamReader))
                {
                    var serializer = JsonSerializer.Create(JsonSerializerSettings);
                    var typedBody = serializer.Deserialize<T>(jsonTextReader);
                    return new ObjectResponseResult<T>(typedBody, string.Empty);
                }
            }
            catch (JsonException exception)
            {
                var message = "Could not deserialize the response body stream as " + typeof(T).FullName + ".";
                throw new ApiException(message, (int)response.StatusCode, string.Empty, headers, exception);
            }
        }
    }

    private string ConvertToString(object value, System.Globalization.CultureInfo cultureInfo)
    {
        if (value == null)
        {
            return "";
        }

        if (value is Enum)
        {
            var name = Enum.GetName(value.GetType(), value);
            if (name != null)
            {
                var field = System.Reflection.IntrospectionExtensions.GetTypeInfo(value.GetType()).GetDeclaredField(name);
                if (field != null)
                {
                    var attribute = System.Reflection.CustomAttributeExtensions.GetCustomAttribute(field, typeof(System.Runtime.Serialization.EnumMemberAttribute))
                        as System.Runtime.Serialization.EnumMemberAttribute;
                    if (attribute != null)
                    {
                        return attribute.Value != null ? attribute.Value : name;
                    }
                }

                var converted = Convert.ToString(Convert.ChangeType(value, Enum.GetUnderlyingType(value.GetType()), cultureInfo));
                return converted == null ? string.Empty : converted;
            }
        }
        else if (value is bool)
        {
            return Convert.ToString((bool)value, cultureInfo).ToLowerInvariant();
        }
        else if (value is byte[])
        {
            return Convert.ToBase64String((byte[])value);
        }
        else if (value is string[])
        {
            return string.Join(",", (string[])value);
        }
        else if (value.GetType().IsArray)
        {
            var valueArray = (Array)value;
            var valueTextArray = new string[valueArray.Length];
            for (var i = 0; i < valueArray.Length; i++)
            {
                valueTextArray[i] = ConvertToString(valueArray.GetValue(i), cultureInfo);
            }
            return string.Join(",", valueTextArray);
        }

        var result = Convert.ToString(value, cultureInfo);
        return result == null ? "" : result;
    }

}

public partial class ApiException : Exception
{
    public int StatusCode { get; private set; }

    public string Response { get; private set; }

    public IReadOnlyDictionary<string, IEnumerable<string>> Headers { get; private set; }

    public ApiException(string message, int statusCode, string response, IReadOnlyDictionary<string, IEnumerable<string>> headers, Exception innerException)
        : base(message + "\n\nStatus: " + statusCode + "\nResponse: \n" + ((response == null) ? "(null)" : response.Substring(0, response.Length >= 512 ? 512 : response.Length)), innerException)
    {
        StatusCode = statusCode;
        Response = response;
        Headers = headers;
    }

    public override string ToString()
    {
        return string.Format("HTTP Response: \n\n{0}\n\n{1}", Response, base.ToString());
    }
}
#pragma warning restore 8601
#pragma warning restore 8604
#pragma warning restore 8618
#pragma warning restore 8625
