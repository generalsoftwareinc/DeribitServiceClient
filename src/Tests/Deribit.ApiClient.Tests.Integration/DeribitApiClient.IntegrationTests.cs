using Deribit.ApiClient.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;
using NuGet.Frameworks;

namespace Deribit.ApiClient.Tests.Integration
{
    [TestClass]
    public class DeribitApiClient_IntegrationTests
    {
        [TestMethod]
        [TestCategory("Logic"), TestCategory("Slow")]
        public async Task DeribitApiClient_RunAsync_starts_then_CancelToken_stops_multiple_times_then_DisposeAsync()
        {
            // This is a combined test method testing multiple steps for performance reasons:
            // 1. RunAsync connects and starts processing incoming and outgoing messages
            // 2. CancelToken passed to RunAsync can cancel it running and things get closed in a way that the same apiClient instance can re-run again
            // 3. Reset resets internal state and messge counters, so the next RunAsync will behave like it would be the first
            // 4. Calling RunAsync the 2nd time works like for the first time
            // 5. CancelToken passed to the 2nd RunAsync call can cancel it running the same way and the same apiClient instance can re-run again
            // 6. Calling RunAsync the 3nd time works like for the previous times, the only difference is that message counters are not starting from 0 because Reset was not called before this 3rd RunAsync call.
            // 7. DisposeAsync() stops it running and disposes resources
            // 8. Trying to Run after disposed should fail

            // Arrange
            int delayTimeMillisec = 200;
            var cancellationTokenSource = new CancellationTokenSource();
            await using (var apiClient = Utils.GetDeribitApiClient(logger: Utils.LoggerFactory.CreateLogger<DeribitApiClient>(LogLevel.Debug)))
            {
                Assert.IsFalse(apiClient.IsRunning);

                // Act: STEP 1: start the ApiClient
                var runTask = Task.Run(() => apiClient.RunAsync(cancellationTokenSource.Token));
                
                // wait some time to have some messages
                await Task.Delay(delayTimeMillisec);
                Assert.IsTrue(apiClient.IsRunning);
                Assert.IsTrue(apiClient.TotalReceivedMessagesCount > 0);


                // Act: STEP 2: ask it to cancel running
                cancellationTokenSource.Cancel();

                // wait for the apiClient to stop running
                await runTask;
                Assert.IsFalse(apiClient.IsRunning);
                Assert.IsTrue(apiClient.TotalReceivedMessagesCount > 0);


                // Act: STEP 3: reset
                apiClient.Reset();
                Assert.IsFalse(apiClient.IsRunning);
                Assert.AreEqual(0, apiClient.BookMessagesCount);
                Assert.AreEqual(0, apiClient.HeartBeatMessagesCount);
                Assert.AreEqual(0, apiClient.SubscriptionMessagesCount);
                Assert.AreEqual(0, apiClient.TickerMessagesCount);
                Assert.AreEqual(0, apiClient.TokenRefreshMessagesCount);
                Assert.AreEqual(0, apiClient.TotalReceivedMessagesCount);


                // Act: STEP 4: restart the same ApiClient instance
                cancellationTokenSource = new CancellationTokenSource();
                runTask = Task.Run(() => apiClient.RunAsync(cancellationTokenSource.Token));

                // wait some time to have some messages
                await Task.Delay(delayTimeMillisec);
                Assert.IsTrue(apiClient.IsRunning);
                Assert.IsTrue(apiClient.TotalReceivedMessagesCount > 0);


                // Act: STEP 5: ask it to cancel running
                cancellationTokenSource.Cancel();

                // wait for the apiClient to stop running
                await runTask;
                Assert.IsFalse(apiClient.IsRunning);
                Assert.IsTrue(apiClient.TotalReceivedMessagesCount > 0);

                
                // Act: STEP 6: restart the same ApiClient instance, but this time without Reset()
                var previousTotalReceivedMessagesCount = apiClient.TotalReceivedMessagesCount;
                cancellationTokenSource = new CancellationTokenSource();
                runTask = Task.Run(() => apiClient.RunAsync(cancellationTokenSource.Token));

                // wait some time to have some messages
                await Task.Delay(delayTimeMillisec);
                Assert.IsTrue(apiClient.IsRunning);
                Assert.IsTrue(apiClient.TotalReceivedMessagesCount > previousTotalReceivedMessagesCount);
                previousTotalReceivedMessagesCount = apiClient.TotalReceivedMessagesCount;


                // Act: STEP 7: Dispose
                await apiClient.DisposeAsync();
                Assert.IsFalse(apiClient.IsRunning);
                Assert.AreEqual(previousTotalReceivedMessagesCount, apiClient.TotalReceivedMessagesCount);


                // Act: STEP 8: Trying to Run after disposed should fail
                cancellationTokenSource = new CancellationTokenSource();
                await Assert.ThrowsExceptionAsync<ObjectDisposedException>(() => Task.Run(() => apiClient.RunAsync(cancellationTokenSource.Token)));


                // TODO assert that
                // - no token refresh or other timers run
                // - all internal queues are completed and no async tasks are waiting to provess incoming or outgoing messages
            }
        }

