using Galaxxis.Core.Business.Provider;
using Galaxxis.Core.Extensions;
using Galaxxis.Data.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Galaxxis.Core.Engine
{
    public sealed partial class Processor
    {
        public Task ProcessAsync(long universeTicks)
        {
            return Task.Run(() => Process(universeTicks));
        }

        /// <summary>
        /// This is done for every single tick gone until last processed tick.
        /// </summary>
        private void Process(long universeTicks)
        {
            // skip if task is still running
            if (this.IsWorking)
            {
                Debug.WriteLine($"{DateStamp}: Still processing... Tick {universeTicks} is queued.");
                return;
            }

            // mark processing, block another run simultaniously
            this.IsWorking = true;

            // log time started processing
            DateTime tStartProcessing = DateTime.Now;

            // for debugging purposes
            //System.Threading.Thread.Sleep(5000);

            // process db objects (for now only one universe supported)
            using (GalaxxisContext context = new GalaxxisContext())
            {
                // get universe
                Universe universe = context.Universe.First();

                // process all ticks gone until last processed tick
                while (universe.UniverseTicks < universeTicks)
                {
                    // log time started processing
                    DateTime tStartTick = DateTime.Now;

                    // raise tick to be processed
                    universe.UniverseTicks++;

                    // add new Events to Database
                    while (EventQueue.Any())
                    {
                        Event @event = EventQueue.Dequeue();
                        Add_EventAction(@event, context, universe.UniverseTicks);
                    }

                    // iterate events, do action and remove
                    List<Event> events = EventProvider.GetEvents(universe.UniverseTicks, context);
                    foreach (Event @event in events)
                    {
                        Do_EventAction(@event, context);
                        context.Event.Remove(@event);
                    }

                    // write log
                    Debug.WriteLine($"{DateStamp}: Tick {universe.UniverseTicks} done in {tStartTick.MsGone()} ms.");
                }

                // save all changes
                context.SaveChanges();
            }
            // mark not processing, unlock processor
            this.IsWorking = false;

            // write log
            Debug.WriteLine($"{DateStamp}: Processing completely done in {tStartProcessing.MsGone()} ms.");
        }

    }
}
