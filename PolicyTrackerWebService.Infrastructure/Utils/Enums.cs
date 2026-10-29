namespace PolicyTrackerWebService.Infrastructure.Utils
{
    public static class Enums
    {
        public enum Role
        {
            Admin = 1,
            Staff = 2,
        }

        public enum BusinessType
        {
            Fresh = 1,
            Renewal = 2,
            Portability = 3,
            Rollover = 4,
            Endorsement = 5,
        }

        /// <summary>Outcome of the renewal of a policy term.</summary>
        public enum RenewalStatus
        {
            Due = 1,
            Quote_Sent = 2,
            Follow_Up = 3,
            Renewed = 4,
            Lost = 5,
        }

        public enum LostReason
        {
            Client_Declined = 1,
            Changed_Broker = 2,
            Went_Direct = 3,
            Ported_Out = 4,
            Business_Closed = 5,
            Vehicle_Sold = 6,
            Other = 7,
        }

        public enum PaymentMode
        {
            Cheque = 1,
            UPI = 2,
            Cash = 3,
            NEFT_RTGS = 4,
            Online_By_Client = 5,
        }

        public enum StatementStatus
        {
            Found = 1,
            Missing = 2,
            Other_POS_Code = 3,
            Overdue = 4,
            Awaiting_Statement = 5,
            Not_Applicable = 6,
        }

        public enum MatchStatus
        {
            Matched_Policy_No = 1,
            Matched_Name_Date = 2,
            Manual = 3,
            Unmatched = 4,
        }
    }
}
