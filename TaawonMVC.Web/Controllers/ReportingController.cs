using Microsoft.Reporting.WebForms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using TaawonMVC.Donors;
using TaawonMVC.Web.Models.Reporting;
using TaawonMVC.Web.Reporting.Dataset.ProjectTableAdapters;
using static TaawonMVC.Web.Reporting.Dataset.Project;
using System.Data.SqlClient;
using System.Data;
using TaawonMVC.Models;
using TaawonMVC.InterventionType;
using TaawonMVC.InterventionType.DTO;
using TaawonMVC.Neighborhood.DTO;
using TaawonMVC.Neighborhood;
using TaawonMVC.Donors.DTO;
using TaawonMVC.BuildingUses.DTO;
using TaawonMVC.BuildingUses;
using TaawonMVC.UploadProjectFiles;
using TaawonMVC.UploadProjectFiles.DTO;

namespace TaawonMVC.Web.Controllers
{

    public class ProjectAdp : ViewProjectTableAdapter
    {
        public  ProjectAdp(String Sql)
        {

            this.Adapter.SelectCommand = new SqlCommand(Sql, this.Connection);
        }
        public String   QueryStr(String Wheresql , String OrderBy)
        {
            this.Adapter.SelectCommand.CommandText = this.Adapter.SelectCommand.CommandText  + " Where " + Wheresql + " Order by " + OrderBy;

            return this.Adapter.SelectCommand.CommandText;
        }

       

        public int cmdTimeout
        {
            get
            {
                return this.CommandCollection[0].CommandTimeout;
            }
            set
            {
                int i = 0;
                while ((i < this.CommandCollection.Length))
                {
                    if (!(this.CommandCollection[i] == null))
                    {
                        ((System.Data.SqlClient.SqlCommand)(this.CommandCollection[i])).CommandTimeout = value;
                    }

                    i = (i + 1);
                }

            }
        }
    }

    public class ApplicationAdp : ApplicationViewTableAdapter
    {
        public ApplicationAdp(String Sql)
        {

            this.Adapter.SelectCommand = new SqlCommand(Sql, this.Connection);
        }
        public String QueryStr(String Wheresql, String OrderBy)
        {
            this.Adapter.SelectCommand.CommandText = this.Adapter.SelectCommand.CommandText + " Where " + Wheresql + " Order by " + OrderBy;

            return this.Adapter.SelectCommand.CommandText;
        }



        public int cmdTimeout
        {
            get
            {
                return this.CommandCollection[0].CommandTimeout;
            }
            set
            {
                int i = 0;
                while ((i < this.CommandCollection.Length))
                {
                    if (!(this.CommandCollection[i] == null))
                    {
                        ((System.Data.SqlClient.SqlCommand)(this.CommandCollection[i])).CommandTimeout = value;
                    }

                    i = (i + 1);
                }

            }
        }
    }



    public class ReportingController : Controller
    {
        private readonly IDonorsAppService _donorsAppService;
        private readonly InterventionTypeAppService _InterventionTypeAppService;
        private readonly INeighborhoodAppService _neighborhoodAppService;
        private readonly IBuildingUsesAppService _buildingUsesAppService;
        private readonly IUploadProjectFilesAppService _uploadProjectFilesAppService;
        public ReportingController(IDonorsAppService donorsAppService , 
                  InterventionTypeAppService interventionTypeAppService,
                  INeighborhoodAppService neighborhoodAppService,
                  IBuildingUsesAppService buildingUsesAppService,
                  IUploadProjectFilesAppService uploadProjectFilesAppService)
        {
            _donorsAppService = donorsAppService;
            _InterventionTypeAppService = interventionTypeAppService;
            _neighborhoodAppService = neighborhoodAppService;
            _buildingUsesAppService = buildingUsesAppService;
            _uploadProjectFilesAppService = uploadProjectFilesAppService;
        }
        // GET: Reporting

        public ActionResult Index()
        {
           


            return View();
        }

        public ActionResult ApplicationRptView()
        {
            List<SelectListItem> OrderBy = new List<SelectListItem>();
            OrderBy.Add(new SelectListItem() { Text = "Full Name", Value = "fullName" });
            OrderBy.Add(new SelectListItem() { Text = "Reject Date", Value = "RejectDate" });
            OrderBy.Add(new SelectListItem() { Text = "Approved Date", Value = "ApprovedDate" });

            List<SelectListItem> ApplicationStatus = new List<SelectListItem>();
            ApplicationStatus.Add(new SelectListItem() { Text = "Rejected", Value = "Rejected" });
            ApplicationStatus.Add(new SelectListItem() { Text = "Approved", Value = "Approved" });

            var reportingViewModel = new ReportingViewModel()
            {
                OrderBy = OrderBy,
                ApplicationStatus = ApplicationStatus
            };

            return View("ApplicationRptView", reportingViewModel);
        }

