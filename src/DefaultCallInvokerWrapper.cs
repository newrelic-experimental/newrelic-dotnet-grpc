using System;
using System.Reflection;
using System.Threading.Tasks;
using NewRelic.Agent.Api;
using NewRelic.Agent.Extensions.Logging;
using NewRelic.Agent.Extensions.Providers.Wrapper;

namespace Custom.Providers.Wrapper.Grpc
{
    public class DefaultCallInvokerWrapper : IWrapper
    {
        private const string AssemblyName = "Grpc.Core";
        private const string TypeName = "Grpc.Core.DefaultCallInvoker";
        private string[] MethodNames = new string[]{
                "BlockingUnaryCall", "AsyncUnaryCall", "AsyncServerStreamingCall", "AsyncClientStreamingCall", "AsyncDuplexStreamingCall"
        };

        bool IWrapper.IsTransactionRequired => true;

        public CanWrapResponse CanWrap(InstrumentedMethodInfo instrumentedMethodInfo)
        {
            var method = instrumentedMethodInfo.Method;
            var canWrap = method.MatchesAny(
                assemblyNames: new[] { AssemblyName },
                typeNames: new[] { TypeName },
                methodNames: MethodNames
            );

            return new CanWrapResponse(canWrap);
        }

        public AfterWrappedMethodDelegate BeforeWrappedMethod(InstrumentedMethodCall instrumentedMethodCall, IAgent agent, ITransaction transaction)
        {
            if (instrumentedMethodCall.IsAsync)
            {
                transaction.AttachToAsync();
            }

            if (instrumentedMethodCall.MethodCall.MethodArguments.Length > 2)
            {
                var callOptions = instrumentedMethodCall.MethodCall.MethodArguments[2];
                if (callOptions != null)
                {
                    TryAttachHeadersToRequest(agent, callOptions);
                }
            }

            var segment = transaction.StartMethodSegment(instrumentedMethodCall.MethodCall, instrumentedMethodCall.MethodCall.Method.Type.ToString(), instrumentedMethodCall.MethodCall.Method.MethodName);

            if (instrumentedMethodCall.IsAsync)
            {
                return Delegates.GetAsyncDelegateFor<Task>(agent, segment);
            }
            else
            {
                return Delegates.GetDelegateFor(segment);
            }
        }

        private static void TryAttachHeadersToRequest(IAgent agent, object callOptions)
        {
            var headers = callOptions?.GetType().GetProperty("Headers")?.GetValue(callOptions);
            if (headers == null)
            {
                FieldInfo headersFieldInfo = callOptions?.GetType().GetField("headers", BindingFlags.NonPublic | BindingFlags.Instance);
                object newMetadata = Activator.CreateInstance(headersFieldInfo.FieldType);
                if (newMetadata != null)
                {
                    headersFieldInfo?.SetValue(callOptions, newMetadata);
                    headers = callOptions?.GetType().GetProperty("Headers")?.GetValue(callOptions);
                }
                else
                {
                    return;
                }
            }

            var setHeaders = new Action<object, string, string>((carrier, key, value) =>
            {
                carrier.GetType().GetMethod("Add", new Type[] { typeof(string), typeof(string) })?.Invoke(carrier, new object[] { key, value });
            });

            try
            {
                agent.CurrentTransaction.InsertDistributedTraceHeaders<object>(headers, setHeaders);
            }
            catch (Exception ex)
            {
                agent.HandleWrapperException(ex);
            }
        }
    }
}
