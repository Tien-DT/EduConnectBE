using EduConnect.Application.Services.Interface;
using EduConnect.Domain.Entities;
using EduConnect.Infrastruture.Repositories.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduConnect.Application.Services.Class
{
    public class ExamService: BaseService<Exam, int>, IExamService
    {
        public ExamService(IExamRepository examRepository) : base(examRepository)
        {
        }
    
    }
}
