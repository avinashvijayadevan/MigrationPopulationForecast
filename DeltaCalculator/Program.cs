using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeltaCalculator
{
    class Program
    {
        static void Main(string[] args)
        {
            TravelContext tc = new TravelContext();
            List<DestinationAggregation> destinationList = tc.DestinationAggregations.ToList();
            List<OrignAggregation> originList = tc.OrignAggregations.ToList();
            List<DeltaPopulation> deltaPopulation = new List<DeltaPopulation>();

            foreach (DestinationAggregation da in destinationList)
            {
                DeltaPopulation delta = new DeltaPopulation();
                foreach (OrignAggregation oa in originList)
                {
                    if (da.Destination == oa.Origin && da.Year == oa.Year && da.Month == oa.Month)
                    {
                        delta.Place = da.Destination;
                        delta.DeltaCount = da.Count - oa.Count;
                        delta.Year = da.Year;
                        delta.Month = da.Month;
                        deltaPopulation.Add(delta);
                    }
                }
            }
            //var newList = deltaPopulation.GroupBy(x => new { x.Place, x.Year, x.Month })
            //                            .Select(y => new DestinationAggregation()
            //                            {
            //                                Destination = y.Key.Place,
            //                                Year = y.Key.Year,
            //                                Month = y.Key.Month,
            //                                Count = y.Sum(x => x.DeltaCount),
            //                            }
            //                            );

            //var dd = (from a in newList
            //          orderby a.Destination, a.Year, a.Month
            //          select a).ToList();

            int pKey = tc.DeltaPopulations.Max(x => x.DeltaPopulationId);
            foreach (DeltaPopulation dp in deltaPopulation)
            {
                pKey += 1;
                dp.DeltaPopulationId = pKey;
                tc.Add(dp);
            }
            tc.SaveChanges();
        }
    }
}
