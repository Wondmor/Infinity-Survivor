using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BankManager : MonoBehaviour
{
    public static BankManager Instance;

    public List<Bank> Banks = new List<Bank>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public bool Transfer(string fromAccountNumber, string toAccountNumber, float amount)
    {
        // 解析转出账户
        string fromBankCode = fromAccountNumber.Split('-')[0];
        Bank fromBank = Banks.Find(b => b.Code == fromBankCode);
        if (fromBank == null) return false;

        Account fromAccount = fromBank.GetAccount(fromAccountNumber);
        if (fromAccount == null) return false;

        // 解析转入账户
        string toBankCode = toAccountNumber.Split('-')[0];
        Bank toBank = Banks.Find(b => b.Code == toBankCode);
        if (toBank == null) return false;

        Account toAccount = toBank.GetAccount(toAccountNumber);
        if (toAccount == null) return false;

        // 执行转账
        if (!fromAccount.Withdraw(amount, TransactionType.TransferOut, toAccountNumber))
            return false;

        if (!toAccount.Deposit(amount, TransactionType.TransferIn, fromAccountNumber))
        {
            // 存款失败，回滚转出
            fromAccount.Deposit(amount, TransactionType.TransferIn, toAccountNumber);
            return false;
        }

        return true;
    }
}