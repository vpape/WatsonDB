using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Data.Entity;
using System;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.ModelConfiguration;
using System.Data.Entity.ModelConfiguration.Configuration;
using System.Data.Entity.Validation;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Web.UI;
using System.Web.UI.WebControls;
using Newtonsoft.Json;
using System.IO;
using System.Net;
using System.Text;
using System.Data;
using System.Net.Http;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using System.Web.Mvc;
using WatsonDB.Models;
using CrystalDecisions.CrystalReports.Engine;

namespace WatsonDB.Controllers
{
    //[Authorize(Roles = "Employee, Manager, Owner, Admin")]
    public class Group_HealthController : Controller
    {
        private WatsonDBContext db = new WatsonDBContext();
        private static Group_Health grpHealth = new Group_Health();
        private static Employee employee = new Employee();
        private static List<Family_Info> family = new List<Family_Info>();
        private static List<Other_Insurance> otherIns = new List<Other_Insurance>();

        ApplicationDbContext context;
        public Group_HealthController()
        {
            context = new ApplicationDbContext();
        }

        // GET: Group_Health
        public ActionResult Index()
        {
            return View();
        }

        //====================================
        // Get: GroupHealthEnrollment
        //====================================
        public ActionResult GroupHealthEnrollment(int? Employee_id, int? GroupHealthInsurance_id, string userId)
        {
            userId = userId ?? User.Identity.GetUserId();

            employee = db.Employees.Where(i => i.Id == userId).FirstOrDefault();

            Employee_id = employee.Employee_id;

            ViewBag.Employee_id = Employee_id;
            ViewBag.GroupHealthInsurance_id = GroupHealthInsurance_id;

            GroupHealthGrpHEnrollmentVM groupHGrpHEnrollmentVM = new GroupHealthGrpHEnrollmentVM();

            groupHGrpHEnrollmentVM.employee = db.Employees.FirstOrDefault(i => i.Employee_id == Employee_id);
            groupHGrpHEnrollmentVM.grpHealth = db.Group_Health.FirstOrDefault(i => i.Employee_id == Employee_id);

            groupHGrpHEnrollmentVM.spouse = db.Family_Info.FirstOrDefault(i => i.Employee_id == Employee_id && i.RelationshipToInsured == "Spouse");
            groupHGrpHEnrollmentVM.family = db.Family_Info.Where(i => i.Employee_id == Employee_id && i.RelationshipToInsured != "Spouse").ToList();
            if (groupHGrpHEnrollmentVM.spouse != null)
            {
                groupHGrpHEnrollmentVM.spouseInsurance = db.Other_Insurance.FirstOrDefault(i => i.Employee_id == Employee_id && i.FamilyMember_id == groupHGrpHEnrollmentVM.spouse.FamilyMember_id);
                groupHGrpHEnrollmentVM.otherIns = db.Other_Insurance.Where(i => i.Employee_id == Employee_id && i.FamilyMember_id != groupHGrpHEnrollmentVM.spouse.FamilyMember_id).ToList();
            }
            else
            {
                groupHGrpHEnrollmentVM.spouseInsurance = null;
                groupHGrpHEnrollmentVM.otherIns = db.Other_Insurance.Where(i => i.Employee_id == Employee_id).ToList();
            }

            return View(groupHGrpHEnrollmentVM);

        }

        //====================================
        // Post: Create EmploymentInfoGrpH
        //====================================
        public JsonResult EmploymentInfoGrpHealthEnrollment(int? Employee_id, string GroupName, string IMSGroupNumber, string Department, string EnrollmentType,
             string Payroll_id, string Class, string AnnualSalary, /*string JobTitle, DateTime? HireDate,*/ DateTime? EffectiveDate, string HoursWorkedPerWeek)
        {
            //Employee emp = new Employee();
            Employee emp = db.Employees
             .Where(i => i.Employee_id == Employee_id)
             .Single();

            emp.Department = Department;
            emp.EnrollmentType = EnrollmentType;
            emp.Payroll_id = Payroll_id;
            emp.Class = Class;
            emp.AnnualSalary = AnnualSalary;
            //emp.JobTitle = JobTitle;
            //emp.HireDate = HireDate;
            emp.EffectiveDate = EffectiveDate;
            emp.HoursWorkedPerWeek = HoursWorkedPerWeek;

            Group_Health g = db.Group_Health
                .Where(i => i.Employee_id == Employee_id)
                .Single();

            g.GroupName = GroupName;
            g.IMSGroupNumber = IMSGroupNumber;

            db.SaveChanges();

            int result = g.Employee_id;

            return Json(new { data = result }, JsonRequestBehavior.AllowGet);
        }

