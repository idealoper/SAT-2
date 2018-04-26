using System;

namespace SAT_2
{
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
}
