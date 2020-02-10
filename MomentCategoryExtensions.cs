using System;

namespace WhatsAppGroupAnalysis
{
    static class MomentCategoryExtensions
    {
        public static MomentCategory GetCategory(this DateTime dt)
        {
            var h = dt.Hour;

            if (h >= 0 && h < 6)
            {
                return MomentCategory.Corujão;
            }
            if (h >= 6 && h < 12)
            {
                return MomentCategory.Morning;
            }
            if (h >= 12 && h < 19)
            {
                return MomentCategory.Afternoon;
            }
            if (h > 19 && h < 23)
            {
                return MomentCategory.Night;
            }
            
            return MomentCategory.Corujão;
            
        }
    }
}
