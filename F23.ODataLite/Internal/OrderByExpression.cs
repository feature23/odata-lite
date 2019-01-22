using System;

namespace F23.ODataLite.Internal
{
    internal class OrderByExpression
    {
        public OrderByExpression(string rawValue)
        {
            var parts = rawValue.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);

            Property = parts[0];

            if (parts.Length > 1)
            {
                Descending = parts[1].Equals("desc", StringComparison.OrdinalIgnoreCase);
            }
        }

        public string Property { get; }

        public bool Descending { get; }
    }
}
