using System;
namespace Crowdfunding.DTO
{
	public class PaymentViewModel
	{

		public Guid ProjectID { get; set; }
		public decimal Amount { get; set; }
		public Guid? RewardID { get; set; }
		public string PublicKey { get; set; }
		public string SecretKey { get; set; }


	}
}

