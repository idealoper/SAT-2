using System;
using System.Collections.Generic;
using System.Linq;

namespace SAT_2
{
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
}
