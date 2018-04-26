using System;
using System.Collections.Generic;

namespace SAT_2
{
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
