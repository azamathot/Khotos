using System.Diagnostics.CodeAnalysis;
using System.Drawing;

namespace SharedModels.Orders.Status
{
    public enum KnownPaymentStatus
    {
        NotAssigned,
        NotPaid,
        PartiallyPaid,
        FullyPaid
    }

    public readonly struct PaymentStatus : IEquatable<PaymentStatus>
    {
        public static readonly PaymentStatus Empty;
        public static PaymentStatus NotAssigned = new PaymentStatus(Status.KnownPaymentStatus.NotAssigned, "Не назначена", Color.Gray);
        public static PaymentStatus NotPaid = new PaymentStatus(Status.KnownPaymentStatus.NotPaid, "Не оплачена", Color.Red);
        public static PaymentStatus PartiallyPaid = new PaymentStatus(Status.KnownPaymentStatus.PartiallyPaid, "Частично оплачена", Color.Orange);
        public static PaymentStatus FullyPaid = new PaymentStatus(Status.KnownPaymentStatus.FullyPaid, "Полностью оплачена", Color.LimeGreen);

        public readonly short KnownPaymentStatus;
        public readonly string Value;
        public readonly Color Color;
        public readonly int StatusId => KnownPaymentStatus;
        internal PaymentStatus(KnownPaymentStatus knownOrderPayment, string value, Color color)
        {
            KnownPaymentStatus = unchecked((short)knownOrderPayment);
            Value = value;
            Color = color;
        }
        public static PaymentStatus FromId(int paymentStatus)
        {
            return (KnownPaymentStatus)paymentStatus switch
            {
                Status.KnownPaymentStatus.NotAssigned => NotAssigned,
                Status.KnownPaymentStatus.NotPaid => NotPaid,
                Status.KnownPaymentStatus.PartiallyPaid => PartiallyPaid,
                Status.KnownPaymentStatus.FullyPaid => FullyPaid,
                _ => Empty,
            };
        }
        public static bool operator ==(PaymentStatus left, PaymentStatus right) =>
            left.KnownPaymentStatus == right.KnownPaymentStatus
                && left.Value.Equals(right.Value, StringComparison.OrdinalIgnoreCase)
                && left.Color == right.Color;

        public static bool operator !=(PaymentStatus left, PaymentStatus right) => !(left == right);
        public override bool Equals([NotNullWhen(true)] object? obj) => obj is OrderStatus other && Equals(other);

        public bool Equals(PaymentStatus other) => this == other;

        public override int GetHashCode()
        {
            return HashCode.Combine(Value.GetHashCode(), KnownPaymentStatus.GetHashCode());
        }
    }
}