        //====================================
        //Post-GrpHealthEnrollmentNew
        //====================================

        //DEMO LICENSE KEY for Selectpdf Html to PDF API:
        //"7df5f5a6-4672-4cd7-9277-3de3615ffdfc";

        public JsonResult GrpHealthEnrollmentNew(int Employee_id,/*DateTime? CafeteriaPlanYear,*/ string empCoveredByOtherIns,
            string empInsCarrier, string empInsPolicyNumber, string empInsPhoneNumber, string NoMedical, string MECPlan,
            string StandardPlan, string BuyUpPlan, string GrpHEnrollmentEmpSignature, DateTime? GrpHEnrollmentEmpSignatureDate, string Myself, string Spouse,
            string Dependent, string OtherCoverageSelection, string OtherReasonSelection, string ReasonForGrpCoverageRefusal, string GrpHRefusalEmpSignature,
            DateTime? GrpHRefusalEmpSignatureDate, FormCollection collection)
        {
            string response = "";

            int record = (from grpH in db.Group_Health
                          where grpH.Employee_id == Employee_id
                          select grpH).Count();

            if (record > 0)
            {
                response = "Record already exists.";
            }
            else
            {

                Group_Health g = db.Group_Health
                    .Where(i => i.Employee_id == Employee_id)
                    .Single();

                //Group_Health g = new Group_Health();

                g.Employee_id = Employee_id;
                //g.OtherInsuranceCoverage = empCoveredByOtherIns;
                //g.InsuranceCarrier = empInsCarrier;
                //g.PolicyNumber = empInsPolicyNumber;
                //g.PhoneNumber = empInsPhoneNumber;

                //g.CafeteriaPlanYear = CafeteriaPlanYear;
                g.NoMedicalPlan = NoMedical;
                g.MECPlan = MECPlan;
                g.StandardPlan = StandardPlan;
                g.BuyUpPlan = BuyUpPlan;

                g.GrpHEnrollmentEmpSignature = GrpHEnrollmentEmpSignature;
                g.GrpHEnrollmentEmpSignatureDate = GrpHEnrollmentEmpSignatureDate;
                g.Myself = Myself;
                g.Spouse = Spouse;
                g.Dependent = Dependent;
                g.OtherCoverage = OtherCoverageSelection;
                g.OtherReason = OtherReasonSelection;
                g.ReasonForGrpCoverageRefusal = ReasonForGrpCoverageRefusal;
                g.GrpHRefusalEmpSignature = GrpHRefusalEmpSignature;
                g.GrpHRefusalEmpSignatureDate = GrpHRefusalEmpSignatureDate;

                db.SaveChanges();
            }

            int result = Employee_id;

            return Json(new { data = result, error = response }, JsonRequestBehavior.AllowGet);
        }

        //====================================
        //Edit-EditGroupHealthIns
        //====================================
        public ActionResult EditGroupHealthIns(int? Employee_id, int? GroupHealthInsurance_id, string userId)
        {
            userId = userId ?? User.Identity.GetUserId();

            employee = db.Employees.Where(i => i.Id == userId).FirstOrDefault();

            Employee_id = employee.Employee_id;

            ViewBag.GroupHealthInsurance_id = GroupHealthInsurance_id;
            ViewBag.Employee_id = Employee_id;

            GroupHealthGrpHEnrollmentVM groupHGrpHEnrollmentVM = new GroupHealthGrpHEnrollmentVM();

            groupHGrpHEnrollmentVM.employee = db.Employees.FirstOrDefault(i => i.Employee_id == Employee_id);
            groupHGrpHEnrollmentVM.grpHealth = db.Group_Health.FirstOrDefault(i => i.Employee_id == Employee_id);

            groupHGrpHEnrollmentVM.spouse = db.Family_Info.FirstOrDefault(i => i.Employee_id == Employee_id && i.RelationshipToInsured == "Spouse");
            groupHGrpHEnrollmentVM.family = db.Family_Info.Where(i => i.Employee_id == Employee_id && i.RelationshipToInsured != "Spouse").ToList();
            if (groupHGrpHEnrollmentVM.spouse != null)
            {
                groupHGrpHEnrollmentVM.spouseInsurance = db.Other_Insurance.FirstOrDefault(i => i.Employee_id == Employee_id && i.FamilyMember_id == groupHGrpHEnrollmentVM.spouse.FamilyMember_id);
                groupHGrpHEnrollmentVM.otherIns = db.Other_Insurance.Where(i => i.Employee_id == Employee_id && i.FamilyMember_id != groupHGrpHEnrollmentVM.spouse.FamilyMember_id).ToList();
            }
            else
            {
                groupHGrpHEnrollmentVM.spouseInsurance = null;
                groupHGrpHEnrollmentVM.otherIns = db.Other_Insurance.Where(i => i.Employee_id == Employee_id).ToList();
            }

            return View(groupHGrpHEnrollmentVM);

        }

