# NSubstitute-MockHttpMessageHandler

This is a quick and simple mock HttpMessageHandler to use in combination with NSubstitue.
It allows for setting up a response queue with prepared answers for use in unit-testing.

It will return enqueued HttpResponses from a queue (FIFO).
Depending on the flag on creation, the mock handler will throw errors if the requests do not match the enqueued requests signatures.

To use the MockHttpMessageHandler (and thus mocking the HttpClient request-response interaction), use the following in the test setup:
```csharp
var mockHttpMessageHandler = new MockHttpMessageHandler(true);
var httpClient = new HttpClient(_mockHttpMessageHandler);
```
The handler can then be cleared and preloaded:
```csharp
// Clear everything in the MessageHandler queue
mockHttpMessageHandler.ClearQueue();
// Add a Json response based on a provided object (Json serialized)
mockHttpMessageHandler.AddMockResponseFromObject($"/api/employee", HttpMethod.Get, new List<Employee> { testEmployee }, HttpStatusCode.OK);
 
// Or add a specific HttpResponse, for instance, a stream
var stream = new MemoryStream(ASCIIEncoding.Default.GetBytes("TestMessage"));
var streamContent = new StreamContent(stream);
mockHttpMessageHandler.AddMockResponse($"/api/downloads/{report.Id}", HttpMethod.Get, streamContent, HttpStatusCode.OK);
```
