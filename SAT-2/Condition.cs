using System;
using System.Collections.Generic;

namespace SAT_2
{
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
}
