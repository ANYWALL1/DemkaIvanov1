using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IvanDemka1.Models
{
    [Table("Gender")]
    public class Gender
    {
        [Key]
        [StringLength(1)]
        public string Code { get; set; }

        [StringLength(10)]
        public string Name { get; set; }

        public virtual ICollection<Client> Clients { get; set; }
    }

    [Table("Client")]
    public class Client
    {
        [Key]
        public int ID { get; set; }

        [Required]
        [StringLength(50)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(50)]
        public string LastName { get; set; }

        [StringLength(50)]
        public string Patronymic { get; set; }

        public DateTime? Birthday { get; set; }

        [Required]
        public DateTime RegistrationDate { get; set; }

        [StringLength(255)]
        public string Email { get; set; }

        [Required]
        [StringLength(20)]
        public string Phone { get; set; }

        [Required]
        [StringLength(1)]
        public string GenderCode { get; set; }

        [StringLength(1000)]
        public string PhotoPath { get; set; }

        [ForeignKey("GenderCode")]
        public virtual Gender Gender { get; set; }

        public virtual ICollection<ClientService> ClientServices { get; set; }
    }

    [Table("Service")]
    public class Service
    {
        [Key]
        public int ID { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        [Required]
        public decimal Cost { get; set; }

        [Required]
        public int DurationInSeconds { get; set; }

        public string Description { get; set; }

        public double? Discount { get; set; }

        [StringLength(1000)]
        public string MainImagePath { get; set; }

        public virtual ICollection<ServicePhoto> ServicePhotos { get; set; }
        public virtual ICollection<ClientService> ClientServices { get; set; }
    }

    [Table("ServicePhoto")]
    public class ServicePhoto
    {
        [Key]
        public int ID { get; set; }

        [Required]
        public int ServiceID { get; set; }

        [Required]
        [StringLength(1000)]
        public string PhotoPath { get; set; }

        [ForeignKey("ServiceID")]
        public virtual Service Service { get; set; }
    }

    [Table("ClientService")]
    public class ClientService
    {
        [Key]
        public int ID { get; set; }

        [Required]
        public int ClientID { get; set; }

        [Required]
        public int ServiceID { get; set; }

        [Required]
        public DateTime StartTime { get; set; }

        public string Comment { get; set; }

        [ForeignKey("ClientID")]
        public virtual Client Client { get; set; }

        [ForeignKey("ServiceID")]
        public virtual Service Service { get; set; }
    }
}
