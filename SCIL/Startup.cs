using Microsoft.Extensions.DependencyInjection;
using SCIL.Flix;
using SCIL.Logger;
using SCIL.Processor;
using SCIL.Processor.FlixInstructionGenerators;
using SCIL.Processor.Nodes.Visitor;

namespace SCIL
{
    public class Startup
    {
        public static void ConfigureServices(IServiceCollection services, Configuration configuration, ILogger logger)
        {
            services
                .RegistrerAllTypes<IVisitor>()
                .RegistrerAllTypes<IFlixInstructionGenerator>()
                .AddSingleton<ControlFlowGraph>()
                .AddSingleton<FlixCodeGeneratorVisitor>()
                .AddSingleton<IFlixExecutor, FlixExecutor>()
                .AddSingleton(logger)
                .AddSingleton<FileProcessor>()
                .AddSingleton<ModuleProcessor>()
                .AddSingleton(configuration);
        }
    }
}
