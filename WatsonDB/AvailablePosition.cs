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
    
    public partial class AvailablePosition
    {
        public int Job_id { get; set; }
        public int JobApplicant_id { get; set; }
        public string Department { get; set; }
        public string Title { get; set; }
        public string JobDescription { get; set; }
        public Nullable<System.DateTime> PostedDate { get; set; }
        public Nullable<bool> PositionPosted { get; set; }
    }
}
