using Workify_Full.Models;

namespace Workify_Full.ViewModels
{
    public class ProfileViewModel
    {
        public ApplicationUser User { get; set; } = null!;
        public IList<Review> Reviews { get; set; } = new List<Review>();
        public decimal WalletAvailable { get; set; }
        public decimal WalletEscrow { get; set; }
        public string WalletCurrency { get; set; } = "EGP";
        public double AverageRating => Reviews.Count > 0
            ? Reviews.Average(r => r.Rating)
            : 0;
    }
}
