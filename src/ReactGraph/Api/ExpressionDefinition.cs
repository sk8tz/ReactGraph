using System;
using System.Linq.Expressions;
using ReactGraph.Construction;
using ReactGraph.Graph;
using ReactGraph.NodeInfo;

namespace ReactGraph.Api
{
    internal class ExpressionDefinition<T> : IExpressionDefinition<T>
    {
        readonly FormulaDescriptor<T> formulaDescriptor;
        readonly string expressionId;
        readonly ExpressionParser expressionParser;
        readonly DirectedGraph<INodeInfo> graph;
        readonly NodeRepository repo;
        readonly INodeInfo<T> formulaNode;
        Edge<INodeInfo> edge;

        public ExpressionDefinition(
            FormulaDescriptor<T> formulaDescriptor, 
            string expressionId,
            ExpressionParser expressionParser, 
            DirectedGraph<INodeInfo> graph, 
            NodeRepository repo)
        {
            this.formulaDescriptor = formulaDescriptor;
            this.expressionId = expressionId;
            this.expressionParser = expressionParser;
            this.graph = graph;
            this.repo = repo;
            formulaNode = (INodeInfo<T>) formulaDescriptor.GetOrCreateNodeInfo(repo);
            AddDependenciesToGraph(formulaDescriptor);
        }

        public IMemberDefinition Bind<TProp>(Expression<Func<TProp>> targetProperty, Action<Exception> onError, string propertyId = null)
        {
            if (edge != null)
                throw new InvalidOperationException("You can only bind a single expression to a property");

            var targetPropertyDescriptor = expressionParser.GetTargetInfo(targetProperty);
            var targetNode = targetPropertyDescriptor.GetOrCreateWritableNodeInfo(repo);

            var valueSource = formulaNode as IValueSource<TProp>;
            if (valueSource == null)
            {
                var message = string.Format("Cannot bind target of type {0} to source of type {1}", typeof(TProp), typeof(T));
                throw new ArgumentException(message);
            }

            targetNode.SetSource(valueSource, onError);

            edge = graph.AddEdge(formulaNode, targetNode, expressionId, propertyId);
            return new MemberDefinition(edge.Target);
        }

        public IMemberDefinition Action(Expression<Action<T>> action, Action<Exception> onError, string actionId = null)
        {
            string nodeLabel = ExpressionStringBuilder.ToString(action);
            return Action(action.Compile(), nodeLabel, onError, actionId);
        }

        public IMemberDefinition Action(Action<T> action, string nodeLabel, Action<Exception> onError, string actionId = null)
        {
            var actionNode = new ActionNodeInfo<T>(action, nodeLabel);
            actionNode.SetSource(formulaNode, onError);

            edge = graph.AddEdge(formulaNode, actionNode, expressionId, actionId);
            return new MemberDefinition(edge.Target);
        }

        private void AddDependenciesToGraph(DependencyDescriptor descriptor)
        {
            foreach (var dependency in descriptor.Dependencies)
            {
                graph.AddEdge(dependency.GetOrCreateNodeInfo(repo), formulaNode, null, descriptor == formulaDescriptor ? expressionId : null);
                AddDependenciesToGraph(dependency);
            }
        }
    }
}