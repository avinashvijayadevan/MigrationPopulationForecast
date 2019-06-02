using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading.Tasks;

namespace Pensive
{
    public class Pensive
    {
        public static void Main(string[] args)
        {
            int argumentId = args.Count() > 0 ? Int32.Parse(args[0]) : 0;
            if (argumentId == 0)
            {
                Int32.TryParse(System.Configuration.ConfigurationManager.AppSettings["ArgumentId"], out argumentId);
            }

            //1 - Generates only dummy data
            //2 - Calculates the delta population for the dummy data imported via data factory job.
            //3 - Predicts the population for next 3 years from the current month - calls the ML Service
            //23 - Does 2 and 3 together

            TravelContext dbContext = new TravelContext();
            switch (argumentId)
            {
                case 1:
                    Simulator.GenerateDummyTravelData(dbContext);
                    break;
                case 2:
                    DeltaCalculator.CalculateDeltaPopulationFromDummyData(dbContext);
                    break;
                case 3:
                    DeltaPredictor.PredictMigrationPopulation(dbContext).Wait();
                    break;
                case 23:
                    DeltaCalculator.CalculateDeltaPopulationFromDummyData(dbContext);
                    DeltaPredictor.PredictMigrationPopulation(dbContext).Wait();
                    break;
                default:
                    break;
            }
        }
    }
    public class Simulator
    {
        public static void GenerateDummyTravelData(TravelContext dbContext)
        {
            List<PassengerInfo> passengerInfoList = new List<PassengerInfo>();
            int reqCount = Int32.Parse(System.Configuration.ConfigurationManager.AppSettings["NoOfTravelRecordsToGenerate"]);
            List<string> destList = (System.Configuration.ConfigurationManager.AppSettings["Destinations"].Split(",".ToCharArray())).ToList<string>();

            Parallel.ForEach(destList, desti =>
            {
                Parallel.For(0, reqCount, i =>
                {
                    PassengerInfo passenger = new PassengerInfo();
                    Random randomNumber = new Random(2);
                    var age = GetRandomDate(1945);
                    var travelDate = GetRandomDate(2015).Item1.Date;
                    passenger.Gender = randomNumber.Next(1, 2);
                    passenger.StringDateOfBirth =
                    (age.Item1 < DateTime.MinValue) ? String.Format("{0:MM/dd/yyyy}", DateTime.Today.AddMonths(-4).Date) : String.Format("{0:MM/dd/yyyy}", age.Item1.Date);
                    passenger.Mode = (TravelMode)(new Random(1)).Next(1, 3);
                    passenger.StringTravelDate =
                    (travelDate < DateTime.MinValue) ? String.Format("{0:MM/dd/yyyy}", DateTime.Today.AddDays(-30)) : String.Format("{0:MM/dd/yyyy}", travelDate);
                    passenger.Origin = GetRandomPlaces().Item1;
                    passenger.Destination = desti;
                    Console.WriteLine(passenger.DateOfBirth + "--" + passenger.Origin + "--" + passenger.Destination + "--" + passenger.Age + "--" + passenger.TravelDate);
                    passengerInfoList.Add(passenger);
                });
            });

            int counter = 1;
            foreach (PassengerInfo passenger in passengerInfoList)
            {
                TravelRawData travelRecord = new TravelRawData()
                {
                    DateOfBirth = passenger.StringDateOfBirth,
                    Destination = passenger.Destination,
                    Gender = passenger.Gender == 1 ? true : false,
                    Mode = (int)passenger.Mode,
                    Origin = passenger.Origin,
                    TravelDate = passenger.StringTravelDate,
                };

                dbContext.Add(travelRecord);
                if (counter >= 100)
                {
                    dbContext.SaveChanges();
                    counter = 1;
                }
            }

            dbContext.SaveChanges();

            //string fileNameSuffix = DateTime.Now.ToString().Replace("/", "-").Replace(":", "-") + ".csv";
            //CreateCSV<PassengerInfo>(passengerInfoList, "travelInfoSampe_" + fileNameSuffix);
        }
        private static Tuple<string> GetRandomPlaces()
        {
            string[] origin = System.Configuration.ConfigurationManager.AppSettings["Origins"].Split(",".ToCharArray());
            int y = ((new Random()).Next(10000, origin.Count() * 10000)) / 10000;
            return new Tuple<string>(origin[y]);
        }
        private static Tuple<DateTime, int> GetRandomDate(int fromYear)
        {
            DateTime start = new DateTime(fromYear, 1, 1);
            int range = (DateTime.Today - start).Days;
            DateTime randomDate = start.AddDays((new Random()).Next(range));
            return new Tuple<DateTime, int>(randomDate, DateTime.Today.Year - randomDate.Year);
        }
        public static void CreateCSV<T>(List<T> list, string filePath)
        {
            using (StreamWriter sw = new StreamWriter(filePath))
            {
                CreateHeader(list, sw);
                CreateRows(list, sw);
            }
        }
        private static void CreateHeader<T>(List<T> list, StreamWriter sw)
        {
            PropertyInfo[] properties = typeof(T).GetProperties();
            for (int i = 0; i < properties.Length - 1; i++)
            {
                sw.Write(properties[i].Name + ",");
            }
            var lastProp = properties[properties.Length - 1].Name;
            sw.Write(lastProp + sw.NewLine);
        }
        private static void CreateRows<T>(List<T> list, StreamWriter sw)
        {
            foreach (var item in list)
            {
                if (item != null)
                {
                    PropertyInfo[] properties = typeof(T).GetProperties();
                    for (int i = 0; i < properties.Length - 1; i++)
                    {
                        var prop = properties[i];
                        sw.Write(prop.GetValue(item) + ",");
                    }
                    var lastProp = properties[properties.Length - 1];
                    sw.Write(lastProp.GetValue(item) + sw.NewLine);
                }
            }
        }
    }
    public class DeltaCalculator
    {
        public static void CalculateDeltaPopulationFromDummyData(TravelContext dbContext)
        {
            dbContext.DeltaPopulations.RemoveRange(dbContext.DeltaPopulations.Where(x => x.IsPredicted == false));
            dbContext.SaveChanges();

            List<DestinationAggregation> destinationList = dbContext.DestinationAggregations.ToList();
            List<OrignAggregation> originList = dbContext.OrignAggregations.ToList();
            List<DeltaPopulation> currentPopulationList = dbContext.DeltaPopulations.ToList();

            foreach (DestinationAggregation da in destinationList)
            {
                var match = originList.Find(oa => da.Destination == oa.Origin && da.Year == oa.Year && da.Month == oa.Month);
                if (match != null)
                {
                    DeltaPopulation delta = new DeltaPopulation();
                    delta.Place = da.Destination;
                    delta.DeltaCount = da.Count - match.Count;
                    delta.Year = da.Year;
                    delta.Month = da.Month;
                    dbContext.Add(delta);
                }
                else
                {
                    DeltaPopulation delta = new DeltaPopulation();
                    delta.Place = da.Destination;
                    delta.DeltaCount = da.Count;
                    delta.Year = da.Year;
                    delta.Month = da.Month;
                    dbContext.Add(delta);
                }
            }

            dbContext.SaveChanges();
        }
    }
    public class DeltaPredictor
    {
        public static async Task PredictMigrationPopulation(TravelContext dbContext)
        {
            List<DeltaPopulation> currentPopulationList = dbContext.DeltaPopulations.ToList();

            List<string> places = (from deltaRecord in currentPopulationList
                                   select deltaRecord.Place).Distinct().ToList();

            StringTable inpuToMLService = new StringTable();
            inpuToMLService.Values = new string[places.Count * 36, 3];
            int rowIndex = 0;
            foreach (string place in places)
            {
                for (int i = 1; i <= 36; i++)
                {
                    inpuToMLService.Values.SetValue(place, rowIndex, 0);
                    inpuToMLService.Values.SetValue(DateTime.Today.AddMonths(i).Year.ToString(), rowIndex, 1);
                    inpuToMLService.Values.SetValue(DateTime.Today.AddMonths(i).Month.ToString(), rowIndex, 2);
                    rowIndex += 1;
                }
            }


            using (var client = new HttpClient())
            {
                var scoreRequest = new
                {
                    Inputs = new Dictionary<string, StringTable>() {
                        {
                            "input1",
                            new StringTable()
                            {
                                ColumnNames = new string[] {"Place", "Year", "Month"},
                                Values =inpuToMLService.Values
                            }
                        },
                    },
                    GlobalParameters = new Dictionary<string, string>() { { "Append score columns to output", "True" }, }
                };

                const string apiKey = "HDPhr8awTKqENg091NficxualFwzKYkx8uIxU+4GCZGrjdaGxkU8u8PsJRH8bSpSxr1TknixAdtYD9v2V1RmwQ==";
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                client.BaseAddress = new Uri("https://ussouthcentral.services.azureml.net/workspaces/ce227c6f27064648ad83c97981125a9d/services/7e4766faff6c408882020544a25cd7e2/execute?api-version=2.0&details=true");

                // WARNING: The 'await' statement below can result in a deadlock if you are calling this code from the UI thread of an ASP.Net application.
                // One way to address this would be to call ConfigureAwait(false) so that the execution does not attempt to resume on the original context.
                // For instance, replace code such as:
                //      result = await DoSomeTask()
                // with the following:
                //      result = await DoSomeTask().ConfigureAwait(false)

                HttpResponseMessage response = await client.PostAsJsonAsync("", scoreRequest);
                if (response.IsSuccessStatusCode)
                {
                    dbContext.DeltaPopulations.RemoveRange(dbContext.DeltaPopulations.Where(x => x.IsPredicted == true));
                    dbContext.SaveChanges();

                    string jsonRequest = await response.Content.ReadAsStringAsync();
                    RootObject mlResponse = JsonConvert.DeserializeObject<RootObject>(jsonRequest);
                    List<List<string>> predictedrecords = mlResponse.Results.output1.value.Values;
                    foreach (List<string> predictedRecord in predictedrecords)
                    {
                        DeltaPopulation prediction = new DeltaPopulation();
                        prediction.Place = predictedRecord[0];
                        prediction.Year = Int32.Parse(predictedRecord[1]);
                        prediction.Month = Int32.Parse(predictedRecord[2]);
                        prediction.DeltaCount = (int)decimal.Parse(predictedRecord[3]);
                        prediction.IsPredicted = true;
                        dbContext.Add(prediction);
                    }
                    dbContext.SaveChanges();
                }
                else
                {
                    Console.WriteLine(string.Format("The request failed with status code: {0}", response.StatusCode));

                    // Print the headers - they include the requert ID and the timestamp, which are useful for debugging the failure
                    Console.WriteLine(response.Headers.ToString());

                    string responseContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(responseContent);
                }
            }
        }
    }
    public class Value
    {
        public List<string> ColumnNames { get; set; }
        public List<string> ColumnTypes { get; set; }
        public List<List<string>> Values { get; set; }
    }
    public class Output1
    {
        public string type { get; set; }
        public Value value { get; set; }
    }
    public class Results
    {
        public Output1 output1 { get; set; }
    }
    public class RootObject
    {
        public Results Results { get; set; }
    }
    public class StringTable
    {
        public string[] ColumnNames { get; set; }
        public string[,] Values { get; set; }
    }
    public class PassengerInfo
    {
        public string StringDateOfBirth { get; set; }
        public string StringTravelDate { get; set; }
        public DateTime DateOfBirth { get; set; } = DateTime.Today.AddYears(-30);
        public string Origin { get; set; } = "Ahmedabad";
        public string Destination { get; set; } = "Mumbai";
        public TravelMode Mode { get; set; } = TravelMode.Train;
        public DateTime TravelDate { get; set; } = DateTime.Today.AddMonths(-3);
        public int Age { get; set; } = 30;
        public int Gender { get; set; } = 1;
    }
    public enum TravelMode
    {
        Bus = 1,
        Train,
        Air
    }

}
