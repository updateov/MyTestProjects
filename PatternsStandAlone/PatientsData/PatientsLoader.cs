using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PatientsData
{
    public static class PatientsLoader
    {
        public static List<String> LoadPatients()
        {
            var toRet = new List<String>();
            toRet.Add((new Patient() { Key = "666-3-1-1", MRN = "4554", BedId = "2", BedName = "New Bed 1", FirstName = "Bha", LastName = "gwe", Fetuses = "1", GestationalAge = "36+4" }).ToString());
            toRet.Add((new Patient() { Key = "666-2-1-1", MRN = "6546", BedId = "3", BedName = "New Bed 2", FirstName = "Kty", LastName = "gg", Fetuses = "1", GestationalAge = "38+1" }).ToString());
            toRet.Add((new Patient() { Key = "666-3-1-1", MRN = "4554", BedId = "2", BedName = "New Bed 1", FirstName = "Bha", LastName = "gwe", Fetuses = "1", GestationalAge = "36+4" }).ToString());
            toRet.Add((new Patient() { Key = "666-2-1-1", MRN = "6546", BedId = "3", BedName = "New Bed 2", FirstName = "Rge", LastName = "bfr", Fetuses = "1", GestationalAge = "38+1" }).ToString());
            toRet.Add((new Patient() { Key = "666-3-1-1", MRN = "4554", BedId = "2", BedName = "New Bed 1", FirstName = "Bha", LastName = "gwe", Fetuses = "1", GestationalAge = "36+4" }).ToString());

            return toRet;
        }

        //public static List<String> LoadPatientsList()
        //{
        //    var patients = LoadPatients();
        //    var toRet = new List<String>();
        //    foreach (var item in patients)
        //    {
        //        toRet.Add(item.BedName + " " + item.FirstName[0] + item.LastName[0]);
        //    }

        //    return toRet;
        //}
    }
}
