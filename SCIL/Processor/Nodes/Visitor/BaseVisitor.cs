using System.Linq;

namespace SCIL.Processor.Nodes.Visitor
{
    public class BaseVisitor : IVisitor
    {
        public virtual void Visit(Block block)
        {
            // ReSharper disable once ForCanBeConvertedToForeach
            // Needs to be for since replace will throw a exception if not
            for (int i = 0; i < block.Nodes.Count; i++)
            {
                var node = block.Nodes.ElementAt(i);
                node.Accept(this);
            }
        }

        public virtual void Visit(Node node)
        {
        }

        public virtual void Visit(Method method)
        {
            // ReSharper disable once ForCanBeConvertedToForeach
            // Needs to be for since replace will throw a exception if not
            for (int i = 0; i < method.Blocks.Count; i++)
            {
                var block = method.Blocks.ElementAt(i);
                block.Accept(this);
            }
        }

        public virtual void Visit(Type type)
        {
            // ReSharper disable once ForCanBeConvertedToForeach
            // Needs to be for since replace will throw a exception if not
            for (int i = 0; i < type.Methods.Count; i++)
            {
                var method = type.Methods.ElementAt(i);
                method.Accept(this);
            }
        }

        public virtual void Visit(Module module)
        {
            // ReSharper disable once ForCanBeConvertedToForeach
            // Needs to be for since replace will throw a exception if not
            for (int i = 0; i < module.Types.Count; i++)
            {
                var type = module.Types.ElementAt(i);
                type.Accept(this);
            }
        }
    }
}