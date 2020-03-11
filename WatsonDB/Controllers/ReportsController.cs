using System;
using System.Linq;
using System.ComponentModel;
using System.Web.UI.WebControls;
using System.IO;
using System.Data;
using System.Web.Mvc;
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using System.Web.Services;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Threading.Tasks;
using System.Configuration;
using System.Data.SqlClient;
//using Microsoft.Reporting.WebForms;
using WatsonDB.ReportService2010;

namespace WatsonDB.Controllers
{
    public class ReportsController : Controller
    {
        private WatsonDBContext db = new WatsonDBContext();
        private static Group_Health grpHealth = new Group_Health();
   
        // GET: Reports
        public ActionResult Index()
        {
            return View();
        }
        
        //Crystal Reports
        public ActionResult GrpHealthPDF(int? Employee_id)
        {
            ViewBag.Employee_id = Employee_id;

            var pdf = (from g in db.Group_Health
                       .Where(i => i.Employee_id == Employee_id)
                       join e in db.Employees on g.Employee_id equals e.Employee_id
                       join f in db.Family_Info on g.Employee_id equals f.Employee_id
                       join o in db.Other_Insurance on f.Employee_id equals o.Employee_id
                       select new
                       {
                           FirstName = e.FirstName ?? "",
                           LastName = e.LastName ?? "",
                           MailingAddress = e.MailingAddress ?? "",
                           City = e.City ?? "",
                           State = e.State ?? "",
                           ZipCode = e.ZipCode ?? "",
                           DateOfBirth = e.DateOfBirth ?? new DateTime(1, 1, 1),
                           MaritalStatus = e.MaritalStatus ?? "",
                           SSN = e.SSN ?? "",
                           Gender = e.Gender ?? "",
                           EmailAddress = e.EmailAddress ?? "",
                           PhoneNumber = e.PhoneNumber ?? "",
                           Department = e.Department ?? "",
                           EnrollmentType = e.EnrollmentType ?? "",
                           Payroll_id = e.Payroll_id ?? "",
                           Class = e.Class ?? "",
                           AnnualSalary = e.AnnualSalary ?? "",
                           //JobTitle = e.JobTitle ?? "",
                           //HireDate = e.HireDate ?? new DateTime(1,1,1),
                           EffectiveDate = e.EffectiveDate ?? new DateTime(1, 1, 1),
                           HoursWorkedPerWeek = e.HoursWorkedPerWeek ?? "",

                           FamilyFirstName = f.FirstName ?? "",
                           FamilyLastName = f.LastName ?? "",
                           FamilySSN = f.SSN ?? "",
                           FamilyMailingAddress = f.MailingAddress ?? "",
                           FamilyCity = f.City ?? "",
                           FamilyState = f.State ?? "",
                           FamilyZipCode = f.ZipCode ?? "",
                           FamilyGender = f.Gender ?? "",
                           FamilyDOB = f.DateOfBirth ?? new DateTime(1, 1, 1),
                           Employer = f.Employer ?? "",
                           EmployerMailingAddress = f.EmployerMailingAddress ?? "",
                           EmployerCity = f.EmployerCity ?? "",
                           EmployerState = f.EmployerState ?? "",
                           EmployerZipCode = f.EmployerZipCode ?? "",
                           EmployerPhoneNumber = f.EmployerPhoneNumber ?? "",
                           Medical = f.Medical ?? "",
                           Dental = f.Dental ?? "",
                           Vision = f.Vision ?? "",
                           Indemnity = f.Indemnity ?? "",

                           GroupName = g.GroupName ?? "",
                           IMSGroupNumber = g.IMSGroupNumber ?? "",
                           InsuranceCarrier = g.InsuranceCarrier ?? "",
                           PolicyNumber = g.PolicyNumber ?? "",
                           GrpHPhoneNumber = g.PhoneNumber,
                           NoMedicalPlan = g.NoMedicalPlan ?? "",
                           MECPlan = g.MECPlan ?? "",
                           StandardPlan = g.StandardPlan ?? "",
                           BuyUpPlan = g.BuyUpPlan ?? "",
                           GrpHEnrollmentEmpSignature = g.GrpHEnrollmentEmpSignature ?? "",
                           GrpHEnrollmentEmpSignatureDate = g.GrpHEnrollmentEmpSignatureDate ?? new DateTime(1, 1, 1),
                           Myself = g.Myself ?? "",
                           Spouse = g.Spouse ?? "",
                           Dependent = g.Dependent ?? "",
                           OtherCoverage = g.OtherCoverage ?? "",
                           OtherReason = g.OtherReason ?? "",
                           OtherInsuranceCoverage = g.OtherInsuranceCoverage ?? "",
                           ReasonForGrpCoverageRefusal = g.ReasonForGrpCoverageRefusal ?? "",
                           GrpHRefusalEmpSignature = g.GrpHRefusalEmpSignature ?? "",
                           GrpHRefusalEmpSignatureDate = g.GrpHRefusalEmpSignatureDate ?? new DateTime(1, 1, 1),

                           CoveredByOtherInsurance = o.CoveredByOtherInsurance ?? "",
                           OtherInsuranceCarrier = o.InsuranceCarrier ?? "",
                           OtherPolicyNymber = o.PolicyNumber ?? "",
                           OtherMailingAddress = o.MailingAddress ?? "",
                           OtherCity = o.City ?? "",
                           OtherState = o.State ?? "",
                           OtherZipCode = o.ZipCode ?? "",
                           OtherPhoneNumber = o.PhoneNumber ?? ""

                       }).ToList();


            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(Server.MapPath("~/CrystalReports"), "GrpHEnrollmentForm2.rpt"));
            rd.SetDatabaseLogon("Watson", "Watson2020!@");
            rd.SetDataSource(pdf);
            rd.Refresh();

            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();

            Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            stream.Seek(0, SeekOrigin.Begin);
            return File(stream, "application/pdf", "GrpHEnrollment.pdf");

        }

