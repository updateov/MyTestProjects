using System;
using System.Reflection;

namespace PeriCALM.Patterns.Curve.UI.Chart
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
			IntendedUseTitle = Statics.AppResources.LanguageManager.TextTranslated["AboutIntendedUseTitle"];
			IntendedUse = Statics.AppResources.LanguageManager.TextTranslated["AboutIntendedUse"];
			ProductInformationTitle = Statics.AppResources.LanguageManager.TextTranslated["AboutProductInformationTitle"];
            ProductInfo = Statics.AppResources.LanguageManager.TextTranslated["AboutProductInfo"];
            ProductInstructions = Statics.AppResources.LanguageManager.TextTranslated["AboutProductInstructions"];
            ProductInfoVersion = "Version Unofficial Build";
            ProductUDITitle = Statics.AppResources.LanguageManager.TextTranslated["AboutProductUDITitle"];
            ProductUDINumber = Statics.AppResources.LanguageManager.TextTranslated["AboutProductUDINumber"];
            ProductCopyright = "Copyright PeriGen, Inc. 1997-2016. All rights reserved.";
			ProductPatentsDisclaimer = Statics.AppResources.LanguageManager.TextTranslated["AboutProductPatentsDisclaimer"];
			ProductPatentsTitle = Statics.AppResources.LanguageManager.TextTranslated["AboutProductPatentsTitle"];
			Patent1 = Statics.AppResources.LanguageManager.TextTranslated["AboutPatent1"];
			Patent2 = Statics.AppResources.LanguageManager.TextTranslated["AboutPatent2"];
			Patent3 = Statics.AppResources.LanguageManager.TextTranslated["AboutPatent3"];
			Patent4 = Statics.AppResources.LanguageManager.TextTranslated["AboutPatent4"];
			Patent5 = Statics.AppResources.LanguageManager.TextTranslated["AboutPatent5"];
            Patent6 = Statics.AppResources.LanguageManager.TextTranslated["AboutPatent6"];
            Patent7 = Statics.AppResources.LanguageManager.TextTranslated["AboutPatent7"];
            Patent8 = Statics.AppResources.LanguageManager.TextTranslated["AboutPatent8"];
            ContactInformationTitle = Statics.AppResources.LanguageManager.TextTranslated["AboutContactInformationTitle"];
            SupportAndInquiriesTitle = Statics.AppResources.LanguageManager.TextTranslated["AboutSupportAndInquiriesTitle"];
			Company = Statics.AppResources.LanguageManager.TextTranslated["AboutCompany"];
			Address = Statics.AppResources.LanguageManager.TextTranslated["AboutAddress"];
			City = Statics.AppResources.LanguageManager.TextTranslated["AboutCity"];
			Province = Statics.AppResources.LanguageManager.TextTranslated["AboutProvince"];
			Country = Statics.AppResources.LanguageManager.TextTranslated["AboutCountry"];
			PostalCode = Statics.AppResources.LanguageManager.TextTranslated["AboutPostalCode"];
            PhoneTitle = Statics.AppResources.LanguageManager.TextTranslated["AboutPhoneTitle"];
            Phone = Statics.AppResources.LanguageManager.TextTranslated["AboutPhone"];
            PhoneTechnicalTitle = Statics.AppResources.LanguageManager.TextTranslated["AboutPhoneTechnicalTitle"];
            PhoneTechnical1 = Statics.AppResources.LanguageManager.TextTranslated["AboutPhoneTechnical1"];
            PhoneTechnical2 = Statics.AppResources.LanguageManager.TextTranslated["AboutPhoneTechnical2"];
            EmailTechnicalTitle = Statics.AppResources.LanguageManager.TextTranslated["AboutEmailTechnicalTitle"];
            EmailTechnical = Statics.AppResources.LanguageManager.TextTranslated["AboutEmailTechnicalEmail"];
            FaxTitle = Statics.AppResources.LanguageManager.TextTranslated["AboutFaxTitle"];
            Fax = Statics.AppResources.LanguageManager.TextTranslated["AboutFax"];
            WebsiteTitle = Statics.AppResources.LanguageManager.TextTranslated["AboutWebsiteTitle"];
			WebsiteURL = Statics.AppResources.LanguageManager.TextTranslated["AboutWebsiteURL"];
            EmailTitle = Statics.AppResources.LanguageManager.TextTranslated["AboutEmailTitle"];
            Email = Statics.AppResources.LanguageManager.TextTranslated["AboutEmail"];
        }

	
	}
}
