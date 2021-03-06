﻿
using System;

using System.Collections.Generic;

using System.Linq;

using System.Web;

using System.ComponentModel.DataAnnotations;





namespace PraslaBonnerWondwossenFinalProject.Models

{

    // we may need to add more types here

    public enum TransactionTypes { Deposit, Transfer, Withdrawal, Fee }

    public class Transaction



    {

        //primary key
        [Key]
        public int TransactionID { get; set; }

        [Required(ErrorMessage = "Date is Required")]
        public DateTime Date { get; set; }

        [Required(ErrorMessage = "Select a valid Transaction Type")]

        public TransactionTypes Type { get; set; }

        [Required(ErrorMessage = "Enter a valid Tranaction Amount")]

        public Decimal Amount { get; set; }

        public String Description { get; set; }

        //define one to many relationships - One customer can have many transactions - need a customer class
        public virtual AppUser Customer { get; set; }

        public virtual Dispute Dispute { get; set; }

        [Display(Name = "To Account")]
        public virtual BankAccount ToAccount { get; set; }
        [Display(Name = "From Account")]
        public virtual BankAccount FromAccount { get; set; }


    }

}