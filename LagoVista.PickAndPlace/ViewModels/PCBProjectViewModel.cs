﻿using LagoVista.Core.Commanding;
using LagoVista.Core.ViewModels;
using LagoVista.PCB.Eagle.Managers;
using LagoVista.PCB.Eagle.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace LagoVista.PickAndPlace.ViewModels
{
    public class PCBProjectViewModel : ViewModelBase
    {
        public event EventHandler GenerateIsolationEvent;

        PCBProject _project;
        public PCBProject Project
        {
            get { return _project; }
            set { Set(ref _project, value); }
        }

        private PrintedCircuitBoard _pcb;
        public PrintedCircuitBoard PCB
        {
            get { return _pcb; }
            set { Set(ref _pcb, value); }
        }

        public PCBProjectViewModel(PCBProject project)
        {
            Project = project;
            SaveDefaultProfileCommand = new RelayCommand(SaveDefaultProfile);
            OpenEagleBoardCommand = new RelayCommand(OpenEagleBoard);
            OpenTopEtchingCommand = new RelayCommand(OpenTopEtching);
            OpenBottomEtchingCommand = new RelayCommand(OpenBottomEtching);
            CenterBoardCommand = new RelayCommand(CenterBoard);
            GenerateIsolationMillingCommand = new RelayCommand(GenerateIsolation);

            if (!String.IsNullOrEmpty(Project.EagleBRDFilePath))
            {

                try
                {
                    var doc = XDocument.Load(Project.EagleBRDFilePath);
                    PCB = EagleParser.ReadPCB(doc);
                    Project.FiducialOptions = PCB.Holes.Where(drl => drl.Drill > 2).ToList();
                }
                catch (Exception) { }
            }
        }

        public bool CanCenterboard()
        {
            return PCB != null;
        }

        public bool CanGenerateIsolation()
        {
            return PCB != null;
        }

        public async Task LoadDefaultSettings()
        {
            Project = await Storage.GetAsync<PCBProject>("Default.pcbproj");
            if (Project == null)
            {
                Project = PCBProject.Default;
            }
        }

        public async void OpenEagleBoard()
        {
            var result = await Popups.ShowOpenFileAsync(Constants.FileFilterPCB);
            if (!String.IsNullOrEmpty(result))
            {
                try
                {
                    Project.EagleBRDFilePath = result;

                    var doc = XDocument.Load(Project.EagleBRDFilePath);
                    PCB = EagleParser.ReadPCB(doc);
                    Project.FiducialOptions = PCB.Holes.Where(drl => drl.Drill > 2).ToList();
                }
                catch
                {
                    await Popups.ShowAsync("Could not open Eage File");
                }
            }
        }

        public async void OpenTopEtching()
        {
            var result = await Popups.ShowOpenFileAsync(Constants.FileFilterGCode);
            if (!String.IsNullOrEmpty(result))
            {
                Project.TopEtchingFilePath = result;
            }
        }

        public async void OpenBottomEtching()
        {
            var result = await Popups.ShowOpenFileAsync(Constants.FileFilterGCode);
            if (!String.IsNullOrEmpty(result))
            {
                Project.BottomEtchingFilePath = result;
            }
        }

        public void CenterBoard()
        {
            Project.ScrapSides = Math.Round((Project.StockWidth - PCB.Width) / 2, 2);
            Project.ScrapTopBottom = Math.Round((Project.StockHeight - PCB.Height) / 2, 2);
        }

        public void GenerateIsolation()
        {
            GenerateIsolationEvent?.Invoke(this, null);
        }

        public async Task<bool> LoadExistingFile(string file)
        {
            Project = await Storage.GetAsync<PCBProject>(file);
            return Project != null;
        }

        public async void SaveDefaultProfile()
        {
            var brdFileName = Project.EagleBRDFilePath;
            Project.EagleBRDFilePath = String.Empty;

            await Storage.StoreAsync(Project, "Default.pcbproj");
            Project.EagleBRDFilePath = brdFileName;
        }

        public RelayCommand CenterBoardCommand { get; private set; }

        public RelayCommand SaveDefaultProfileCommand { get; private set; }
        public RelayCommand OpenEagleBoardCommand { get; private set; }
        public RelayCommand OpenTopEtchingCommand { get; private set; }
        public RelayCommand OpenBottomEtchingCommand { get; private set; }
        public RelayCommand GenerateIsolationMillingCommand { get; private set; }

        public string AddDrillBit(DrillBit bit)
        {
            if(ConsolidatedDrillBit == null)
            {
                return null;
            }

            /* If it already exists here, don't add it again */
            if(ConsolidatedDrillBit.Bits.Where(bt=>bt.ToolName== bit.ToolName).Any())
            {
                return null;
            }

            foreach(var consolidatedDrill in Project.ConsolidatedDrillRack)
            {
                if (consolidatedDrill != ConsolidatedDrillBit && consolidatedDrill.Bits.Where(bt => bt.ToolName == bit.ToolName).Any())
                {
                    return consolidatedDrill.NewToolName;
                }
            }

            ConsolidatedDrillBit.AddBit(bit);

            return null;
        }

        public void RemoveBit(DrillBit bit)
        {
            var localBit = ConsolidatedDrillBit.Bits.Where(bt => bt.ToolName == bit.ToolName).FirstOrDefault();
            if(localBit != null)
            {
                ConsolidatedDrillBit.Bits.Remove(localBit);
            }
        }

        ConsolidatedDrillBit _consolidatedDrillBit;
        public ConsolidatedDrillBit ConsolidatedDrillBit
        {
            get { return _consolidatedDrillBit; }
            set{ Set(ref _consolidatedDrillBit, value);}
        }
    }
}
