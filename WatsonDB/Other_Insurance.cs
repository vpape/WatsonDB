//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace WatsonDB
{
    using System;
    using System.Collections.Generic;
    
    public partial class Other_Insurance
    {
        public int FamilyMember_id { get; set; }
        public int OtherInsurance_id { get; set; }
        public int Employee_id { get; set; }
        public string InsuranceCarrier { get; set; }
        public string PolicyNumber { get; set; }
        public string MailingAddress { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
        public string PhoneNumber { get; set; }
        public string CoveredByOtherInsurance { get; set; }
        public string PObox { get; set; }
    
        public virtual Family_Info Family_Info { get; set; }
    }
}
