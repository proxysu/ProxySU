using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProxySuper.Core.Models
{
    public class ProjectProgress
    {
        private string _step;

        private string _desc;

        private int _percentage;

        private string _logs;

        public ProjectProgress()
        {
            _step = "步骤";

            _desc = "步骤描述";

            _percentage = 0;

            _logs = string.Empty;

            StepUpdate = () => { };
        }


        public Action StepUpdate { get; set; }

        public Action LogsUpdate { get; set; }

        public string Desc
        {
            get { return _desc; }
            set
            {
                _desc = value;
                StepUpdate();

                _logs += _desc + "\n";
                LogsUpdate();
            }
        }

        public string Step
        {
            get { return _step; }
            set
            {
                _step = value;
                StepUpdate();

                _logs += Step + "\n";
                LogsUpdate();
            }
        }

        public int Percentage
        {
            get { return _percentage; }
            set
            {
                _percentage = value;
                StepUpdate();
            }
        }

        public string Logs
        {
            get { return _logs; }
            set
            {
                _logs = value;
                LogsUpdate();
            }
        }
    }
}