        //====================================
        //Post-GrpHealthInsEditUpdate
        //====================================
        public static string apiEndpoint = "https://selectpdf.com/api2/convert/";
        public static string apiKey = "7df5f5a6-4672-4cd7-9277-3de3615ffdfc";
        public static string GrpHInsURL = "http://localhost:57772/Group_Health/EditGroupHealthIns?Employee_id=";

        public static void Main(string[] args)
        {
            // POST JSON example using WebClient (and Newtonsoft for JSON serialization)
            SelectPdfPostWithWebClient();
        }

        // POST JSON example using WebClient (and Newtonsoft for JSON serialization)
        public static void SelectPdfPostWithWebClient()
        {

        }

        public JsonResult GrpHealthInsEditUpdate(int? Employee_id, int? InsurancePlan_id, /*DateTime? CafeteriaPlanYear,*/ string NoMedical, string MECPlan,
                string StandardPlan, string BuyUpPlan, string GrpHEnrollmentEmpSignature, DateTime? GrpHEnrollmentEmpSignatureDate, string Myself, string Spouse,
                string Dependent, string OtherCoverageSelection, string OtherReasonSelection, string ReasonForGrpCoverageRefusal, string GrpHRefusalEmpSignature,
                DateTime? GrpHRefusalEmpSignatureDate, FormCollection collection)
        {

            Group_Health g = db.Group_Health
                 .Where(i => i.Employee_id == Employee_id)
                 .Single();

            //g.CafeteriaPlanYear = CafeteriaPlanYear;
            g.NoMedicalPlan = NoMedical;
            g.MECPlan = MECPlan;
            g.StandardPlan = StandardPlan;
            g.BuyUpPlan = BuyUpPlan;

            g.GrpHEnrollmentEmpSignature = GrpHEnrollmentEmpSignature;
            g.GrpHEnrollmentEmpSignatureDate = GrpHEnrollmentEmpSignatureDate;
            g.Myself = Myself;
            g.Spouse = Spouse;
            g.Dependent = Dependent;
            g.OtherCoverage = OtherCoverageSelection;
            g.OtherReason = OtherReasonSelection;
            g.ReasonForGrpCoverageRefusal = ReasonForGrpCoverageRefusal;
            g.GrpHRefusalEmpSignature = GrpHRefusalEmpSignature;
            g.GrpHRefusalEmpSignatureDate = GrpHRefusalEmpSignatureDate;

            //InsurancePlan insPlan = db.InsurancePlans
            //  .Where(i => i.InsurancePlan_id == InsurancePlan_id)
            //  .Single();

            //insPlan.MECPlan = InsMECPlan;
            //insPlan.StandardPlan = InsStndPlan;
            //insPlan.BuyUpPlan = InsBuyUpPlan;

            //ViewBag.insPlan = insPlan;

            if (ModelState.IsValid)
            {
                db.Entry(g).State = System.Data.Entity.EntityState.Modified;

                try
                {
                    db.SaveChanges();
                }
                catch (Exception err)
                {
                    Console.WriteLine(err);
                }
            }

            int result = g.Employee_id;

            //CreateGrpHealthPDF(g.Employee_id);

            return Json(new { data = result }, JsonRequestBehavior.AllowGet);
        }

        //public void CreateGrpHealthPDF(int? Employee_id)
        //{
        //    System.Console.WriteLine("Starting conversion with WebClient ...");

        //    // set parameters
        //    SelectPdfParameters parameters = new SelectPdfParameters();
        //    parameters.key = apiKey;
        //    parameters.url = GrpHInsURL + Employee_id;

        //    // JSON serialize parameters
        //    string jsonData = JsonConvert.SerializeObject(parameters);
        //    byte[] byteData = Encoding.UTF8.GetBytes(jsonData);

        //    // create WebClient object
        //    WebClient webClient = new WebClient();
        //    webClient.Headers.Add(HttpRequestHeader.ContentType, "application/json");

        //    // POST parameters (if response code is not 200 OK, a WebException is raised)
        //    try
        //    {
        //        byte[] result = webClient.UploadData(apiEndpoint, "POST", byteData);

        //        // all ok - read PDF and write on disk (binary read!!!!)
        //        MemoryStream ms = new MemoryStream(result);

        //        // write to file
        //        FileStream file = new FileStream("test2.pdf", FileMode.Create, FileAccess.Write);
        //        ms.WriteTo(file);
        //        file.Close();

