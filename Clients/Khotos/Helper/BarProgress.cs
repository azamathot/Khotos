using BlazorBootstrap;

namespace Khotos.Helper
{
    public class BarProgress
    {
        public static ProgressColor GetProgressColor(double w)
        {
            if (w > 66 && w <= 100)
            {
                return ProgressColor.Success;
            }
            else if (w <= 66 && w > 33)
            {
                return ProgressColor.Warning;
            }
            else if (w <= 33 && w > 0)
            {
                return ProgressColor.Secondary;
            }
            else
            {
                return ProgressColor.Danger;
            }
        }

        public static ProgressType GetProgressType(double w)
        {
            if (w == 100)
            {
                return ProgressType.Striped;
            }
            else if (w > 0 && w < 100)
            {
                return ProgressType.StripedAndAnimated;
            }
            else
            {
                return ProgressType.Striped;
            }
        }
    }
}
