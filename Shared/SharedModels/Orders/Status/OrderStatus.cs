using System.Diagnostics.CodeAnalysis;
using System.Drawing;

namespace SharedModels.Orders.Status
{
    public enum KnownOrderStatus
    {
        Created,
        UnderConsideration,
        FailureToExecute,
        InDeveloping,
        Suspended,
        InImplementation,
        Completed
    }

    public readonly struct OrderStatus : IEquatable<OrderStatus>
    {
        public static readonly OrderStatus Empty;
        public static OrderStatus Created = new OrderStatus(KnownOrderStatus.Created, "Создан", Color.Lime);
        public static OrderStatus UnderConsideration = new OrderStatus(KnownOrderStatus.UnderConsideration, "На рассмотрении", Color.Gold);
        public static OrderStatus FailureToExecute = new OrderStatus(KnownOrderStatus.FailureToExecute, "Отказ в выполнении", Color.Red);
        public static OrderStatus InDeveloping = new OrderStatus(KnownOrderStatus.InDeveloping, "В разработке", Color.Orange);
        public static OrderStatus Suspended = new OrderStatus(KnownOrderStatus.Suspended, "Приостановлен", Color.LightGray);
        public static OrderStatus InImplementation = new OrderStatus(KnownOrderStatus.InImplementation, "На внедрении", Color.LightSkyBlue);
        public static OrderStatus Completed = new OrderStatus(KnownOrderStatus.Completed, "Выполнен", Color.CornflowerBlue);

        public readonly string Value;
        public readonly Color Color;
        public readonly short StatusId;
        internal OrderStatus(KnownOrderStatus knownOrderStatus, string value, Color color)
        {
            StatusId = unchecked((short)knownOrderStatus);
            Value = value;
            Color = color;
        }
        public static OrderStatus FromId(int orderStatus)
        {
            return (KnownOrderStatus)orderStatus switch
            {
                KnownOrderStatus.Created => Created,
                KnownOrderStatus.UnderConsideration => UnderConsideration,
                KnownOrderStatus.FailureToExecute => FailureToExecute,
                KnownOrderStatus.InDeveloping => InDeveloping,
                KnownOrderStatus.Suspended => Suspended,
                KnownOrderStatus.InImplementation => InImplementation,
                KnownOrderStatus.Completed => Completed,
                _ => Empty,
            };
        }
        public static bool operator ==(OrderStatus left, OrderStatus right) =>
            left.StatusId == right.StatusId
                && left.Value.Equals(right.Value, StringComparison.OrdinalIgnoreCase)
                && left.Color == right.Color;

        public static bool operator !=(OrderStatus left, OrderStatus right) => !(left == right);

        public override bool Equals([NotNullWhen(true)] object? obj) => obj is OrderStatus other && Equals(other);

        public bool Equals(OrderStatus other) => this == other;

        public override int GetHashCode()
        {
            return HashCode.Combine(Value.GetHashCode(), StatusId.GetHashCode());
        }
    }
}
