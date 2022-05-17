namespace NewsletterApp.DTO
{
    
    // Root myDeserializedClass = JsonConvert.DeserializeObject<SubscribersResponse>(myJsonResponse);
    public class CustomFields
    {
    }

    public class Metadata
    {
        public string self { get; set; }
    }

    public class Result
    {
        public string address_line_1 { get; set; }
        public string address_line_2 { get; set; }
        public object alternate_emails { get; set; }
        public string city { get; set; }
        public string country { get; set; }
        public string email { get; set; }
        public string first_name { get; set; }
        public string id { get; set; }
        public string last_name { get; set; }
        public List<string> list_ids { get; set; }
        public string postal_code { get; set; }
        public string state_province_region { get; set; }
        public string phone_number { get; set; }
        public string whatsapp { get; set; }
        public string line { get; set; }
        public string facebook { get; set; }
        public string unique_name { get; set; }
        public Metadata _metadata { get; set; }
        public CustomFields custom_fields { get; set; }
        public DateTime created_at { get; set; }
        public string updated_at { get; set; }
    }

    public class SubscribersResponse
    {
        public List<Result> result { get; set; }
        public int contact_count { get; set; }
        public Metadata _metadata { get; set; }
    }


}