        //    }
        //    catch (WebException webEx)
        //    {
        //        // an error occurred
        //        System.Console.WriteLine("Error: " + webEx.Message);

        //        HttpWebResponse response = (HttpWebResponse)webEx.Response;
        //        Stream responseStream = response.GetResponseStream();

        //        // get details of the error message if available (text read!!!)
        //        StreamReader readStream = new StreamReader(responseStream);
        //        string message = readStream.ReadToEnd();
        //        responseStream.Close();

        //        System.Console.WriteLine("Error Message: " + message);
        //    }
        //    catch (Exception ex)
        //    {
        //        System.Console.WriteLine("Error: " + ex.Message);
        //    }

        //    System.Console.WriteLine("Finished.");

        //    // return resulted pdf document
        //    //FileResult fileResult = new FileContentResult(pdf, "application/pdf");
        //    //fileResult.FileDownloadName = "GrpHealth_Insurance.pdf";
        //    //return fileResult;

        //}

        //SalaryRedirect-Start----------------------------------------------------------------------------------

        public ActionResult SalaryRedirection(int Employee_id)
        {

            GroupHealthGrpHEnrollmentVM groupHGrpHEnrollmentVM = new GroupHealthGrpHEnrollmentVM();

            groupHGrpHEnrollmentVM.employee = db.Employees.FirstOrDefault(i => i.Employee_id == Employee_id);

            ViewBag.Employee_id = groupHGrpHEnrollmentVM.employee.Employee_id;


            return View(groupHGrpHEnrollmentVM);
        }