        public ActionResult ApplicationRpt()
        {
            List<SelectListItem> orderBy = new List<SelectListItem>();
            orderBy.Add(new SelectListItem() { Text = "Full Name", Value = "fullName" });
            orderBy.Add(new SelectListItem() { Text = "Reject Date", Value = "RejectDate" });
            orderBy.Add(new SelectListItem() { Text = "Approved Date", Value = "ApprovedDate" });

            List<SelectListItem> ApplicationStatus = new List<SelectListItem>();
            ApplicationStatus.Add(new SelectListItem() { Text = "Rejected", Value = "Rejected" });
            ApplicationStatus.Add(new SelectListItem() { Text = "Approved", Value = "Approved" });

            var reportingViewModel = new ReportingViewModel()
            {
                OrderBy = orderBy,
                ApplicationStatus = ApplicationStatus
            };

            var reportViewer = new ReportViewer()
            {
                ProcessingMode = ProcessingMode.Local,
                SizeToReportContent = true,
                Width = Unit.Percentage(100),
                Height = Unit.Percentage(100),
            };

            ApplicationViewDataTable AD = new ApplicationViewDataTable();
            ApplicationAdp AP = new ApplicationAdp("Select * From ApplicationView");
            AP.cmdTimeout = 0;
            String Wheresql = "";
            String OrderBy = Request["OrderBy"];

            if (!string.IsNullOrWhiteSpace(Request["FullName"]))
                Wheresql = CheckWhere(Wheresql) + " fullName Like N'%" + Request["FullName"]+"%'";

            if (!string.IsNullOrWhiteSpace(Request["previousRestorationSource"]))
                Wheresql = CheckWhere(Wheresql) + " previousRestorationSource Like N'%" + Request["previousRestorationSource"] + "%'";

            if (!string.IsNullOrWhiteSpace(Request["ApprovedDateFrom"]) && !string.IsNullOrWhiteSpace(Request["ApprovedDateTo"]))
                Wheresql = CheckWhere(Wheresql) + " format(ApprovedDate,'dd/mm/yyyy') between " + Request["ApprovedDateFrom"] + " and " + Request["ApprovedDateTo"];

            if (!string.IsNullOrWhiteSpace(Request["RejectDateFrom"]) && !string.IsNullOrWhiteSpace(Request["RejectDateTo"]))
                Wheresql = CheckWhere(Wheresql) + " RejectDate  between " + Request["RejectDateFrom"] + " and " + Request["RejectDateTo"];

            if (!string.IsNullOrWhiteSpace(Request["otherOwnershipType"]))
                Wheresql = CheckWhere(Wheresql) + " otherOwnershipType Like N'%" + Request["otherOwnershipType"] + "%'";

            if (!string.IsNullOrWhiteSpace(Request["InterestedRepairingEntityName"]))
                Wheresql = CheckWhere(Wheresql) + " interestedRepairingEntityName Like N'%" + Request["InterestedRepairingEntityName"] + "%'";

            if (!string.IsNullOrWhiteSpace(Request["ApplicationStatus"]))
                Wheresql = CheckWhere(Wheresql) + " ApplicationStatus= " + Request["ApplicationStatus"];

           

            if (Wheresql.Length==0)
            {
                ApplicationAdp AP2 = new ApplicationAdp("Select * From ApplicationView " + OrderBy);
                AP2.cmdTimeout = 0;
                AP2.Adapter.Fill(AD);
            }
            else
            {
                AP.QueryStr(Wheresql, OrderBy);
                AP.Adapter.Fill(AD);
            }

            foreach (DataRow row in AD.Rows)
            {
                var AppStatus = row["ApplicationStatus"].ToString();
                if(!string.IsNullOrWhiteSpace(AppStatus))
                {
                    var applicationStatus = Convert.ToInt32(row["ApplicationStatus"].ToString());
                    if (applicationStatus == 0)
                    {
                        row["AppStatus"] = "Rejected";
                    }
                    else
                    {
                        row["AppStatus"] = "Approved";
                    }
                }
                else
                {
                    row["AppStatus"] = "Not Processed";
                }
                
            }

            reportViewer.LocalReport.ReportPath = @"Reporting\ApplicationsReport.rdlc";

            reportViewer.LocalReport.DataSources.Add(new ReportDataSource("ApplicationDataSet", (object)AD));
            reportViewer.LocalReport.Refresh();
            ViewBag.ReportViewer = reportViewer;

            return View("ApplicationRptView", reportingViewModel);

        }

        public ActionResult ProjectRptView()

