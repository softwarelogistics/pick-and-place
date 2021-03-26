using LagoVista.PCB.Eagle.Managers;
using LagoVista.PCB.Eagle.Models;
using System;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace LagoVista.PickAndPlace.Managers
{
    public partial class PCBManager
    {
        public Task<bool> OpenFileAsync(string path)
        {
            try
            {
                var doc = XDocument.Load(path);

                Board = EagleParser.ReadPCB(doc);
        
                return Task.FromResult(true);
            }
            catch(Exception)
            {
                return Task.FromResult(false);
            }
        }

        public async Task<bool> OpenProjectAsync(string projectFile)
        {
            try
            {
                Project = await PCBProject.OpenAsync(projectFile);

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}