        //====================================
        //Post-SalaryRedirectionUpdate
        //====================================
        public JsonResult SalaryRedirectionUpdate(int Employee_id, int? Deductions_id, string MedicalInsProvider, string EEelectionPreTaxMedIns,
            string PremiumPreTaxMedIns, string EEelectionPostTaxMedIns, string PremiumPostTaxMedIns, string DentalInsProvider, string EEelectionPreTaxDentalIns,
            string PremiumPreTaxDentalIns, string EEelectionPostTaxDentalIns, string PremiumPostTaxDentalIns, string VisionInsProvider, string EEelectionPreTaxVisionIns,
            string PremiumPreTaxVisionIns, string EEelectionPostTaxVisionIns, string PremiumPostTaxVisionIns, string StDisabilityProvider, string EEelectionPreTaxStDisability, string PremiumPreTaxStDisability,
            string EEelectionPostTaxStDisability, string PremiumPostTaxStDisability, string HospitalIndemProvider, string EEelectionPreTaxHospitalIndem,
            string PremiumPreTaxHospitalIndem, string EEelectionPostTaxHospitalIndem, string PremiumPostTaxHospitalIndem, string TermLifeInsProvider,
            string EEelectionPreTaxTermLifeIns, string PremiumPreTaxTermLifeIns, string EEelectionPostTaxTermLifeIns, string PremiumPostTaxTermLifeIns,
            string WholeLifeInsProvider, string EEelectionPreTaxWholeLifeIns, string PremiumPreTaxWholeLifeIns, string EEelectionPostTaxWholeLifeIns,
            string PremiumPostTaxWholeLifeIns, string OtherInsProvider, string EEelectionPreTaxOtherIns, string PremiumPreTaxOtherIns, string EEelectionPostTaxOtherIns,
            string PremiumPostTaxOtherIns, string AccidentProvider, string EEelectionPreTaxAccidentIns, string PremiumPreTaxAccidentIns, string EEelectionPostTaxAccidentIns,
            string PremiumPostTaxAccidentIns, string CancerProvider, string EEelectionPreTaxCancerIns, string PremiumPreTaxCancerIns, string EEelectionPostTaxCancerIns,
            string PremiumPostTaxCancerIns, string TotalPreTax, string TotalPostTax, string empInitials1, string PreTaxBenefitWaiverinitials, string empSignature,
            DateTime empSignatureDate)
        {

            Deduction d = new Deduction();

            d.Employee_id = Employee_id;
            d.MedicalInsProvider = MedicalInsProvider;
            d.EEelectionPreTaxMedIns = EEelectionPreTaxMedIns;
            d.PremiumPreTaxMedIns = PremiumPreTaxMedIns;
            d.EEelectionPostTaxMedIns = EEelectionPostTaxMedIns;
            d.PremiumPostTaxMedIns = PremiumPostTaxMedIns;

            d.DentalInsProvider = DentalInsProvider;
            d.EEelectionPreTaxDentalIns = EEelectionPreTaxDentalIns;
            d.PremiumPreTaxDentalIns = PremiumPreTaxDentalIns;
            d.EEelectionPostTaxDentalIns = EEelectionPostTaxDentalIns;
            d.PremiumPostTaxDentalIns = PremiumPostTaxDentalIns;

            d.VisionInsProvider = VisionInsProvider;
            d.EEelectionPreTaxVisionIns = EEelectionPreTaxVisionIns;
            d.PremiumPreTaxVisionIns = PremiumPreTaxVisionIns;
            d.EEelectionPostTaxVisionIns = EEelectionPostTaxVisionIns;
            d.PremiumPostTaxVisionIns = PremiumPostTaxVisionIns;



            d.StDisabilityProvider = StDisabilityProvider;
            d.EEelectionPreTaxStDisability = EEelectionPreTaxStDisability;
            d.PremiumPreTaxStDisability = PremiumPreTaxStDisability;
            d.EEelectionPostTaxStDisability = EEelectionPostTaxStDisability;
            d.PremiumPostTaxStDisability = PremiumPostTaxStDisability;

            d.HospitalIndemProvider = HospitalIndemProvider;
            d.EEelectionPreTaxHospitalIndem = EEelectionPreTaxHospitalIndem;
            d.PremiumPreTaxHospitalIndem = PremiumPreTaxHospitalIndem;
            d.EEelectionPostTaxHospitalIndem = EEelectionPostTaxHospitalIndem;
            d.PremiumPostTaxHospitalIndem = PremiumPostTaxHospitalIndem;

            d.TermLifeInsProvider = TermLifeInsProvider;
            d.EEelectionPreTaxTermLifeIns = EEelectionPreTaxTermLifeIns;
            d.PremiumPreTaxTermLifeIns = PremiumPreTaxTermLifeIns;
            d.EEelectionPostTaxTermLifeIns = EEelectionPostTaxTermLifeIns;
            d.PremiumPostTaxTermLifeIns = PremiumPostTaxTermLifeIns;

            d.WholeLifeInsProvider = WholeLifeInsProvider;
            d.EEelectionPreTaxWholeLifeIns = EEelectionPreTaxWholeLifeIns;
            d.PremiumPreTaxWholeLifeIns = PremiumPreTaxWholeLifeIns;
            d.EEelectionPostTaxWholeLifeIns = EEelectionPostTaxWholeLifeIns;
            d.PremiumPostTaxWholeLifeIns = PremiumPostTaxWholeLifeIns;

            d.AccidentProvider = AccidentProvider;
            d.EEelectionPreTaxAccidentIns = EEelectionPreTaxAccidentIns;
            d.PremiumPreTaxAccidentIns = PremiumPreTaxAccidentIns;
            d.EEelectionPostTaxAccidentIns = EEelectionPostTaxAccidentIns;
            d.PremiumPostTaxAccidentIns = PremiumPostTaxAccidentIns;

            d.CancerProvider = CancerProvider;
            d.EEelectionPreTaxCancerIns = EEelectionPreTaxCancerIns;
            d.PremiumPreTaxCancerIns = PremiumPreTaxCancerIns;
            d.EEelectionPostTaxCancerIns = EEelectionPostTaxCancerIns;
            d.PremiumPostTaxCancerIns = PremiumPostTaxCancerIns;

            d.OtherInsProvider = OtherInsProvider;
            d.EEelectionPreTaxOtherIns = EEelectionPreTaxOtherIns;
            d.PremiumPreTaxOtherIns = PremiumPreTaxOtherIns;
            d.EEelectionPostTaxOtherIns = EEelectionPostTaxOtherIns;
            d.PremiumPostTaxOtherIns = PremiumPostTaxOtherIns;

            d.TotalPreTax = TotalPreTax;
            d.TotalPostTax = TotalPostTax;
            d.EmployeeSignature = empSignature;
            d.EmployeeSignatureDate = empSignatureDate;
            d.EmployeeInitials = empInitials1;
            d.PreTaxBenefitWaiverinitials = PreTaxBenefitWaiverinitials;


            db.Deductions.Add(d);
            db.SaveChanges();

            int result = d.Deductions_id;

            return Json(new { data = result }, JsonRequestBehavior.AllowGet);
        }

        //----------------------------------------------------------------------------------------

        public ActionResult EditSalaryRedirection(int? Employee_id, int? Deductions_id)
        {
            GroupHealthGrpHEnrollmentVM grpHGrpEnrollmentVM = new GroupHealthGrpHEnrollmentVM();

            grpHGrpEnrollmentVM.employee = db.Employees.FirstOrDefault(i => i.Employee_id == Employee_id);
            grpHGrpEnrollmentVM.deduction = db.Deductions.FirstOrDefault(i => i.Employee_id == Employee_id);

            //ViewBag.Deductions_id = grpHGrpEnrollmentVM.deduction.Deductions_id;
            ViewBag.Employee_id = grpHGrpEnrollmentVM.employee.Employee_id;

            //    if (Employee_id == null)
            //    {
            //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            //    }

            //    Group_Health g = db.Group_Health.Find(Employee_id);
            //    if (g == null)
            //    {
            //        return HttpNotFound();
            //    }

            return View(grpHGrpEnrollmentVM);
        }

