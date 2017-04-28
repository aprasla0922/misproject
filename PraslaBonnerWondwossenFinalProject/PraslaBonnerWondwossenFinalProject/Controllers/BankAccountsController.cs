﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using PraslaBonnerWondwossenFinalProject.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace PraslaBonnerWondwossenFinalProject.Controllers
{
    public class BankAccountsController : Controller
    {
        private AppDbContext db = new AppDbContext();

        // GET: BankAccounts
        public ActionResult Index()
        {
            return View(db.BankAccounts.ToList());
        }

        // GET: BankAccounts/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BankAccount bankAccount = db.BankAccounts.Find(id);
            if (bankAccount == null)
            {
                return HttpNotFound();
            }
            return View(bankAccount);
        }

        // GET: BankAccounts/Create
        [Authorize(Roles = "Customer")]
        public ActionResult Create()
        {
            AppUser person = db.Users.Find(User.Identity.GetUserId());
            if (person.isActive == true)
            {
                return View();
            }
            else
            {
                return RedirectToAction("InactiveAccountError", "Account");
            }

        }

        // POST: BankAccounts/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "BankAccountID,Type,Balance")] BankAccount bankAccount)
        {
            
                if (ModelState.IsValid)
                {
                    var item = db.BankAccounts.OrderByDescending(i => i.AccountNumber).FirstOrDefault();
                    bankAccount.AccountNumber = item.AccountNumber + 1;
                    //default names
                    if ((int)bankAccount.Type == 0)
                    {
                        bankAccount.Name = "Longhorn Savings";
                    }
                    else if ((int)bankAccount.Type == 1)
                    {
                        bankAccount.Name = "Longhorn Checking";
                    }
                    else if ((int)bankAccount.Type == 2)
                    {
                        bankAccount.Name = "Longhorn IRA";
                    }
                    else if ((int)bankAccount.Type == 3)
                    {
                        bankAccount.Name = "Longhorn Stock";
                    }
                    Decimal originalDepo = bankAccount.Balance;
                    Dispute now = null;
                    String transactionDescrip;
                    if (bankAccount.Balance > 5000)
                    {
                        bankAccount.Balance = 0;
                        //create dispute
                        now = new Dispute()
                        {
                            Status = Status.WaitingOnManager,
                            CustomerDescription = "Customer " + User.Identity.Name + "has submitted a deposit of " + String.Format("{0:C}", Convert.ToString(originalDepo)) + ". Please approve or deny this deposit.",
                            DisputeAmount = originalDepo
                            //TODO: Assign to Manager

                        };
                        transactionDescrip = "Account Opening Initial Deposit of $" + Convert.ToString(originalDepo) + " Waiting on Manager approval.";
                    }
                    else
                    {
                        transactionDescrip = "Initial Deposit of $" + Convert.ToString(originalDepo) + "";
                    }
                    AppUser current = db.Users.Find(User.Identity.GetUserId());
                    bankAccount.Customer = current;
                    current.BankAccounts.Add(bankAccount);
                    db.BankAccounts.Add(bankAccount);
                    //create transaction
                    Transaction deposit = new Transaction()
                    {
                        Date = DateTime.Now,
                        Type = TransactionTypes.Deposit,
                        Amount = bankAccount.Balance,
                        Description = transactionDescrip,
                        Customer = current,
                        Dispute = now,
                        ToAccount = bankAccount
                    };
                    db.Transactions.Add(deposit);
                    if (now != null)
                    {
                        db.Disputes.Add(now);
                    }
                    db.SaveChanges();
                    return RedirectToAction("Index", "Customers");
                }

                return View(bankAccount);
            
        }

        // GET: BankAccounts/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BankAccount bankAccount = db.BankAccounts.Find(id);
            if (bankAccount == null)
            {
                return HttpNotFound();
            }
            return View(bankAccount);
        }

        // POST: BankAccounts/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "BankAccountID,AccountNumber,Type,Name,Balance")] BankAccount bankAccount)
        {
            AppUser person = db.Users.Find(User.Identity.GetUserId());
            if (person.isActive == true)
            {
                if (ModelState.IsValid)
                {
                    db.Entry(bankAccount).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                return View(bankAccount);
            }
            else
            {
                //TODO:change so reditexts to error: inactive page
                return RedirectToAction("InactiveAccountError", "Account");
            }
        }

        // GET: BankAccounts/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BankAccount bankAccount = db.BankAccounts.Find(id);
            if (bankAccount == null)
            {
                return HttpNotFound();
            }
            return View(bankAccount);
        }

        // POST: BankAccounts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            BankAccount bankAccount = db.BankAccounts.Find(id);
            db.BankAccounts.Remove(bankAccount);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
