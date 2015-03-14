namespace SmugMug.Api
{
    public enum AccountTypeEnum
    {
        Pro,
        Power,
        Standard,
        Portfolio
    };

    public enum AccountStatusEnum
    {
        Active,
        Expired,
    };

	public struct Account
	{
		public User User;
	}
}
