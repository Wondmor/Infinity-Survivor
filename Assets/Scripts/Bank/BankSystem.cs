using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
public enum TransactionType
{
    Deposit,
    Withdrawal,
    TransferIn,
    TransferOut
}
[Serializable]
public class Transaction
{
    public DateTime Time;
    public TransactionType Type;
    public float Amount;
    public string FromAccount;
    public string ToAccount;

    public Transaction(DateTime time, TransactionType type, float amount, string from, string to)
    {
        Time = time;
        Type = type;
        Amount = amount;
        FromAccount = from;
        ToAccount = to;
    }
}
[Serializable]
public class Account
{
    public string AccountNumber;
    public Bank Bank;
    public float Balance;
    public List<Transaction> Transactions = new List<Transaction>();

    public Account(string number, Bank bank)
    {
        AccountNumber = number;
        Bank = bank;
        Balance = 0f;
    }

    public bool Deposit(float amount, TransactionType type = TransactionType.Deposit, string fromAccount = null)
    {
        if (amount <= 0) return false;
        Balance += amount;
        Transactions.Add(new Transaction(DateTime.Now, type, amount, fromAccount ?? AccountNumber, AccountNumber));
        return true;
    }

    public bool Withdraw(float amount, TransactionType type = TransactionType.Withdrawal, string toAccount = null)
    {
        if (amount <= 0 || Balance < amount) return false;
        Balance -= amount;
        Transactions.Add(new Transaction(DateTime.Now, type, amount, AccountNumber, toAccount ?? AccountNumber));
        return true;
    }

    public List<Transaction> GetTransactions(DateTime? start = null, DateTime? end = null)
    {
        if (start == null && end == null) return Transactions;
        return Transactions.Where(t => 
            (start == null || t.Time >= start) && 
            (end == null || t.Time <= end)).ToList();
    }
}
[Serializable]
public class Bank
{
    public string Name;
    public string Code;
    private List<Account> accounts = new List<Account>();

    public Account CreateAccount()
    {
        string accNumber = $"{Code}-{accounts.Count + 1:D3}";
        Account acc = new Account(accNumber, this);
        accounts.Add(acc);
        return acc;
    }

    public Account GetAccount(string accountNumber)
    {
        return accounts.Find(a => a.AccountNumber == accountNumber);
    }
}