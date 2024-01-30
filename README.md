# NSubstitute-MockHttpMessageHandler

This is a quick and simple mock HttpMessageHandler to use in combination with NSubstitue.
It allows for setting up a response queue with prepared answers for use in unit-testing.

It will return enqueued HttpResponses from a queue (FIFO).
Depending on the flag on creation, the mock handler will throw errors if the requests do not match the enqueued requests signatures.
