﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Persol_HMS.Models
{
    public class Drug
    {
        [Key]
        public int ID { get; set; }
        [ForeignKey(nameof(Patient))]
        public string PatientNo { get; set; }
        [ForeignKey(nameof(Medical))]
        public int MedicalID { get; set; }
        public string DrugName { get; set; }
        public string Dosage { get; set; }
        public double Price { get; set; } = 0;
        public string TimeOfDay { get; set; }
        public string TimeToTake { get; set; }
        public string? Note { get; set; }
        public DateTime Date { get; set; }
        public virtual Patient Patient { get; set; }
    }
}
