using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BankDemo : MonoBehaviour
{
    void Start()
    {
        // 创建银行
        Bank bankA = new Bank { Name = "Bank A", Code = "A" };
        Bank bankB = new Bank { Name = "Bank B", Code = "B" };
        BankManager.Instance.Banks.AddRange(new[] { bankA, bankB });

        // 创建账户
        Account accA = bankA.CreateAccount();
        Account accB = bankB.CreateAccount();

        // 存款
        accA.Deposit(100);
        accB.Deposit(200);

        // 跨行转账
        bool success = BankManager.Instance.Transfer(accA.AccountNumber, accB.AccountNumber, 50);
        Debug.Log(success ? "Transfer successful" : "Transfer failed");

        // 查询账单
        Debug.Log("Account A Transactions:");
        foreach (var t in accA.GetTransactions())
            Debug.Log($"{t.Time} {t.Type} {t.Amount} From:{t.FromAccount} To:{t.ToAccount}");
    }
}