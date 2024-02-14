using System.Net;
using System.Text.Json;

namespace fgiele.github.TestHelper
{
    public class MockHttpMessageHandler : HttpMessageHandler
    { 
        private readonly Queue<Call> _calls = new Queue<Call>();
        private readonly List<Call> _receivedCalls = new List<Call>();
        private readonly bool _errorIfIncorrectCall;

        public MockHttpMessageHandler(bool errorIfIncorrectCall = false)
        {
            _errorIfIncorrectCall = errorIfIncorrectCall;
        }

        public int TimesCalled()
        {
            return _receivedCalls.Count;
        }

        public int TimesCalled(string requestUrl, HttpMethod method, string? args = null)
        {
            return _receivedCalls.Count(call => call.Args == args
                                    && call.Method == method
                                    && call.RequestUrl.Equals(requestUrl, StringComparison.InvariantCultureIgnoreCase));
        }

        public void AddMockResponse(string requestUrl, HttpMethod method, HttpContent? response, HttpStatusCode status, string? args = null)
        {
            _calls.Enqueue(new Call
            {
                Args = args,
                Method = method,
                RequestUrl = requestUrl,
                Response = response,
                StatusCode = status
            });
        }

        public void AddMockResponseFromObject(string requestUrl, HttpMethod method, object responseObject, HttpStatusCode status, string? args = null)
        {
            _calls.Enqueue(new Call
            {
                Args = args,
                Method = method,
                RequestUrl = requestUrl,
                Response = new StringContent(JsonSerializer.Serialize(responseObject)),
                StatusCode = status
            });
        }

        public void ClearQueue()
        {
            _calls.Clear();
            _receivedCalls.Clear();
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            if (_calls.Count == 0)
            {
                throw new ArgumentException($"Unexpected request ({request.RequestUri}), no mock responses available");
            }
            var expectedCall = _calls.Dequeue();

            if (_errorIfIncorrectCall)
            {
                if (request.RequestUri == null)
                {
                    throw new ArgumentException($"Unexpected empty request, expected {expectedCall.RequestUrl}");
                }
                if (request.Method != expectedCall.Method)
                {
                    throw new ArgumentException($"Unexpected {request.Method} request, expected {expectedCall.Method}");
                }
                var requestUrl = request.RequestUri.AbsolutePath;
                if (!requestUrl.Equals(expectedCall.RequestUrl, StringComparison.InvariantCultureIgnoreCase))
                {
                    throw new ArgumentException($"{requestUrl} does not match, expected {expectedCall.RequestUrl}");
                }

                if (request.Content == null && !string.IsNullOrEmpty(expectedCall.Args))
                {
                    throw new ArgumentException($"Unexpected empty body, expected {expectedCall.Args}");
                }
                if (request.Content != null)
                {
                    var requestArgs = await request.Content.ReadAsStringAsync(cancellationToken);

                    if (requestArgs != expectedCall.Args)
                    {
                        throw new ArgumentException($"{requestArgs} does not match {expectedCall.Args}");
                    }
                }
            }

            _receivedCalls.Add(new Call
            {
                Args = request.Content != null ? await request.Content.ReadAsStringAsync(cancellationToken) : null,
                Method = request.Method,
                RequestUrl = request.RequestUri?.AbsolutePath ?? string.Empty
            });

            return new HttpResponseMessage
            {
                StatusCode = expectedCall.StatusCode,
                Content = expectedCall.Response
            };
        }

        internal class Call()
        {
            public string? Args { get; set; }

            public HttpMethod Method { get; set; } = HttpMethod.Get;

            public string RequestUrl { get; set; } = String.Empty;

            public HttpContent? Response { get; set; }

            public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.NotFound;
        }
    }
}