        //====================================
        //Post-SalaryRedirectionEditUpdate
        //====================================
        public JsonResult SalaryRedirectionEditUpdate(int Employee_id, int? Deductions_id, string MedicalInsProvider, string EEelectionPreTaxMedIns,
            string PremiumPreTaxMedIns, string EEelectionPostTaxMedIns, string PremiumPostTaxMedIns, string DentalInsProvider, string EEelectionPreTaxDentalIns,
            string PremiumPreTaxDentalIns, string EEelectionPostTaxDentalIns, string PremiumPostTaxDentalIns, string VisionInsProvider, string EEelectionPreTaxVisionIns,
            string PremiumPreTaxVisionIns, string EEelectionPostTaxVisionIns, string PremiumPostTaxVisionIns, string StDisabilityProvider, string EEelectionPreTaxStDisability, string PremiumPreTaxStDisability,
            string EEelectionPostTaxStDisability, string PremiumPostTaxStDisability, string HospitalIndemProvider, string EEelectionPreTaxHospitalIndem,
            string PremiumPreTaxHospitalIndem, string EEelectionPostTaxHospitalIndem, string PremiumPostTaxHospitalIndem, string TermLifeInsProvider,
            string EEelectionPreTaxTermLifeIns, string PremiumPreTaxTermLifeIns, string EEelectionPostTaxTermLifeIns, string PremiumPostTaxTermLifeIns,
            string WholeLifeInsProvider, string EEelectionPreTaxWholeLifeIns, string PremiumPreTaxWholeLifeIns, string EEelectionPostTaxWholeLifeIns,
            string PremiumPostTaxWholeLifeIns, string OtherInsProvider, string EEelectionPreTaxOtherIns, string PremiumPreTaxOtherIns, string EEelectionPostTaxOtherIns,
            string PremiumPostTaxOtherIns, string AccidentProvider, string EEelectionPreTaxAccidentIns, string PremiumPreTaxAccidentIns, string EEelectionPostTaxAccidentIns,
            string PremiumPostTaxAccidentIns, string CancerProvider, string EEelectionPreTaxCancerIns, string PremiumPreTaxCancerIns, string EEelectionPostTaxCancerIns,
            string PremiumPostTaxCancerIns, string TotalPreTax, string TotalPostTax, string empInitials1, string PreTaxBenefitWaiverinitials, string empSignature,
            DateTime empSignatureDate)
        {
            Deduction d = db.Deductions
                .Where(i => i.Deductions_id == Deductions_id)
                .Single();

            d.Employee_id = Employee_id;
            d.MedicalInsProvider = MedicalInsProvider;
            d.EEelectionPreTaxMedIns = EEelectionPreTaxMedIns;
            d.PremiumPreTaxMedIns = PremiumPreTaxMedIns;
            d.EEelectionPostTaxMedIns = EEelectionPostTaxMedIns;
            d.PremiumPostTaxMedIns = PremiumPostTaxMedIns;

            d.DentalInsProvider = DentalInsProvider;
            d.EEelectionPreTaxDentalIns = EEelectionPreTaxDentalIns;
            d.PremiumPreTaxDentalIns = PremiumPreTaxDentalIns;
            d.EEelectionPostTaxDentalIns = EEelectionPostTaxDentalIns;
            d.PremiumPostTaxDentalIns = PremiumPostTaxDentalIns;

            d.VisionInsProvider = VisionInsProvider;
            d.EEelectionPreTaxVisionIns = EEelectionPreTaxVisionIns;
            d.PremiumPreTaxVisionIns = PremiumPreTaxVisionIns;
            d.EEelectionPostTaxVisionIns = EEelectionPostTaxVisionIns;
            d.PremiumPostTaxVisionIns = PremiumPostTaxVisionIns;

            d.StDisabilityProvider = StDisabilityProvider;
            d.EEelectionPreTaxStDisability = EEelectionPreTaxStDisability;
            d.PremiumPreTaxStDisability = PremiumPreTaxStDisability;
            d.EEelectionPostTaxStDisability = EEelectionPostTaxStDisability;
            d.PremiumPostTaxStDisability = PremiumPostTaxStDisability;

            d.HospitalIndemProvider = HospitalIndemProvider;
            d.EEelectionPreTaxHospitalIndem = EEelectionPreTaxHospitalIndem;
            d.PremiumPreTaxHospitalIndem = PremiumPreTaxHospitalIndem;
            d.EEelectionPostTaxHospitalIndem = EEelectionPostTaxHospitalIndem;
            d.PremiumPostTaxHospitalIndem = PremiumPostTaxHospitalIndem;

            d.TermLifeInsProvider = TermLifeInsProvider;
            d.EEelectionPreTaxTermLifeIns = EEelectionPreTaxTermLifeIns;
            d.PremiumPreTaxTermLifeIns = PremiumPreTaxTermLifeIns;
            d.EEelectionPostTaxTermLifeIns = EEelectionPostTaxTermLifeIns;
            d.PremiumPostTaxTermLifeIns = PremiumPostTaxTermLifeIns;

            d.WholeLifeInsProvider = WholeLifeInsProvider;
            d.EEelectionPreTaxWholeLifeIns = EEelectionPreTaxWholeLifeIns;
            d.PremiumPreTaxWholeLifeIns = PremiumPreTaxWholeLifeIns;
            d.EEelectionPostTaxWholeLifeIns = EEelectionPostTaxWholeLifeIns;
            d.PremiumPostTaxWholeLifeIns = PremiumPostTaxWholeLifeIns;

            d.AccidentProvider = AccidentProvider;//data shows null
            d.EEelectionPreTaxAccidentIns = EEelectionPreTaxAccidentIns;
            d.PremiumPreTaxAccidentIns = PremiumPreTaxAccidentIns;
            d.EEelectionPostTaxAccidentIns = EEelectionPostTaxAccidentIns;
            d.PremiumPostTaxAccidentIns = PremiumPostTaxAccidentIns;

            d.CancerProvider = CancerProvider; //data shows null
            d.EEelectionPreTaxCancerIns = EEelectionPreTaxCancerIns;
            d.PremiumPreTaxCancerIns = PremiumPreTaxCancerIns;
            d.EEelectionPostTaxCancerIns = EEelectionPostTaxCancerIns;
            d.PremiumPostTaxCancerIns = PremiumPostTaxCancerIns;

            d.OtherInsProvider = OtherInsProvider;
            d.EEelectionPreTaxOtherIns = EEelectionPreTaxOtherIns;
            d.PremiumPreTaxOtherIns = PremiumPreTaxOtherIns;
            d.EEelectionPostTaxOtherIns = EEelectionPostTaxOtherIns;
            d.PremiumPostTaxOtherIns = PremiumPostTaxOtherIns;

            d.TotalPreTax = TotalPreTax;
            d.TotalPostTax = TotalPostTax;
            d.EmployeeSignature = empSignature;
            d.EmployeeSignatureDate = empSignatureDate;
            d.EmployeeInitials = empInitials1;
            d.PreTaxBenefitWaiverinitials = PreTaxBenefitWaiverinitials;

            if (ModelState.IsValid)
            {
                db.Entry(d).State = System.Data.Entity.EntityState.Modified;

                try
                {
                    db.SaveChanges();
                }
                catch (Exception err)
                {
                    Console.WriteLine(err);
                }

                RedirectToAction("Overview", "Employee", new { d.Employee_id });
            }

            int result = d.Deductions_id;

            return Json(new { data = result }, JsonRequestBehavior.AllowGet);
        }