        [TestMethod]
        [TestCategory("Logic"), TestCategory("Slow")]
        public async Task DeribitApiClient_RunAsync_starts_then_DisconnectAsync_stops_multiple_times_then_Dispose()
        {
            // This is a combined test method testing multiple steps for performance reasons:
            // 1. RunAsync connects and starts processing incoming and outgoing messages
            // 2. DisconnectAsync can cancel it running and things get closed in a way that the same apiClient instance can re-run again
            // 3. DisconnectAsync again does nothing
            // 4. Reset resets internal state and messge counters, so the next RunAsync will behave like it would be the first
            // 5. Calling RunAsync the 2nd time works like for the first time
            // 6. DisconnectAsync can cancel it running the same way and the same apiClient instance can re-run again
            // 7. Calling RunAsync the 3nd time works like for the previous times, the only difference is that message counters are not starting from 0 because Reset was not called before this 3rd RunAsync call.
            // 8. Dispose() stops it running and disposes resources
            // 9. Trying to Run after disposed should fail

            // Arrange
            int delayTimeMillisec = 200;
            using (var apiClient = Utils.GetDeribitApiClient(logger: Utils.LoggerFactory.CreateLogger<DeribitApiClient>(LogLevel.Debug)))
            {
                Assert.IsFalse(apiClient.IsRunning);

                // Act: STEP 1: start the ApiClient
                var runTask = Task.Run(() => apiClient.RunAsync(CancellationToken.None));

                // wait some time to have some messages
                await Task.Delay(delayTimeMillisec);
                Assert.IsTrue(apiClient.IsRunning);
                Assert.IsTrue(apiClient.TotalReceivedMessagesCount > 0);


                // Act: STEP 2: ask it to disconnect
                await apiClient.DisconnectAsync(CancellationToken.None);

                // wait for the apiClient to stop running
                await runTask;
                Assert.IsFalse(apiClient.IsRunning);
                Assert.IsTrue(apiClient.TotalReceivedMessagesCount > 0);


                // Act: STEP 3: ask it to disconnect again after disconnected, should not fail
                await apiClient.DisconnectAsync(CancellationToken.None);
                Assert.IsFalse(apiClient.IsRunning);
                Assert.IsTrue(apiClient.TotalReceivedMessagesCount > 0);


                // Act: STEP 4: reset
                apiClient.Reset();
                Assert.IsFalse(apiClient.IsRunning);
                Assert.AreEqual(0, apiClient.BookMessagesCount);
                Assert.AreEqual(0, apiClient.HeartBeatMessagesCount);
                Assert.AreEqual(0, apiClient.SubscriptionMessagesCount);
                Assert.AreEqual(0, apiClient.TickerMessagesCount);
                Assert.AreEqual(0, apiClient.TokenRefreshMessagesCount);
                Assert.AreEqual(0, apiClient.TotalReceivedMessagesCount);


                // Act: STEP 5: restart the same ApiClient instance
                runTask = Task.Run(() => apiClient.RunAsync(CancellationToken.None));

                // wait some time to have some messages
                await Task.Delay(delayTimeMillisec);
                Assert.IsTrue(apiClient.IsRunning);
                Assert.IsTrue(apiClient.TotalReceivedMessagesCount > 0);


                // Act: STEP 6: ask it to disconnect
                await apiClient.DisconnectAsync(CancellationToken.None);

                // wait for the apiClient to stop running
                await runTask;
                Assert.IsFalse(apiClient.IsRunning);
                Assert.IsTrue(apiClient.TotalReceivedMessagesCount > 0);


                // Act: STEP 7: restart the same ApiClient instance, but this time without Reset()
                var previousTotalReceivedMessagesCount = apiClient.TotalReceivedMessagesCount;
                runTask = Task.Run(() => apiClient.RunAsync(CancellationToken.None));

                // wait some time to have some messages
                await Task.Delay(delayTimeMillisec);
                Assert.IsTrue(apiClient.IsRunning);
                Assert.IsTrue(apiClient.TotalReceivedMessagesCount > previousTotalReceivedMessagesCount);
                previousTotalReceivedMessagesCount = apiClient.TotalReceivedMessagesCount;


                // Act: STEP 8: Dispose
                apiClient.Dispose(); // intentionally using Dispose() and not DisposeAsync() here!
                Assert.IsFalse(apiClient.IsRunning);
                Assert.AreEqual(previousTotalReceivedMessagesCount, apiClient.TotalReceivedMessagesCount);


                // Act: STEP 9: Trying to Run after disposed should fail
                await Assert.ThrowsExceptionAsync<ObjectDisposedException>(() => Task.Run(() => apiClient.RunAsync(CancellationToken.None)));


                // TODO assert that
                // - no token refresh or other timers run
                // - all internal queues are completed and no async tasks are waiting to provess incoming or outgoing messages
            }
        }
    }
}