       {
            var donors = _donorsAppService.getAllDonors();
            List<SelectListItem> OrderBy = new List<SelectListItem>();
            OrderBy.Add(new SelectListItem() { Text = "Project Name", Value = "projectArabicName" });
            OrderBy.Add(new SelectListItem() { Text = "year", Value = "year" });
            OrderBy.Add(new SelectListItem() { Text = "Number of Families", Value = "numberOfFamilies" });

            var ReportingViewModel = new ReportingViewModel
            {
              Donors = donors,
                OrderBy= OrderBy
            };

            //List<string> OrderBy = new List<string>
            //{
            //    ""
            //}


         


            return View("ProjectRptView", ReportingViewModel);

        }

       

        public ActionResult projectRpt()
        {
            var donors = _donorsAppService.getAllDonors();
            List<SelectListItem> orderBy = new List<SelectListItem>();
            orderBy.Add(new SelectListItem() { Text = "Project Name", Value = "projectArabicName" });
            orderBy.Add(new SelectListItem() { Text = "year", Value = "year" });
            orderBy.Add(new SelectListItem() { Text = "Number of Families", Value = "numberOfFamilies" });
            var ReportingViewModel = new ReportingViewModel
            {
                Donors = donors,
                OrderBy=orderBy
            };

            var reportViewer = new ReportViewer()
            {
                ProcessingMode = ProcessingMode.Local,
                SizeToReportContent = true,
                Width = Unit.Percentage(100),
                Height = Unit.Percentage(100),
            };

            ViewProjectDataTable PD = new ViewProjectDataTable();
            ProjectAdp PA = new ProjectAdp("select * from ViewProject");
            PA.cmdTimeout = 0;
            String Wheresql = "";
            String OrderBy = Request["OrderBy"] ;


            if (!string.IsNullOrWhiteSpace(Request["Donors"]))
                Wheresql = CheckWhere(Wheresql) + " donorId=" + Request["Donors"];
            
            if(Convert.ToInt32(Request["CheckedYear"])==0)
            {
                if (!string.IsNullOrWhiteSpace(Request["year"]))
                    Wheresql = CheckWhere(Wheresql) + " year=" + Request["year"];
            }

            if(Convert.ToInt32(Request["CheckedYear"]) == 1)
            {
                if (!string.IsNullOrWhiteSpace(Request["YearFrom"]) && !string.IsNullOrWhiteSpace(Request["YearTo"]))
                    Wheresql = CheckWhere(Wheresql) + " year between " + Request["YearFrom"] + " and " + Request["YearTo"];
            }
           

            if (!string.IsNullOrWhiteSpace(Request["Budget"]))
                Wheresql = CheckWhere(Wheresql) + " budget=" + Request["Budget"];

            if (!string.IsNullOrWhiteSpace(Request["Contractor"]))
                Wheresql = CheckWhere(Wheresql) + " contractor Like N'%" + Request["Contractor"]+"%'";

            if (!string.IsNullOrWhiteSpace(Request["locationFromMap"]))
                Wheresql = CheckWhere(Wheresql) + " locationFromMap=" + Request["locationFromMap"];

            if (!string.IsNullOrWhiteSpace(Request["projectPeriod"]))
                Wheresql = CheckWhere(Wheresql) + " projectPeriod=" + Request["projectPeriod"];

            if (!string.IsNullOrWhiteSpace(Request["Name"]))
                Wheresql = CheckWhere(Wheresql) + " projectArabicName Like N'%" + Request["Name"] + "%' or projectEnglishName like N'%" + Request["Name"] + "%'";

         

            if (Wheresql.Length==0 )
            {
                ProjectAdp PA2 = new ProjectAdp("select * from ViewProject order by "+ OrderBy );
                PA2.cmdTimeout = 0;
                PA2.Adapter.Fill(PD);
            }
            else
            {
                PA.QueryStr(Wheresql, OrderBy);
                PA.Adapter.Fill(PD);
            }

              
           

            //if (!string.IsNullOrWhiteSpace(Request["Donors"]) && !string.IsNullOrWhiteSpace(Request["year"]))
            //{
            //    var donorId = Convert.ToInt32(Request["Donors"]);
            //    var year = Convert.ToInt32(Request["year"]);
            //    // PA.Fill(PD);
            //    PA.FillByDonorAndYear(PD, year, donorId);

            //}
            //else if (string.IsNullOrWhiteSpace(Request["Donors"]) && !string.IsNullOrWhiteSpace(Request["year"]))
            //{
            //    var year = Convert.ToInt32(Request["year"]);
            //    PA.FillByYear(PD,year);
            //}
            //else if (!string.IsNullOrWhiteSpace(Request["Donors"]) && string.IsNullOrWhiteSpace(Request["year"]))
            //{
            //    var donorId = Convert.ToInt32(Request["Donors"]);
            //    PA.FillByDonor(PD, donorId);
            //}
            //else
            //{
            //    PA.Fill(PD);
            //}
            
           

            


            reportViewer.LocalReport.ReportPath = @"Reporting\ProjectsReport.rdlc";

            reportViewer.LocalReport.DataSources.Add(new ReportDataSource("ProjectsDataSet", (object)PD));

            

           // System.Collections.Generic.List<ReportParameter> paramList = new System.Collections.Generic.List<ReportParameter>();
          
        //    paramList.Add(new ReportParameter("User1", this.User.Identity.Name));
           

        //  reportViewer.LocalReport.SetParameters(paramList);

            reportViewer.LocalReport.Refresh();
            ViewBag.ReportViewer = reportViewer;

            return View("ProjectRptView", ReportingViewModel);
        }

