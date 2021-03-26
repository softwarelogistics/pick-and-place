﻿using LagoVista.PCB.Eagle.Models;
using LagoVista.PickAndPlace.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.ViewModels
{
    public partial class MainViewModel
    {
        public JobControlViewModel JobControlVM { get; private set; }
        public MachineControlViewModel MachineControlVM { get; private set; }

        PCBProject _project;
        public PCBProject Project
        {
            get { return _project; }
            set
            {
                _project = value;
                Machine.PCBManager.Project = value;
                RaisePropertyChanged();
            }
        }

        public MRUs MRUs
        {
            get; set;
        }
    }

    public class FileInfo
    {
        public string FullPath { get; set; }
        public string FileName { get; set; }
    }

    public class MRUs
    {
        public MRUs()
        {
            PnPJobs = new List<string>();
            GCodeFiles = new List<string>();
            BoardFiles = new List<string>();
            ProjectFiles = new List<string>();
        }

        public List<String> PnPJobs { get; set; }
        public List<String> GCodeFiles { get; set; }
        public List<String> BoardFiles { get; set; }
        public List<String> ProjectFiles { get; set; }
    }
}
