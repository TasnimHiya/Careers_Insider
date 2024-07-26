using JobPortal.Models;
using JobPortal.Repository;
using Microsoft.Ajax.Utilities;
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace JobPortal.Controllers
{
    public class JobSeekerController : Controller
    {
        private bool IsValid()
        {
            return Session["SeekerId"] != null;
        }
        // GET: JobSeeker
        public ActionResult Index()
        {
            bool isValid = IsValid();
            if (isValid)
            {
                PublicRepository publicRepository = new PublicRepository();
                var jobs = publicRepository.GetJobDetails();
                JobSeekerRepository repo = new JobSeekerRepository();
                int id = Convert.ToInt32(Session["SeekerId"]);
                var applications = repo.GetJobApplications(id);
                return View(new Index { JobApplications = applications, JobDetails = jobs });
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }
        /// <summary>
        /// Display profile of the job seeker
        /// </summary>
        /// <returns></returns>
        public ActionResult JobSeekerProfile()
        {
            try
            {
                JobSeekerRepository jobSeekerRepository = new JobSeekerRepository();
                PublicRepository repo = new PublicRepository();
                int seekerId = (int)Session["SeekerId"];
                var jobSeeker = jobSeekerRepository.JobSeekers().Find(model => model.SeekerId == seekerId);
                var edu = jobSeekerRepository.GetEducationDetails(seekerId);
                var userSkills = repo.JobSeekerSkills(seekerId).ToList();
                var userSkillsId = repo.JobSeekerSkills(seekerId).Select(js => js.SkillId).ToList();

                var skills = repo.DisplaySkills().Where(skil => !userSkillsId.Contains(skil.SkillId)).ToList();
                var viewModel = new JobSeekerProfile
                {
                    JobSeekerDetails = jobSeeker,
                    EducationDetails = edu,
                    Skills = userSkills,
                    AllSkills = skills
                };
                return View(viewModel);
            }catch(Exception ex)
            {
                ExceptionLogging.SendErrorToText(ex);
                return View("Error");
            }
        }
        public ActionResult UpdateProfile()
        {
            JobSeekerRepository jobSeekerRepository = new JobSeekerRepository();
            var jobSeeker = jobSeekerRepository.JobSeekers().Find(model => model.SeekerId == (int)Session["SeekerId"]);
            return View(jobSeeker);

        }
        /// <summary>
        /// Update profile job seeker
        /// </summary>
        /// <param name="jobSeeker">Job seeker instance</param>
        /// <param name="imageUpload">Uploaded image</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult UpdateProfile(JobSeekerModel jobSeeker, HttpPostedFileBase imageUpload)
        {
            try
            {
                JobSeekerRepository jobSeekerRepository = new JobSeekerRepository();
                if (jobSeekerRepository.JobSeekerUpdate(jobSeeker, imageUpload, Convert.ToInt32(Session["SeekerId"])))
                {
                    TempData["Message"] = "Updated";
                }
                return RedirectToAction("JobSeekerProfile");
            }
            catch (Exception ex)
            {
                ExceptionLogging.SendErrorToText(ex);
                return View("Error");
            }

        }

        public ActionResult AddEducationDetails()
        {
            return View();
        }
        [HttpPost]
        /// <summary>
        /// To add eduction details
        /// </summary>
        /// <param name="educationList">Form contain list of all the eduction deatils of the user</param>
        /// <returns></returns>
        public ActionResult AddEducationDetails(EducationDetails educationList)
        {
            try
            {
                int id = (int)Session["SeekerId"];
                JobSeekerRepository jobSeekerRepository = new JobSeekerRepository();
                if (jobSeekerRepository.AddEducationDetails(educationList, id))
                {
                    TempData["Message"] = "Added successfully";
                    return RedirectToAction("JobSeekerProfile");
                }
                return View();
            }
            catch (Exception ex)
            {
                ExceptionLogging.SendErrorToText(ex);
                return View("Error");
            }
        }

        [HttpPost]
        public ActionResult UpdateResume(HttpPostedFileBase resumeFile)
        {
            try
            {
                JobSeekerRepository jobSeekerRepository = new JobSeekerRepository();
                if (jobSeekerRepository.UpdateResume(resumeFile, Convert.ToInt32(Session["SeekerId"])))
                {
                    TempData["Message"] = "Resume Updated";
                }
               return RedirectToAction("JobSeekerProfile");
            }
            catch (Exception ex)
            {
                ExceptionLogging.SendErrorToText(ex);
                return View("Error");
            }

        }
        public ActionResult UpdateEducationDetails(int id)
        {
            JobSeekerRepository jobSeekerRepository = new JobSeekerRepository();
            var educationDetails = jobSeekerRepository.GetEducationDetails(Convert.ToInt32(Session["SeekerId"])).Find(ed => ed.EducationId == id);
            return View(educationDetails);
        }
        /// <summary>
        /// Update Education details
        /// </summary>
        /// <param name="educationDetails"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult UpdateEducationDetails(EducationDetails educationDetails) {
            JobSeekerRepository jobSeekerRepository = new JobSeekerRepository();
            try
            {
                if (jobSeekerRepository.UpdateEducationDetails(educationDetails))
                {
                    TempData["Message"] = "Updated Successfully";
                }
                return RedirectToAction("JobSeekerProfile");
            }
            catch (Exception ex)
            {
                ExceptionLogging.SendErrorToText(ex);
                return View("Error");
            }
        }
        /// <summary>
        /// Delete educational details
        /// </summary>
        /// <param name="id">Education id</param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult DeleteEducationDetails(int id)
        {
            JobSeekerRepository jobSeekerRepository=new JobSeekerRepository();
            try {
                if (jobSeekerRepository.DeleteEducationDetails(id))
                {
                    TempData["Message"] = "Deleted Successfully ";
                }
                return RedirectToAction("JobSeekerProfile");
            }
            catch (Exception ex)
            {
                ExceptionLogging.SendErrorToText(ex);
                return View("Error");
            }
        }
        /// <summary>
        /// Delete Jobseeker skill id
        /// </summary>
        /// <param name="id">Skill id</param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult DeleteJobSeekerSkill(int id)
        {
            try {
                JobSeekerRepository jobSeekerRepository = new JobSeekerRepository();
                if (jobSeekerRepository.DeleteJobSeekerSkill((int)id))
                {
                    TempData["Message"] = "Deleted Successullly";
                }
                return RedirectToAction("JobSeekerProfile");
            }
            catch (Exception ex)
            {
                ExceptionLogging.SendErrorToText(ex);
                return View("Error");
            }
        }
        /// <summary>
        /// Display job vacancies posted by the employer
        /// </summary>
        /// <returns></returns>
        public ActionResult Jobs()
        {
            try
            {
                PublicRepository publicRepository = new PublicRepository();
                DateTime currentDate = DateTime.Now;
                var jobs = publicRepository.GetJobDetails().Where(job => job.ApplicationDeadline >= currentDate && job.IsPublished).ToList();
                return View(jobs);
            }
            catch (Exception ex)
            {
                ExceptionLogging.SendErrorToText(ex);
                return View("Error");
            }

        }
        /// <summary>
        /// Filter the job details based on the search string
        /// </summary>
        /// <param name="search">Search string</param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Jobs(string search)
        {
            try
            {
                PublicRepository publicRepository = new PublicRepository();
                var jobs = publicRepository.GetJobDetails();
                if (!string.IsNullOrEmpty(search))
                {
                    jobs = jobs.Where(job =>job.JobTitle.Contains(search) || job.CategoryName.Contains(search) ||job.Location.Contains(search) && job.ApplicationDeadline > DateTime.Now && job.IsPublished).ToList();
                }

                return View(jobs);
            }
            catch (Exception ex)
            {
                ExceptionLogging.SendErrorToText(ex);
                return View("Error");
            }
        }

        [HttpGet]
        /// <summary>
        /// Apply for the job 
        /// </summary>
        /// <param name="id">JobId</param>
        /// <returns></returns>
        public ActionResult ApplyJob(int  id) {
            try
            {
                JobApplication application = new JobApplication {
                    SeekerId = Convert.ToInt32(Session["SeekerId"]),
                    JobApplicationID = id,
                    ApplicationDate = DateTime.Now
                    };
                JobSeekerRepository repo = new JobSeekerRepository();
                if (repo.CreateJobApplication(application))
                {
                    TempData["Message"] = "Applied Successfull";
                }
                return Redirect(Request.UrlReferrer.ToString());
            }
            catch (Exception ex)
            {
                ExceptionLogging.SendErrorToText(ex);
                return View("Error");
            }
        }
        /// <summary>
        /// Visit job to store who visisted the job
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult ViewJob(int id)
        {
            try
            {
                JobSeekerRepository jobSeekerRepository = new JobSeekerRepository();
                ViewJob obj = new ViewJob
                {
                    JobId = id,
                    SeekerId = Convert.ToInt32(Session["SeekerId"]),
                    ViewDate = DateTime.Now,
                };
                if (jobSeekerRepository.VisitJob(obj))
                {
                    return new HttpStatusCodeResult(200);
                }
                return new HttpStatusCodeResult(400);
            }
            catch(Exception ){
                return new HttpStatusCodeResult(400);
            }
        }    /// saad 321 no. line thake 
       
    }
}