﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace LagoVista.PCB.Eagle.Models
{
    public class Signal
    {
        public List<ContactRef> Contacts { get; private set; }
        public List<Wire> Wires { get; private set; }

        public string Name { get; set; }

        public static Signal Create(XElement element)
        {
            return new Signal()
            {
                Name = element.GetString("name"),
                Wires = (from childWires in element.Descendants("wire") select Wire.Create(childWires)).ToList(),
                Contacts = (from refs in element.Descendants("contactref") select ContactRef.Create(refs)).ToList(),
            };
        }

        public List<Wire> UnroutedWires
        {
            get { return Wires.Where(wire => wire.Layer == 19).ToList(); }
        }

        public List<Wire> TopWires
        {
            get { return Wires.Where(wire => wire.Layer == 1).ToList(); }
        }

        public List<Wire> BottomWires
        {
            get { return Wires.Where(wire => wire.Layer == 16).ToList(); }
        }

        private List<Trace> FindTraces(List<Wire> unprocessedWires)
        {
            var traces = new List<Trace>();

            /* Grab first fire of list of wires not processed */
            var candidateWire = unprocessedWires.FirstOrDefault();
            while (candidateWire != null)
            {
                /* This wire is process so remove it */
                unprocessedWires.Remove(candidateWire);

                /* Create our new trace, add the first wire and add it to the traces */
                var trace = new Trace();
                trace.Wires.Add(candidateWire);
                traces.Add(trace);
                var newCandidates = new List<Wire>();

                /* Continue searching if we have a candidate coming in OR we have a new candidate that was put on the trace to review */
                while (candidateWire != null)
                {

                    var wiresProcessed = new List<Wire>();

                    foreach (var wire in unprocessedWires)
                    {

                        if (candidateWire.Rect.X2 == wire.Rect.X1 && candidateWire.Rect.Y2 == wire.Rect.Y1)
                        {
                            /* Add a new candidate to review */
                            newCandidates.Add(wire);

                            /* Make the junctions to the connected traces/wires */
                            candidateWire.EndJunctions.Add(wire);
                            wire.StartJunctions.Add(candidateWire);

                            /* Of course we add it to the trace */
                            trace.Wires.Add(wire);
                            wiresProcessed.Add(wire);
                        }

                        if (candidateWire.Rect.X1 == wire.Rect.X2 && candidateWire.Rect.Y1 == wire.Rect.Y2)
                        {

                            /* Add a new candidate to review */
                            newCandidates.Add(wire);

                            /* Make the junctions to the connected traces/wires */
                            candidateWire.StartJunctions.Add(wire);
                            wire.EndJunctions.Add(candidateWire);

                            /* Of course we add it to the trace */
                            trace.Wires.Add(wire);
                            wiresProcessed.Add(wire);
                        }

                        if (candidateWire.Rect.X2 == wire.Rect.X2 && candidateWire.Rect.Y2 == wire.Rect.Y2)
                        {

                            /* Add a new candidate to review */
                            newCandidates.Add(wire);

                            /* Make the junctions to the connected traces/wires */
                            candidateWire.EndJunctions.Add(wire);
                            wire.EndJunctions.Add(candidateWire);

                            /* Of course we add it to the trace */
                            trace.Wires.Add(wire);
                            wiresProcessed.Add(wire);
                        }

                        if (candidateWire.Rect.X1 == wire.Rect.X1 && candidateWire.Rect.Y1 == wire.Rect.Y1)
                        {
                            /* Add a new candidate to review */
                            newCandidates.Add(wire);

                            /* Make the junctions to the connected traces/wires */
                            candidateWire.StartJunctions.Add(wire);
                            wire.StartJunctions.Add(candidateWire);

                            /* Of course we add it to the trace */
                            trace.Wires.Add(wire);
                            wiresProcessed.Add(wire);
                        }
                    }

                    /* If the wire was added in this pass, remove it from the unprocessedWires list */
                    foreach (var wire in wiresProcessed)
                    {
                        if(wire.Rect.X2 == 34.925)
                        {
                            Debugger.Break();
                        }

                        unprocessedWires.Remove(wire);
                    }

                    /* If we have a new candidate review it */
                    candidateWire = newCandidates.FirstOrDefault();
                    if (candidateWire != null)
                    {
                        newCandidates.Remove(candidateWire);
                    }

                }

            
                /* Grab next wire...if any to be checked */
                candidateWire = unprocessedWires.FirstOrDefault();
            }

            return traces;
        }

        public List<Trace> TopTraces
        {
            get { return FindTraces(TopWires.ToList()); }
        }

        public List<Trace> BottomTraces
        {

            get { return FindTraces(BottomWires.ToList()); }
        }
    }
}
