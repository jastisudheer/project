﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Persol_HMS.Models
{
    public class Lab
    {
        [Key]
        public int ID { get; set; }
        [ForeignKey(nameof(Patient))]
        public string PatientNo { get; set; }
        public string LabName { get; set; }
        [Required(ErrorMessage = "Please select a lab result.")]
        public string? Result { get; set; } = "";
        public string Status { get; set; } = "Pending";
        public string? Notes { get; set; } = "";
        [ForeignKey(nameof(Medical))]
        public int? MedicalID { get; set; }
        public string? Diagnosis { get; set; } = "";
        public DateTime Date { get; set; }
    }
}
