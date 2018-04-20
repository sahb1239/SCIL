using Microsoft.Extensions.DependencyInjection;
using SCIL.Flix;
using SCIL.Logger;
using SCIL.Processor;
using SCIL.Processor.FlixInstructionGenerators;
using SCIL.Processor.Nodes.Visitor;

namespace SCIL
{
    class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .RegistrerAllTypes<IVisitor>()
                .RegistrerAllTypes<IFlixInstructionGenerator>()
                .AddSingleton<IFlixExecutor, FlixExecutor>()
                .AddSingleton<ILogger, ConsoleLogger>()
                .AddSingleton<ModuleProcessor>();
        }
    }
}
