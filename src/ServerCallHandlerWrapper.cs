using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using NewRelic.Agent.Api;
using NewRelic.Agent.Extensions.Providers.Wrapper;

namespace Custom.Providers.Wrapper.Grpc
{
    public class ServerCallHandlerWrapper : IWrapper
    {
        bool IWrapper.IsTransactionRequired => false;

        private const string AssemblyName = "Grpc.Core";
        private const string TypeName = "Grpc.Core.Server";
        private string[] MethodNames = new string[] {
                "HandleCallAsync"
        };

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
            transaction = agent.CreateTransaction(
                      isWeb: true,
                      category: EnumNameCache<WebTransactionType>.GetName(WebTransactionType.Custom),
                      transactionDisplayName: "gRPC",
                      doNotTrackAsUnitOfWork: true,
                      wrapperOnCreate: () => { });

            var segment = transaction.StartTransactionSegment(instrumentedMethodCall.MethodCall, instrumentedMethodCall.MethodCall.Method.MethodName);
            segment.AlwaysDeductChildDuration = true;

            if (instrumentedMethodCall.IsAsync)
            {
                transaction.AttachToAsync();
            }

            if (instrumentedMethodCall.MethodCall.MethodArguments.Length > 0)
            {
                var newRpc = instrumentedMethodCall.MethodCall.MethodArguments[0];
                object requestMetadata = newRpc?.GetType().GetProperty("RequestMetadata")?.GetValue(newRpc);
                if (requestMetadata != null)
                {
                    TryAcceptHeadersFromRequest(agent, requestMetadata);
                }
            }

            if (instrumentedMethodCall.IsAsync)
            {
                return Delegates.GetAsyncDelegateFor<Task>(agent, segment, true, onComplete: (t) =>
                {
                    segment.End();
                    transaction.End();
                });
            }
            else
            {
                return Delegates.GetDelegateFor(
                        onFailure: transaction.NoticeError,
                        onComplete: () =>
                        {
                            segment.End();
                            transaction.End();
                        });
            }
        }

        private static void TryAcceptHeadersFromRequest(IAgent agent, object requestMetadata)
        {
            Func<object, string, IEnumerable<string>> getHeader = (object carrier, string key) =>
            {
                string value = null;
                foreach (var entry in carrier as IEnumerable)
                {
                    var entryKey = entry?.GetType().GetProperty("Key")?.GetValue(entry);
                    if (key.Equals(entryKey))
                    {
                        var entryValue = entry?.GetType().GetProperty("Value")?.GetValue(entry);
                        if (entryValue != null)
                        {
                            value = entryValue as string;
                        }
                        break;
                    }
                }
                return value == null ? null : new string[] { value };
            };
            try
            {
                agent.CurrentTransaction.AcceptDistributedTraceHeaders(requestMetadata, getHeader, TransportType.Other);
            }
            catch (Exception ex)
            {
                agent.HandleWrapperException(ex);
            }
        }

    }
}
