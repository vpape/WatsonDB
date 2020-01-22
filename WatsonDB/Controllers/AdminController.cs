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

namespace WatsonDB.Controllers
{
    //[Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private WatsonDBContext db = new WatsonDBContext();
        private static List<Employee> employee = new List<Employee>();
        private static List<Family_Info> family = new List<Family_Info>();
        private static List<Other_Insurance> otherIns = new List<Other_Insurance>();
        private static Group_Health grpHealth = new Group_Health();
        private static Life_Insurance lifeIns = new Life_Insurance();
        private static List<JobApplicant> applicant = new List<JobApplicant>();

        ApplicationDbContext context;
        public AdminController()
        {
            context = new ApplicationDbContext();
        }

        public ActionResult Index()
        {

            if (User.Identity.IsAuthenticated)
            {
                var user = User.Identity;
                ViewBag.Name = user.Name;

                if (!isAdminUser())
                {
                    return RedirectToAction("Index", "Home");
                }
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }

            var roles = context.Roles.ToList();
            return View(roles);
        }

        public bool isAdminUser()
        {
            if (User.Identity.IsAuthenticated)
            {
                var user = User.Identity;
                ApplicationDbContext context = new ApplicationDbContext();
                var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
                var s = UserManager.GetRoles(user.GetUserId());
                if (s[0].ToString() == "Admin")
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return false;
        }

        //====================================
        // Get: Overview
        //====================================
        public ActionResult Overview(int? Employee_id)
        {

            //var family_Infoes = db.Family_Infoes.Include(f => f.Employee).Where(f => f.Employee_id == id);

            Employee employee = db.Employees.Where(i => i.Employee_id == Employee_id).FirstOrDefault();
            if (Employee_id == null)
            {
                //return View(db.Employees.Find(Employee_id));
                return View(db.Employees.ToList());

            }
            else
            {
                return View(db.Employees.Find(Employee_id));
                //return View(db.Employees.ToList());
            }
        }


        //----------------------------------------------------------------------------------------

        public ActionResult EnrollmentSelection()
        {
            return View();
        }

        public ActionResult CreateEmployee()
        {
            return View();
        }

        //====================================
        // Post: CreateEmployeeNew
        //====================================
        public JsonResult CreateEmployeeNew(string Role, string EmployeeNumber, string CurrentEmployer, string JobTitle, string SSN, DateTime HireDate,
            string MaritalStatus, string FirstName, string LastName, DateTime DateOfBirth, string Gender, string Active, string Retired, string CobraState)
        {
            Employee e = new Employee();

            //e.Id = Id;
            e.UserRole = Role;
            e.CurrentEmployer = CurrentEmployer;
            e.JobTitle = JobTitle;
            e.SSN = SSN;
            e.HireDate = HireDate;
            e.MaritalStatus = MaritalStatus;
            e.FirstName = FirstName;
            e.LastName = LastName;
            e.DateOfBirth = DateOfBirth;
            e.Gender = Gender;
            e.Active = Active;
            e.Retired = Retired;
            e.CobraStateContinuation = CobraState;

            ViewBag.Employee_id = e.Employee_id;

            db.Employees.Add(e);
            db.SaveChanges();

            int result = e.Employee_id;

            return Json(new { data = result }, JsonRequestBehavior.AllowGet);
        }

        //====================================
        // Post: EmployeeContact
        //====================================
        public JsonResult EmployeeContact(int Employee_id, string MailingAddress, string PObox, string City, string State, string ZipCode, string CityLimits,
            string County, string PhysicalAddress, string City2, string State2, string ZipCode2, string EmailAddress, string PhoneNumber, string CellPhone)
        {
            Employee e = db.Employees
               .Where(i => i.Employee_id == Employee_id)
               .Single();

            e.MailingAddress = MailingAddress;
            e.PObox = PObox;
            e.City = City;
            e.State = State;
            e.ZipCode = ZipCode;
            e.CityLimits = CityLimits;
            e.County = County;
            e.PhysicalAddress = PhysicalAddress;
            e.CityTwo = City2;
            e.StateTwo = State2;
            e.ZipCodeTwo = ZipCode2;
            e.EmailAddress = EmailAddress;
            e.PhoneNumber = PhoneNumber;
            e.CellPhone = CellPhone;

            ViewBag.Employee_id = e.Employee_id;

            if (ModelState.IsValid)
            {
                try
                {
                    db.SaveChanges();

                    if (e.MaritalStatus == "Married")
                    {
                        RedirectToAction("SpouseEnrollment", "Employee", new { e.Employee_id, e.MaritalStatus });
                        RedirectToAction("SpouseEnrollment");
                    }
                    else if (e.MaritalStatus == "MarriedwDep")
                    {
                        RedirectToAction("SpouseEnrollment", new { e.Employee_id, e.MaritalStatus });
                        RedirectToAction("SpouseEnrollment", "Employee", new { e.Employee_id, e.MaritalStatus });
                    }
                    else if (e.MaritalStatus == "SinglewDep")
                    {
                        RedirectToAction("SpouseEnrollment", "Employee");
                        RedirectToAction("DependentEnrollment", "Employee", new { e.Employee_id, e.MaritalStatus });
                    }
                    else
                    {
                        RedirectToAction("EnrollmentSelection", "Employee", new { e.Employee_id, e.MaritalStatus });
                    }
                }

                catch (Exception emp)
                {
                    Console.WriteLine(emp);
                }
            }

            int result = e.Employee_id;

            return Json(new { data = result }, JsonRequestBehavior.AllowGet);
        }

        //====================================
        // Post: EmployeeOtherInsurance
        //====================================
        public JsonResult EmployeeOtherInsurance(int Employee_id, int? GroupHealthInsurance_id, string empOtherGrpHinsCoverage, string empInsCarrier,
           string empInsPolicyNumber, string empInsPhoneNumber)
        {
            Group_Health grp = new Group_Health();

            grp.Employee_id = Employee_id;
            grp.OtherInsuranceCoverage = empOtherGrpHinsCoverage;
            grp.InsuranceCarrier = empInsCarrier;
            grp.PolicyNumber = empInsPolicyNumber;
            grp.PhoneNumber = empInsPhoneNumber;

            ViewBag.GroupHealthInsurance_id = grp.GroupHealthInsurance_id;

            int result = grp.GroupHealthInsurance_id;

            db.Group_Health.Add(grp);
            db.SaveChanges();

            return Json(new { data = result }, JsonRequestBehavior.AllowGet);
        }

        //Edit-Emp
        public ActionResult EditEmployee(int? Employee_id, string MaritalStatus)
        {
            EmployeeAndInsuranceVM employeeAndInsuranceVM = new EmployeeAndInsuranceVM();

            employeeAndInsuranceVM.employee = db.Employees.FirstOrDefault(i => i.Employee_id == Employee_id);
            employeeAndInsuranceVM.grpHealth = db.Group_Health.FirstOrDefault(i => i.Employee_id == Employee_id);

            //ViewBag.MaritalStatus = employeeAndInsuranceVM.employee.MaritalStatus;

            if (Employee_id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Employee e = db.Employees.Find(Employee_id);
            if (e == null)
            {
                return HttpNotFound();
            }


            ViewBag.Employee_id = e.Employee_id;

            return View(employeeAndInsuranceVM);
        }

        //====================================
        // Post: EmployeeEditUpdate
        //====================================
        public JsonResult EmployeeEditUpdate(int? Employee_id, string EmpRole, string EmployeeNumber, string CurrentEmployer, string JobTitle, string SSN,
            DateTime? HireDate, string FirstName, string LastName, DateTime? DateOfBirth, string Gender, string MaritalStatus, string MailingAddress, string PObox,
            string City, string State, string ZipCode, string County, string PhysicalAddress, string City2, string State2, string ZipCode2, string CityLimits,
            string EmailAddress, string PhoneNumber, string CellPhone, string OtherGrpHinsCoverage, string InsCarrier, string InsPolicyNumber,
            string InsPhoneNumber, string Active, string Retired, string CobraState)
        {
            var e = db.Employees
                .Where(i => i.Employee_id == Employee_id)
                .Single();

            //e.Id = Id;
            e.UserRole = EmpRole;
            e.CurrentEmployer = CurrentEmployer;
            e.JobTitle = JobTitle;
            e.SSN = SSN;
            e.HireDate = HireDate;
            e.FirstName = FirstName;
            e.LastName = LastName;
            e.DateOfBirth = DateOfBirth;
            e.Gender = Gender;
            e.MaritalStatus = MaritalStatus;
            e.MailingAddress = MailingAddress;
            e.PObox = PObox;
            e.City = City;
            e.State = State;
            e.ZipCode = ZipCode;
            e.County = County;
            e.PhysicalAddress = PhysicalAddress;
            e.CityTwo = City2;
            e.StateTwo = State2;
            e.ZipCodeTwo = ZipCode2;
            e.CityLimits = CityLimits;
            e.EmailAddress = EmailAddress;
            e.PhoneNumber = PhoneNumber;
            e.CellPhone = CellPhone;
            e.Active = Active;
            e.Retired = Retired;
            e.CobraStateContinuation = CobraState;


            var grph = db.Group_Health
              .Where(i => i.Employee_id == Employee_id)
              .Single();

            //Group_Health grph = new Group_Health();

            grph.OtherInsuranceCoverage = OtherGrpHinsCoverage;
            grph.InsuranceCarrier = InsCarrier;
            grph.PolicyNumber = InsPolicyNumber;
            grph.PhoneNumber = InsPhoneNumber;

            ViewBag.Employee_id = e.Employee_id;
            ViewBag.EmployeeRole = e.UserRole;

            if (ModelState.IsValid)
            {
                db.Entry(e).State = System.Data.Entity.EntityState.Modified;
                db.Entry(grph).State = System.Data.Entity.EntityState.Modified;

                try
                {
                    db.SaveChanges();
                }
                catch (Exception err)
                {
                    Console.WriteLine(err);
                }

                RedirectToAction("Overview", new { e.Employee_id });
            }

            int result = e.Employee_id;

            return Json(new { data = result }, JsonRequestBehavior.AllowGet);
        }

        //Get-EmpDetail
        public ActionResult EmployeeDetail(int? Employee_id)
        {
            EmployeeAndInsuranceVM employeeAndInsuranceVM = new EmployeeAndInsuranceVM();

            employeeAndInsuranceVM.employee = db.Employees.FirstOrDefault(i => i.Employee_id == Employee_id);
            employeeAndInsuranceVM.grpHealth = db.Group_Health.FirstOrDefault(i => i.Employee_id == Employee_id);

            if (Employee_id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Employee e = db.Employees.Find(Employee_id);
            if (e == null)
            {
                return HttpNotFound();
            }

            return View(employeeAndInsuranceVM);
        }

        //----------------------------------------------------------------------------------------

        //DeleteEmp Method
        public ActionResult DeleteEmployee(int? Employee_id)
        {
            EmployeeAndInsuranceVM employeeAndInsuranceVM = new EmployeeAndInsuranceVM();

            employeeAndInsuranceVM.employee = db.Employees.FirstOrDefault(i => i.Employee_id == Employee_id);
            employeeAndInsuranceVM.grpHealth = db.Group_Health.FirstOrDefault(i => i.Employee_id == Employee_id);

            if (Employee_id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Employee e = db.Employees.Find(Employee_id);
            if (e == null)
            {
                return HttpNotFound();
            }

            return View(employeeAndInsuranceVM);
        }

        //====================================
        // Post: DeleteConfirmed
        //====================================
        [System.Web.Mvc.HttpPost, System.Web.Mvc.ActionName("DeleteEmployee")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int? Employee_id)
        {
            var employee = db.Employees.Find(Employee_id);
            if (employee == null)
            {
                ViewBag.ErrorMessage = $"Employee cannot be found";
                return View("NotFound");
                //return HttpNotFound();
            }

            db.Employees.Remove(db.Employees.FirstOrDefault(e => e.Employee_id == Employee_id));
            db.SaveChanges();

            //db.DeleteEmployeeAndDependents(Employee_id);
            //db.Employees.Remove(e);

            return RedirectToAction("Overview", "Admin");
        }

        //----------------------------------------------------------------------------------------

        public ActionResult FamilyOverview(int? Employee_id, int? FamilyMember_id)
        {
            ViewBag.Employee_id = Employee_id;
            ViewBag.FamilyMember_id = FamilyMember_id;

            var familyInfo = (from fi in db.Family_Info
                              where fi.Employee_id == Employee_id
                              select fi).ToList();

            return View(familyInfo);
        }

        //show/hide family enrollment code block based on martial status
        public ActionResult FamilyEnrollment(int? Employee_id, int? FamilyMember_id, string MaritalStatus)
        {

            Employee e = db.Employees.Find(Employee_id);

            ViewBag.Employee_id = Employee_id;
            ViewBag.FamilyMember_id = FamilyMember_id;

            return View();
        }

        //----------------------------------------------------------------------------------------

        //SpEnrollment Method
        public ActionResult SpouseEnrollment(int Employee_id, int? FamilyMember_id, string MaritalStatus, string RelationshipToInsured)
        {
            ViewBag.Employee_id = Employee_id;
            ViewBag.FamilyMember_id = FamilyMember_id;
            ViewBag.RelationshipToInsured = RelationshipToInsured = "Spouse";

            Employee e = db.Employees.Find(Employee_id);
            ViewBag.MaritalStatus = e.MaritalStatus;

            return View();
        }

        //====================================
        // Post: SpEnrollmentNew
        //====================================
        public JsonResult SpEnrollmentNew(int Employee_id, int? FamilyMember_id, string RelationshipToInsured, string MaritalStatus, string SSN,
            string FirstName, string LastName, DateTime DateOfBirth, string Gender)
        {
            string response = "";

            //int record = (from fi in db.Family_Info
            //              where fi.Employee_id == Employee_id
            //              select fi).Count();

            //if (record > 0)
            //{
            //    response = "Record already exists.";

            //}
            //else
            //{ }

            Family_Info sp = new Family_Info();

            sp.Employee_id = Employee_id;
            sp.RelationshipToInsured = RelationshipToInsured;
            sp.MaritalStatus = MaritalStatus;
            sp.SSN = SSN;
            sp.FirstName = FirstName;
            sp.LastName = LastName;
            sp.DateOfBirth = DateOfBirth;
            sp.Gender = Gender;

            db.Family_Info.Add(sp);
            db.SaveChanges();


            int result = sp.FamilyMember_id;

            return Json(new { data = result, error = response }, JsonRequestBehavior.AllowGet);

        }

        //====================================
        // Post: SpEnrollmentContact
        //====================================
        public JsonResult SpEnrollmentContact(int? Employee_id, int? FamilyMember_id, string MailingAddress, string PObox, string City, string State,
            string ZipCode, string County, string PhysicalAddress, string City2, string State2, string ZipCode2, string EmailAddress, string PhoneNumber,
            string CellPhone)
        {

            var sp = (from fi in db.Family_Info
                      where fi.FamilyMember_id == FamilyMember_id
                      select fi).SingleOrDefault();

            sp.MailingAddress = MailingAddress;
            sp.PObox = PObox;
            sp.City = City;
            sp.State = State;
            sp.ZipCode = ZipCode;
            sp.County = County;
            sp.PhysicalAddress = PhysicalAddress;
            sp.CityTwo = City2;
            sp.StateTwo = State2;
            sp.ZipCodeTwo = ZipCode2;
            sp.EmailAddress = EmailAddress;
            sp.PhoneNumber = PhoneNumber;
            sp.CellPhone = CellPhone;

            db.SaveChanges();

            int result = sp.FamilyMember_id;

            return Json(new { data = result }, JsonRequestBehavior.AllowGet);
        }

        //====================================
        // Post: SpouseEmployment
        //====================================
        public JsonResult SpouseEmployment(int? FamilyMember_id, int Employee_id, string Employer, string EmployerAddress, string EmployerPObox,
            string EmployerCity, string EmployerState, string EmployerZipCode, string EmployerPhoneNumber, string spOtherInsurance,
            string spOtherMedicalCoverage, string spOtherDentalCoverage, string spOtherVisionCoverage, string spIndemnityCoverage)
        {

            var sp = (from fi in db.Family_Info
                      where fi.FamilyMember_id == FamilyMember_id
                      select fi).SingleOrDefault();

            sp.Employee_id = Employee_id;
            sp.Employer = Employer;
            sp.EmployerMailingAddress = EmployerAddress;
            sp.EmployerPObox = EmployerPObox;
            sp.EmployerCity = EmployerCity;
            sp.EmployerState = EmployerState;
            sp.EmployerZipCode = EmployerZipCode;
            sp.EmployerPhoneNumber = EmployerPhoneNumber;

            sp.OtherInsuranceCoverage = spOtherInsurance;
            sp.Medical = spOtherMedicalCoverage;
            sp.Dental = spOtherDentalCoverage;
            sp.Vision = spOtherVisionCoverage;
            sp.Indemnity = spIndemnityCoverage;


            db.SaveChanges();

            int result = sp.FamilyMember_id;

            return Json(new { data = result }, JsonRequestBehavior.AllowGet);
        }

        //====================================
        // Post: SpOtherInsurance
        //====================================
        public JsonResult SpOtherInsurance(int Employee_id, int FamilyMember_id, string spInsuranceCoverage, string spInsCarrier, string spInsMailingAddress,
            string spInsCity, string spInsState, string spInsZipCode, string spInsPhoneNumber, string spInsPolicyNumber)
        {
            Other_Insurance other = new Other_Insurance();

            other.Employee_id = Employee_id;
            other.FamilyMember_id = FamilyMember_id;
            other.CoveredByOtherInsurance = spInsuranceCoverage;
            other.InsuranceCarrier = spInsCarrier;
            other.MailingAddress = spInsMailingAddress;
            other.City = spInsCity;
            other.State = spInsState;
            other.ZipCode = spInsZipCode;
            other.PhoneNumber = spInsPhoneNumber;
            other.PolicyNumber = spInsPolicyNumber;

            db.Other_Insurance.Add(other);
            db.SaveChanges();

            int result = other.OtherInsurance_id;

            return Json(new { data = result }, JsonRequestBehavior.AllowGet);
        }

        //----------------------------------------------------------------------------------------

        //EditSp Method
        public ActionResult EditSpouse(int? FamilyMember_id, int? Employee_id, int? OtherInsurance_id, string MaritalStatus, string RelationshipToInsured)
        {
            SpouseAndDependentInsVM spAndDepInsVM = new SpouseAndDependentInsVM();

            spAndDepInsVM.family = db.Family_Info.FirstOrDefault(i => i.FamilyMember_id == FamilyMember_id);
            spAndDepInsVM.otherIns = db.Other_Insurance.FirstOrDefault(i => i.FamilyMember_id == FamilyMember_id);

            if (FamilyMember_id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Family_Info f = db.Family_Info.Find(FamilyMember_id);
            if (f == null)
            {
                return HttpNotFound();
            }

            ViewBag.Employee_id = f.Employee_id;
            ViewBag.FamilyMember_id = f.FamilyMember_id;
            ViewBag.MaritalStatus = f.MaritalStatus;
            ViewBag.RelationshipToInsured = f.RelationshipToInsured;


            return View(spAndDepInsVM);
        }

        //====================================
        // Post: SpEditUpdate
        //====================================
        public JsonResult SpEditUpdate(int? Employee_id, int? FamilyMember_id, string RelationshipToInsured, string MaritalStatus, string SSN,
            string FirstName, string LastName, DateTime DateOfBirth, string Gender, string MailingAddress, string PObox, string City, string State,
            string ZipCode, string County, string PhysicalAddress, string City2, string State2, string ZipCode2, string EmailAddress, string PhoneNumber,
            string CellPhone, string Employer, string EmployerAddress, string EmployerPObox, string EmployerCity, string EmployerState, string EmployerZipCode,
            string EmployerPhoneNumber, string spOtherInsurance, string spOtherMedicalCoverage, string spOtherDentalCoverage, string spOtherVisionCoverage,
            string spIndemnityCoverage, string spInsuranceCoverage, string spInsCarrier, string spInsMailingAddress, string spInsCity, string spInsState,
            string spInsZipCode, string spInsPhoneNumber, string spInsPolicyNumber)
        {
            var sp = db.Family_Info
                .Where(i => i.FamilyMember_id == FamilyMember_id)
                .Where(i => i.RelationshipToInsured == "Spouse")
                .Single();

            ViewBag.FamilyMember_id = sp.FamilyMember_id;
            ViewBag.RelationshipToInsured = "Spouse";
            ViewBag.MaritalStatus = sp.MaritalStatus;

            sp.RelationshipToInsured = RelationshipToInsured;
            sp.MaritalStatus = MaritalStatus;
            sp.SSN = SSN;
            sp.FirstName = FirstName;
            sp.LastName = LastName;
            sp.DateOfBirth = DateOfBirth;
            sp.Gender = Gender;
            sp.MailingAddress = MailingAddress;
            sp.PObox = PObox;
            sp.City = City;
            sp.State = State;
            sp.ZipCode = ZipCode;
            sp.County = County;
            sp.PhysicalAddress = PhysicalAddress;
            sp.CityTwo = City2;
            sp.StateTwo = State2;
            sp.ZipCodeTwo = ZipCode2;
            sp.EmailAddress = EmailAddress;
            sp.PhoneNumber = PhoneNumber;
            sp.CellPhone = CellPhone;
            sp.Employer = Employer;
            sp.EmployerMailingAddress = EmployerAddress;
            sp.EmployerPObox = EmployerPObox;
            sp.EmployerCity = EmployerCity;
            sp.EmployerState = EmployerState;
            sp.EmployerZipCode = EmployerZipCode;
            sp.EmployerPhoneNumber = EmployerPhoneNumber;

            sp.OtherInsuranceCoverage = spOtherInsurance;
            sp.Medical = spOtherMedicalCoverage;
            sp.Dental = spOtherDentalCoverage;
            sp.Vision = spOtherVisionCoverage;
            sp.Indemnity = spIndemnityCoverage;

            Other_Insurance spOtherIns = db.Other_Insurance
                  .Where(i => i.FamilyMember_id == FamilyMember_id)
                  .Single();

            spOtherIns.CoveredByOtherInsurance = spInsuranceCoverage;
            spOtherIns.InsuranceCarrier = spInsCarrier;
            spOtherIns.MailingAddress = spInsMailingAddress;
            spOtherIns.City = spInsCity;
            spOtherIns.State = spInsState;
            spOtherIns.ZipCode = spInsZipCode;
            spOtherIns.PhoneNumber = spInsPhoneNumber;
            spOtherIns.PolicyNumber = spInsPolicyNumber;


            if (ModelState.IsValid)
            {
                db.Entry(sp).State = System.Data.Entity.EntityState.Modified;
                db.Entry(spOtherIns).State = System.Data.Entity.EntityState.Modified;

                try
                {
                    db.SaveChanges();
                }
                catch (Exception err)
                {
                    Console.WriteLine(err);
                }
            }

            int result = sp.Employee_id;

            return Json(new { data = result }, JsonRequestBehavior.AllowGet);

        }

        //Get-SpDetail
        public ActionResult SpouseDetail(int? Employee_id, int? FamilyMember_id, string MaritalStatus)
        {
            //ViewBag.spouseExist = !(MaritalStatus == "Single" || MaritalStatus == "SinglewDep");

            SpouseAndDependentInsVM spAndDepInsVM = new SpouseAndDependentInsVM();

            spAndDepInsVM.family = db.Family_Info.FirstOrDefault(i => i.FamilyMember_id == FamilyMember_id);
            spAndDepInsVM.otherIns = db.Other_Insurance.FirstOrDefault(i => i.FamilyMember_id == FamilyMember_id);

            if (FamilyMember_id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Family_Info f = db.Family_Info.Find(FamilyMember_id);
            if (family == null)
            {
                return HttpNotFound();
            }

            ViewBag.FamilyMember_id = spAndDepInsVM.family.FamilyMember_id;
            ViewBag.Employee_id = spAndDepInsVM.family.Employee_id;
            ViewBag.RelationshipToInsured = spAndDepInsVM.family.RelationshipToInsured;

            return View(spAndDepInsVM);
        }

        //----------------------------------------------------------------------------------------

        //DeleteSp Method
        public ActionResult DeleteSpouse(int? Employee_id, int? FamilyMember_id)
        {
            SpouseAndDependentInsVM spAndDepInsVM = new SpouseAndDependentInsVM();

            spAndDepInsVM.family = db.Family_Info.FirstOrDefault(i => i.FamilyMember_id == FamilyMember_id);
            spAndDepInsVM.otherIns = db.Other_Insurance.FirstOrDefault(i => i.FamilyMember_id == FamilyMember_id);

            if (FamilyMember_id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Family_Info sp = db.Family_Info.Find(FamilyMember_id);
            if (sp == null)
            {
                return HttpNotFound();
            }

            return View(spAndDepInsVM);
        }

        //====================================
        // Post: SpDeleteConfirmed
        //====================================
        [System.Web.Mvc.HttpPost, System.Web.Mvc.ActionName("DeleteSpouse")]
        [ValidateAntiForgeryToken]
        public ActionResult SpDeleteConfirmed(int? FamilyMember_id)
        {
            Family_Info sp = db.Family_Info.Find(FamilyMember_id);
            if (sp == null)
            {
                return HttpNotFound();
            }

            db.Family_Info.Remove(db.Family_Info.FirstOrDefault(f => f.FamilyMember_id == FamilyMember_id));
            db.SaveChanges();

            //db.Other_Insurance.Find(FamilyMember_id);
            //db.DeleteEmployeeAndDependents(FamilyMember_id);
            //db.Family_Info.Remove(sp);
            //db.Other_Insurance.Remove(other);

            return RedirectToAction("FamilyOverview", new { sp.Employee_id });

        }


        //----------------------------------------------------------------------------------------

        public ActionResult AddDependent(int Employee_id, int? FamilyMember_id, string RelationshipToInsured)
        {

            //SpouseAndDependentInsVM spAndDepInsVM = new SpouseAndDependentInsVM();

            //spAndDepInsVM.family = db.Family_Info.FirstOrDefault(i => i.Employee_id == Employee_id);
            //spAndDepInsVM.otherIns = db.Other_Insurance.FirstOrDefault(i => i.Employee_id == Employee_id);

            ViewBag.Employee_id = Employee_id;
            ViewBag.FamilyMember_id = FamilyMember_id;
            ViewBag.RelationshipToInsured = RelationshipToInsured = "Dependent";

            return View();
        }

        //====================================
        // Post: DependentEnrollment
        //====================================
        public JsonResult DependentEnrollment(int Employee_id, int? FamilyMember_id, string RelationshipToInsured, string SSN, string DepFirstName, string DepLastName,
            DateTime DateOfBirth, string Gender, string MailingAddress, string City, string State, string ZipCode, string County, string Student, string Disabled,
            string NonStandardDependent, string AddDropDepLifeIns)
        {
            Family_Info dep = new Family_Info();

            dep.Employee_id = Employee_id;
            dep.RelationshipToInsured = RelationshipToInsured;
            dep.SSN = SSN;
            dep.FirstName = DepFirstName;
            dep.LastName = DepLastName;
            dep.DateOfBirth = DateOfBirth;
            dep.Gender = Gender;
            dep.MailingAddress = MailingAddress;
            dep.City = City;
            dep.State = State;
            dep.ZipCode = ZipCode;
            dep.County = County;
            dep.Student = Student;
            dep.Disabled = Disabled;
            dep.NonStandardDependent = NonStandardDependent;
            dep.AddDropDepLifeIns = AddDropDepLifeIns;

            db.Family_Info.Add(dep);
            db.SaveChanges();

            int result = dep.FamilyMember_id;

            return Json(new { data = result }, JsonRequestBehavior.AllowGet);
        }

        //====================================
        // Post: DependentOtherInsurance
        //====================================
        public JsonResult DependentOtherInsurance(int Employee_id, int FamilyMember_id, string depInsuranceCoverage, string depInsCarrier, string depInsPolicyNumber,
            string depInsPhoneNumber)
        {
            Other_Insurance other = new Other_Insurance();

            other.Employee_id = Employee_id;
            other.FamilyMember_id = FamilyMember_id;
            other.CoveredByOtherInsurance = depInsuranceCoverage;
            other.InsuranceCarrier = depInsCarrier;
            other.PolicyNumber = depInsPolicyNumber;
            other.PhoneNumber = depInsPhoneNumber;

            db.Other_Insurance.Add(other);
            db.SaveChanges();

            int result = other.OtherInsurance_id;

            return Json(new { data = result }, JsonRequestBehavior.AllowGet);
        }

        //EditDep Method
        public ActionResult EditDependent(int? Employee_id, int? FamilyMember_id, string RelationshipToInsured)
        {
            SpouseAndDependentInsVM spAndDepInsVM = new SpouseAndDependentInsVM();

            spAndDepInsVM.family = db.Family_Info.FirstOrDefault(i => i.FamilyMember_id == FamilyMember_id);
            spAndDepInsVM.otherIns = db.Other_Insurance.FirstOrDefault(i => i.FamilyMember_id == FamilyMember_id);
            //spAndDepInsVM.dependent = db.Family_Info.FirstOrDefault(i => i.FamilyMember_id == FamilyMember_id && i.RelationshipToInsured == "Dependent");

            if (FamilyMember_id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Family_Info f = db.Family_Info.Find(FamilyMember_id);
            if (f == null)
            {
                return HttpNotFound();
            }

            ViewBag.FamilyMember_id = spAndDepInsVM.family.FamilyMember_id;
            ViewBag.Employee_id = spAndDepInsVM.family.Employee_id;
            ViewBag.RelationshipToInsured = spAndDepInsVM.family.RelationshipToInsured;

            return View(spAndDepInsVM);
        }

        //====================================
        // Post: DependentEditUpdate
        //====================================
        public JsonResult DependentEditUpdate(int? Employee_id, int? FamilyMember_id, string RelationshipToInsured, string SSN, string DepFirstName, string DepLastName,
            DateTime DateOfBirth, string Gender, string EmpNumber, string MailingAddress, string City, string State, string ZipCode, string County, string Student,
            string Disabled, string NonStandardDependent, string AddDropDepLifeIns, string depInsuranceCoverage, string depInsCarrier, string depInsPolicyNumber,
            string depInsPhoneNumber)
        {
            Family_Info dep = db.Family_Info
                .Where(i => i.FamilyMember_id == FamilyMember_id)
                .Where(i => i.RelationshipToInsured == "Dependent")
                .Single();

            dep.RelationshipToInsured = RelationshipToInsured;
            dep.SSN = SSN;
            dep.FirstName = DepFirstName;
            dep.LastName = DepLastName;
            dep.DateOfBirth = DateOfBirth;
            dep.Gender = Gender;
            dep.MailingAddress = MailingAddress;
            dep.City = City;
            dep.State = State;
            dep.ZipCode = ZipCode;
            dep.County = County;
            dep.Student = Student;
            dep.Disabled = Disabled;
            dep.NonStandardDependent = NonStandardDependent;
            dep.AddDropDepLifeIns = AddDropDepLifeIns;

            var emp = new Employee()
            {
                SSN = EmpNumber
            };
            ViewBag.EmpNumber = emp;

            Other_Insurance depOtherIns = db.Other_Insurance
                    .Where(i => i.FamilyMember_id == FamilyMember_id)
                    .Single();

            depOtherIns.CoveredByOtherInsurance = depInsuranceCoverage;
            depOtherIns.InsuranceCarrier = depInsCarrier;
            depOtherIns.PolicyNumber = depInsPolicyNumber;
            depOtherIns.PhoneNumber = depInsPhoneNumber;


            int result = dep.Employee_id;

            if (ModelState.IsValid)
            {
                db.Entry(dep).State = System.Data.Entity.EntityState.Modified;
                db.Entry(depOtherIns).State = System.Data.Entity.EntityState.Modified;

                try
                {
                    db.SaveChanges();
                }
                catch (Exception error)
                {
                    Console.WriteLine(error);
                }

                RedirectToAction("FamilyOverview", new { dep.Employee_id });
            }

            return Json(new { data = result }, JsonRequestBehavior.AllowGet);
        }

        //Get-DepDetail
        public ActionResult DependentDetail(int? Employee_id, int? FamilyMember_id, string MaritalStatus, string RelationshipToInsured)
        {
            //ViewBag.spouseExist = !(MaritalStatus == "Single" || MaritalStatus == "SinglewDep");
            SpouseAndDependentInsVM spAndDepInsVM = new SpouseAndDependentInsVM();

            spAndDepInsVM.family = db.Family_Info.FirstOrDefault(i => i.FamilyMember_id == FamilyMember_id);
            spAndDepInsVM.otherIns = db.Other_Insurance.FirstOrDefault(i => i.FamilyMember_id == FamilyMember_id);

            if (FamilyMember_id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Family_Info f = db.Family_Info.Find(FamilyMember_id);
            if (family == null)
            {
                return HttpNotFound();
            }

            ViewBag.FamilyMember_id = f.FamilyMember_id;
            ViewBag.Employee_id = f.Employee_id;
            ViewBag.RelationshipToInsured = f.RelationshipToInsured;

            return View(spAndDepInsVM);
        }

        //Get-DepDetail
        public JsonResult GetDepDetail(int FamilyMember_id)
        {
            var sp = db.Family_Info
                 .Where(i => i.FamilyMember_id == FamilyMember_id)
                 .Single();

            ViewBag.FamilyMember_id = sp.FamilyMember_id;

            int result = sp.FamilyMember_id;

            return Json(new { data = result }, JsonRequestBehavior.AllowGet);
        }

        //----------------------------------------------------------------------------------------

        //DeleteDep Method
        public ActionResult DeleteDependent(int? FamilyMember_id)
        {
            SpouseAndDependentInsVM spAndDepInsVM = new SpouseAndDependentInsVM();

            spAndDepInsVM.family = db.Family_Info.FirstOrDefault(i => i.FamilyMember_id == FamilyMember_id);
            spAndDepInsVM.otherIns = db.Other_Insurance.FirstOrDefault(i => i.FamilyMember_id == FamilyMember_id);

            if (FamilyMember_id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Family_Info dep = db.Family_Info.Find(FamilyMember_id);
            if (dep == null)
            {
                return HttpNotFound();
            }

            return View(spAndDepInsVM);
        }

        //====================================
        // Post: DepDeleteConfirm
        //====================================
        [System.Web.Mvc.HttpPost, System.Web.Mvc.ActionName("DeleteDependent")]
        [ValidateAntiForgeryToken]
        public ActionResult DepDeleteConfirm(int? FamilyMember_id)
        {
            Family_Info dep = db.Family_Info.Find(FamilyMember_id);
            if (dep == null)
            {
                return HttpNotFound();
            }

            db.Family_Info.Remove(db.Family_Info.FirstOrDefault(f => f.FamilyMember_id == FamilyMember_id));
            db.SaveChanges();

            //db.Other_Insurance.Find(FamilyMember_id);
            //db.Other_Insurance.Remove(other);
            //db.DeleteEmployeeAndDependents(FamilyMember_id);
            //db.Family_Info.Remove(sp);

            return RedirectToAction("FamilyOverview", new { dep.Employee_id });
        }

        //----------------------------------------------------------------------------------------

        //====================================
        // Get: GroupHealthEnrollment
        //====================================
        public ActionResult GroupHealthEnrollment(int? Employee_id, int? GroupHealthInsurance_id)
        {
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
        public ActionResult EditGroupHealthIns(int? Employee_id, int? GroupHealthInsurance_id)
        {
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

            CreateGrpHealthPDF(g.Employee_id);

            return Json(new { data = result }, JsonRequestBehavior.AllowGet);
        }

        public void CreateGrpHealthPDF(int Employee_id)
        {
            System.Console.WriteLine("Starting conversion with WebClient ...");

            // set parameters
            SelectPdfParameters parameters = new SelectPdfParameters();
            parameters.key = apiKey;
            parameters.url = GrpHInsURL + Employee_id;

            // JSON serialize parameters
            string jsonData = JsonConvert.SerializeObject(parameters);
            byte[] byteData = Encoding.UTF8.GetBytes(jsonData);

            // create WebClient object
            WebClient webClient = new WebClient();
            webClient.Headers.Add(HttpRequestHeader.ContentType, "application/json");

            // POST parameters (if response code is not 200 OK, a WebException is raised)
            try
            {
                byte[] result = webClient.UploadData(apiEndpoint, "POST", byteData);

                // all ok - read PDF and write on disk (binary read!!!!)
                MemoryStream ms = new MemoryStream(result);

                // write to file
                FileStream file = new FileStream("test2.pdf", FileMode.Create, FileAccess.Write);
                ms.WriteTo(file);
                file.Close();

            }
            catch (WebException webEx)
            {
                // an error occurred
                System.Console.WriteLine("Error: " + webEx.Message);

                HttpWebResponse response = (HttpWebResponse)webEx.Response;
                Stream responseStream = response.GetResponseStream();

                // get details of the error message if available (text read!!!)
                StreamReader readStream = new StreamReader(responseStream);
                string message = readStream.ReadToEnd();
                responseStream.Close();

                System.Console.WriteLine("Error Message: " + message);
            }
            catch (Exception ex)
            {
                System.Console.WriteLine("Error: " + ex.Message);
            }

            System.Console.WriteLine("Finished.");

            // return resulted pdf document
            //FileResult fileResult = new FileContentResult(pdf, "application/pdf");
            //fileResult.FileDownloadName = "GrpHealth_Insurance.pdf";
            //return fileResult;

        }

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

                RedirectToAction("Overview", "Admin", new { d.Employee_id });
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

                RedirectToAction("Overview", "Admin", new { g.Employee_id });
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

        //----------------------------------------------------------------------------------------

        public ActionResult LifeInsuranceEnrollment(int? LifeInsurance_id, int? Employee_id, int? Beneficiary_id, string Message)
        {
            ViewBag.LifeInsurance_id = LifeInsurance_id;
            ViewBag.Employee_id = Employee_id;
            ViewBag.Beneficiary_id = Beneficiary_id;
            ViewBag.Message = Message;


            EmployeeAndInsuranceVM empAndInsVM = new EmployeeAndInsuranceVM();

            empAndInsVM.employee = db.Employees.FirstOrDefault(i => i.Employee_id == Employee_id);
            empAndInsVM.lifeIns = db.Life_Insurance.FirstOrDefault(i => i.Employee_id == Employee_id);
            empAndInsVM.benefiList = db.Beneficiaries.Where(i => i.Employee_id == Employee_id).ToList();

            empAndInsVM.spouse = db.Family_Info.FirstOrDefault(i => i.Employee_id == Employee_id && i.RelationshipToInsured == "Spouse");
            empAndInsVM.family = db.Family_Info.Where(i => i.Employee_id == Employee_id && i.RelationshipToInsured != "Spouse").ToList();
            if (empAndInsVM.spouse != null)
            {
                empAndInsVM.spouse = db.Family_Info.FirstOrDefault(i => i.Employee_id == Employee_id && i.FamilyMember_id == empAndInsVM.spouse.FamilyMember_id);
                empAndInsVM.family = db.Family_Info.Where(i => i.Employee_id == Employee_id && i.FamilyMember_id != empAndInsVM.spouse.FamilyMember_id).ToList();
            }
            else
            {
                empAndInsVM.spouse = null;
                empAndInsVM.family = db.Family_Info.Where(i => i.Employee_id == Employee_id).ToList();
            }

            return View(empAndInsVM);
        }

        //====================================
        // Post: LifeInsEnrollmentNew
        //====================================
        public JsonResult LifeInsEnrollmentNew(int Employee_id, string GroupPlanNumber, DateTime? BenefitsEffectiveDate, string InitialEnrollment, string ReEnrollment,
            string AddEmployeeAndDependents, string DropRefuseCoverage, string InformationChange, string IncreaseAmount, string FamilyStatusChange, string SubTotalCode,
            string Married, DateTime? DateOfMarriage, string OtherDependents, DateTime? DateOfAdoption, /*string AddDep, string DropDep,*/ string AddDropDep, string DropEmployee,
            string DropDependents, DateTime? LastDayOfCoverage, string TerminationEmploymentOfDropCoverage, string Retirement, DateTime? LastDayWorked, string OtherEvent,
            string OtherEventReason, DateTime? OtherEventDate, string DropBasicLife, string EmployeeDentalDrop, string SpouseDentalDrop, string DependentDentalDrop,
            string EmployeeVisionDrop, string SpouseVisionDrop, string DependentVisionDrop, string TerminationEmploymentLossOfOtherCoverage,
            DateTime? TerminationEmploymentDateLossOfOtherCoverage, string Divorce, DateTime? DivorceDate, string DeathOfSpouse, DateTime? DeathOfSpouseDate,
            string TerminationOrExpirationOfCoverage, DateTime? TerminationOrExpirationOfCoverageDate, string DentalCoverageLost, string VisionCoverageLost,
            string CoveredUnderOtherInsurance, string Other, string OtherReason, string DoNotWantDentalCoverage, string EmployeeCoveredUnderOtherDental,
            string SpouseCoveredUnderOtherDental, string DependentsCoveredUnderOtherDental, string DoNotWantVisionCoverage, string EmployeeCoveredUnderOtherVision,
            string SpouseCoveredUnderOtherVision, string DependentsCoveredUnderOtherVision, string PrimaryBeneficiary, string ContingentBeneficiary,
            string OwnerBasicLifeADandDPolicyAmount, string ManagerBasicLifeADandDPolicyAmount, string EmployeeBasicLifeADandDPolicyAmount,
            string DoNotWantBasicLifeCoverageADandD, string PreviousPolicyAmount, string DentalPlan, string VisionPlan, string EmployeeSignature,
            DateTime? EmployeeSignatureDate)
        {
            string response = "";

            int record = (from life in db.Life_Insurance
                          where life.Employee_id == Employee_id
                          select life).Count();

            if (record > 0)
            {
                response = "Record already exists.";

            }
            else
            {

                Life_Insurance lifeIns = new Life_Insurance();

                lifeIns.Employee_id = Employee_id;
                lifeIns.GroupPlanNumber = GroupPlanNumber;
                lifeIns.BenefitsEffectiveDate = BenefitsEffectiveDate;
                lifeIns.InitialEnrollment = InitialEnrollment;
                lifeIns.ReEnrollment = ReEnrollment;
                lifeIns.AddEmployeeAndDependents = AddEmployeeAndDependents;
                lifeIns.DropRefuseCoverage = DropRefuseCoverage;
                lifeIns.InformationChange = InformationChange;
                lifeIns.IncreaseAmount = IncreaseAmount;
                lifeIns.FamilyStatusChange = FamilyStatusChange;
                lifeIns.SubTotalCode = SubTotalCode;
                lifeIns.MarriedOrHaveSpouse = Married;
                lifeIns.DateOfMarriage = DateOfMarriage;
                lifeIns.HaveChildrenOrHaveDependents = OtherDependents;
                lifeIns.PlacementDateOfAdoptedChild = DateOfAdoption;
                //lifeIns.AddDependent = AddDep;
                //lifeIns.DropDependent = DropDep;
                lifeIns.DropEmployee = DropEmployee;
                lifeIns.DropDependents = DropDependents;
                lifeIns.LastDayOfCoverage = LastDayOfCoverage;
                lifeIns.TerminationEmploymentOfDropCoverage = TerminationEmploymentOfDropCoverage;
                lifeIns.Retirement = Retirement;
                lifeIns.LastDayWorked = LastDayWorked;
                lifeIns.OtherEvent = OtherEvent;
                lifeIns.OtherEventReason = OtherEventReason;
                lifeIns.OtherEventDate = OtherEventDate;
                lifeIns.DropBasicLife = DropBasicLife;
                lifeIns.EmployeeDentalDrop = EmployeeDentalDrop;
                lifeIns.SpouseDentalDrop = SpouseDentalDrop;
                lifeIns.DependentDentalDrop = DependentDentalDrop;
                lifeIns.EmployeeVisionDrop = EmployeeVisionDrop;
                lifeIns.SpouseVisionDrop = SpouseVisionDrop;
                lifeIns.DependentVisionDrop = DependentVisionDrop;
                lifeIns.TerminationEmploymentLossOfOtherCoverage = TerminationEmploymentLossOfOtherCoverage;
                lifeIns.TerminationEmploymentDateLossOfOtherCoverage = TerminationEmploymentDateLossOfOtherCoverage;
                lifeIns.Divorce = Divorce;
                lifeIns.DivorceDate = DivorceDate;
                lifeIns.DeathOfSpouse = DeathOfSpouse;
                lifeIns.DeathOfSpouseDate = DeathOfSpouseDate;
                lifeIns.TerminationOrExpirationOfCoverage = TerminationOrExpirationOfCoverage;
                lifeIns.TerminationOrExpirationOfCoverageDate = TerminationOrExpirationOfCoverageDate;
                lifeIns.DentalCoverageLost = DentalCoverageLost;
                lifeIns.VisionCoverageLost = VisionCoverageLost;
                lifeIns.CoveredUnderOtherInsurance = CoveredUnderOtherInsurance;
                lifeIns.Other = Other;
                lifeIns.OtherReason = OtherReason;

                lifeIns.DentalCoverage = DentalPlan;
                lifeIns.VisionCoverage = VisionPlan;

                lifeIns.DoNotWantDentalCoverage = DoNotWantDentalCoverage;
                lifeIns.EmployeeCoveredUnderOtherDentalPlan = EmployeeCoveredUnderOtherDental;
                lifeIns.SpouseCoveredUnderOtherDentalPlan = SpouseCoveredUnderOtherDental;
                lifeIns.DependentsCoveredUnderOtherDentalPlan = DependentsCoveredUnderOtherDental;

                lifeIns.DoNotWantVisionCoverage = DoNotWantVisionCoverage;
                lifeIns.EmployeeCoveredUnderOtherVisionPlan = EmployeeCoveredUnderOtherVision;
                lifeIns.SpouseCoveredUnderOtherVisionPlan = SpouseCoveredUnderOtherVision;
                lifeIns.DependentsCoveredUnderOtherVisionPlan = DependentsCoveredUnderOtherVision;
                lifeIns.OwnerBasicLifeWithADandDPolicyAmount = OwnerBasicLifeADandDPolicyAmount;
                lifeIns.ManagerBasicLifeWithADandDPolicyAmount = ManagerBasicLifeADandDPolicyAmount;
                lifeIns.EmployeeBasicLifeWithADandDPolicyAmount = EmployeeBasicLifeADandDPolicyAmount;
                lifeIns.DoNotWantBasicLifeCoverageWithADandD = DoNotWantBasicLifeCoverageADandD;
                lifeIns.AmountOfPreviousPolicy = PreviousPolicyAmount;
                lifeIns.EmployeeSignature = EmployeeSignature;
                lifeIns.EmployeeSignatureDate = EmployeeSignatureDate;

                db.Life_Insurance.Add(lifeIns);

                db.SaveChanges();
            }

            int result = Employee_id;


            return Json(new { data = result, error = response }, JsonRequestBehavior.AllowGet);
        }

        //----------------------------------------------------------------------------------------

        //Edit-LifeIns
        public ActionResult EditLifeInsurance(int? LifeInsurance_id, int? Employee_id, int? Beneficiary_id)
        {
            ViewBag.LifeInsurance_id = LifeInsurance_id;
            ViewBag.Employee_id = Employee_id;
            ViewBag.Beneficiary_id = Beneficiary_id;

            EmployeeAndInsuranceVM empAndInsVM = new EmployeeAndInsuranceVM();

            empAndInsVM.employee = db.Employees.FirstOrDefault(i => i.Employee_id == Employee_id);
            empAndInsVM.lifeIns = db.Life_Insurance.FirstOrDefault(i => i.Employee_id == Employee_id);
            empAndInsVM.benefiList = db.Beneficiaries.Where(i => i.Employee_id == Employee_id).ToList();

            empAndInsVM.spouse = db.Family_Info.FirstOrDefault(i => i.Employee_id == Employee_id && i.RelationshipToInsured == "Spouse");
            empAndInsVM.family = db.Family_Info.Where(i => i.Employee_id == Employee_id && i.RelationshipToInsured != "Spouse").ToList();
            if (empAndInsVM.spouse != null)
            {
                empAndInsVM.spouse = db.Family_Info.FirstOrDefault(i => i.Employee_id == Employee_id && i.FamilyMember_id == empAndInsVM.spouse.FamilyMember_id);
                empAndInsVM.family = db.Family_Info.Where(i => i.Employee_id == Employee_id && i.FamilyMember_id != empAndInsVM.spouse.FamilyMember_id).ToList();
            }
            else
            {
                empAndInsVM.spouse = null;
                empAndInsVM.family = db.Family_Info.Where(i => i.Employee_id == Employee_id).ToList();
            }

            return View(empAndInsVM);
        }

        //====================================
        // Post: EditLifeInsUpdate
        //====================================
        public JsonResult EditLifeInsUpdate(int? LifeInsurance_id, int? Employee_id, int? InsurancePlan_id, string GroupPlanNumber, DateTime? BenefitsEffectiveDate,
            string InitialEnrollment, string ReEnrollment, string AddEmployeeAndDependents, string DropRefuseCoverage, string InformationChange, string IncreaseAmount,
            string FamilyStatusChange, string SubTotalCode, string Married, DateTime? DateOfMarriage, string OtherDependents, DateTime? DateOfAdoption, /*string AddDep,*/
                                                                                                                                                        /*string DropDep,*/ string AddDropDep, string DropEmployee, string DropDependents, DateTime? LastDayOfCoverage, string TerminationEmploymentOfDropCoverage, string Retirement,
            DateTime? LastDayWorked, string OtherEvent, string OtherEventReason, DateTime? OtherEventDate, string DropBasicLife, string EmployeeDentalDrop,
            string SpouseDentalDrop, string DependentDentalDrop, string EmployeeVisionDrop, string SpouseVisionDrop, string DependentVisionDrop,
            string TerminationEmploymentLossOfOtherCoverage, DateTime? TerminationEmploymentDateLossOfOtherCoverage, string Divorce, DateTime? DivorceDate,
            string DeathOfSpouse, DateTime? DeathOfSpouseDate, string TerminationOrExpirationOfCoverage, DateTime TerminationOrExpirationOfCoverageDate,
            string DentalCoverageLost, string VisionCoverageLost, string CoveredUnderOtherInsurance, string Other, string OtherReason, string DentalCoverage,
            string DoNotWantDentalCoverage, string EmployeeCoveredUnderOtherDental, string SpouseCoveredUnderOtherDental, string DependentsCoveredUnderOtherDental,
            string VisionCoverage, string DoNotWantVisionCoverage, string EmployeeCoveredUnderOtherVision, string SpouseCoveredUnderOtherVision,
            string DependentsCoveredUnderOtherVision, string PrimaryBeneficiary, string ContingentBeneficiary, string OwnerBasicLifeADandDPolicyAmount,
            string ManagerBasicLifeADandDPolicyAmount, string EmployeeBasicLifeADandDPolicyAmount, string DoNotWantBasicLifeCoverageADandD, string PreviousPolicyAmount,
            string DentalPlan, string VisionPlan, string EmployeeSignature, DateTime? EmployeeSignatureDate)
        {
            Life_Insurance lifeIns = db.Life_Insurance
                .Where(i => i.Employee_id == Employee_id)
                .SingleOrDefault();

            ViewBag.LifeInsurance_id = lifeIns.LifeInsurance_id;

            lifeIns.GroupPlanNumber = GroupPlanNumber;
            lifeIns.BenefitsEffectiveDate = BenefitsEffectiveDate;
            lifeIns.InitialEnrollment = InitialEnrollment;
            lifeIns.ReEnrollment = ReEnrollment;
            lifeIns.AddEmployeeAndDependents = AddEmployeeAndDependents;
            lifeIns.DropRefuseCoverage = DropRefuseCoverage;
            lifeIns.InformationChange = InformationChange;
            lifeIns.IncreaseAmount = IncreaseAmount;
            lifeIns.FamilyStatusChange = FamilyStatusChange;
            lifeIns.SubTotalCode = SubTotalCode;
            lifeIns.MarriedOrHaveSpouse = Married;
            lifeIns.DateOfMarriage = DateOfMarriage;
            lifeIns.HaveChildrenOrHaveDependents = OtherDependents;
            lifeIns.PlacementDateOfAdoptedChild = DateOfAdoption;
            //lifeIns.AddDependent = AddDep;
            //lifeIns.DropDependent = DropDep;
            lifeIns.DropEmployee = DropEmployee;
            lifeIns.DropDependents = DropDependents;
            lifeIns.LastDayOfCoverage = LastDayOfCoverage;
            lifeIns.TerminationEmploymentOfDropCoverage = TerminationEmploymentOfDropCoverage;
            lifeIns.Retirement = Retirement;
            lifeIns.LastDayWorked = LastDayWorked;
            lifeIns.OtherEvent = OtherEvent;
            lifeIns.OtherEventReason = OtherEventReason;
            lifeIns.OtherEventDate = OtherEventDate;
            lifeIns.DropBasicLife = DropBasicLife;
            lifeIns.EmployeeDentalDrop = EmployeeDentalDrop;
            lifeIns.SpouseDentalDrop = SpouseDentalDrop;
            lifeIns.DependentDentalDrop = DependentDentalDrop;
            lifeIns.EmployeeVisionDrop = EmployeeVisionDrop;
            lifeIns.SpouseVisionDrop = SpouseVisionDrop;
            lifeIns.DependentVisionDrop = DependentVisionDrop;
            lifeIns.TerminationEmploymentLossOfOtherCoverage = TerminationEmploymentLossOfOtherCoverage;
            lifeIns.TerminationEmploymentDateLossOfOtherCoverage = TerminationEmploymentDateLossOfOtherCoverage;
            lifeIns.Divorce = Divorce;
            lifeIns.DivorceDate = DivorceDate;
            lifeIns.DeathOfSpouse = DeathOfSpouse;
            lifeIns.DeathOfSpouseDate = DeathOfSpouseDate;
            lifeIns.TerminationOrExpirationOfCoverage = TerminationOrExpirationOfCoverage;
            lifeIns.TerminationOrExpirationOfCoverageDate = TerminationOrExpirationOfCoverageDate;//-- NonNullable error Parameter for DateTime
            lifeIns.DentalCoverageLost = DentalCoverageLost;
            lifeIns.VisionCoverageLost = VisionCoverageLost;
            lifeIns.CoveredUnderOtherInsurance = CoveredUnderOtherInsurance;
            lifeIns.Other = Other;
            lifeIns.OtherReason = OtherReason;

            lifeIns.DentalCoverage = DentalPlan;
            lifeIns.VisionCoverage = VisionPlan;

            lifeIns.DoNotWantDentalCoverage = DoNotWantDentalCoverage;
            lifeIns.EmployeeCoveredUnderOtherDentalPlan = EmployeeCoveredUnderOtherDental;
            lifeIns.SpouseCoveredUnderOtherDentalPlan = SpouseCoveredUnderOtherDental;
            lifeIns.DependentsCoveredUnderOtherDentalPlan = DependentsCoveredUnderOtherDental;

            lifeIns.DoNotWantVisionCoverage = DoNotWantVisionCoverage;
            lifeIns.EmployeeCoveredUnderOtherVisionPlan = EmployeeCoveredUnderOtherVision;
            lifeIns.SpouseCoveredUnderOtherVisionPlan = SpouseCoveredUnderOtherVision;
            lifeIns.DependentsCoveredUnderOtherVisionPlan = DependentsCoveredUnderOtherVision;
            lifeIns.OwnerBasicLifeWithADandDPolicyAmount = OwnerBasicLifeADandDPolicyAmount;
            lifeIns.ManagerBasicLifeWithADandDPolicyAmount = ManagerBasicLifeADandDPolicyAmount;
            lifeIns.EmployeeBasicLifeWithADandDPolicyAmount = EmployeeBasicLifeADandDPolicyAmount;
            lifeIns.DoNotWantBasicLifeCoverageWithADandD = DoNotWantBasicLifeCoverageADandD;
            lifeIns.AmountOfPreviousPolicy = PreviousPolicyAmount;
            lifeIns.EmployeeSignature = EmployeeSignature;
            lifeIns.EmployeeSignatureDate = EmployeeSignatureDate;

            //Family_Info depAddDrop = db.Family_Info
            //    .Where(i => i.Employee_id == Employee_id)
            //    .SingleOrDefault();

            //depAddDrop.AddDropDepLifeIns = AddDropDep;

            //InsurancePlan insPlan = db.InsurancePlans
            //    .Where(i => i.Employee_id == Employee_id)
            //    .SingleOrDefault();

            //ViewBag.InsurancePlan_id = insPlan.InsurancePlan_id;

            //insPlan.DentalPlan = DentalPlan;
            //insPlan.VisionPlan = VisionPlan;

            if (ModelState.IsValid)
            {
                db.Entry(lifeIns).State = System.Data.Entity.EntityState.Modified;

                try
                {
                    db.SaveChanges();
                }
                catch (Exception err)
                {
                    Console.WriteLine(err);
                }
            }

            int result = lifeIns.LifeInsurance_id;

            return Json(new { data = result }, JsonRequestBehavior.AllowGet);
        }

        //----------------------------------------------------------------------------------------

        public ActionResult AddBeneficiary(int? Employee_id, int? Beneficiary_id)
        {
            ViewBag.Employee_id = Employee_id;
            ViewBag.Beneficiary_id = Beneficiary_id;

            EmployeeAndInsuranceVM empAndInsVM = new EmployeeAndInsuranceVM();

            //empAndInsVM.benefiList = db.Beneficiaries.Where(i => i.Beneficiary_id == Beneficiary_id).ToList();
            empAndInsVM.beneficiary = db.Beneficiaries.FirstOrDefault(i => i.Employee_id == Employee_id);

            return View(empAndInsVM);
        }

        //====================================
        // Post: AddBeneficiaryUpdate
        //====================================
        public JsonResult AddBeneficiaryUpdate(int Employee_id, string PrimaryBeneficiary, string ContingentBeneficiary, string FirstName, string LastName, string SSN,
            string RelationshipToEmployee, DateTime DateOfBirth, string PhoneNumber, string PercentageOfBenefits, string MailingAddress, string City, string State,
            string ZipCode)
        {

            Beneficiary benefi = new Beneficiary();

            benefi.Employee_id = Employee_id;
            benefi.PrimaryBeneficiary = PrimaryBeneficiary;
            benefi.ContingentBeneficiary = ContingentBeneficiary;
            benefi.FirstName = FirstName;
            benefi.LastName = LastName;
            benefi.SSN = SSN;
            benefi.RelationshipToEmployee = RelationshipToEmployee;
            benefi.DOB = DateOfBirth;
            benefi.PhoneNumber = PhoneNumber;
            benefi.PercentageOfBenefits = PercentageOfBenefits;
            benefi.Address = MailingAddress;
            benefi.CIty = City;
            benefi.State = State;
            benefi.ZipCode = ZipCode;

            db.Beneficiaries.Add(benefi);
            db.SaveChanges();

            int result = Employee_id;

            return Json(new { data = result }, JsonRequestBehavior.AllowGet);
        }


        //----------------------------------------------------------------------------------------
        public ActionResult EditBeneficiary(int? Employee_id, int? Beneficiary_id)
        {
            ViewBag.Employee_id = Employee_id;
            ViewBag.Beneficiary_id = Beneficiary_id;

            EmployeeAndInsuranceVM empAndInsVM = new EmployeeAndInsuranceVM();

            empAndInsVM.beneficiary = db.Beneficiaries.FirstOrDefault(i => i.Beneficiary_id == Beneficiary_id);
            //empAndInsVM.benefiList = db.Beneficiaries.Where(i => i.Employee_id == Employee_id).ToList();

            return View(empAndInsVM);
        }

        //====================================
        // Post: EditBeneficaryUpdate
        //====================================
        public JsonResult EditBeneficaryUpdate(int? Employee_id, int? Beneficiary_id, string PrimaryBeneficiary, string ContingentBeneficiary, string FirstName,
            string LastName, string SSN, string RelationshipToEmployee, string MailingAddress, string City, string State, string ZipCode, DateTime? DOB,
            string PhoneNumber, string PercentageOfBenefits)
        {
            Beneficiary benefi = db.Beneficiaries
                     .Where(i => i.Employee_id == Employee_id)
                     .Where(i => i.Beneficiary_id == Beneficiary_id)
                     .SingleOrDefault();

            benefi.PrimaryBeneficiary = PrimaryBeneficiary;
            benefi.ContingentBeneficiary = ContingentBeneficiary;
            benefi.FirstName = FirstName;
            benefi.LastName = LastName;
            benefi.SSN = SSN;
            benefi.RelationshipToEmployee = RelationshipToEmployee;
            benefi.Address = MailingAddress;
            benefi.CIty = City;
            benefi.State = State;
            benefi.ZipCode = ZipCode;
            benefi.DOB = DOB;
            benefi.PhoneNumber = PhoneNumber;
            benefi.PercentageOfBenefits = PercentageOfBenefits;

            if (ModelState.IsValid)
            {
                db.Entry(benefi).State = System.Data.Entity.EntityState.Modified;

                try
                {
                    db.SaveChanges();
                }
                catch (Exception err)
                {
                    Console.WriteLine(err);
                }
            }

            int result = benefi.Beneficiary_id;

            return Json(new { data = result }, JsonRequestBehavior.AllowGet);
        }

        //Delete-Beneficiaries
        //----------------------------------------------------------------------------------------

        public ActionResult DeleteBeneficiary(int? Beneficiary_id, int? Employee_id)
        {
            ViewBag.Employee_id = Employee_id;
            ViewBag.Beneficiary_id = Beneficiary_id;

            Beneficiary beneficiary = new Beneficiary();

            beneficiary = db.Beneficiaries.FirstOrDefault(i => i.Beneficiary_id == Beneficiary_id);

            if (beneficiary == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Beneficiary beni = db.Beneficiaries.Find(Beneficiary_id);
            if (beni == null)
            {
                return HttpNotFound();
            }

            return View(beneficiary);
        }

        //====================================
        // Post: BeneficiaryConfirmDelete
        //====================================
        [System.Web.Mvc.HttpPost, System.Web.Mvc.ActionName("DeleteBeneficiary")]
        [ValidateAntiForgeryToken]
        public ActionResult BeneficiaryConfirmDelete(int Beneficiary_id)
        {
            Beneficiary beneficiary = db.Beneficiaries.Find(Beneficiary_id);
            if (beneficiary == null)
            {
                return HttpNotFound();
            }
            Beneficiary beni = db.Beneficiaries.Find(Beneficiary_id);

            db.Beneficiaries.Remove(db.Beneficiaries.FirstOrDefault(b => b.Beneficiary_id == Beneficiary_id));
            db.SaveChanges();

            //db.DeleteEmployeeAndDependents(Beneficiary_id);
            //db.Beneficiaries.Remove(beni);
            //db.SaveChanges();

            return RedirectToAction("LifeInsuranceEnrollment", new { beneficiary.Employee_id });
        }

        //----------------------------------------------------------------------------------------


        public ActionResult DeleteLifeInsurance(int? LifeInsurance_id, int? Employee_id)
        {
            ViewBag.Employee_id = Employee_id;
            ViewBag.LifeInsurance_id = LifeInsurance_id;

            if (LifeInsurance_id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Life_Insurance lifeIns = db.Life_Insurance.Find(LifeInsurance_id);
            if (lifeIns == null)
            {
                return HttpNotFound();
            }

            return View(lifeIns);
        }

        //====================================
        // Post: ConfirmDeleteLifeIns
        //====================================
        [System.Web.Mvc.HttpPost, System.Web.Mvc.ActionName("DeleteLifeInsurance")]
        [ValidateAntiForgeryToken]
        public ActionResult ConfirmDeleteLifeIns(int LifeInsurance_id)
        {
            Life_Insurance lifeIns = db.Life_Insurance.Find(LifeInsurance_id);

            //db.DeleteEmployeeAndDependents(LifeInsurance_id);

            db.Life_Insurance.Remove(lifeIns);
            //db.SaveChanges();

            return RedirectToAction("LifeInsuranceEnrollment");
        }

        //----------------------------------------------------------------------------------------

        public ActionResult Career()
        {
            return View();
        }

        public ActionResult JobList(int? Job_id)
        {
            return View();
        }

        public ActionResult JobDescription(int? Job_id)
        {
            return View();
        }

        public ActionResult CreateJobPosition()
        {
            return View();
        }


        // GET: CareerCenter/JobApplicantDetails/id
        public ActionResult JobApplicantDetails(int? JobApplicant_id)
        {
            return View();
        }

        // GET: Create
        public ActionResult CreateJobApplicant()
        {
            return View();
        }

        // POST: Create
        [System.Web.Mvc.HttpPost]
        public JsonResult JobApplicationNew(int JobApplicant_id, int Employee_id, string PositionApplyingFor, DateTime StartDateAvailability,
            string ApplicantSignature, string ApplicantSignatureDate, string FirstName, string LastName, string Address, string PObox,
            string City, string State, string ZipCode, string YearsResidedAtPresentAddress, string PreviousAddress, string PreviousPObox,
            string PreviousCity, string PreviousState, string PreviousZipCode, string YearsResidedAtPreviousAddr, string PhoneNumber, string OverEighteen,
            string PreviouslyEmployedAtWatson, DateTime EmploymentDatesAtWatsonFrom, DateTime EmploymentDatesAtWatsonTo, string DriversLicense,
            string LicenseState, string LicenseNumber, DateTime LicenseExpirationDate, string ElementaryInstitution, string YearsCompletedElementary,
            string FieldOfStudyElementary, string GraduateElementary, string HighSchoolInstitution, string YearsCompletedHighSchool,
            string FieldOfStudyHighSchool, string GraduateHighSchool, string CollegeInstitution, string YearsCompletedCollege,
            string FieldOfStudyCollege, string GraduateOrDegreeCollege, string TechnicalInstitution, string YearsCompletedTechnical,
            string FieldOfStudyTechnical, string GraduateOrDegreeTechnical, string AdditionalInstitution, string YearsCompletedAtAdditional,
            string FieldOfStudyAtAdditional, string GraduateOrDegreeAtAdditional, string Veteran, string MOSorSpecializedTraining,
            string PresentOrLastEmployer, string EmployerPhoneNumber, string EmployerAddress, string EmployerCity, string EmployerState,
            string EmployerZipCode, DateTime EmployedFromDate, DateTime EmployedToDate, string TitleOfPositionHeld, string TypeOfWorkedPerformed,
            string NameOfSupervisor, string PresentOrLastEmployerTwo, string EmployerPhoneNumberTwo, string EmployerAddressTwo,
            string EmployerCityTwo, string EmployerStateTwo, string EmployerZipCodeTwo, DateTime EmployedFromDateTwo, DateTime EmployedToDateTwo,
            string TitleOfPositionHeldTwo, string TypeOfWorkedPerformedTwo, string NameOfSupervisorTwo, string PresentOrLastEmployerThree,
            string EmployerPhoneNumberThree, string EmployerAddressThree, string EmployerCityThree, string EmployerStateThree,
            string EmployerZipCodeThree, DateTime EmployedFromDateThree, DateTime EmployedToDateThree, string TitleOfPositionHeldThree,
            string TypeOfWorkedPerformedThree, string NameOfSupervisorThree, string TerminatedOrResigned, string ReasonForTerminationOrResignation,
            string ReasonForEmploymentGaps, string ReferenceFirstName, string ReferenceLastName, string ReferencePhoneNumber,
            string ReferenceAddress, string ReferenceCity, string ReferenceState, string ReferenceZipCode, string ReferenceOccupation,
            string YearsKnownReference, string ReferenceFirstNameTwo, string ReferenceLastNameTwo, string ReferenceAddressTwo,
            string ReferenceCityTwo, string ReferenceStateTwo, string ReferenceZipCodeTwo, string ReferenceOccupationTwo, string YearsKnownReferenceTwo,
            string ReferencePhoneNumberTwo, string ReferenceFirstNameThree, string ReferenceLastNameThree, string ReferenceAddressThree,
            string ReferenceCityThree, string ReferenceStateThree, string ReferenceZipCodeThree, string ReferenceOccupationThree,
            string YearsKnownReferenceThree, string ReferencePhoneNumberThree, string Experience, string ListOtherExperience, string Certification,
            string ApplicantSignatureTwo, DateTime ApplicantSignatureDateTwo)
        {
            JobApplicant a = new JobApplicant();

            a.JobApplicant_id = JobApplicant_id;
            a.Employee_id = Employee_id;
            a.PositionApplyingFor = PositionApplyingFor;
            a.StartDateAvailability = StartDateAvailability;
            a.ApplicantSignature = ApplicantSignature;
            a.FirstName = FirstName;
            a.LastName = LastName;
            a.Address = Address;
            a.City = City;
            a.State = State;
            a.ZipCode = ZipCode;
            a.YearsResidedAtPresentAddr = YearsResidedAtPresentAddress;
            a.PreviousAddress = PreviousAddress;
            a.PreviousCity = PreviousCity;
            a.PreviousState = PreviousState;
            a.PreviousZipCode = PreviousZipCode;
            a.YearsResidedAtPreviousAddr = YearsResidedAtPreviousAddr;
            a.PhoneNumber = PhoneNumber;
            //a.Age = Age;
            a.OverEighteen = OverEighteen;
            a.PreviouslyEmployedAtWatson = PreviouslyEmployedAtWatson;
            a.EmploymentDatesAtWatsonFrom = EmploymentDatesAtWatsonFrom;
            a.EmploymentDatesAtWatsonTo = EmploymentDatesAtWatsonTo;
            a.DriversLicense = DriversLicense;
            a.LicenseState = LicenseState;
            a.LicenseNumber = LicenseNumber;
            a.LicenseExpirationDate = LicenseExpirationDate;
            a.ElementaryInstitution = ElementaryInstitution;
            a.YearsCompletedElementary = YearsCompletedElementary;
            a.FieldOfStudyElementary = FieldOfStudyElementary;
            a.GraduateElementary = GraduateElementary;
            a.HighSchoolInstitution = HighSchoolInstitution;
            a.YearsCompletedHighSchool = YearsCompletedHighSchool;
            a.FieldOfStudyHighSchool = FieldOfStudyHighSchool;
            a.GraduateHighSchool = GraduateHighSchool;
            a.CollegeInstitution = CollegeInstitution;
            a.YearsCompletedCollege = YearsCompletedCollege;
            a.FieldOfStudyCollege = FieldOfStudyCollege;
            a.GraduateOrDegreeCollege = GraduateOrDegreeCollege;
            a.TechnicalInstitution = TechnicalInstitution;
            a.YearsCompletedTechnical = YearsCompletedTechnical;
            a.FieldOfStudyTechnical = FieldOfStudyTechnical;
            a.GraduateOrDegreeTechnical = GraduateOrDegreeTechnical;
            a.AdditionalInstitution = AdditionalInstitution;
            a.YearsCompletedAtAdditional = YearsCompletedAtAdditional;
            a.FieldOfStudyAtAdditional = FieldOfStudyAtAdditional;
            a.GraduateOrDegreeAtAdditional = GraduateOrDegreeAtAdditional;
            a.Veteran = Veteran;
            a.MOSorSpecializedTraining = MOSorSpecializedTraining;
            a.PresentOrLastEmployer = PresentOrLastEmployer;
            a.EmployerPhoneNumber = EmployerPhoneNumber;
            a.EmployerAddress = EmployerAddress;
            a.EmployerCity = EmployerCity;
            a.EmployerState = EmployerState;
            a.EmployerZipCode = EmployerZipCode;
            a.EmployedFromDate = EmployedFromDate;
            a.EmployedToDate = EmployedToDate;
            a.TitleOfPositionHeld = TitleOfPositionHeld;
            a.TypeOfWorkedPerformed = TypeOfWorkedPerformed;
            a.NameOfSupervisor = NameOfSupervisor;
            a.PresentOrLastEmployerTwo = PresentOrLastEmployerTwo;
            a.EmployerPhoneNumberTwo = EmployerPhoneNumberTwo;
            a.EmployerAddressTwo = EmployerAddressTwo;
            a.EmployerCityTwo = EmployerCityTwo;
            a.EmployerStateTwo = EmployerStateTwo;
            a.EmployerZipCodeTwo = EmployerZipCodeTwo;
            a.EmployedFromDateTwo = EmployedFromDateTwo;
            a.EmployedToDateTwo = EmployedToDateTwo;
            a.TitleOfPositionHeldTwo = TitleOfPositionHeldTwo;
            a.TypeOfWorkedPerformedTwo = TypeOfWorkedPerformedTwo;
            a.NameOfSupervisorTwo = NameOfSupervisorTwo;
            a.PresentOrLastEmployerThree = PresentOrLastEmployerThree;
            a.EmployerPhoneNumberThree = EmployerPhoneNumberThree;
            a.EmployerAddressThree = EmployerAddressThree;
            a.EmployerCityThree = EmployerCityThree;
            a.EmployerStateThree = EmployerStateThree;
            a.EmployerZipCodeThree = EmployerZipCodeThree;
            a.EmployedFromDateThree = EmployedFromDateThree;
            a.EmployedToDateThree = EmployedToDateThree;
            a.TitleOfPositionHeldThree = TitleOfPositionHeldThree;
            a.TypeOfWorkedPerformedThree = TypeOfWorkedPerformedThree;
            a.NameOfSupervisorThree = NameOfSupervisorThree;
            a.TerminatedOrResigned = TerminatedOrResigned;
            a.ReasonForTerminationOrResignation = ReasonForTerminationOrResignation;
            a.ReasonForEmploymentGaps = ReasonForEmploymentGaps;
            a.ReferenceFirstName = ReferenceFirstName;
            a.ReferenceLastName = ReferenceLastName;
            a.ReferencePhoneNumber = ReferencePhoneNumber;
            a.ReferenceAddress = ReferenceAddress;
            a.ReferenceCity = ReferenceCity;
            a.ReferenceState = ReferenceState;
            a.ReferenceZipCode = ReferenceZipCode;
            a.ReferenceOccupation = ReferenceOccupation;
            a.YearsKnownReference = YearsKnownReference;
            a.ReferenceFirstNameTwo = ReferenceFirstNameTwo;
            a.ReferenceLastNameTwo = ReferenceLastNameTwo;
            a.ReferenceAddressTwo = ReferenceAddressTwo;
            a.ReferenceCityTwo = ReferenceCityTwo;
            a.ReferenceStateTwo = ReferenceStateTwo;
            a.ReferenceZipCodeTwo = ReferenceZipCodeTwo;
            a.ReferenceOccupationTwo = ReferenceOccupationTwo;
            a.YearsKnownReferenceTwo = YearsKnownReferenceTwo;
            a.ReferencePhoneNumberTwo = ReferencePhoneNumberTwo;
            a.ReferenceFirstNameThree = ReferenceFirstNameThree;
            a.ReferenceLastNameThree = ReferenceLastNameThree;
            a.ReferenceAddressThree = ReferenceAddressThree;
            a.ReferenceCityThree = ReferenceCityThree;
            a.ReferenceStateThree = ReferenceStateThree;
            a.ReferenceZipCodeThree = ReferenceZipCodeThree;
            a.ReferenceOccupationThree = ReferenceOccupationThree;
            a.YearsKnownReferenceThree = YearsKnownReferenceThree;
            a.ReferencePhoneNumberThree = ReferencePhoneNumberThree;
            a.Experience = Experience;
            a.ListOtherExperience = ListOtherExperience;
            a.Certification = Certification;
            a.ApplicantSignatureTwo = ApplicantSignatureTwo;
            a.ApplicantSignatureDateTwo = ApplicantSignatureDateTwo;

            db.JobApplicants.Add(a);
            db.SaveChanges();

            int result = a.JobApplicant_id;

            return Json(new { data = result }, JsonRequestBehavior.AllowGet);
        }

        // GET: CareerCenter/EditJobApplicant/id
        public ActionResult EditJobApplicant(int? id)
        {
            return View();
        }

        // POST: CareerCenter/EditJobApplicant/id
        [System.Web.Mvc.HttpPost]
        public ActionResult EditJobApplicantUpdate(FormCollection collection)
        {
            return View();
        }

        public ActionResult DeleteJobApplicant(int? JobApplicant_id)
        {
            ViewBag.JobApplicant_id = JobApplicant_id;

            if (JobApplicant_id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            JobApplicant jobApplicant = db.JobApplicants.Find(JobApplicant_id);
            if (jobApplicant == null)
            {
                return HttpNotFound();
            }

            return View(jobApplicant);
        }

        //====================================
        // Post: ConfirmDeleteJobApplicant
        //====================================
        [System.Web.Mvc.HttpPost, System.Web.Mvc.ActionName("DeleteJobApplicant")]
        [ValidateAntiForgeryToken]
        public ActionResult ConfirmDeleteJobApplicant(int JobApplicant_id)
        {
            JobApplicant jobApplicant = db.JobApplicants.Find(JobApplicant_id);

            //db.DeleteEmployeeAndDependents(LifeInsurance_id);

            db.JobApplicants.Remove(jobApplicant);
            //db.SaveChanges();

            return RedirectToAction("JobList", "Admin");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}