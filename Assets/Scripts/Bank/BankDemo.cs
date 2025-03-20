using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BankDemo : MonoBehaviour
{
    void Start()
    {
        // ��������
        Bank bankA = new Bank { Name = "Bank A", Code = "A" };
        Bank bankB = new Bank { Name = "Bank B", Code = "B" };
        BankManager.Instance.Banks.AddRange(new[] { bankA, bankB });

        // �����˻�
        Account accA = bankA.CreateAccount();
        Account accB = bankB.CreateAccount();

        // ���
        accA.Deposit(100);
        accB.Deposit(200);

        // ����ת��
        bool success = BankManager.Instance.Transfer(accA.AccountNumber, accB.AccountNumber, 50);
        Debug.Log(success ? "Transfer successful" : "Transfer failed");

        // ��ѯ�˵�
        Debug.Log("Account A Transactions:");
        foreach (var t in accA.GetTransactions())
            Debug.Log($"{t.Time} {t.Type} {t.Amount} From:{t.FromAccount} To:{t.ToAccount}");
    }
}