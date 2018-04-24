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

    class Dilemma
    {
        public Condition Condition1 { get; private set; }

        public Condition Condition2 { get; private set; }

        public Dilemma(Condition condition1, Condition condition2)
        {
            Condition1 = condition1 ?? throw new ArgumentNullException(nameof(condition1));
            Condition2 = condition2 ?? throw new ArgumentNullException(nameof(condition2));
        }
    }

    class Condition : IEquatable<Condition>
    {
        private string _expr;
        public string VariableName => _expr.TrimStart('!');

        public bool IsNegative => _expr.StartsWith("!");

        public Condition(string expr)
        {
            if (string.IsNullOrWhiteSpace(expr))
                throw new ArgumentException(nameof(expr));
            if (expr.Length - expr.Replace("!", "").Length > 1)
                throw new ArgumentException(nameof(expr));
            _expr = expr.Trim();

        }

        public static implicit operator Condition(string expr)
        {
            return new Condition(expr);
        }

        public Condition GetInvertCondition()
        {
            if (IsNegative)
                return new Condition(VariableName);
            else
                return new Condition("!" + VariableName);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Condition);
        }

        public bool Equals(Condition other)
        {
            return other != null &&
                   string.Compare(_expr, other._expr, ignoreCase: true) == 0;
                   
        }

        public override int GetHashCode()
        {
            var hashCode = 1589808793;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(_expr.ToUpperInvariant());
            return hashCode;
        }

        public static bool operator ==(Condition condition1, Condition condition2)
        {
            return EqualityComparer<Condition>.Default.Equals(condition1, condition2);
        }

        public static bool operator !=(Condition condition1, Condition condition2)
        {
            return !(condition1 == condition2);
        }

        public override string ToString()
        {
            return _expr;
        }
    }

    class Graph
    {
        private HashSet<Condition> _vertexes = new HashSet<Condition>();
        private Dictionary<Condition, HashSet<Condition>> _edges = new Dictionary<Condition, HashSet<Condition>>();
        private Dictionary<Condition, HashSet<Condition>> _linksMap = new Dictionary<Condition, HashSet<Condition>>();

        public Condition[] Vertexes => _vertexes.ToArray();

        public Edge[] Edges => _edges.SelectMany(x => x.Value.Select(z => new Edge(x.Key, z))).ToArray();

        public Edge[] this[Condition vertex] => _edges.ContainsKey(vertex) ? _edges[vertex].Select(x => new Edge(vertex, x)).ToArray() : new Edge[0];

        public bool AddVertex(Condition vertex)
        {
            if (vertex == null)
                throw new ArgumentNullException(nameof(vertex));
            if (_vertexes.Add(vertex))
            {
                _edges.Add(vertex, new HashSet<Condition>());
                _linksMap.Add(vertex, new HashSet<Condition>());
                return true;
            }

            return false;
        }

        public bool RemoveVertex(Condition vertex)
        {
            if (vertex == null)
                throw new ArgumentNullException(nameof(vertex));
            if(_vertexes.Remove(vertex))
            {
                foreach(var nextVertex in _edges[vertex])
                {
                    _linksMap[nextVertex].Remove(vertex);
                }

                foreach(var prevVertex in _linksMap[vertex])
                {
                    _edges[prevVertex].Remove(vertex);
                }

                _edges.Remove(vertex);
                _linksMap.Remove(vertex);

                return true;
            }
            return false;
        }

        public bool AddEdge(Condition srcVertex, Condition dstVertex)
        {
            if (srcVertex == null)
                throw new ArgumentNullException(nameof(srcVertex));
            if (dstVertex == null)
                throw new ArgumentNullException(nameof(dstVertex));
            if (!_vertexes.Contains(srcVertex))
                throw new ArgumentException(nameof(srcVertex));
            if (!_vertexes.Contains(dstVertex))
                throw new ArgumentException(nameof(dstVertex));

            if (_edges[srcVertex].Add(dstVertex))
            {
                _linksMap[dstVertex].Add(srcVertex);
                return true;
            }

            return false;
        }

        public bool RemoveEdge(Condition srcVertex, Condition dstVertex)
        {
            if (srcVertex == null)
                throw new ArgumentNullException(nameof(srcVertex));
            if (dstVertex == null)
                throw new ArgumentNullException(nameof(dstVertex));
            if (!_vertexes.Contains(srcVertex))
                throw new ArgumentException(nameof(srcVertex));
            if (!_vertexes.Contains(dstVertex))
                throw new ArgumentException(nameof(dstVertex));

            if (_edges.ContainsKey(srcVertex))
            {
                _edges[srcVertex].Add(dstVertex);
                _linksMap[dstVertex].Add(srcVertex);
                return true;
            }

            return false;
        }

        public Graph GetTransponate()
        {
            return new Graph
            {
                _vertexes = new HashSet<Condition>(_vertexes.ToArray()),
                _edges = _linksMap.ToDictionary(x => x.Key, x => new HashSet<Condition>(x.Value)),
                _linksMap = _edges.ToDictionary(x => x.Key, x => new HashSet<Condition>(x.Value))
            };
        }

        public Graph GetCopy()
        {
            return new Graph
            {
                _vertexes = new HashSet<Condition>(_vertexes.ToArray()),
                _linksMap = _linksMap.ToDictionary(x => x.Key, x => new HashSet<Condition>(x.Value)),
                _edges = _edges.ToDictionary(x => x.Key, x => new HashSet<Condition>(x.Value))
            };
        }

        public IEnumerable<Tuple<Condition, int, int>> Dfs(Condition startVertex)
        {
            var vertexInOuts = new Dictionary<Condition, Tuple<int, int?>>();
            int time = 0;
            var bypassGraph = new Stack<Tuple<Condition, Edge[], int>>();
            bypassGraph.Push(Tuple.Create(startVertex, this[startVertex], -1));
            vertexInOuts.Add(startVertex, Tuple.Create(time, (int?)null));
            while (bypassGraph.Any())
            {
                time++;

                var tuple = bypassGraph.Pop();
                var vertex = tuple.Item1;
                var edges = tuple.Item2;
                var currentEdgeIndex = tuple.Item3 + 1;

                if (currentEdgeIndex < edges.Length)
                {
                    bypassGraph.Push(Tuple.Create(vertex, edges, currentEdgeIndex));

                    var nextVertex = edges[currentEdgeIndex].Destination;
                    if (!vertexInOuts.ContainsKey(nextVertex))
                    {

                        vertexInOuts.Add(nextVertex, Tuple.Create(time, (int?)null));
                        bypassGraph.Push(Tuple.Create(nextVertex, this[nextVertex], -1));
                    }
                }
                else
                {
                    vertexInOuts[vertex] = Tuple.Create(vertexInOuts[vertex].Item1, (int?)time);
                }
            }

            return vertexInOuts.Select(x => Tuple.Create(x.Key, x.Value.Item1, x.Value.Item2.Value));
        }
    }

    class Edge : IEquatable<Edge>
    {
        public Condition Source { get; }

        public Condition Destination { get; }

        public Edge(Condition source, Condition destination)
        {
            Source = source ?? throw new ArgumentNullException(nameof(source));
            Destination = destination ?? throw new ArgumentNullException(nameof(destination));
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Edge);
        }

        public bool Equals(Edge other)
        {
            return other != null &&
                   EqualityComparer<Condition>.Default.Equals(Source, other.Source) &&
                   EqualityComparer<Condition>.Default.Equals(Destination, other.Destination);
        }

        public override int GetHashCode()
        {
            var hashCode = 1918477335;
            hashCode = hashCode * -1521134295 + EqualityComparer<Condition>.Default.GetHashCode(Source);
            hashCode = hashCode * -1521134295 + EqualityComparer<Condition>.Default.GetHashCode(Destination);
            return hashCode;
        }

        public static bool operator ==(Edge edge1, Edge edge2)
        {
            return EqualityComparer<Edge>.Default.Equals(edge1, edge2);
        }

        public static bool operator !=(Edge edge1, Edge edge2)
        {
            return !(edge1 == edge2);
        }

        public override string ToString()
        {
            return Source.ToString() + " -> " + Destination.ToString();
        }
    }
}