        //Crystal Reports
        public ActionResult AuthorizationFormPDF(int? Employee_id)
        {

            ViewBag.Employee_id = Employee_id;

            var pdf = (from g in db.Group_Health
                        .Where(i => i.Employee_id == Employee_id)
                       join e in db.Employees on g.Employee_id equals e.Employee_id
                       select new
                       {
                           CurrentEmployer = e.CurrentEmployer ?? "",
                           FirstName = e.FirstName ?? "",
                           LastName = e.LastName ?? "",
                           DateOfBirth = e.DateOfBirth ?? new DateTime(1, 1, 1),
                           SSN = e.SSN ?? "",
                           MailingAddress = e.MailingAddress ?? "",
                           City = e.City ?? "",
                           State = e.State ?? "",
                           ZipCode = e.ZipCode ?? "",
                           NameOfPersonOneReleaseInfoTo = g.NameOfPersonOneReleaseInfoTo ?? "",
                           PersonOneRelationship = g.PersonOneRelationship ?? "",
                           NameOfPersonTwoReleaseInfoTo = g.NameOfPersonTwoReleaseInfoTo ?? "",
                           PersonTwoRelationship = g.PersonTwoRelationship ?? "",
                           AuthorizationFormPolicyHolderSignature = g.AuthorizationFormPolicyHolderSignature ?? "",
                           AuthorizationFormPolicyHolderSignatureDate = g.AuthorizationFormPolicyHolderSignatureDate ?? new DateTime(1, 1, 1),
                           PersonOneSignature = g.PersonOneSignature ?? "",
                           PersonOneSignatureDate = g.PersonOneSignatureDate ?? new DateTime(1, 1, 1),
                           PersonTwoSignature = g.PersonTwoSignature ?? "",
                           PersonTwoSignatureDate = g.PersonTwoSignatureDate ?? new DateTime(1, 1, 1)
                       }).ToList();


            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(Server.MapPath("~/CrystalReports"), "AuthorizationFORM.rpt"));
            rd.SetDatabaseLogon("Watson", "Watson2020!@");
            rd.SetDataSource(pdf);

            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();


            Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            stream.Seek(0, SeekOrigin.Begin);
            return File(stream, "application/pdf", "AuthorizationFORM.pdf");

        }

        //Crystal Reports
        public ActionResult SalaryRedirectionForm(int? Employee_id)
        {
            ViewBag.Employee_id = Employee_id;

            var pdf = (from e in db.Employees
                        .Where(i => i.Employee_id == Employee_id)
                       join d in db.Deductions on e.Employee_id equals d.Employee_id
                       select new
                       {
                           FirstName = e.FirstName ?? "",
                           LastName = e.LastName ?? "",
                           MailingAddress = e.MailingAddress ?? "",
                           City = e.City ?? "",
                           State = e.State ?? "",
                           ZipCode = e.ZipCode ?? "",
                           SSN = e.SSN ?? "",
                           HireDate = e.HireDate ?? new DateTime(1, 1, 1),
                           EffectiveDate = e.EffectiveDate ?? new DateTime(1, 1, 1),
                           CurrentEmployer = e.CurrentEmployer ?? "",

                           MedicalInsProvider = d.MedicalInsProvider ?? "",
                           EEelectionPreTaxMedIns = d.EEelectionPreTaxMedIns ?? "",
                           PremiumPreTaxMedIns = d.PremiumPreTaxMedIns ?? "",
                           EEelectionPostTaxMedIns = d.EEelectionPostTaxMedIns ?? "",
                           PremiumPostTaxMedIns = d.PremiumPostTaxMedIns ?? "",

                           DentalInsProvider = d.DentalInsProvider ?? "",
                           EEelectionPreTaxDentalIns = d.EEelectionPreTaxDentalIns ?? "",
                           PremiumPreTaxDentalIns = d.PremiumPreTaxDentalIns ?? "",
                           EEelectionPostTaxDentalIns = d.EEelectionPostTaxDentalIns ?? "",
                           PremiumPostTaxDentalIns = d.PremiumPostTaxDentalIns ?? "",

                           VisionInsProvider = d.VisionInsProvider ?? "",
                           EEelectionPreTaxVisionIns = d.EEelectionPreTaxVisionIns ?? "",
                           PremiumPreTaxVisionIns = d.PremiumPreTaxVisionIns ?? "",
                           EEelectionPostTaxVisionIns = d.EEelectionPostTaxVisionIns ?? "",
                           PremiumPostTaxVisionIns = d.PremiumPostTaxVisionIns ?? "",

                           AccidentProvider = d.AccidentProvider ?? "",
                           EEelectionPreTaxAccidentIns = d.EEelectionPreTaxAccidentIns ?? "",
                           PremiumPreTaxAccidentIns = d.PremiumPreTaxAccidentIns ?? "",
                           EEelectionPostTaxAccidentIns = d.EEelectionPostTaxAccidentIns ?? "",
                           PremiumPostTaxAccidentIns = d.PremiumPostTaxAccidentIns ?? "",

                           CancerProvider = d.CancerProvider ?? "",
                           EEelectionPreTaxCancerIns = d.EEelectionPreTaxCancerIns ?? "",
                           PremiumPreTaxCancerIns = d.PremiumPreTaxCancerIns ?? "",
                           EEelectionPostTaxCancerIns = d.EEelectionPostTaxCancerIns ?? "",
                           PremiumPostTaxCancerIns = d.PremiumPostTaxCancerIns ?? "",

                           StDisabilityProvider = d.StDisabilityProvider ?? "",
                           EEelectionPreTaxStDisability = d.EEelectionPreTaxStDisability ?? "",
                           PremiumPreTaxStDisability = d.PremiumPreTaxStDisability ?? "",
                           EEelectionPostTaxStDisability = d.EEelectionPostTaxStDisability ?? "",
                           PremiumPostTaxStDisability = d.PremiumPostTaxStDisability ?? "",

                           HospitalIndemProvider = d.HospitalIndemProvider ?? "",
                           EEelectionPreTaxHospitalIndem = d.EEelectionPreTaxHospitalIndem ?? "",
                           PremiumPreTaxHospitalIndem = d.PremiumPreTaxHospitalIndem ?? "",
                           EEelectionPostTaxHospitalIndem = d.EEelectionPostTaxHospitalIndem ?? "",
                           PremiumPostTaxHospitalIndem = d.PremiumPostTaxHospitalIndem ?? "",

                           TermLifeInsProvider = d.TermLifeInsProvider ?? "",
                           EEelectionPreTaxTermLifeIns = d.EEelectionPreTaxTermLifeIns ?? "",
                           PremiumPreTaxTermLifeIns = d.PremiumPreTaxTermLifeIns ?? "",
                           EEelectionPostTaxTermLifeIns = d.EEelectionPostTaxTermLifeIns ?? "",
                           PremiumPostTaxTermLifeIns = d.PremiumPostTaxTermLifeIns ?? "",

                           WholeLifeInsProvider = d.WholeLifeInsProvider ?? "",
                           EEelectionPreTaxWholeLifeIns = d.EEelectionPreTaxWholeLifeIns ?? "",
                           PremiumPreTaxWholeLifeIns = d.PremiumPreTaxWholeLifeIns ?? "",
                           EEelectionPostTaxWholeLifeIns = d.EEelectionPostTaxWholeLifeIns ?? "",
                           PremiumPostTaxWholeLifeIns = d.PremiumPostTaxWholeLifeIns ?? "",

                           OtherInsProvider = d.OtherInsProvider ?? "",
                           EEelectionPreTaxOtherIns = d.EEelectionPreTaxOtherIns ?? "",
                           PremiumPreTaxOtherIns = d.PremiumPreTaxOtherIns ?? "",
                           EEelectionPostTaxOtherIns = d.EEelectionPostTaxOtherIns ?? "",
                           PremiumPostTaxOtherIns = d.PremiumPostTaxOtherIns ?? "",
                           EmployeeInitials = d.EmployeeInitials ?? "",
                           PreTaxBenefitWaiverinitials = d.PreTaxBenefitWaiverinitials ?? "",
                           EmployeeSignature = d.EmployeeSignature ?? "",
                           EmployeeSignatureDate = d.EmployeeSignatureDate ?? new DateTime(1, 1, 1)

                       }).ToList().FirstOrDefault();


            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(Server.MapPath("~/CrystalReports"), "SalaryRedirectionForm.rpt"));
            rd.SetDatabaseLogon("Watson", "Watson2020!@");
            rd.SetDataSource(pdf);
            rd.Refresh();

            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();

            Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            stream.Seek(0, SeekOrigin.Begin);
            return File(stream, "application/pdf", "SalaryRedirectionForm.pdf");

        }



        //Report Web Service
        static void Main(string[] args)
        {
            ReportingService2010 rs = new ReportingService2010();
            rs.Credentials = System.Net.CredentialCache.DefaultCredentials;
            rs.Url = "http://1MJMPQB/ReportServer2019/ReportService2010.asmx";

            Property name = new Property();
            name.Name = "Name";

            Property description = new Property();
            description.Name = "Description";

            Property[] properties = new Property[2];
            properties[0] = name;
            properties[1] = description;

            try
            {
                Property[] returnProperties = rs.GetProperties(
                "/Reports2019/AuthorizationForm", properties);

                foreach (Property p in returnProperties)
                {
                    Console.WriteLine(p.Name + ": " + p.Value);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public class ConfigureReports
        {
            private const string Root = "/";
            private ReportingService2010 rs;

            public ConfigureReports()
            {
                try
                {
                    //Connect to Reporting Services
                    ReportingService2010 rs = new ReportingService2010();
                    rs.Credentials = System.Net.CredentialCache.DefaultCredentials;
                    rs.Url = "http://1MJMPQB/ReportServer2019/ReportService2010.asmx";
                }
                catch(System.Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }
    }
}