        //AuthorizationForm-Start-----------------------------------------------------------------------------

        public ActionResult AuthorizationForm(int? Employee_id, int? GroupHealthInsurance_id)
        {
            ViewBag.Employee_id = Employee_id;
            ViewBag.GroupHealthInsurance_id = GroupHealthInsurance_id;

            EmployeeAndInsuranceVM employeeAndInsVM = new EmployeeAndInsuranceVM();

            employeeAndInsVM.employee = db.Employees.FirstOrDefault(i => i.Employee_id == Employee_id);
            employeeAndInsVM.grpHealth = db.Group_Health.FirstOrDefault(i => i.Employee_id == Employee_id);


            return View(employeeAndInsVM);
        }

        //====================================
        //Post-AuthorizationFormNew
        //====================================
        public JsonResult AuthorizationFormNew(int? GroupHealthInsurance_id, int Employee_id, string PersonOneReleaseInfoTo, string PersonOneRelationship,
            string PersonTwoReleaseInfoTo, string PersonTwoRelationship, string PolicyHolderSignature, DateTime? PolicyHolderSignatureDate,
            string PersonOneSignature, DateTime? PersonOneSignatureDate, string PersonTwoSignature, DateTime? PersonTwoSignatureDate)
        {

            Group_Health g = db.Group_Health
                .Where(i => i.GroupHealthInsurance_id == GroupHealthInsurance_id)
                .SingleOrDefault();

            g.Employee_id = Employee_id;
            g.NameOfPersonOneReleaseInfoTo = PersonOneReleaseInfoTo;
            g.PersonOneRelationship = PersonOneRelationship;
            g.NameOfPersonTwoReleaseInfoTo = PersonTwoReleaseInfoTo;
            g.PersonTwoRelationship = PersonTwoRelationship;
            g.AuthorizationFormPolicyHolderSignature = PolicyHolderSignature;
            g.AuthorizationFormPolicyHolderSignatureDate = PolicyHolderSignatureDate;
            g.PersonOneSignature = PersonOneSignature;
            g.PersonOneSignatureDate = PersonOneSignatureDate;
            g.PersonTwoSignature = PersonTwoSignature;
            g.PersonTwoSignatureDate = PersonTwoSignatureDate;

            db.SaveChanges();

            int result = g.GroupHealthInsurance_id;

            return Json(new { data = result }, JsonRequestBehavior.AllowGet);
        }

