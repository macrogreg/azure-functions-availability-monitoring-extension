using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Host.Bindings;

namespace Microsoft.Azure.WebJobs.Extensions.AvailabilityMonitoring
{
    internal class ConstantValueProvider<TBoundValueType> : IValueProvider
    {
        public static Type BoundValueType { get { return typeof(TBoundValueType); } }

        private readonly Task<object> _boundValue;

        public ConstantValueProvider(TBoundValueType boundValue)
        {
            _boundValue = Task.FromResult((object) boundValue);
        }

        Type IValueProvider.Type
        {
            get { return BoundValueType; }
        }

        Task<object> IValueProvider.GetValueAsync()
        {
            return _boundValue;
        }

        string IValueProvider.ToInvokeString()
        {
            // @ToDo
            throw new NotImplementedException();
        }
    }
}