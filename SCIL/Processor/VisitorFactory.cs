using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using SCIL.Processor;
using SCIL.Processor.Nodes.Visitor;

namespace SCIL
{
    public class VisitorFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public VisitorFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IEnumerable<IVisitor> GetVisitors()
        {
            return _serviceProvider.GetServices<IVisitor>().Select(visitor => new
                {
                    visitor,
                    attribute = visitor.GetType().GetCustomAttribute<RegistrerVisitorAttribute>()
                }).Where(e => e.attribute != null)
                .Where(e => !e.attribute.Ignored)
                .OrderBy(e => e.attribute.Order)
                .Select(e => e.visitor);
        }
    }
}