using System;
using System.Reflection;

namespace CurveChartControl
{
	public class AboutData
	{
		public string IntendedUseTitle { get; set; }
		public string IntendedUse { get; set; }
		public string ProductInformationTitle { get; set; }
        public string ProductInfo { get; set; }
        public string ProductInstructions { get; set; }
        public string ProductInfoVersion { get; set; }
        public string ProductUDITitle { get; set; }
        public string ProductUDINumber { get; set; }
		public string ProductCopyright { get; set; }
		public string ProductPatentsDisclaimer { get; set; }
		public string ProductPatentsTitle { get; set; }
		public string Patent1 { get; set; }
		public string Patent2 { get; set; }
		public string Patent3 { get; set; }
		public string Patent4 { get; set; }
		public string Patent5 { get; set; }
        public string Patent6 { get; set; }
        public string Patent7 { get; set; }
        public string Patent8 { get; set; }
        public string ContactInformationTitle { get; set; }
        public string SupportAndInquiriesTitle { get; set; }
		public string Company { get; set; }
		public string Address { get; set; }
		public string City { get; set; }
		public string Province { get; set; }
		public string Country { get; set; }
		public string PostalCode { get; set; }
        public string PhoneTitle { get; set; }
        public string Phone { get; set; }
        public string PhoneTechnicalTitle { get; set; }
        public string PhoneTechnical1 { get; set; }
        public string PhoneTechnical2 { get; set; }
        public string EmailTechnicalTitle { get; set; }
        public string EmailTechnical { get; set; }
        public string FaxTitle { get; set; }
        public string Fax { get; set; }
        public string WebsiteTitle { get; set; }
        public string WebsiteURL { get; set; }
        public string EmailTitle { get; set; }
        public string Email { get; set; }
        public bool IsPowerByPeriGen { get; set; }
		public bool IsPerigen { get { return !this.IsPowerByPeriGen; } }

		public AboutData()
		{
			IntendedUseTitle = AppResources.LanguageManager.TextTranslated["AboutIntendedUseTitle"];
			IntendedUse = AppResources.LanguageManager.TextTranslated["AboutIntendedUse"];
			ProductInformationTitle = AppResources.LanguageManager.TextTranslated["AboutProductInformationTitle"];
            ProductInfo = AppResources.LanguageManager.TextTranslated["AboutProductInfo"];
            ProductInstructions = AppResources.LanguageManager.TextTranslated["AboutProductInstructions"];
            ProductInfoVersion = "Version Unofficial Build";
            ProductUDITitle = AppResources.LanguageManager.TextTranslated["AboutProductUDITitle"];
            ProductUDINumber = AppResources.LanguageManager.TextTranslated["AboutProductUDINumber"];
            ProductCopyright = "Copyright PeriGen, Inc. 1997-2016. All rights reserved.";
			ProductPatentsDisclaimer = AppResources.LanguageManager.TextTranslated["AboutProductPatentsDisclaimer"];
			ProductPatentsTitle = AppResources.LanguageManager.TextTranslated["AboutProductPatentsTitle"];
			Patent1 = AppResources.LanguageManager.TextTranslated["AboutPatent1"];
			Patent2 = AppResources.LanguageManager.TextTranslated["AboutPatent2"];
			Patent3 = AppResources.LanguageManager.TextTranslated["AboutPatent3"];
			Patent4 = AppResources.LanguageManager.TextTranslated["AboutPatent4"];
			Patent5 = AppResources.LanguageManager.TextTranslated["AboutPatent5"];
            Patent6 = AppResources.LanguageManager.TextTranslated["AboutPatent6"];
            Patent7 = AppResources.LanguageManager.TextTranslated["AboutPatent7"];
            Patent8 = AppResources.LanguageManager.TextTranslated["AboutPatent8"];
            ContactInformationTitle = AppResources.LanguageManager.TextTranslated["AboutContactInformationTitle"];
            SupportAndInquiriesTitle = AppResources.LanguageManager.TextTranslated["AboutSupportAndInquiriesTitle"];
			Company = AppResources.LanguageManager.TextTranslated["AboutCompany"];
			Address = AppResources.LanguageManager.TextTranslated["AboutAddress"];
			City = AppResources.LanguageManager.TextTranslated["AboutCity"];
			Province = AppResources.LanguageManager.TextTranslated["AboutProvince"];
			Country = AppResources.LanguageManager.TextTranslated["AboutCountry"];
			PostalCode = AppResources.LanguageManager.TextTranslated["AboutPostalCode"];
            PhoneTitle = AppResources.LanguageManager.TextTranslated["AboutPhoneTitle"];
            Phone = AppResources.LanguageManager.TextTranslated["AboutPhone"];
            PhoneTechnicalTitle = AppResources.LanguageManager.TextTranslated["AboutPhoneTechnicalTitle"];
            PhoneTechnical1 = AppResources.LanguageManager.TextTranslated["AboutPhoneTechnical1"];
            PhoneTechnical2 = AppResources.LanguageManager.TextTranslated["AboutPhoneTechnical2"];
            EmailTechnicalTitle = AppResources.LanguageManager.TextTranslated["AboutEmailTechnicalTitle"];
            EmailTechnical = AppResources.LanguageManager.TextTranslated["AboutEmailTechnicalEmail"];
            FaxTitle = AppResources.LanguageManager.TextTranslated["AboutFaxTitle"];
            Fax = AppResources.LanguageManager.TextTranslated["AboutFax"];
            WebsiteTitle = AppResources.LanguageManager.TextTranslated["AboutWebsiteTitle"];
			WebsiteURL = AppResources.LanguageManager.TextTranslated["AboutWebsiteURL"];
            EmailTitle = AppResources.LanguageManager.TextTranslated["AboutEmailTitle"];
            Email = AppResources.LanguageManager.TextTranslated["AboutEmail"];
        }

	
	}
}