        public String CheckWhere(String wheresql)
        {
            if (wheresql.Length >0 )
            {
                  return wheresql + " and ";
            }
            return wheresql;
        }

        public ActionResult ReportsListView()
        {

            return View("ReportsListView");
        }

        public ActionResult ProjectCertificateView()
        {
            return View("ProjectCertificateView");
        }

        public ActionResult ProjectCertificateRpt()
        {
            var reportViewer = new ReportViewer()
            {
                ProcessingMode = ProcessingMode.Local,
                SizeToReportContent = true,
                Width = Unit.Percentage(100),
                Height = Unit.Percentage(100),
            };
            var projectId =Convert.ToInt32(Request["Id"]);
            ProjectsApplicationViewDataTable PD = new ProjectsApplicationViewDataTable();
            ProjectsApplicationViewTableAdapter PA = new ProjectsApplicationViewTableAdapter();
            PA.ProjectApplicationFillByProjectId(PD, projectId);

            foreach (DataRow row in PD.Rows )
            {
                GetInterventionTypeInput input = new GetInterventionTypeInput();
                input.Id =int.Parse(row["interventionTypeId"].ToString());
                GetInterventionTypeOutput output= _InterventionTypeAppService.GetInterventionTypeById(input);
                row["InterVType"] = output.InterventionName;

                getNeighborhoodIntput NInput = new getNeighborhoodIntput();
                NInput.Id = Convert.ToInt32(row["neighborhoodID"].ToString());
                getNeighborhoodOutput NOutput = _neighborhoodAppService.getNeighborhoodById(NInput);
                row["Neighborhood"] = NOutput.EName;

               
                var DonorId = row["donorId"].ToString();
                if (!string.IsNullOrWhiteSpace(DonorId))
                {
                    GetDonorsInput DInput = new GetDonorsInput();
                    DInput.Id = Convert.ToInt32(row["donorId"].ToString());
                    var DOutput = _donorsAppService.GetDonorById(DInput);
                    row["Donor"] = DOutput.DonorName;
                }
                else
                {
                    row["Donor"] = "no Donor";
                }

                var isBuildingHasRoof = Convert.ToByte(row["isBuildingHasRoof"].ToString());
                if(isBuildingHasRoof==1)
                {
                    row["BuildingRoof"] = "Yes";
                }
                else
                {
                    row["BuildingRoof"] = "No";
                }

                GetBuildingUsesInput UInput = new GetBuildingUsesInput();
                UInput.id = Convert.ToInt32(row["buildingUsesID"].ToString());
                var UOutput = _buildingUsesAppService.getBuildingUsesById(UInput);
                row["BuildingUses"] = UOutput.UsedFor;

                
            }

            var rows = PD.Rows;
            var ProjectID =Convert.ToInt32(rows[0]["Id"]);
           // var AllUploadedProjectFiles = _uploadProjectFilesAppService.GetAllUploadedProjectFiles();
           // var ProjectUploadedFiles = from UF in AllUploadedProjectFiles where UF.ProjectId == ProjectID orderby UF.Id descending select UF;
            UploadProjectFilesViewDataTable UPD = new UploadProjectFilesViewDataTable();
            UploadProjectFilesViewTableAdapter UPA = new UploadProjectFilesViewTableAdapter();
            UPA.FillByProjectUploadFilesProjectId(UPD, ProjectID);











            reportViewer.LocalReport.ReportPath = @"Reporting\ProjectCertificate.rdlc";

            reportViewer.LocalReport.DataSources.Add(new ReportDataSource("ProjectApplicationDataSet", (object)PD));
            reportViewer.LocalReport.DataSources.Add(new ReportDataSource("ProjectFilesDataSet", (object)UPD));
            reportViewer.LocalReport.Refresh();
            ViewBag.ReportViewer = reportViewer;

            return View("ProjectCertificateView");

        }
    }
}