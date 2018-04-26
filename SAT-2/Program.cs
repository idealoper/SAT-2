using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAT_2
{
    class Program
    {
        static void Main(string[] args)
        {
            var sat2 = new List<Dilemma>
            {
                new Dilemma("a", "!b"),
                new Dilemma("a", "b"),
                new Dilemma("b", "c"),
                new Dilemma("!c", "!a"),
                new Dilemma("a", "!d"),
                new Dilemma("d", "b"),
            };

            
            var implicationGraph = new Graph();

            foreach (var delemma in sat2)
            {
                implicationGraph.AddVertex(delemma.Condition1.GetInvertCondition());
                implicationGraph.AddVertex(delemma.Condition2.GetInvertCondition());

                implicationGraph.AddVertex(delemma.Condition1);
                implicationGraph.AddVertex(delemma.Condition2);

                implicationGraph.AddEdge(delemma.Condition1.GetInvertCondition(), delemma.Condition2);
                implicationGraph.AddEdge(delemma.Condition2.GetInvertCondition(), delemma.Condition1);
            }

            var copy = implicationGraph.GetCopy();
            
            var strongComponents = new List<Condition[]>();
            while (implicationGraph.Vertexes.Any())
            {
                var vertexInOuts = new List<Tuple<Condition, int, int>>();

                foreach (var v in implicationGraph.Vertexes)
                {
                    var currentVertexInOuts = implicationGraph.Dfs(v);
                    vertexInOuts.AddRange(currentVertexInOuts);
                }

                var strongComponentTopVertex = vertexInOuts.OrderByDescending(x => x.Item3).First().Item1;

                var transponatedCopy = implicationGraph.GetTransponate();
                var strongComponent = transponatedCopy.Dfs(strongComponentTopVertex).Select(x => x.Item1).ToArray();
                strongComponents.Add(strongComponent);
                foreach(var vertex in strongComponent)
                {
                    implicationGraph.RemoveVertex(vertex);
                }
            }

            bool decisionNotExists = false;
            var variables = new HashSet<string>();
            foreach (var strongComponent in strongComponents)
            {
                var currentVars = strongComponent.Select(x => x.VariableName).Distinct().ToArray();
                foreach (var currentVar in currentVars)
                    variables.Add(currentVar);

                if (strongComponent.Length != currentVars.Count())
                {
                    decisionNotExists = true;
                    break;
                   
                }
            }

            var disunctions = sat2.Select(x => $"({x.Condition1} || {x.Condition2})").ToArray();
            var expr = string.Join(" && ", disunctions);
            Console.WriteLine(expr);

            if (decisionNotExists)
            {
                Console.WriteLine("Решений нет");
            }
            else
            {
                Console.Write("Решение: ");
                foreach (var strongComponent in strongComponents.Reverse<Condition[]>())
                {
                    foreach(var item in strongComponent)
                    {
                        if(variables.Remove(item.VariableName))
                        {
                            Console.Write(item);
                            Console.Write(" ");
                        }

                        if (!variables.Any()) break;
                    }

                    if (!variables.Any()) break;
                }
            }

            Console.ReadKey(true);
        }

        
    }
}