        //----------------------------------------------------------------------------------------

        //Edit-AuthorizationForm
        public ActionResult EditAuthorizationForm(int? Employee_id, int? GroupHealthInsurance_id)
        {
            ViewBag.Employee_id = Employee_id;
            ViewBag.GroupHealthInsurance_id = GroupHealthInsurance_id;

            EmployeeAndInsuranceVM employeeAndInsVM = new EmployeeAndInsuranceVM();

            employeeAndInsVM.employee = db.Employees.FirstOrDefault(i => i.Employee_id == Employee_id);
            employeeAndInsVM.grpHealth = db.Group_Health.FirstOrDefault(i => i.Employee_id == Employee_id);

            //if (Employee_id == null)
            //{
            //    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            //}

            //Group_Health g = db.Group_Health.Find(Employee_id);
            //if (g == null)
            //{
            //    return HttpNotFound();
            //}

            return View(employeeAndInsVM);
        }

        //====================================
        //Post-AuthorizationFormEditUpdate
        //====================================
        public JsonResult AuthorizationFormEditUpdate(int? Employee_id, int? GroupHealthInsurance_id, string PersonOneReleaseInfoTo, string PersonOneRelationship,
            string PersonTwoReleaseInfoTo, string PersonTwoRelationship, string PolicyHolderSignature, DateTime PolicyHolderSignatureDate,
            string PersonOneSignature, DateTime PersonOneSignatureDate, string PersonTwoSignature, DateTime PersonTwoSignatureDate)
        {

            Group_Health g = db.Group_Health
                .Where(i => i.GroupHealthInsurance_id == GroupHealthInsurance_id)
                .Single();

            g.NameOfPersonOneReleaseInfoTo = PersonOneReleaseInfoTo;
            g.PersonOneRelationship = PersonOneRelationship;
            g.NameOfPersonTwoReleaseInfoTo = PersonTwoReleaseInfoTo;
            g.PersonTwoRelationship = PersonTwoRelationship;
            g.AuthorizationFormPolicyHolderSignature = PolicyHolderSignature;
            g.AuthorizationFormPolicyHolderSignatureDate = PolicyHolderSignatureDate;
            g.PersonOneSignature = PersonOneSignature;
            g.PersonOneSignatureDate = PersonOneSignatureDate;
            g.PersonTwoSignature = PersonTwoSignature;
            g.PersonTwoSignatureDate = PersonTwoSignatureDate;

            if (ModelState.IsValid)
            {
                db.Entry(g).State = System.Data.Entity.EntityState.Modified;

                try
                {
                    db.SaveChanges();
                }
                catch (Exception err)
                {
                    Console.WriteLine(err);
                }

                RedirectToAction("Overview", "Employee", new { g.Employee_id });
            }

            int result = g.Employee_id;

            return Json(new { data = result }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DeleteGroupHealthIns(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Group_Health groupHealth = db.Group_Health.Find(id);
            if (groupHealth == null)
            {
                return HttpNotFound();
            }

            return View(groupHealth);
        }

        //====================================
        //Post-ConfirmDeleteGroupHealth
        //====================================
        [System.Web.Mvc.HttpPost, System.Web.Mvc.ActionName("DeleteGroupHealthIns")]
        [ValidateAntiForgeryToken]
        public ActionResult ConfirmDeleteGroupHealth(int id)
        {
            Group_Health groupHealth = db.Group_Health.Find(id);
            db.Group_Health.Remove(groupHealth);
            db.SaveChanges();

            //db.DeleteEmployeeAndDependents(id);

            return RedirectToAction("GroupHealthEnrollment");
        }

    }
}